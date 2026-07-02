namespace Its.PleaseProtect.Api.Database.Seeders;

using Serilog;
using System.Diagnostics.CodeAnalysis;
using PasswordGenerator;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Its.PleaseProtect.Api.Utils;

[ExcludeFromCodeCoverage]
public class DataSeeder
{
    private readonly DataContext context;
    private readonly Password pwd = new Password(32);
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOrganizationUserRepository _orgUserRepo;
    private readonly IUserRepository _userRepo;

    public DataSeeder(
        DataContext ctx,
        IUserRepository userRepo,
        IOrganizationUserRepository orgUserRepo,
        UserManager<IdentityUser> userManager)
    {
        context = ctx;
        _userRepo = userRepo;
        _orgUserRepo = orgUserRepo;
        _userManager = userManager;
    }

    private void SeedDefaultOrganization()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        if (!context.Organizations.Any())
        {
            var orgs = new List<MOrganization>()
            {
                new MOrganization
                {
                    OrgId = Guid.NewGuid(),
                    OrgName = "DEFAULT",
                    OrgDescription = "Default initial created organization",
                    OrgCreatedDate = DateTime.UtcNow,
                    OrgCustomId = "default"
                }
            };

            context.Organizations.AddRange(orgs);
            context.SaveChanges();
        }
    }

    private void SeedGlobalOrganization()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        string orgId = "global";

        var query = context.Organizations!.Where(x => x.OrgCustomId!.Equals(orgId)).FirstOrDefault();
        if (query == null)
        {
            //Create if not exist
            var orgs = new List<MOrganization>()
            {
                new MOrganization
                {
                    OrgId = Guid.NewGuid(),
                    OrgName = "GLOBAL",
                    OrgDescription = "Global/Root initial created organization",
                    OrgCreatedDate = DateTime.UtcNow,
                    OrgCustomId = orgId
                }
            };
            context.Organizations.AddRange(orgs);

            var apiKey = new MApiKey()
            {
                KeyId = Guid.NewGuid(),
                KeyCreatedDate = DateTime.UtcNow,
                OrgId = orgId,
                ApiKey = pwd.Next(),
                KeyDescription = "Auto created root key"
            };
            context.ApiKeys!.Add(apiKey);

            context.SaveChanges();
        }
    }

    private void UpdateDefaultOrganizationCustomId()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        var query = context.Organizations!.Where(x => x.OrgName!.Equals("DEFAULT")).FirstOrDefault();
        if (query == null)
        {
            Log.Error("Default organization 'DEFAULT' not found!!!");
            return;
        }
        query.OrgCustomId = "default";
        context.SaveChanges();
    }

    private void AddRole(string name, string definition, string level, string desc)
    {
        var query = context.Roles!.Where(x => x.RoleName!.Equals(name)).FirstOrDefault();
        if (query != null)
        {
            //Already exist
            return;
        }

        var r = new MRole() 
        {
            RoleName = name,
            RoleDefinition = definition,
            RoleLevel = level,
            RoleDescription = desc
        };

        context!.Roles!.Add(r);
    }

    private void SeedDefaultRoles()
    {
        AddRole("OWNER", ".+:.+", "ORGANIZATION", "Organization Owner");
        AddRole("VIEWER", ".+:Get.+", "ORGANIZATION", "Organization Viewer");

        context.SaveChanges();
    }

    private void UpdateApiKeyRole()
    {
        var apiKeys = context.ApiKeys!.Where(x => x.RolesList!.Equals(null) || x.RolesList!.Equals("")).ToList();
        apiKeys.ForEach(a => a.RolesList = "OWNER");
        context.SaveChanges();
    }

    private void SeedInitialAdminUser()
    {
        var username = Environment.GetEnvironmentVariable("INITIAL_USER");

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("INITIAL_USER not set. Skipping initial admin seeding.");
            return;
        }

        var query = context.Users!.Where(x => x.UserName!.Equals(username)).FirstOrDefault();
        if (query != null)
        {
            //Already exist
            return;
        }

        var u = new MUser()
        {
            UserId = Guid.NewGuid(),
            UserName = username,
            Name = "Admin",
            LastName = "Admin",
            IsOrgInitialUser = "YES",
            UserCreatedDate = DateTime.UtcNow,
            UserEmail = "admin@localhost"
        };

        context.Users!.Add(u);

        var ou = new MOrganizationUser()
        {
            OrgUserId = Guid.NewGuid(),
            OrgCustomId = "default",
            UserId = u.UserId.ToString(),
            UserName = username,
            IsOrgInitialUser = "YES",
            CreatedDate = DateTime.UtcNow,
            UserEmail = u.UserEmail,
            RolesList = "OWNER",
            UserStatus = "Active",
            Tags = "initial,admin"
        };

        context.OrganizationUsers!.Add(ou);


        var idpUser = new IdentityUser
        {
            UserName = u.UserName,
            Email = u.UserEmail,
        };

        var initialPassword = ServiceUtils.GeneratePassword();
        var t2 = _userManager.CreateAsync(idpUser, initialPassword);
        var result = t2.Result;

        Console.WriteLine($"##### MigrateUsers : Added [{u.UserName}] [{initialPassword}] [{result.Succeeded}]");

        context.SaveChanges();
    }

    public void Seed()
    {
        SeedDefaultOrganization();
        UpdateDefaultOrganizationCustomId();

        SeedGlobalOrganization();
        SeedDefaultRoles();
        UpdateApiKeyRole();

        SeedInitialAdminUser();
    }

    public void MigrateUsers()
    {
        var users = context.Users!.ToList();
        foreach (var u in users)
        {
            //ใช้สำหรับ migrate user จาก KeycloakIDP มายัง NativeIDP
            Console.WriteLine($"MigrateUsers : Checking for user [{u.UserName}]...");
            var t1 = _userManager.FindByNameAsync(u.UserName!);

            var user = t1.Result;
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = u.UserName,
                    Email = u.UserEmail,
                };

                var initialPassword = ServiceUtils.GeneratePassword();
                var t2 = _userManager.CreateAsync(user, initialPassword);
                var result = t2.Result;

                Console.WriteLine($"MigrateUsers : Added [{u.UserName}] [{initialPassword}] [{result.Succeeded}] [{result.Errors.FirstOrDefault()?.Description}]");
            }
        }
    }
}
