using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using AdminProvider.ModeratorsManagement.Services;
using AdminProvider.ModeratorsManagement.Utillities;
using AdminProvider.OrdersManagement.Data;
using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Repositories;
using AdminProvider.OrdersManagement.Services;
using AdminProvider.ProductsManagement.Data;
using AdminProvider.ProductsManagement.Interfaces;
using AdminProvider.ProductsManagement.Repositories;
using AdminProvider.ProductsManagement.Services;
using AdminProvider.UsersManagement.Data;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Repositories;
using AdminProvider.UsersManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
// Access Token Validation
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtAccess:Key"])),
            ValidIssuer = builder.Configuration["JwtAccess:Issuer"],
            ValidAudience = builder.Configuration["JwtAccess:Audience"],
            ClockSkew = TimeSpan.Zero, // No clock skew for strict expiration checks
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.HttpContext.Request.Cookies["AccessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token; 
                }
                else
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("No token found in cookie.");
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                if (claimsIdentity == null)
                {
                    context.Fail("Token är ogiltigt. Claims identity är null.");
                    return Task.CompletedTask;
                }

                // Extract role claim
                var roleClaim = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(roleClaim))
                {
                    context.Fail("Token saknar roll.");
                    return Task.CompletedTask;
                }

                // Log the extracted role (for debugging)
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Användarens roll: {roleClaim}");

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError($"Authentication failed: {context.Exception.Message}");
                if (context.Exception.StackTrace != null)
                {
                    logger.LogError(context.Exception.StackTrace);
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
builder.Services.AddScoped<ICustomPasswordHasher<AdminEntity>, CustomPasswordHasher>();

builder.Services.AddScoped<ISignOutService, SignOutService>();
builder.Services.AddScoped<ISignInService, SignInService>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Configure Database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDatabase")), ServiceLifetime.Scoped);
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDatabase")), ServiceLifetime.Scoped);
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")), ServiceLifetime.Scoped);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Moderator", policy => policy.RequireRole("Moderator"));
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5000") // Frontend URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Allow credentials (cookies, tokens)
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
