using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdventureWorksAPIs.Identity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresClaimAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _claimName;
        private readonly string _claimValue;

        public RequiresClaimAttribute(string claimName, string claimValue)
        {
            _claimName = claimName ?? throw new ArgumentNullException(nameof(claimName));
            _claimValue = claimValue ?? throw new ArgumentNullException(nameof(claimValue));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.HasClaim(_claimName, _claimValue))
            {
                context.Result = new ForbidResult();
            }

            await Task.CompletedTask;
        }
    }
}
