var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Hello from ASP.NET Core"
    });
});

app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    if (request.Username == "demo-admin" && request.Password == "123456")
    {
        return Results.Ok(new
        {
            authenticated = true,
            username = request.Username
        });
    }

    return Results.BadRequest(new
    {
        authenticated = false,
        error = "Invalid username or password"
    });
});

app.MapGet("/api/auth/me", () =>
{
    return Results.Ok(new
    {
        authenticated = false
    });
});

app.Run();

public record LoginRequest(string Username, string Password);
