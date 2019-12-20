# Formula.SimpleIdentity
Easy localy stored membership based authentication

# Adding Local Simple Authentication
- In the Startup.cs (ConfigureServices function) enable authentication with the following;
  - services.AddSimpleMembership(this.Configuration, typeof(Startup).Assembly.GetName().Name);
- Run the migrations

### Configuring Stores and Migrations
To run without a persistent data store, you may run all operations in memory by setting 
InMemoryAuthProvider to true in the appSettings.

To work with persistent storage this must be set to false, and a connectionName (as also specified in the appSettings), might optionally be passed as a parameter to AddSimpleMembership (default connection string is DefaultConnection).

#### Running Migrations
To build and run migrations you must install the EF Core CLI tool on your machine, and add the Microsoft.EntityFrameworkCore.Design package to your project.

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
```

To generate the migrations in your project run;

```bash
dotnet ef migrations add InitialAspNetIdentityDbMigration --context Formula.SimpleMembership.SimpleMembershipDbContext --output-dir Data/Migrations/SimpleMembership/IdentityDb
dotnet ef database update --context Formula.SimpleMembership.SimpleMembershipDbContext
```

If you installed your migrations a different project, you may specify the "migrationsAssembly", as a parameter to AddSimpleMembership.

## Using the Account Controller

### Registration
Registration can be performed by posting to https://localhost:5001/user/register
with the following body.

```JSON
{
    "userName" : "myuser",
    "email" : "me@earth.com",
    "password" : "Letmein123!",
    "confirmPassword" : "Letmein123!"
}
```

###  Login
Login can be performed by posting to https://localhost:5001/user/login
with the following body.

```JSON
{
    "userName" : "myuser",
    "password" : "Letmein123!"
}
```

# Relavant Links
- [ASP.NET Core Identity Series](https://chsakell.com/2018/04/28/asp-net-core-identity-series-getting-started)

# Packages / Projects Used
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/)
- [Microsoft.AspNetCore.Mvc](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc/)
- [Microsoft.AspNetCore.Authorization](https://www.nuget.org/packages/Microsoft.AspNetCore.Authorization/)
- [Microsoft.AspNetCore.Authentication.Cookies](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.Cookies/)
- [Microsoft.Extensions.Identity.Stores](https://www.nuget.org/packages/Microsoft.Extensions.Identity.Stores/)
- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/)
- [Microsoft.AspNetCore.Identity](Microsoft.AspNetCore.Identity)
- [Microsoft.AspNetCore.Identity.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore/)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer)
- [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory)
- [NETCore.Encrypt](https://www.nuget.org/packages/NETCore.Encrypt/)