using Microsoft.EntityFrameworkCore;
using Fitness.Data.Context;
using Fitness.Data.Repositories;
using Fitness.Business.Interfaces;
using Fitness.Business.Services;

namespace Fitness.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var logger = LoggerFactory.Create(config => config.AddConsole())
                .CreateLogger<Program>();
            logger.LogInformation($"Connection string being used: {connectionString}");

            // Services toevoegen aan de container
            builder.Services.AddDbContext<FitnessContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.LogTo(message => logger.LogInformation(message));
            });

            // Registratie van repositories
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            builder.Services.AddScoped<IProgramRepository, ProgramRepository>();
            builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
            builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            builder.Services.AddScoped<IRunningSessionRepository, RunningSessionRepository>();
            builder.Services.AddScoped<ICyclingSessionRepository, CyclingSessionRepository>();
            builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

            // Registratie van services
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
                { 
                    Title = "Fitness API", 
                    Version = "v1",
                    Description = "API for fitness equipment reservations"
                });
                
                // Enable XML comments
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                // Comment out HTTPS redirection in development
                // app.UseHttpsRedirection();
            }
            else 
            {
                app.UseHttpsRedirection();
            }

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
