namespace Its.Otep.Api.Database;

using Its.Otep.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class DataContext : DbContext, IDataContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration, DbContextOptions<DataContext> options) : base(options)
    {
        Configuration = configuration;
    }

    public DbSet<MMasterRef>? MasterRefs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MMasterRef>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();
    }
}
