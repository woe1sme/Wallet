namespace Wallet.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // Add services to the container.
        builder.Services.AddCustomDbContext(configuration);
        builder.Services.AddCustomSwagger();
        builder.Services.AddCustomMapping();
        builder.Services.AddCustomRepositories();
        builder.Services.AddCustomServices();
        builder.Services.AddCustomJsonSerializer();

        builder.Services.AddControllers();
        builder.Services.AddCustomMassTransit();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
