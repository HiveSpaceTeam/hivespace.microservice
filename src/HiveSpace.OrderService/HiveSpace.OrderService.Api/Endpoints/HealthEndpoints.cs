namespace HiveSpace.OrderService.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Service = "HiveSpace.OrderService",
            Timestamp = DateTime.UtcNow
        }))
        .WithName("GetHealth")
        .WithTags("Health")
        .AllowAnonymous();

        return app;
    }
}
