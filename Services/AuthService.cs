using Billetera;
using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Models;
using BilleteraVirtual.API.Security;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

public class AuthService : Billetera.AuthService.AuthServiceBase
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
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Correo o contraseña incorrectos"));
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return new AuthResponse { Token = token };
    }

    // ✅ Nuevo Método: Crear Usuario (Se especifica `Billetera.RegisterRequest`)
    public override async Task<RegisterResponse> CrearUsuario(Billetera.RegisterRequest request, ServerCallContext context)
    {
        // 🔹 Verificar si el correo ya está registrado
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "El correo ya está registrado"));
        }

        // 🔹 Encriptar la contraseña
        string hashedPassword = _bcryptService.HashPassword(request.Password);

        // 🔹 Crear el usuario en la base de datos
        var newUser = new User
        {
            Cedula = request.Cedula,
            FirstName = request.FirstName,
            Email = request.Email,
            Clave = hashedPassword
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(); // 🔹 Guardamos primero el usuario para obtener su ID

        // 🔹 Generar un nuevo ID de cuenta dentro del rango [100000 - 999999]
        int nextAccountId = await GetNextAccountId();

        var newAccount = new Account
        {
            Id = nextAccountId,
            UserId = newUser.Id,
            Amount = 0, // 🔹 La cuenta empieza con saldo en 0
            Status = 1  // 🔹 Estado activo
        };

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync(); // 🔹 Guardamos la cuenta en la BD

        return new RegisterResponse
        {
            UserId = newUser.Id,
            Message = "Usuario registrado exitosamente"
        };
    }
    private async Task<int> GetNextAccountId()
    {
        int minId = 100000;
        int maxId = 999999;

        var lastAccount = await _context.Accounts
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        if (lastAccount == null || lastAccount.Id < minId)
        {
            return minId; // 🔹 Si no hay cuentas, empezar desde el mínimo
        }

        int nextId = lastAccount.Id + 1;

        return (nextId > maxId) ? minId : nextId; // 🔹 Si se pasa del rango, reinicia el ID
    }

}
