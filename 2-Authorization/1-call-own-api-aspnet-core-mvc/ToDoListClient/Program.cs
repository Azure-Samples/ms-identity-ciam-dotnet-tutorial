using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ToDoListClient.Options;
using ToDoListClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

var downstreamApiOptions = builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi)
    .Get<DownstreamApiOptions>()!;

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
        .EnableTokenAcquisitionToCallDownstreamApi(downstreamApiOptions.Scopes!.Split(' '))
         .AddInMemoryTokenCaches();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IToDoListService, ToDoListService>(client =>
{
    client.BaseAddress = new Uri(downstreamApiOptions.BaseUrl);
});

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
