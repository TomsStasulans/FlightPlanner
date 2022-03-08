using FlightPlanner.Core.Services;
using FlightPlanner.Data;
using FlightPlanner.Handlers;
using FlightPlanner.Models;
using FlightPlanner.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var configuration = builder.Configuration;

builder.Services.AddDbContext<FlightPlannerDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("flight-planner"));
});

builder.Services.AddRazorPages();
builder.Services.AddTransient<IFlightPlannerDbContext, FlightPlannerDbContext>();
builder.Services.AddTransient<IDbService, DbService>();
builder.Services.AddTransient<IEntityService<Flight>, EntityService<Flight>>();
builder.Services.AddTransient<IEntityService<Airport>, EntityService<Airport>>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();
