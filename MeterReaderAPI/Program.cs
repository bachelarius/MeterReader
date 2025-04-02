using MeterReaderAPI.Accounts;
using MeterReaderAPI.Data;
using MeterReaderAPI.MeterReadings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/meter-reading-uploads", MeterReadingUploadsHandler.Handle)
    .WithName("UploadMeterReadings")
    .WithOpenApi();

app.Run();
