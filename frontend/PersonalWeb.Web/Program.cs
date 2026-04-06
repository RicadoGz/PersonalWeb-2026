using PersonalWeb.Web.Components;

var builder = WebApplication.CreateBuilder(args);

//start the server 
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
//this make dynamic http client to call backend api
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5264");
});
//create a backend and frontend shared service to call backend api
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//redirect http to https
app.UseHttpsRedirection();
//serve static files
app.UseStaticFiles();
//add antiforgery middleware to protect against CSRF attacks
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
