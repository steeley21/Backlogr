using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class AdminControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public AdminControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUsers_ForAdmin()
    {
        using var client = _factory.CreateClient();

        var adminUserId = Guid.Parse("81111111-1111-1111-1111-111111111111");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", RoleNames.Admin);

        await SeedUserAsync(adminUserId, "admin_get", "admin_get@example.com", "Admin Get", RoleNames.User, RoleNames.Admin);
        await SeedUserAsync(Guid.Parse("82222222-2222-2222-2222-222222222222"), "listed_user", "listed_user@example.com", "Listed User", RoleNames.User);

        var response = await client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<AdminUserSummaryDto>>();
        users.Should().NotBeNull();
        users!.Should().Contain(u => u.UserName == "admin_get");
        users.Should().Contain(u => u.UserName == "listed_user");
    }

    [Fact]
    public async Task CreateUser_ShouldAllowAdminToCreateUser()
    {
        using var client = _factory.CreateClient();

        var adminUserId = Guid.Parse("83333333-3333-3333-3333-333333333333");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", RoleNames.Admin);

        await SeedUserAsync(adminUserId, "admin_create_user", "admin_create_user@example.com", "Admin Create User", RoleNames.User, RoleNames.Admin);

        var response = await client.PostAsJsonAsync("/api/admin/users", new AdminCreateUserRequestDto
        {
            UserName = "created_user",
            DisplayName = "Created User",
            Email = "created_user@example.com",
            Password = "Password1",
            Role = AdminAssignableRole.User
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<AdminUserSummaryDto>();
        created.Should().NotBeNull();
        created!.UserName.Should().Be("created_user");
        created.Roles.Should().BeEquivalentTo([RoleNames.User]);
    }

    [Fact]
    public async Task CreateUser_ShouldForbidAdminFromCreatingAdmin()
    {
        using var client = _factory.CreateClient();

        var adminUserId = Guid.Parse("84444444-4444-4444-4444-444444444444");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", RoleNames.Admin);

        await SeedUserAsync(adminUserId, "admin_no_admin", "admin_no_admin@example.com", "Admin No Admin", RoleNames.User, RoleNames.Admin);

        var response = await client.PostAsJsonAsync("/api/admin/users", new AdminCreateUserRequestDto
        {
            UserName = "should_fail_admin",
            DisplayName = "Should Fail Admin",
            Email = "should_fail_admin@example.com",
            Password = "Password1",
            Role = AdminAssignableRole.Admin
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_ShouldAllowSuperAdminToCreateAdmin()
    {
        using var client = _factory.CreateClient();

        var superAdminUserId = Guid.Parse("85555555-5555-5555-5555-555555555555");
        client.DefaultRequestHeaders.Add("X-Test-UserId", superAdminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", $"{RoleNames.Admin},{RoleNames.SuperAdmin}");

        await SeedUserAsync(superAdminUserId, "super_create_admin", "super_create_admin@example.com", "Super Create Admin", RoleNames.User, RoleNames.Admin, RoleNames.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/admin/users", new AdminCreateUserRequestDto
        {
            UserName = "created_admin",
            DisplayName = "Created Admin",
            Email = "created_admin@example.com",
            Password = "Password1",
            Role = AdminAssignableRole.Admin
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<AdminUserSummaryDto>();
        created.Should().NotBeNull();
        created!.Roles.Should().Contain(RoleNames.User);
        created.Roles.Should().Contain(RoleNames.Admin);
        created.Roles.Should().NotContain(RoleNames.SuperAdmin);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldForbidAdmin_WhenChangingRoles()
    {
        using var client = _factory.CreateClient();

        var adminUserId = Guid.Parse("86666666-6666-6666-6666-666666666666");
        var targetUserId = Guid.Parse("87777777-7777-7777-7777-777777777777");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", RoleNames.Admin);

        await SeedUserAsync(adminUserId, "admin_role_edit", "admin_role_edit@example.com", "Admin Role Edit", RoleNames.User, RoleNames.Admin);
        await SeedUserAsync(targetUserId, "target_user", "target_user@example.com", "Target User", RoleNames.User);

        var response = await client.PutAsJsonAsync($"/api/admin/users/{targetUserId}/role", new AdminUpdateUserRoleRequestDto
        {
            Role = AdminAssignableRole.Admin
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldAllowSuperAdminToPromoteAndDemoteUser()
    {
        using var client = _factory.CreateClient();

        var superAdminUserId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var targetUserId = Guid.Parse("89999999-9999-9999-9999-999999999999");
        client.DefaultRequestHeaders.Add("X-Test-UserId", superAdminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", $"{RoleNames.Admin},{RoleNames.SuperAdmin}");

        await SeedUserAsync(superAdminUserId, "super_role_edit", "super_role_edit@example.com", "Super Role Edit", RoleNames.User, RoleNames.Admin, RoleNames.SuperAdmin);
        await SeedUserAsync(targetUserId, "promoted_user", "promoted_user@example.com", "Promoted User", RoleNames.User);

        var promoteResponse = await client.PutAsJsonAsync($"/api/admin/users/{targetUserId}/role", new AdminUpdateUserRoleRequestDto
        {
            Role = AdminAssignableRole.Admin
        });

        promoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var promoted = await promoteResponse.Content.ReadFromJsonAsync<AdminUserSummaryDto>();
        promoted.Should().NotBeNull();
        promoted!.Roles.Should().Contain(RoleNames.Admin);
        promoted.Roles.Should().Contain(RoleNames.User);

        var demoteResponse = await client.PutAsJsonAsync($"/api/admin/users/{targetUserId}/role", new AdminUpdateUserRoleRequestDto
        {
            Role = AdminAssignableRole.User
        });

        demoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var demoted = await demoteResponse.Content.ReadFromJsonAsync<AdminUserSummaryDto>();
        demoted.Should().NotBeNull();
        demoted!.Roles.Should().BeEquivalentTo([RoleNames.User]);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnConflict_WhenSuperAdminTargetsSelf()
    {
        using var client = _factory.CreateClient();

        var superAdminUserId = Guid.Parse("8aaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        client.DefaultRequestHeaders.Add("X-Test-UserId", superAdminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", $"{RoleNames.Admin},{RoleNames.SuperAdmin}");

        await SeedUserAsync(superAdminUserId, "super_self_edit", "super_self_edit@example.com", "Super Self Edit", RoleNames.User, RoleNames.Admin, RoleNames.SuperAdmin);

        var response = await client.PutAsJsonAsync($"/api/admin/users/{superAdminUserId}/role", new AdminUpdateUserRoleRequestDto
        {
            Role = AdminAssignableRole.User
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task SeedUserAsync(Guid userId, string userName, string email, string displayName, params string[] roles)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in roles.Distinct())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName
                });

                createRoleResult.Succeeded.Should().BeTrue();
            }
        }

        var existingUser = await userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null)
        {
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = userName,
                Email = email,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await userManager.CreateAsync(user, "Password1");
            createUserResult.Succeeded.Should().BeTrue();
            existingUser = user;
        }

        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(existingUser, role);
                addToRoleResult.Succeeded.Should().BeTrue();
            }
        }
    }
}
