using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text.Json;

namespace Its.PleaseProtect.Api.Services
{
    public class CustomRoleService : BaseService, ICustomRoleService
    {
        private readonly ICustomRoleRepository? repository = null;
        private readonly IApiDescriptionGroupCollectionProvider _provider;
        private readonly IRedisHelper _redis;

        public CustomRoleService(ICustomRoleRepository repo,
            IRedisHelper redis,
            IApiDescriptionGroupCollectionProvider provider) : base()
        {
            repository = repo;
            _provider = provider;
            _redis = redis;
        }

        public async Task<MVCustomRole> GetCustomRoleById(string orgId, string customRoleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            var result = await repository!.GetCustomRoleById(customRoleId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            var roleDef = result.RoleDefinition;
            if (string.IsNullOrEmpty(roleDef))
            {
                roleDef = "[]";
            }

            var perms = JsonSerializer.Deserialize<List<ControllerNode>>(roleDef);
            var flatPermissions = new Dictionary<string, bool>();

            if (perms != null)
            {
                flatPermissions = GetFlatenPermission(perms);
            }

            var permissions = GetInitialPermission("api", flatPermissions);

            r.CustomRole = result;
            r.CustomRole.Permissions = permissions;
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        private Dictionary<string, bool> GetFlatenPermission(List<ControllerNode> controlers)
        {
            var flattenMap = new Dictionary<string, bool>();

            foreach (var ctrl in controlers)
            {
                foreach (var permission in ctrl.ApiPermissions)
                {
                    var key = $"{permission.ControllerName}:{permission.ApiName}";

                    //ตรง key สามารถซ้ำได้ เพราะ API name ชื่อเดียวอาจจะมีหลาย signature
                    if (!flattenMap.ContainsKey(key))
                    { 
                        flattenMap.Add(key, permission.IsAllowed);
                    }
                }
            }

            return flattenMap;
        }

        private void CachRolePermissions(string orgId, MCustomRole cr, bool isDelete)
        {
            var cacheKeyPrefix = CacheHelper.CreateCustomRoleCacheLoaderKey(orgId);
            var permissions = cr.Permissions;

            foreach (var perm in permissions)
            {
                var apis = perm.ApiPermissions;
                foreach (var api in apis)
                {
                    var cacheKey = $"{cacheKeyPrefix}:{cr.RoleId}:{perm.ControllerName}:{api.ApiName}";
                    var value = api.IsAllowed;
                    if (isDelete)
                    {
                        _ = _redis.DeleteAsync(cacheKey);
                        Console.WriteLine($"Del cache ==> [{cacheKey}]");
                    }
                    else 
                    {
                        _ = _redis.SetObjectAsync(cacheKey, value); //No expiration
                        Console.WriteLine($"Set cache ==> [{cacheKey}] with value [{value}] ");
                    }
                }
            }
        }

        public async Task<MVCustomRole> AddCustomRole(string orgId, MCustomRole customRole)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(customRole.RoleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Custom role name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsRoleNameExist(customRole.RoleName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Custom role name [{customRole.RoleName}] already exist!!!";

                return r;
            }

            customRole.Permissions ??= []; //Empty array
            customRole.RoleDefinition = JsonSerializer.Serialize(customRole.Permissions)!;

            var result = await repository!.AddCustomRole(customRole);
            r.CustomRole = result;

            CachRolePermissions(orgId, result, false);

            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        public async Task<MVCustomRole> DeleteCustomRoleById(string orgId, string customRoleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            var currentCr = await GetCustomRoleById(orgId, customRoleId);
            if (currentCr.CustomRole == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteCustomRoleById(customRoleId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            CachRolePermissions(orgId, currentCr.CustomRole, true);

            r.CustomRole = m;
            return r;
        }

        public async Task<List<MCustomRole>> GetCustomRoles(string orgId, VMCustomRole param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetCustomRoles(param);

            return result;
        }

        public async Task<int> GetCustomRoleCount(string orgId, VMCustomRole param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetCustomRoleCount(param);

            return result;
        }

        public async Task<MVCustomRole> UpdateCustomRoleById(string orgId, string customRoleId, MCustomRole customRole)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            var roleName = customRole.RoleName;
            var cr = await repository!.GetCustomRoleByName(roleName!);
            if ((cr != null) && (cr.RoleId.ToString() != customRoleId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Custom role name [{roleName}] already exist!!!";

                return r;
            }

            customRole.Permissions ??= []; //Empty array
            customRole.RoleDefinition = JsonSerializer.Serialize(customRole.Permissions)!;

            var result = await repository!.UpdateCustomRoleById(customRoleId, customRole);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            var currentCr = await GetCustomRoleById(orgId, customRoleId);
            if (currentCr.CustomRole == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            CachRolePermissions(orgId, currentCr.CustomRole, false);

            r.CustomRole = result;
            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        private List<ControllerNode> GetInitialPermission(string filterApiGroup, Dictionary<string, bool> flatPermissions)
        {
            var controllers = new List<ControllerNode>();
            var controlerMap = new Dictionary<string, ControllerNode>();
            var checkDupDic = new Dictionary<string, bool>();

            foreach (var group in _provider.ApiDescriptionGroups.Items)
            {
                foreach (var api in group.Items)
                {
                    var controller = api.ActionDescriptor.RouteValues["controller"]!;
                    var action = api.ActionDescriptor.RouteValues["action"];
                    var route = api.RelativePath!;
                    string apiGroup = route.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];
//Console.WriteLine($"DEBUG_A [{apiGroup}] [{filterApiGroup}] [{controller}] [{action}]");
                    if (apiGroup != filterApiGroup)
                    {
                        continue;
                    }

                    ControllerNode ctrlNode;
                    if (controlerMap.ContainsKey(controller))
                    {
                        ctrlNode = controlerMap[controller];
                    }
                    else
                    {
                        ctrlNode = new ControllerNode();
                        ctrlNode.ControllerName = controller;

                        controlerMap.Add(controller, ctrlNode);
                        controllers.Add(ctrlNode);
                    }

                    var key = $"{controller}:{action}";
                    var isSelected = false;
                    if (flatPermissions.TryGetValue(key, out bool value))
                    {
                        isSelected = value;
                    }

                    var apiNode = new ApiNode()
                    {
                        ApiName = action!,
                        ControllerName = controller,
                        IsAllowed  = isSelected,
                    };

                    var checkDupKey = $"{controller}:{action}";
                    if (!checkDupDic.ContainsKey(checkDupKey))
                    {
                        //Key สามารถซ้ำได้ เพราะ API name ชื่อเดียวอาจจะมีหลาย signature ใน controller เดียวกัน
                        ctrlNode.ApiPermissions.Add(apiNode);
                        checkDupDic.Add(checkDupKey, true);
                    }
                }
            }

            return controllers;
        }

        public MVCustomPermission GetInitialUserRolePermissions(string orgId)
        {
            var r = new MVCustomPermission()
            {
                Status = "OK",
                Description = "Success"
            };

            var permissions = GetInitialPermission("api", []);

            r.Permissions = permissions;

            return r;
        }
    }
}
