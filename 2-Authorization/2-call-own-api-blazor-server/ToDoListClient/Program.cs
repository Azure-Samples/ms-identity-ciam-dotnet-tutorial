using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(
        [
            builder.Configuration.GetSection("DownstreamApi:Scopes:Read").Get<string>()!,
            builder.Configuration.GetSection("DownstreamApi:Scopes:Write").Get<string>()!
        ]
    )
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
        .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();
