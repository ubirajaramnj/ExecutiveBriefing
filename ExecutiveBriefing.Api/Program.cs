using ExecutiveBriefing.Api.Middleware;

using ExecutiveBriefing.ApplicationServices.Interfaces;
using ExecutiveBriefing.ApplicationServices.Services;
using ExecutiveBriefing.Domain.Repositories;
using ExecutiveBriefing.Infrastructure.AI;
using ExecutiveBriefing.Infrastructure.Parsers;
using ExecutiveBriefing.Infrastructure.Repositories;
using ExecutiveBriefing.Infrastructure.Scrapers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IBriefingRepository, InMemoryBriefingRepository>();
builder.Services.AddTransient<IWebScraper, WebScraper>();
builder.Services.AddTransient<IPdfParser, PdfParser>();
builder.Services.AddTransient<IAIService, GeminiAIService>();
builder.Services.AddTransient<BriefingService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
