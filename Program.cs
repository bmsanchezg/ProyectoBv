using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Security;
using BilleteraVirtual.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Grpc.AspNetCore.Web; // 📌 Asegúrate de importar esta librería
using System.Text;

try
{
    Console.WriteLine("🚀 Iniciando la aplicación...");

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        WebRootPath = "wwwroot",
        ContentRootPath = Directory.GetCurrentDirectory(),
        EnvironmentName = Environments.Development
    });

    // 🔹 Configurar Kestrel para usar HTTP/2 sin TLS en el puerto 5100
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5100, listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        });
    });

    Console.WriteLine("📂 Directorio actual: " + Directory.GetCurrentDirectory());

    // 📌 Verificar si `appsettings.json` existe antes de cargarlo
    string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
    if (!File.Exists(configPath))
    {
        throw new Exception($"🔴 ERROR: No se encontró `appsettings.json` en {configPath}");
    }
    Console.WriteLine("✅ `appsettings.json` encontrado.");

    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    Console.WriteLine("📌 Cargando configuración de JWT...");
    var jwtKeyString = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKeyString))
    {
        throw new Exception("🔴 ERROR: La clave JWT no está configurada en `appsettings.json`.");
    }
    Console.WriteLine("✅ Clave JWT cargada correctamente.");

    var jwtKey = Encoding.UTF8.GetBytes(jwtKeyString);
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    {
        throw new Exception("🔴 ERROR: Jwt:Issuer o Jwt:Audience no están configurados en `appsettings.json`.");
    }

    // 🔹 Configurar la conexión a PostgreSQL con manejo de errores
    try
    {
        Console.WriteLine("📌 Conectando a PostgreSQL...");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        Console.WriteLine("✅ Conexión a PostgreSQL establecida.");
    }
    catch (Exception ex)
    {
        throw new Exception($"🔴 ERROR al conectar a la base de datos: {ex.Message}");
    }

    // 🔹 Configurar autenticación JWT con logs detallados
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // ✅ Registrar logs en caso de error en autenticación
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"🔴 ERROR de autenticación: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("✅ Token JWT validado correctamente.");
                    return Task.CompletedTask;
                }
            };
        });

    // 🔹 Registrar los servicios de seguridad
    builder.Services.AddSingleton<JwtService>();
    builder.Services.AddSingleton<BCryptService>();

    // 🔹 Registrar servicios gRPC con gRPC-Web
    builder.Services.AddGrpc(options =>
    {
        options.EnableDetailedErrors = true; // Mostrar errores detallados
    });

    // 🔹 Agregar autorización
    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseRouting();

    app.UseAuthentication(); // ✅ Middleware de autenticación
    app.UseAuthorization();  // ✅ Middleware de autorización

    // ✅ Habilitar gRPC-Web (Necesario para Postman)
    app.UseGrpcWeb();

    // ✅ Registrar los servicios gRPC con soporte para gRPC-Web
    app.MapGrpcService<AuthService>().EnableGrpcWeb();
    app.MapGrpcService<BilleteraService>().EnableGrpcWeb();

    Console.WriteLine("✅ Servidor gRPC corriendo en http://localhost:5100...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"🔴 ERROR FATAL: {ex.Message}");
}
