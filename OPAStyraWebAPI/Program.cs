using Microsoft.AspNetCore.Authorization;
using OPAStyraWebAPI.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                        policy.SetIsOriginAllowedToAllowWildcardSubdomains()
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin()
                        ));
services.AddEndpointsApiExplorer();
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

services.AddAuthorization(o => o.AddPolicy("Customers", b => b.RequireRole("customer")
                                 .AddRequirements(new PermissionRequirement("portfolio", "read"))));

services.AddSwaggerGen(t =>
{
    t.OperationFilter<SecurityRequirementsOperationFilter>();

    t.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Name = "authorization",
        Type = SecuritySchemeType.OAuth2,
        BearerFormat = "JWT",
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(configuration["AzureAd:AuthorizationUrl"]),
                TokenUrl = new Uri(configuration["AzureAd:TokenUrl"]),
                Scopes = new Dictionary<string, string> {
                    { configuration["AzureAd:PortfoliosReadScope"], "Allows to read portfolios." }
                }
            }
        }
    });
});

services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IPermissionManager, PermissionManager>();
services.AddHttpClient("Opa", httpClient => httpClient.BaseAddress = new Uri(configuration["OpaUrl"])); //http://localhost:8181/

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1");
    c.OAuthConfigObject = new OAuthConfigObject
    {
        AppName = "OPA Styra API",
        ClientId = configuration["AzureAd:ClientId"],
        AdditionalQueryStringParams = new Dictionary<string, string>()
    };
    c.OAuthUsePkce();
});

app.UseCors(policy => policy.AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(origin => true));
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/freeportfolios", () =>
{
    // Authorization free portfolios
    return new[] { "abcde, bcedf, xwxyir" };
})
.WithName("GetFreePortfolios");

app.MapGet("/portfolios", [Authorize(Policy = "Customers")] () =>
{
    return new[] { "ABCDE, BCEDF, XWXYIR" };
})
.WithName("GetPortfolios");

app.Run();