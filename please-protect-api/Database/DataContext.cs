namespace Its.PleaseProtect.Api.Database;

using Its.PleaseProtect.Api.Models;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MMasterRef>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();

        modelBuilder.Entity<MRole>();
        modelBuilder.Entity<MOrganizationUser>();
        modelBuilder.Entity<MOrganization>();
        modelBuilder.Entity<MApiKey>();
        modelBuilder.Entity<MOrganization>();

        modelBuilder.Entity<MUser>(entity =>
        {
            entity.Property(p => p.PhoneNumber).HasMaxLength(16);

            // PostgreSQL CHECK Constraint สำหรับ E.164
            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_User_PhoneNumber_E164",
                    "phone_number ~ '^\\+[1-9][0-9]{7,14}$'"
                );
            });
        });

        modelBuilder.Entity<MCustomRole>()
            .HasIndex(t => new { t.OrgId, t.RoleName }).IsUnique();

        modelBuilder.Entity<MDocument>()
            .HasIndex(t => new { t.OrgId, t.DocName }).IsUnique();

        modelBuilder.Entity<MIoC>()
            .HasIndex(t => new { t.OrgId, t.IocType, t.DataSet, t.IocValue, t.IocSubType }).IsUnique();

        modelBuilder.Entity<MSubnet>()
            .HasIndex(t => new { t.OrgId, t.Cidr }).IsUnique();

        modelBuilder.Entity<MSubnet>()
            .HasIndex(t => new { t.OrgId, t.Name }).IsUnique();

        modelBuilder.Entity<MJob>();
    }
}
