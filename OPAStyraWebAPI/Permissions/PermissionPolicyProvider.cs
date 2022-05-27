using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace OPAStyraWebAPI.Permissions
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _defaultPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _defaultPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _defaultPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            //var policy = new AuthorizationPolicyBuilder();
            //policy.AddRequirements(new PermissionRequirement(policyName));
            //return Task.FromResult(policy.Build());

            throw new NotImplementedException();
        }
    }
}
