using Microsoft.EntityFrameworkCore;
using BilleteraVirtual.API.Data;
using BilleteraVirtual.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework Core con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar soporte para gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// Configurar middleware
app.UseRouting();

app.MapGrpcService<BilleteraService>();

app.Run();
