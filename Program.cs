using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<KeycloakAdminService>();
builder.Services.AddScoped<ProfileService>();

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionTypeService, TransactionTypeService>();
builder.Services.AddScoped<IMonthlyBudgetService, MonthlyBudgetService>();
builder.Services.AddScoped<IAiConfigService, AiConfigService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddHttpClient();

// ── Autenticação JWT via Keycloak ─────────────────────────────────────
var keycloakClientId = builder.Configuration["Keycloak:ClientId"] ?? "quantia-frontend";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority          = builder.Configuration["Keycloak:Authority"];
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims   = false;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false, // Keycloak não inclui o clientId no aud por padrão
        };
        // Valida que o token foi emitido especificamente para este client (azp)
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var azp = ctx.Principal?.FindFirst("azp")?.Value;
                if (azp != keycloakClientId)
                    ctx.Fail($"Token não autorizado para este cliente. azp={azp}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuantIA API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Token JWT obtido via Keycloak. Formato: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS restrito ao domínio do frontend ─────────────────────────────
var frontendUrl = builder.Configuration["Frontend:Url"] ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
