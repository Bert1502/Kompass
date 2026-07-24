using Kompass.Persistence.B56Import;
using Kompass.Persistence;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddB56Import(
    builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddPersistence(
    builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KompassDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapControllers();

app.UseSwagger();

app.UseSwaggerUI();

app.MapGet(
    "/api/status",
    () => Results.Ok(new
    {
        Anwendung = "KOMPASS API",
        Status = "Bereit",
        ZeitpunktUtc = DateTimeOffset.UtcNow
    }));

app.Run();
