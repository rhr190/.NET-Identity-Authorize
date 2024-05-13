using System.Text;
using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(Options =>
{
    Options.UseSqlServer(builder.
    Configuration.
    GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<JWTService>();

// defining our identity core service
builder.Services.AddIdentityCore<User>(options =>{

    //for password requirement
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})  
    .AddRoles<IdentityRole>() // add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // make use of role manager
    .AddEntityFrameworkStores<Context>() // providing DBContext
    .AddSignInManager<SignInManager<User>>() // make use of signin manager
    .AddUserManager<UserManager<User>>() // make use of user manager
    .AddDefaultTokenProviders(); // create token for email confirmation

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>{
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate token based on JWT:key
            ValidateIssuerSigningKey = true,
            // issuer signing key based on JWT:key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            // issuer param that we're using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            // validate the issuer
            ValidateIssuer = true,
            // don't validate audience (angular)
            ValidateAudience = false,
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
