using System.Text;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WppSender.Api.Middleware;
using WppSender.Application.Auth;
using WppSender.Application.Campanhas;
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

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

// O worker do Hangfire não roda durante os testes de integração (WebApplicationFactory
// usa o Environment "Testing"): os testes chamam ProcessarProximoEnvioUseCase diretamente
// pra simular um passo do motor, de forma determinística, sem depender de um worker real.
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
builder.Services.AddScoped<ProcessarProximoEnvioUseCase>();
builder.Services.AddSingleton<IRelogio, RelogioSistema>();
builder.Services.Configure<WhatsAppServiceOptions>(builder.Configuration.GetSection("WhatsAppService"));
builder.Services.AddHttpClient<IWhatsAppClient, HttpWhatsAppClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
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
builder.Services.AddScoped<ListarEnviosFalhosUseCase>();
builder.Services.AddScoped<ReenviarFalhasUseCase>();
builder.Services.AddScoped<IniciarSessaoWhatsAppUseCase>();
builder.Services.AddScoped<ObterStatusSessaoUseCase>();

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
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    RecurringJob.AddOrUpdate<VarredorDeCampanhasAgendadasJob>(
        "varredor-campanhas-agendadas",
        job => job.ExecutarAsync(),
        Cron.Minutely);
}

app.Run();

public partial class Program { }
