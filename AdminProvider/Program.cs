using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using AdminProvider.ModeratorsManagement.Services;
using AdminProvider.ModeratorsManagement.Utillities;
using AdminProvider.OrdersManagement.Data;
using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models;
using AdminProvider.OrdersManagement.Services;
using AdminProvider.ProductsManagement.Data;
using AdminProvider.ProductsManagement.Interfaces;
using AdminProvider.ProductsManagement.Repositories;
using AdminProvider.ProductsManagement.Services;
using AdminProvider.UsersManagement.Data;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Repositories;
using AdminProvider.UsersManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
