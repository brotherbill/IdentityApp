using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<Context>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// be able to inject JWTService class inside our Controllers
builder.Services.AddScoped<JwtScoped>();

// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options => {
        // password configuration
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // for email confirmation
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()                       // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>()    // be able to make use of RoleManager
    .AddEntityFrameworkStores<Context>()            // provide the DbContext to Identity
    .AddSignInManager<SignInManager<User>>()        // provide the SignInManager to Identity
    .AddUserManager<UserManager<User>>()            // make use of UserManager to create users
    .AddDefaultTokenProviders();                    // be able to create tokens for email confirmation

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
