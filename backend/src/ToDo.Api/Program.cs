using ToDo.Api.Common;
using ToDo.Api.Endpoints;
using ToDo.Application;
using ToDo.Infrastructure;
using ToDo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AngularClient";

// ---- Service registration (composition of the clean-architecture layers) ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// ---- Middleware pipeline ----
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1"));
}

app.UseCors(AngularCorsPolicy);

app.MapTodoEndpoints();
app.MapGet("/", () => Results.Redirect("/swagger"));

// ---- Seed the in-memory database on startup ----
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await TodoDbSeeder.SeedAsync(context);
}

app.Run();

// Exposed so the integration test project can bootstrap the app via WebApplicationFactory.
public partial class Program { }
