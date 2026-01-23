using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;
using System.Net;
using Serilog;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Its.PleaseProtect.Api.Services
{
    public class SubnetService : BaseService, ISubnetService
    {
        private readonly ISubnetRepository? repository = null;
        private readonly IRedisHelper _redis;

        public SubnetService(ISubnetRepository repo,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        private bool IsCidrValid(string cidr)
        {
            if (string.IsNullOrWhiteSpace(cidr))
                return false;

            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            // Validate IP part
            if (!IPAddress.TryParse(parts[0], out var ip))
                return false;

            // Ensure IPv4
            if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            // Validate prefix length
            if (!int.TryParse(parts[1], out int prefix))
                return false;

            return prefix >= 0 && prefix <= 32;
        }

        public async Task<MVSubnet> GetSubnetById(string orgId, string subnetId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVSubnet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(subnetId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Subnet ID [{subnetId}] format is invalid";

                return r;
            }

            var result = await repository!.GetSubnetById(subnetId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Subnet ID [{subnetId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Subnet = result;

            return r;
        }

        public async Task<MVSubnet> AddSubnet(string orgId, MSubnet subnet)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVSubnet();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(subnet.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Subnet name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(subnet.Cidr))
            {
                r.Status = "CIDRMISSING";
                r.Description = $"Subnet CIDR is missing!!!";

                return r;
            }

            var isCidrValid = IsCidrValid(subnet.Cidr);
            if (!isCidrValid)
            {
                r.Status = "CIDR_FORMAT_INVALID";
                r.Description = $"Invalid CIDR format [{subnet.Cidr}] !!!";

                return r;
            }

            var isExist = await repository!.IsSubnetNameExist(subnet.Name);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Subnet name [{subnet.Name}] already exist!!!";

                return r;
            }

            isExist = await repository!.IsCidrExist(subnet.Cidr);
            if (isExist)
            {
                r.Status = "CIDR_DUPLICATE";
                r.Description = $"Subnet CIDR [{subnet.Cidr}] already exist!!!";

                return r;
            }

            var result = await repository!.AddSubnet(subnet);
            r.Subnet = result;

            //Update redis cache to notify CIDR changed
            _ = await UpdateSubnetsCache(orgId);

            return r;
        }

        public async Task<MVSubnet> DeleteSubnetById(string orgId, string subnetId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVSubnet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(subnetId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Subnet ID [{subnetId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteSubnetById(subnetId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Subnet ID [{subnetId}] not found for the organization [{orgId}]";

                return r;
            }

            //Update redis cache & notify CIDR changed
            _ = await UpdateSubnetsCache(orgId);

            r.Subnet = m;
            return r;
        }

        public async Task<List<MSubnet>> GetSubnets(string orgId, VMSubnet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetSubnets(param);

            return result;
        }

        public async Task<int> GetSubnetCount(string orgId, VMSubnet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetSubnetCount(param);

            return result;
        }

        public async Task<MVSubnet> UpdateSubnetById(string orgId, string subnetId, MSubnet subnet)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVSubnet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(subnetId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Subnet ID [{subnetId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(subnet.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Subnet name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(subnet.Cidr))
            {
                r.Status = "CIDRMISSING";
                r.Description = $"Subnet CIDR is missing!!!";

                return r;
            }

            var isCidrValid = IsCidrValid(subnet.Cidr);
            if (!isCidrValid)
            {
                r.Status = "CIDR_FORMAT_INVALID";
                r.Description = $"Invalid CIDR format [{subnet.Cidr}] !!!";

                return r;
            }

            var name = subnet.Name;
            var sn = await repository!.GetSubnetByName(name!);
            if ((sn != null) && (sn.Id.ToString() != subnetId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Subnet name [{name}] already exist!!!";

                return r;
            }

            var cidr = subnet.Cidr;
            sn = await repository!.GetSubnetByCidr(cidr!);
            if ((sn != null) && (sn.Id.ToString() != subnetId))
            {
                r.Status = "CIDR_DUPLICATE";
                r.Description = $"Subnet CIDR [{cidr}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateSubnetById(subnetId, subnet);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Subnet ID [{subnetId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Subnet = result;

            //Update redis cache & notify CIDR changed
            _ = await UpdateSubnetsCache(orgId);

            return r;
        }

        public async Task<MVSubnetCacheUpdate> UpdateSubnetsCache(string orgId)
        {
            var r = new MVSubnetCacheUpdate()
            {
                Status = "OK",
                Description = "Success"
            };

            var param = new VMSubnet()
            {
                FullTextSearch = ""
            };

            var keyPattern = CacheHelper.CreateSubnetCacheKey(orgId, "*");
            var existingKeys = await _redis.GetKeys(keyPattern);
            var keysFromDb = new Dictionary<string, string>();
 
            var itemCount = await GetSubnetCount(orgId, param);
            var itemPerPage = 300;

            int pageCount = (int) Math.Ceiling((double) itemCount / itemPerPage);
            int seq = 0;
            for (var i=0; i<pageCount; i++)
            {
                var offset = i * itemPerPage + 1; //Offset จะเริ่มจาก 1, ไม่ใช่ zero base
                param.Limit = itemPerPage;
                param.Offset = offset;

                var items = await GetSubnets(orgId, param);

                foreach (var subnet in items)
                {
                    seq++;

                    var subnetName = subnet.Name!;
                    var cidr = subnet.Cidr!;
                    var cacheKey = CacheHelper.CreateSubnetCacheKey(orgId, cidr);
                    keysFromDb.Add(cacheKey, "");

                    //Load this to Redis
                    _ = _redis.SetObjectAsync(cacheKey, subnetName);

                    Log.Information($"Cached [{seq}] [{cacheKey}] with value [{subnetName}]");
                }
            }

            //ให้ลบทิ้งถ้ามีอยู่ใน Redis แต่ไม่มีใน DB
            foreach (var key in existingKeys.Keys)
            {
                if (!keysFromDb.ContainsKey(key))
                {
                    Log.Information($"Deleted [{key}] from Redis");
                    await _redis.DeleteAsync(key);
                }
            }

            r.ItemCount = seq;

            return r;
        }
    }
}
