using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Constants;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Expenses;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.Admin;

public class AdminService(UserManager<ApplicationUser> userManager)
{
    public async Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync()
    {
        var users = await userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var responses = new List<AdminUserResponse>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            responses.Add(new AdminUserResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                roles.ToList(),
                user.CreatedAt,
                user.ApprovalStatus));
        }

        return responses;
    }

    public async Task<(bool Success, string? Error)> UpdateRolesAsync(string userId, UpdateUserRolesRequest request)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "User not found.");
        }

        var invalidRoles = request.Roles.Except(Roles.All).ToList();
        if (invalidRoles.Count > 0)
        {
            return (false, $"Invalid roles: {string.Join(", ", invalidRoles)}");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, request.Roles);

        return (true, null);
    }
}