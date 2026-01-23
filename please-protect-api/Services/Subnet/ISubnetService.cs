using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Services
{
    public interface ISubnetService
    {
        public Task<MVSubnet> GetSubnetById(string orgId, string subnetId);
        public Task<MVSubnet> AddSubnet(string orgId, MSubnet subnet);

        public Task<MVSubnet> DeleteSubnetById(string orgId, string subnetId);
        public Task<List<MSubnet>> GetSubnets(string orgId, VMSubnet param);
        public Task<int> GetSubnetCount(string orgId, VMSubnet param);
        public Task<MVSubnet> UpdateSubnetById(string orgId, string subnetId, MSubnet subnet);
        public Task<MVSubnetCacheUpdate> UpdateSubnetsCache(string orgId);
    }
}
