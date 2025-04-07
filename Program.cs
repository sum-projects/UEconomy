using Microsoft.AspNetCore.SignalR;
using UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameService>(sp => new GameService(sp.GetRequiredService<IHubContext<GameHub>>()));

var app = builder.Build();
var gameService = app.Services.GetRequiredService<GameService>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<GameHub>("/gameHub");

app.Run();