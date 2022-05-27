using System.Security.Claims;

namespace OPAStyraWebAPI.Permissions
{
    public interface IPermissionManager
    {
        Task<bool> AssertPermissionRequirementAsync (PermissionRequirement permissionRequirement, 
            ClaimsPrincipal claimsPrincipal);
    }
}
