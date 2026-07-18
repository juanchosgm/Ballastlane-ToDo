using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using ToDo.Api.Common;
using ToDo.Api.Endpoints;
using ToDo.Application;
using ToDo.Application.Common.Interfaces;
using ToDo.Infrastructure;
using ToDo.Infrastructure.Identity;
using ToDo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AngularClient";

// ---- Service registration (composition of the clean-architecture layers) ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// ---- Authentication / Authorization (ASP.NET Core Identity, token-based API endpoints) ----
builder.Services
    .AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<TodoDbContext>();

builder.Services.AddAuthorization();

// Lets the Application layer resolve the caller's id without depending on HttpContext.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Serialise enums (e.g. TodoStatus) as their names ("Pending") rather than numbers,
// so the JSON contract is self-describing and the Angular client can use string unions.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // "Authorize" button: paste the accessToken returned by /api/auth/login.
    const string schemeId = "Bearer";

    options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the accessToken from /api/auth/login (no 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeId, document, null)] = new List<string>()
    });
});

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

app.UseAuthentication();
app.UseAuthorization();

// Identity's built-in endpoints: /api/auth/register, /login, /refresh, ...
app.MapGroup("/api/auth")
    .WithTags("Auth")
    .MapIdentityApi<AppUser>();

app.MapTodoEndpoints();
app.MapGet("/", () => Results.Redirect("/swagger"));

// ---- Seed the demo user on startup (tasks start empty, one per user) ----
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    await IdentitySeeder.SeedAsync(userManager, app.Configuration);
}

app.Run();

// Exposed so the integration test project can bootstrap the app via WebApplicationFactory.
public partial class Program { }
