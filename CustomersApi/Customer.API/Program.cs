using Customer.Application.Workers;
using Messenger.EventBus.EventBusRabbitMQ.Configuration;
using ParanaBanco.CustomerAPI.Configurations;

namespace ParanaBanco.CustomerAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDependencyInjectionConfiguration();
        
        builder.Services.AddEventBusRabbitMQ(builder.Configuration);
        builder.Services.AddHostedService<CustomerWorker>();

        builder.Services.AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        
        UseSwaggerConfiguration(app);

        app.Run();
    }
    
    public static void UseSwaggerConfiguration(IApplicationBuilder app)
    {
        app.UseSwagger(c => { c.RouteTemplate = "api/customers/{documentName}/swagger/swagger.json"; });

        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "api/customers/v1/swagger";
            c.SwaggerEndpoint("swagger.json", "Customers API V1 ....");
        });
    }
}
