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

        public async Task<bool> AssertPermissionRequirementAsync (PermissionRequirement permissionRequirement,
            ClaimsPrincipal claimsPrincipal)
        {
            var rbacPermissionRequest = new RbacPermissionRequest
            {
                Role = claimsPrincipal.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value,
                User = "pepe", // context.User.Identity?.Name
                Action = permissionRequirement.Action,
                Resource = permissionRequirement.Resource
            };

            var rbacRequest = new RbacRequest
            {
                Input = rbacPermissionRequest
            };

            var payload = JsonSerializer.Serialize(rbacRequest).ToLower();
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            //var httpClient = _httpClientFactory.CreateClient("Opa");

            //var httpResponseMessage = await httpClient.PostAsync("v1/data/rules/allow", httpContent);

            //if (httpResponseMessage.IsSuccessStatusCode)
            //{
            //    var contentString = await httpResponseMessage.Content.ReadAsStringAsync();

            //    var options = new JsonSerializerOptions
            //    {
            //        PropertyNameCaseInsensitive = true
            //    };

            //    var resp = JsonSerializer.Deserialize<RbacResponse>(contentString, options);

            //    return resp.Result;
            //}

            //return false;

            return true;
        }
    }
}