using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Security;
using BilleteraVirtual.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configurar la conexión a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Configurar autenticación JWT
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 🔹 Registrar los servicios de seguridad
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<BCryptService>();

// 🔹 Registrar servicios gRPC
builder.Services.AddGrpc();

// 🔹 Agregar autorización
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication(); // ✅ Middleware de autenticación
app.UseAuthorization();  // ✅ Middleware de autorización

// 🔹 Registrar los servicios gRPC
app.MapGrpcService<BilleteraService>();
app.MapGrpcService<AuthService>(); // Nuevo servicio para autenticación

app.Run();
