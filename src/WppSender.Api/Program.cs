using System.Text;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WppSender.Api.Middleware;
using WppSender.Application.Auth;
using WppSender.Application.Campanhas;
using WppSender.Application.Dashboard;
using WppSender.Application.Grupos;
using WppSender.Application.Leads;
using WppSender.Domain;
using WppSender.Infrastructure.Csv;
using WppSender.Infrastructure.Jobs;
using WppSender.Infrastructure.Persistence;
using WppSender.Infrastructure.Security;
using WppSender.Infrastructure.Tempo;
using WppSender.Infrastructure.WhatsApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    Func<HttpContext, RateLimitPartition<string>> particionarPorIp = httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "sem-ip",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });

    options.AddPolicy("login", particionarPorIp);
    options.AddPolicy("registrar", particionarPorIp);
});

var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (origensPermitidas.Length > 0)
        {
            policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "WppSender API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole aqui o token retornado por /api/auth/login (sem o prefixo 'Bearer ')"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

builder.Services.AddDbContext<WppSenderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default"))));

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHangfireServer();
}

builder.Services.AddScoped<IUsuarioRepository, EfUsuarioRepository>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<AutenticarUsuarioUseCase>();
builder.Services.AddScoped<RegistrarUsuarioUseCase>();

builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<IGrupoRepository, EfGrupoRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<ICampanhaRepository, EfCampanhaRepository>();
builder.Services.AddScoped<IEnvioRepository, EfEnvioRepository>();
builder.Services.AddScoped<IConfiguracaoEnvioRepository, EfConfiguracaoEnvioRepository>();
builder.Services.AddScoped<ISessaoWhatsAppRepository, EfSessaoWhatsAppRepository>();
builder.Services.AddScoped<ICampanhaJobScheduler, HangfireCampanhaJobScheduler>();
builder.Services.AddScoped<CampanhaSendJob>();
builder.Services.AddScoped<VarredorDeCampanhasAgendadasJob>();
builder.Services.AddScoped<AtualizarStatusEntregaJob>();
builder.Services.AddScoped<ProcessarProximoEnvioUseCase>();
builder.Services.AddScoped(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppServiceOptions>>().Value;
    return new AtualizarStatusEntregaUseCase(
        sp.GetRequiredService<IEnvioRepository>(),
        sp.GetRequiredService<ICampanhaRepository>(),
        sp.GetRequiredService<IWhatsAppClient>(),
        sp.GetRequiredService<IRelogio>(),
        options.TimeoutConfirmacaoMinutos);
});
builder.Services.AddSingleton<IRelogio, RelogioSistema>();
builder.Services.Configure<WhatsAppServiceOptions>(builder.Configuration.GetSection("WhatsAppService"));
builder.Services.AddHttpClient<IWhatsAppClient, HttpWhatsAppClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
});
builder.Services.AddScoped<ILeadCsvParser, CsvHelperLeadParser>();
builder.Services.AddScoped<ILeadCsvWriter, CsvHelperLeadWriter>();
builder.Services.AddScoped<CriarLeadUseCase>();
builder.Services.AddScoped<EditarLeadUseCase>();
builder.Services.AddScoped<ExcluirLeadUseCase>();
builder.Services.AddScoped<ListarLeadsUseCase>();
builder.Services.AddScoped<ObterLeadUseCase>();
builder.Services.AddScoped<ImportarLeadsCsvUseCase>();
builder.Services.AddScoped<ExportarLeadsCsvUseCase>();
builder.Services.AddScoped<CriarGrupoUseCase>();
builder.Services.AddScoped<EditarGrupoUseCase>();
builder.Services.AddScoped<ExcluirGrupoUseCase>();
builder.Services.AddScoped<ListarGruposUseCase>();

builder.Services.AddScoped<CriarCampanhaUseCase>();
builder.Services.AddScoped<EditarCampanhaUseCase>();
builder.Services.AddScoped<ExcluirCampanhaUseCase>();
builder.Services.AddScoped<ListarCampanhasUseCase>();
builder.Services.AddScoped<ObterCampanhaUseCase>();
builder.Services.AddScoped<IniciarCampanhaUseCase>();
builder.Services.AddScoped<PausarCampanhaUseCase>();
builder.Services.AddScoped<RetomarCampanhaUseCase>();
builder.Services.AddScoped<CancelarCampanhaUseCase>();
builder.Services.AddScoped<ListarEnviosFalhosUseCase>();
builder.Services.AddScoped<ReenviarFalhasUseCase>();
builder.Services.AddScoped<IniciarSessaoWhatsAppUseCase>();
builder.Services.AddScoped<ObterStatusSessaoUseCase>();

builder.Services.AddScoped<ContarCampanhasPorStatusUseCase>();
builder.Services.AddScoped<ContarLeadsRecentesUseCase>();
builder.Services.AddScoped<CalcularTaxaEnvioUseCase>();
builder.Services.AddScoped<ContarLeadsPorGrupoUseCase>();
builder.Services.AddScoped<ObterProximaCampanhaAgendadaUseCase>();
builder.Services.AddScoped<ObterLimiteDiarioEnvioUseCase>();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
    {
        var signingKey = jwtOptions.Value.SigningKey;
        if (string.IsNullOrEmpty(signingKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey nao configurado");
        }

        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Value.Audience,
            ValidateLifetime = true
        };

        bearerOptions.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.TryGetValue("wpp_auth", out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

var proxiesConhecidos = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
if (proxiesConhecidos.Length > 0)
{
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };
    foreach (var proxy in proxiesConhecidos)
    {
        forwardedHeadersOptions.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
    }
    app.UseForwardedHeaders(forwardedHeadersOptions);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var escopo = app.Services.CreateScope();
    var recurringJobManager = escopo.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<VarredorDeCampanhasAgendadasJob>(
        "varredor-campanhas-agendadas",
        job => job.ExecutarAsync(),
        Cron.Minutely);
    recurringJobManager.AddOrUpdate<AtualizarStatusEntregaJob>(
        "atualizar-status-entrega",
        job => job.ExecutarAsync(),
        Cron.Minutely);
}

app.Run();

public partial class Program { }
