using Microsoft.EntityFrameworkCore;
using RaceDay_API.Data;

namespace RaceDay_API
{
    
    // Application entry point. Configures the ASP.NET Core pipeline,
    // registers EF Core with SQL Server, enables Swagger for API
    // documentation, and sets up session-based authentication used by
    // the RaceDay role-based access control system.
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            app.UseHttpsRedirection();

            app.UseSession();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

    }
    }

/*
1. ASP.NET Core Middleware Pipeline (UseRouting, UseSession, UseAuthorization, MapControllers).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/>
Used in: Ordering middleware so sessions and authorization work before controllers are invoked.
Date Accessed: 18 October 2026

2. EF Core DbContext Registration (Microsoft.EntityFrameworkCore.SqlServer 10.0.13).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/>
Used in: Registering RaceDayDbContext with SQL Server and reading the connection string from appsettings.json.
Date Accessed: 18 October 2026

3. OpenApiInfo / SwaggerDoc (Microsoft.OpenApi 2.7.5).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.models.openapiinfo>
Used in: Setting the API title, version, and description in Swagger.
Date Accessed: 18 October 2026

4. Session Middleware (Microsoft.AspNetCore.Session).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state>
Used in: Configuring server-side session state to store UserId and Role after login.
Date Accessed: 18 October 2026

5. Swashbuckle / AddSwaggerGen and IncludeXmlComments (Swashbuckle.AspNetCore 10.3.1).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle>
Used in: Configuring Swagger to read XML documentation comments and display endpoint descriptions.
Date Accessed: 18 October 2026
*/