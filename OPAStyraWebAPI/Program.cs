using Microsoft.AspNetCore.Authorization;
using OPAStyraWebAPI.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OPAStyraWebAPI.Middleware;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                        policy.SetIsOriginAllowedToAllowWildcardSubdomains()
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                        ));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApi(opt =>
             {
                 opt.TokenValidationParameters = new()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidIssuer = "https://login.microsoftonline.com/93c16d38-d1d7-4702-ab62-e9d16603afe5/",
                     ValidAudience = "api://0ca77bae-04a1-42a1-a1e1-1d28d27d66e0"
                 };
             }, opt => {
                 opt.ClientId = "0ca77bae-04a1-42a1-a1e1-1d28d27d66e0";
                 opt.TenantId = "93c16d38-d1d7-4702-ab62-e9d16603afe5";
                 opt.Instance = "https://login.microsoftonline.com";                 
             }
             );

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "TokenAuthenticationScheme";
}).AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>("TokenAuthenticationScheme", null);


builder.Services.AddAuthorization(o => o.AddPolicy("Customers", b => b.RequireRole("customer")
                                 .AddRequirements(new PermissionRequirement("portfolio", "read"))));
builder.Services.AddSwaggerGen(t =>
{
    t.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Name = "authorization",
        Type = SecuritySchemeType.OAuth2,
        BearerFormat = "JWT",
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/93c16d38-d1d7-4702-ab62-e9d16603afe5/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/93c16d38-d1d7-4702-ab62-e9d16603afe5/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string> {
                    { "api://0ca77bae-04a1-42a1-a1e1-1d28d27d66e0/Portfolios.Read", "desc" }
                }
            }
        }
    });
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IPermissionManager, PermissionManager>();
builder.Services.AddHttpClient("Opa", httpClient => httpClient.BaseAddress = new Uri("http://localhost:8181/"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1");
    c.OAuthConfigObject = new OAuthConfigObject
    {
        AppName = "OPA Styra API",
        ClientId = "0ca77bae-04a1-42a1-a1e1-1d28d27d66e0",
        AdditionalQueryStringParams = new Dictionary<string, string>()
    };
});

app.UseCors(policy => policy.AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(origin => true));

app.UseHttpsRedirection();

app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/portfolios", [Authorize(Policy = "Customers")] () =>
{
    return new[] { "Abc, cde, fgt" };
})
.WithName("GetPortfolios");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}