using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RaceDay_API.Data;

namespace RaceDay_API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

/*
1. IServiceCollection.Remove / BuildServiceProvider / CreateScope.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicecollection>
Used in: Removing duplicate database provider registrations and creating a DbContext scope to call EnsureCreated.
Date Accessed: 18 October 2026

2. IWebHostBuilder.ConfigureServices Override.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory>
Used in: Replacing the SQL Server provider with the InMemory provider for isolated tests.
Date Accessed: 18 October 2026

3. UseInMemoryDatabase (Microsoft.EntityFrameworkCore.InMemory 10.0.13).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/providers/in-memory/>
Used in: Providing a test database that has the same schema but does not persist data.
Date Accessed: 18 October 2026

4. WebApplicationFactory<Program> (Microsoft.AspNetCore.Mvc.Testing 10.0.13).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests>
Used in: Preparing the API in-memory for integration tests, allowing request/response testing of all endpoints.
Date Accessed: 18 October 2026
*/