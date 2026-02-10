using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Share.Enums;

namespace Api.Config.Authorization;

public sealed class AuthPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("PERM::"))
            return await base.GetPolicyAsync(policyName);

        var data = policyName["PERM::".Length..];
        var parts = data.Split('|');

        var roles = parts[0].Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => Enum.Parse<EnumRole>(r, true));

        var perms = parts.Length > 1
            ? parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => Enum.Parse<EnumPermission>(p, true))
            : [];

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new AuthRequirement(roles, perms))
            .Build();
    }
}