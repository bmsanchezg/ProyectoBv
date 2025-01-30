using Grpc.Core;
using BilleteraVirtual.API.Data;
using Billetera;
using Microsoft.EntityFrameworkCore;
using BilleteraVirtual.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BilleteraVirtual.API.Services
{
    public class BilleteraService : Billetera.BilleteraService.BilleteraServiceBase
    {
        private readonly ApplicationDbContext _context;

        public BilleteraService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Método para verificar si el usuario está autenticado
        private int? GetAuthenticatedUserId(ServerCallContext context)
        {
            var authHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization")?.Value;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token no proporcionado o inválido"));
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }

        public override async Task<SaldoResponse> ObtenerSaldo(SaldoRequest request, ServerCallContext context)
        {
            // 🔹 Verifica que el usuario esté autenticado
            int? userId = GetAuthenticatedUserId(context);
            if (userId == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Usuario no autenticado"));
            }

            var account = await _context.Accounts
                .Where(a => a.Id == request.AccountId)
                .FirstOrDefaultAsync();

            if (account == null || account.UserId != userId)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "No tienes acceso a esta cuenta"));
            }

            return new SaldoResponse { Saldo = (double)account.Amount };
        }

        public override async Task<TransaccionResponse> RealizarTransaccion(TransaccionRequest request, ServerCallContext context)
        {
            // 🔹 Verifica que el usuario esté autenticado
            int? userId = GetAuthenticatedUserId(context);
            if (userId == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Usuario no autenticado"));
            }

            var sender = await _context.Accounts.FindAsync(request.SenderAccountId);
            var receiver = await _context.Accounts.FindAsync(request.ReceiverAccountId);

            if (sender == null || receiver == null)
            {
                return new TransaccionResponse { Status = "FAILED - Account not found" };
            }

            // 🔹 Verifica que la cuenta de origen pertenece al usuario autenticado
            if (sender.UserId != userId)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "No tienes permiso para operar esta cuenta"));
            }

            if (request.Amount <= 0)
            {
                return new TransaccionResponse { Status = "FAILED - Invalid amount" };
            }

            if (sender.Amount < (decimal)request.Amount)
            {
                return new TransaccionResponse { Status = "FAILED - Insufficient funds" };
            }

            // 🔹 Restar saldo de la cuenta origen
            sender.Amount -= (decimal)request.Amount;

            // 🔹 Sumar saldo en la cuenta destino
            receiver.Amount += (decimal)request.Amount;

            // 🔹 Obtener el próximo ID válido dentro del rango [10000000 - 99999999]
            int nextTransactionId = await GetNextTransactionId();

            var transaction = new Transaction
            {
                Id = nextTransactionId, // ID generado automáticamente dentro del rango correcto
                AccountSend = sender.Id,
                AccountRecived = receiver.Id,
                Amount = (decimal)request.Amount,
                Status = "COMPLETED"
            };

            // 🔹 Guardar la transacción y actualizar las cuentas en una sola operación
            using (var transactionScope = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Transactions.Add(transaction);
                    _context.Accounts.Update(sender);
                    _context.Accounts.Update(receiver);

                    await _context.SaveChangesAsync(); // Guardar cambios en la BD

                    await transactionScope.CommitAsync(); // Confirmar la transacción

                    return new TransaccionResponse { Status = "COMPLETED" };
                }
                catch (Exception)
                {
                    await transactionScope.RollbackAsync(); // Revertir en caso de error
                    return new TransaccionResponse { Status = "FAILED - Error processing transaction" };
                }
            }
        }

        // 🔹 Función para obtener el siguiente ID en el rango de [10000000 - 99999999]
        private async Task<int> GetNextTransactionId()
        {
            int minId = 10000000;
            int maxId = 99999999;

            var lastTransaction = await _context.Transactions
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (lastTransaction == null || lastTransaction.Id < minId)
            {
                return minId;
            }

            int nextId = lastTransaction.Id + 1;

            return (nextId > maxId) ? minId : nextId;
        }
    }
}
