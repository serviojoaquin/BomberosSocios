using BaseApp.Infrastructure.Data;
using System.Reflection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    await app.InitialiseDatabaseAsync();
}

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(static builder => 
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });


app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

app.MapFallbackToFile("index.html");

app.Run();
