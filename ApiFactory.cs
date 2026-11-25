using LibraryApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApi.Tests;

// Boots the real API in memory with a fresh, isolated database
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var old = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (old != null) services.Remove(old);
            var dbName = "TestDb_" + Guid.NewGuid();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName));
        });
    }
}
