using QuotesWar.Api.Configurations;
using QuotesWar.Api.Features.Battles;
using QuotesWar.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBattleModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.UseBattleModuleAsync();
app.MapBattleModule();
app.MapHealthChecks();

app.Run();