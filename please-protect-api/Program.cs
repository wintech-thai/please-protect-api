using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Its.Otep.Api.Database.Seeders;
using Its.Otep.Api.Database;
using StackExchange.Redis;
using Its.Otep.Api.Utils;


namespace Its.Otep.Api
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connStr, o => o.CommandTimeout(1200)));
            builder.Services.AddTransient<DataSeeder>();

            builder.Services.AddScoped<IDataContext, DataContext>();

            builder.Services.AddHttpClient();
            builder.Services.AddHealthChecks();

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

            app.MapHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
