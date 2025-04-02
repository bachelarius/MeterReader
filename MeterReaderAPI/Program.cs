using MeterReaderAPI.Accounts;
using MeterReaderAPI.Data;
using MeterReaderAPI.MeterReadings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

internal class Program {
    private static async Task Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new OpenApiInfo {
                Version = "v1",
                Title = "Meter Reader API",
                Description = "An API for processing meter readings from CSV files"
            });
        });

        // Add DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddSingleton<IAccountsExtractorService, AccountExtractorService>();
        builder.Services.AddTransient<IAccountsSeedService, AccountsSeedService>();
        builder.Services.AddTransient<IMeterReadingExtractorService, MeterReadingExtractorService>();
        builder.Services.AddTransient<IMeterReadingValidator, MeterReadingValidator>();

        var app = builder.Build();

        //Initialize the database
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await app.Services.InitializeDatabaseAsync(cts.Token);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty; // Serves the Swagger UI at the app's root
            });
        }

        app.UseHttpsRedirection();

        app.MapPost("/meter-reading-uploads", async ([FromForm] MeterReadingUploadRequest request, IMeterReadingExtractorService meterReadingExtractor, IMeterReadingValidator meterReadingValidator) => 
            await MeterReadingUploadsHandler.Handle(request.File, meterReadingExtractor, meterReadingValidator))
            .WithName("UploadMeterReadings")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Upload meter readings",
                Description = "Uploads and processes a CSV file containing meter readings",
                RequestBody = new OpenApiRequestBody
                {
                    Description = "CSV file containing meter readings",
                    Required = true,
                    Content = 
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = 
                                {
                                    ["File"] = new OpenApiSchema // Note: Capital F to match the property name
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "CSV file with columns: AccountId,MeterReadingDateTime,MeterReadValue"
                                    }
                                },
                                Required = new HashSet<string> { "File" }
                            }
                        }
                    }
                }
            })
            .DisableAntiforgery(); // Disable antiforgery for this endpoint

        app.Run();
    }
}
