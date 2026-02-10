using Microsoft.EntityFrameworkCore;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Database
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        public DbSet<MOrganization>? Organizations { get; set; }
        public DbSet<MMasterRef>? MasterRefs { get; set; }
        public DbSet<MRole>? Roles { get; set; }
        public DbSet<MApiKey>? ApiKeys { get; set; }
        public DbSet<MUser>? Users { get; set; }
        public DbSet<MOrganizationUser>? OrganizationUsers { get; set; }
        public DbSet<MCustomRole>? CustomRoles { get; set; }
        public DbSet<MDocument>? Documents { get; set; }
        public DbSet<MJob>? Jobs { get; set; }
        public DbSet<MIoC>? Iocs { get; set; }
        public DbSet<MSubnet>? Subnets { get; set; }
        public DbSet<MNotiAlertEvent>? NotiAlertEvents { get; set; }
    }
}