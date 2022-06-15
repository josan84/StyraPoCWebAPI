using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace OPAStyraWebAPI.Permissions
{
    public class PermissionManager : IPermissionManager
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PermissionManager(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> AssertPermissionRequirementAsync(PermissionRequirement permissionRequirement, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity != null ? claimsPrincipal.Identity.Name : string.Empty;

            var roleClaimUri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var userRole = claimsPrincipal.FindFirstValue(roleClaimUri).ToLower() ?? string.Empty;

            var scopeClaimUri = "http://schemas.microsoft.com/identity/claims/scope";
            var userScope = claimsPrincipal.FindFirstValue(scopeClaimUri).ToLower() ?? string.Empty;

            // I might not need PermissionRequirement if I can get the scopes from the token; or 
            // I could just check the permissions from the scope against PermissionRequirement and not needing OPA

            var rbacPermissionRequest = new RbacPermissionRequest
            {
                Role = userRole.ToLower(),
                User = userName != null ? userName.ToLower() : string.Empty,
                Action = permissionRequirement.Action.ToLower(),
                Resource = permissionRequirement.Resource.ToLower()
            };

            var rbacRequest = new RbacRequest
            {
                Input = rbacPermissionRequest
            };

            var payload = JsonSerializer.Serialize(rbacRequest).ToLower();
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient("Opa");

            var httpResponseMessage = await httpClient.PostAsync("v1/data/rules/allow", httpContent);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentString = await httpResponseMessage.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                RbacResponse? resp = JsonSerializer.Deserialize<RbacResponse>(contentString, options);
                return resp == null ? false : resp.Result;
            }

            return false;
        }
    }
}