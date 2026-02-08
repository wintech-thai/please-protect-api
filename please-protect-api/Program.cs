using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Database.Seeders;
using Its.PleaseProtect.Api.Database;
using StackExchange.Redis;
using Its.PleaseProtect.Api.Utils;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Its.PleaseProtect.Api.Authorizations;
using Its.PleaseProtect.Api.Authentications;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Its.PleaseProtect.Api.AuditLogs;
using System.Text;
using System.Net.Http.Headers;

namespace Its.PleaseProtect.Api
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        public static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger = log;


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);


            var cfg = builder.Configuration;
            var connStr = $"Host={cfg["PostgreSQL:Host"]}; Database={cfg["PostgreSQL:Database"]}; Username={cfg["PostgreSQL:User"]}; Password={cfg["PostgreSQL:Password"]}";


            var redisHostStr = $"{cfg["Redis:Host"]}:{cfg["Redis:Port"]}"; 
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                sp => ConnectionMultiplexer.Connect(redisHostStr));

            builder.Services.AddScoped<RedisHelper>();
            builder.Services.AddSingleton<IRedisHelper, RedisHelper>();


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connStr, o => o.CommandTimeout(1200)));
            builder.Services.AddTransient<DataSeeder>();

            builder.Services.AddScoped<IDataContext, DataContext>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
            builder.Services.AddScoped<IOrganizationUserRepository, OrganizationUserRepository>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IIoCRepository, IoCRepository>();
            builder.Services.AddScoped<ISubnetRepository, SubnetRepository>();
            builder.Services.AddScoped<ICustomRoleRepository, CustomRoleRepository>();


            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IOrganizationService, OrganizationService>();
            builder.Services.AddScoped<IOrganizationUserService, OrganizationUserService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IIoCService, IoCService>();
            builder.Services.AddScoped<ICustomRoleService, CustomRoleService>();
            builder.Services.AddScoped<ISubnetService, SubnetService>();
            builder.Services.AddScoped<IObjectStorageService, MinioObjectStorageService>();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();


            builder.Services.AddTransient<IAuthorizationHandler, GenericRbacHandler>();
            builder.Services.AddScoped<IBasicAuthenticationRepo, BasicAuthenticationRepo>();
            builder.Services.AddScoped<IBearerAuthenticationRepo, BearerAuthenticationRepo>();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("es-proxy", c =>
            {
                var url  = Environment.GetEnvironmentVariable("ES_URL");
                var user = Environment.GetEnvironmentVariable("ES_USER");
                var pass = Environment.GetEnvironmentVariable("ES_PASSWORD");

                if (string.IsNullOrWhiteSpace(url))
                    throw new Exception("ES_URL is not set");

                c.BaseAddress = new Uri(url);
                c.Timeout = TimeSpan.FromSeconds(100);

                // ถ้ามี user/pass → ใส่ Basic Auth
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
                {
                    var token = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{user}:{pass}")
                    );

                    c.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", token);
                }
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return handler;
            });

            builder.Services.AddHealthChecks();

            builder.Services.AddAuthentication("BasicOrBearer")
                .AddScheme<AuthenticationSchemeOptions, AuthenticationHandlerProxy>("BasicOrBearer", null);

            builder.Services.AddAuthorization(options => {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("BasicOrBearer");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                options.AddPolicy("GenericRolePolicy", policy => policy.AddRequirements(new GenericRbacRequirement()));
            });
            
            // เปิด middleware สำหรับ gzip
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // ให้บีบอัดแม้เป็น HTTPS
                options.Providers.Add<GzipCompressionProvider>();
            });
            
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                dbContext.Database.Migrate();

                var service = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                service.Seed();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<AuditLogMiddleware>();
            app.MapHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
