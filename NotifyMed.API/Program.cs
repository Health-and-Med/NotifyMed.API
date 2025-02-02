using NotifyMed.Application.Services;
using NotifyMed.Domain.Interfaces;
using NotifyMed.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configuração da string de conexão
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IUserService, UserService>();

// 🔹 Conexão com o banco de dados PostgreSQL
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

// 🔹 Configuração do JWT (Deve ser igual à configuração usada pelos microserviços)
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtConfig["Secret"]!);

builder.Services.AddAuthentication("Bearer") // 🔹 Mantendo compatível com Ocelot
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            ClockSkew = TimeSpan.Zero // 🔹 Evita problemas de delay na expiração do token
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🔹 Configuração do Swagger para suportar JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotifyMed.API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Digite 'Bearer {seu_token_jwt}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
    c.OperationFilter<AuthenticationRequirementsOperationFilter>();
});

var app = builder.Build();

// 🔹 Inicializar banco de dados (caso necessário)
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    dbInitializer.Initialize();
}

// 🔹 Configuração do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotifyMed.API V1");
        c.DisplayRequestDuration(); // Exibe tempo de resposta nas requisições
    });
}

// 🔹 Middleware de autenticação e autorização na ordem correta
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

// 🔹 Configuração do Swagger para proteger os endpoints autenticados
public class AuthenticationRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Security == null)
            operation.Security = new List<OpenApiSecurityRequirement>();

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [scheme] = new List<string>()
        });
    }
}

