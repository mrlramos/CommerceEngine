using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuração do Ocelot
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configuração da autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CommerceEngine_SuperSecretKey_2025_Identity_JWT_Token_Signing_Key_Must_Be_At_Least_256_Bits";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CommerceEngine.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CommerceEngine.APIs";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = jwtIssuer;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adicionar controladores
builder.Services.AddControllers();

// Adicionar Ocelot
builder.Services.AddOcelot();

// Adicionar Swagger para documentação
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CommerceEngine API Gateway",
        Version = "v1",
        Description = "Gateway centralizado para todos os microsserviços do CommerceEngine",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "CommerceEngine Team",
            Email = "dev@commerceengine.com"
        }
    });

    // Configuração para JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configuração do pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// Middleware de logging personalizado
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Gateway Request: {Method} {Path} from {RemoteIp}",
        context.Request.Method,
        context.Request.Path,
        context.Connection.RemoteIpAddress);

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    await next();
    
    stopwatch.Stop();
    
    logger.LogInformation("Gateway Response: {StatusCode} in {ElapsedMs}ms",
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

// Endpoints específicos do Gateway (antes do Ocelot)
app.MapGet("/gateway/health", (IWebHostEnvironment env) => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = env.EnvironmentName
});

app.MapGet("/gateway/info", (IWebHostEnvironment env) => new
{
    Name = "CommerceEngine API Gateway",
    Version = "1.0.0",
    Environment = env.EnvironmentName,
    Uptime = DateTime.UtcNow,
    Services = new
    {
        CatalogAPI = "http://localhost:5001",
        InventoryAPI = "http://localhost:5002",
        OrderAPI = "http://localhost:5003",
        IdentityAPI = "http://localhost:5004"
    }
});

// Mapear controladores
app.MapControllers();

// Usar Ocelot
await app.UseOcelot();

app.Logger.LogInformation("API Gateway iniciado na porta {Port}", 
    app.Environment.IsDevelopment() ? "5000" : "80");

app.Run(); 