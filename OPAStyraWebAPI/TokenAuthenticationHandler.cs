using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

internal class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
{
    public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "customer") };
        var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}