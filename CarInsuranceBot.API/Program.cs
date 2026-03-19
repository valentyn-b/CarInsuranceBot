using CarInsuranceBot.API.Extensions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// 1. Framework Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. Application & Infrastructure Services
builder.Services.AddApplicationServices();
builder.Services.AddTelegramBot(builder.Configuration);
builder.Services.AddExternalIntegrations(builder.Configuration);

var app = builder.Build();

// 3. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
