using Microsoft.AspNetCore.Authorization;

namespace OPAStyraWebAPI.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string resource, string action)
        {
            Resource = resource;
            Action = action;
        }

        public string Resource { get; }
        public string Action { get; }
    }
}