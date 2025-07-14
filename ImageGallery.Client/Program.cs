using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear(); // Clear the default claim type map to prevent duplicate claims

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default authentication scheme for the application
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; // Default challenge scheme for the application
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{ 
    options.AccessDeniedPath = "/Authentication/AccessDenied"; // Path to redirect to when access is denied
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{ 
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Sign-in scheme for the OpenID Connect authentication
    options.Authority = "https://localhost:5001/"; // Authority of the Identity Provider (IDP)
    options.ClientId = "imagegalleryclient"; // Client ID for the client
    options.ClientSecret = "secret"; // Client secret for the client
    options.ResponseType = "code"; // Authorization Code flow
    // By default openid and profile scopes are requested, but you can add more scopes if needed
    //options.Scope.Add("openid"); // OpenID Connect scope
    //options.Scope.Add("profile"); // Profile scope
    //options.CallbackPath= new PathString("/signin-oidc"); // Callback path for the OpenID Connect authentication 
    //options.SignedOutCallbackPath = new PathString("/signout-callback-oidc"); // Callback path for the sign-out process
    options.SaveTokens = true; // Save the tokens received from the IDP
    options.GetClaimsFromUserInfoEndpoint = true; // Retrieve claims from the UserInfo endpoint
    options.ClaimActions.Remove("aud"); // Remove the aud claim to avoid duplicate claims
    options.ClaimActions.DeleteClaim("sid"); // Remove the sid claim to avoid duplicate claims
    options.ClaimActions.DeleteClaim("idp"); // Remove the idp claim to avoid duplicate claims
    options.Scope.Add("roles"); // Add the roles scope to the OpenID Connect request
    options.ClaimActions.MapJsonKey("role", "role"); // Map the role claim from the IDP to the role claim in the application
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(); // 
    app.UseHsts(); // Use HTTP Strict Transport Security (HSTS) to enforce secure connections/
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files from the wwwroot folder

app.UseRouting(); // Enable routing middleware

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
