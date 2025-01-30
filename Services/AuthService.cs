using Billetera;
using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Models;
using BilleteraVirtual.API.Security;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

public class AuthService : Auth.AuthBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly BCryptService _bcryptService;

    public AuthService(ApplicationDbContext context, JwtService jwtService, BCryptService bcryptService)
    {
        _context = context;
        _jwtService = jwtService;
        _bcryptService = bcryptService;
    }

    public override async Task<AuthResponse> Login(AuthRequest request, ServerCallContext context)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_bcryptService.VerifyPassword(request.Password, user.Clave))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password"));
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return new AuthResponse { Token = token };
    }
}
