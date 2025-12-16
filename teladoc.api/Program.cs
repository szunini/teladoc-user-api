using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using teladoc.dao.Context;
using teladoc.dao.repository;
using teladoc.domain.Contracts.Repositories;
using teladoc.domain.Contracts.Services;
using teladoc.domain.Services;
using teladoc.infraestructure.Caching;
using Teladoc.api.Middleware;

namespace Teladoc.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("SqlServer");
            builder.Services.AddDbContext<TeladocDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

            // Register repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Register services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserReadService, UserReadService>();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer(); 
            builder.Services.AddSwaggerGen(options =>
            {                
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            });
            builder.Services.AddSingleton(TimeProvider.System);

            var app = builder.Build();

            // Apply pending migrations and create database if it doesn't exist
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TeladocDbContext>();
                dbContext.Database.Migrate();
            }

            // Add exception handling middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Map OpenAPI minimal endpoints
               //app.MapOpenApi();
                // Enable middleware to serve generated Swagger as JSON endpoint and Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teladoc API V1");
                    options.RoutePrefix = "swagger";
                    options.DisplayRequestDuration();                   
                    
                });
            }
            app.Use(async (context, next) =>
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RequestScope");
                using (logger.BeginScope(new Dictionary<string, object>
                {
                    ["TraceId"] = context.TraceIdentifier
                }))
                {
                    await next();
                }
            });
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
