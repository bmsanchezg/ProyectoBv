using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configurar Kestrel para usar HTTP/2 (Necesario para gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // 🔹 HTTP/2 obligatorio para gRPC
    });

    // 🔹 Opcional: Habilitar HTTPS para Postman (descomentar si es necesario)
    // options.ListenLocalhost(5001, listenOptions =>
    // {
    //     listenOptions.UseHttps(); // 🔹 Habilita TLS en el puerto 5001
    //     listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    // });
});

// 🔹 Configurar la conexión a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Registrar servicios gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// 🔹 Asegurar que gRPC funcione correctamente
app.UseRouting();

app.MapGrpcService<BilleteraService>();

app.Run();
