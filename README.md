# Formula.SimpleIdentity
Easy localy stored membership based authentication

# Adding Local Simple Authentication
- In the Startup.cs (ConfigureServices function) enable authentication with the following;
  - services.AddSimpleMembership(this.Configuration, typeof(Startup).Assembly.GetName().Name);
- Run the migrations

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add AspNetIdentity -c IdentityDbContext -o Data/Migrations/SimpleMembership
dotnet ef database update
```

## Using the Account Controller

### Registration
Registration can be performed by posting to https://localhost:5001/account/register
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
Login can be performed by posting to https://localhost:5001/account/login
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