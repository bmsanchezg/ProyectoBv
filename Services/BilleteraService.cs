using Grpc.Core;
using BilleteraVirtual.API.Data;
using Billetera;


namespace BilleteraVirtual.API.Services
{
    public class BilleteraService : Billetera.BilleteraService.BilleteraServiceBase
    {
        private readonly ApplicationDbContext _context;

        public BilleteraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public override Task<SaldoResponse> ObtenerSaldo(SaldoRequest request, ServerCallContext context)
        {
            var account = _context.Accounts.Find(request.AccountId);

            return Task.FromResult(new SaldoResponse
            {
                Saldo = (double)(account?.Amount ?? 0)
            });
        }
    }
}
