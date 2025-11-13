using System.Reflection;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Endpoints;
using WebAPI.Extensions;
using WebAPI.Middleware;
using WebAPI.Security;
using WebAPI.Security.Enums;
using WebAPI.Services.Abstract;
using WebAPI.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Load secrets.json for sensitive data
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

// CORS
var frontendUrl = builder.Configuration["FrontendUrl"];
var allowedOrigins = frontendUrl.Split(',', StringSplitOptions.RemoveEmptyEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendClients", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Dependency Injection
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ITokenHelper, JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();

// Auth
builder.Services.AddScoped<LogService>();
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
if (tokenOptions is not null)
    builder.Services.RegisterAuth(tokenOptions);

// Database
builder.Services.AddDbContext<ApplicationDBContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// JSON
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policyBuilder => policyBuilder.RequireRole(Role.Admin.ToString()))
    .AddPolicy("seller", policyBuilder => policyBuilder.RequireRole(Role.Seller.ToString()))
    .AddPolicy("multi", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole(Role.Admin.ToString()) ||
        context.User.IsInRole(Role.Seller.ToString())));

// Rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var error = exceptionHandlerFeature?.Error;
        
        var errorResponse = new WebAPI.Models.ErrorResponse(
            statusCode: 500,
            message: "An unexpected error occurred.",
            details: app.Environment.IsDevelopment() ? error?.Message : null,
            traceId: context.TraceIdentifier
        );

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontendClients");
app.UseAuthentication();
app.UseAuthorization();

// Exception logging middleware
app.UseMiddleware<ExceptionLoggingMiddleware>();

// Endpoints
app.RegisterAuthEndpoints();
app.RegisterProductEndpoints();
app.RegisterCommentEndpoints();
app.RegisterAddressEndpoints();
app.RegisterOrderEndpoints();
app.RegisterLogEndpoints();

app.UseStaticFiles();
app.UseIpRateLimiting();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    db.Database.Migrate();
}

app.Run();
