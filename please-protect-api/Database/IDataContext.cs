using Microsoft.EntityFrameworkCore;
using Its.Otep.Api.Models;

namespace Its.Otep.Api.Database
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        public DbSet<MMasterRef>? MasterRefs { get; set; }
    }
}