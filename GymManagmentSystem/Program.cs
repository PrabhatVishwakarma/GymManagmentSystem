using GymManagmentSystem.Models;
using GymManagmentSystem.Data;
using GymManagmentSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

// Configure DinkToPdf - Load native library
var wkHtmlToPdfPath = Path.Combine(builder.Environment.ContentRootPath, "libwkhtmltox");
CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(wkHtmlToPdfPath, "libwkhtmltox.dll"));

// Register DinkToPdf Converter
builder.Services.AddSingleton(typeof(DinkToPdf.Contracts.IConverter), new DinkToPdf.SynchronizedConverter(new DinkToPdf.PdfTools()));

// Register Services
builder.Services.AddScoped<RoleSeederService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PdfReceiptService>();

// Add CORS - Read allowed origins from configuration
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000" }; // Fallback if not configured

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS before HTTPS redirection to avoid preflight issues
app.UseCors("AllowReactApp");

// Disable HTTPS redirection in development to prevent CORS preflight errors
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeederService>();
    roleSeeder.SeedRolesAsync().GetAwaiter().GetResult();
    roleSeeder.SeedAdminUserAsync().GetAwaiter().GetResult();
}

app.Run();
