using Microsoft.AspNetCore.Authorization;
using OPAStyraWebAPI.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                        policy.SetIsOriginAllowedToAllowWildcardSubdomains()
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin()
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

builder.Services.AddAuthorization(o => o.AddPolicy("Customers", b => b.RequireRole("customer")
                                 .AddRequirements(new PermissionRequirement("portfolio", "read"))));

builder.Services.AddSwaggerGen(t =>
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
builder.Services.AddHttpClient("Opa", httpClient => httpClient.BaseAddress = new Uri("http://host.docker.internal:8181/")); //http://localhost:8181/

var app = builder.Build();

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