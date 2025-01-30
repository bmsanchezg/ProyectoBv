using Grpc.Core;
using BilleteraVirtual.API.Data;
using Billetera;
using Microsoft.EntityFrameworkCore;


namespace BilleteraVirtual.API.Services
{
    public class BilleteraService : Billetera.BilleteraService.BilleteraServiceBase
    {
        private readonly ApplicationDbContext _context;

        public BilleteraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task<SaldoResponse> ObtenerSaldo(SaldoRequest request, ServerCallContext context)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == request.AccountId) // ✅ Asegurar que se compara con `Id`
                .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Cuenta no encontrada"));
            }

            return new SaldoResponse { Saldo = (double)account.Amount };
        }

        public override async Task<TransaccionResponse> RealizarTransaccion(TransaccionRequest request, ServerCallContext context)
        {
            var sender = await _context.Accounts.FindAsync(request.SenderAccountId);
            var receiver = await _context.Accounts.FindAsync(request.ReceiverAccountId);

            if (sender == null || receiver == null)
            {
                return new TransaccionResponse { Status = "FAILED - Account not found" };
            }

            if (sender.Amount < (decimal)request.Amount)
            {
                return new TransaccionResponse { Status = "FAILED - Insufficient funds" };
            }

            sender.Amount -= (decimal)request.Amount;
            receiver.Amount += (decimal)request.Amount;

            await _context.SaveChangesAsync();

            return new TransaccionResponse { Status = "COMPLETED" };
        }

    }
}
