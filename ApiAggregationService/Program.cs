using AggregationService;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var serviceCollection = builder.Services;

// Add services to the container
serviceCollection = ServiceCollectionExtensions.ConfigureServices(serviceCollection);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiAggregationService"));

app.UseExceptionHandler("/error");

app.Map("/error", (HttpContext httpContext) =>
{
    return Results.Problem("An error occurred");
});

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.MapControllers(); // Map attribute-routed controllers

app.Run(); // Start the application
