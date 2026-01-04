namespace Its.Otep.Api.Database.Seeders;

using System.Diagnostics.CodeAnalysis;
using PasswordGenerator;

[ExcludeFromCodeCoverage]
public class DataSeeder
{
    private readonly DataContext context;
    private readonly Password pwd = new Password(32);

    public DataSeeder(DataContext ctx)
    {
        context = ctx;
    }

    public void Seed()
    {
    }
}