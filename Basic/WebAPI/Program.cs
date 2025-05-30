using FluentValidation;
using FootballAPI.Security;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebAPI.Data;
using WebAPI.Endpoints;
using WebAPI.Extensions;
using WebAPI.Security;
using WebAPI.Security.Enums;
using WebAPI.Services.Abstract;
using WebAPI.Services.Concrete;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// CORS: Dinamik izin verilecek origin listesi
string[] allowedOrigins = new[]
{
    "http://localhost:4200",
    "http://localhost:8100"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendClients", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// DI - Servisler
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<ITokenHelper, JwtHelper>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();

// 🔐 Auth ayarları
TokenOptions? tokenOptions = builder.Configuration
    .GetSection("TokenOptions").Get<TokenOptions>();
builder.Services.RegisterAuth(tokenOptions);

// 📦 DB bağlantısı
builder.Services.AddDbContext<ApplicationDBContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresSQL")));

// 🔄 JSON döngü hataları önleniyor
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// 👮 Yetkilendirme politikaları
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policyBuilder => { policyBuilder.RequireRole(Role.Admin.ToString()); })
    .AddPolicy("seller", policyBuilder => { policyBuilder.RequireRole(Role.Seller.ToString()); })
    .AddPolicy("multi", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole(Role.Admin.ToString()) ||
        context.User.IsInRole(Role.Seller.ToString())));

var app = builder.Build();

// 🧪 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🌐 CORS aktif ediliyor
app.UseCors("AllowFrontendClients");

// 🔐 Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// 🧩 API endpointleri
app.RegisterAuthEndpoints();
app.RegisterProductEndpoints();
app.RegisterCommentEndpoints();
app.RegisterAddressEndpoints();
app.RegisterOrderEndpoints();

app.UseStaticFiles();
app.Run();
