using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface ISubnetRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsSubnetNameExist(string name);
        public Task<bool> IsCidrExist(string cidr);
        public Task<MSubnet?> GetSubnetByName(string name);
        public Task<MSubnet?> GetSubnetByCidr(string cidr);
        public Task<MSubnet?> UpdateSubnetById(string subnetId, MSubnet subnet);
        public Task<MSubnet> AddSubnet(MSubnet subnet);

        public Task<List<MSubnet>> GetSubnets(VMSubnet param);
        public Task<int> GetSubnetCount(VMSubnet param);
        public Task<MSubnet?> GetSubnetById(string subnetId);
        public Task<MSubnet?> DeleteSubnetById(string subnetId);
    }
}
