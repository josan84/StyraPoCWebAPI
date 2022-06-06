using Microsoft.AspNetCore.Authorization;

namespace OPAStyraWebAPI.Permissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionManager _permissionManager;

        public PermissionAuthorizationHandler(IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            PermissionRequirement requirement)
        {
            // commenting OPA request
            await _permissionManager.AssertPermissionRequirementAsync(requirement, context.User);

            context.Succeed(requirement);
            return;
        }
    }
}
