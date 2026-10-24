ACCOUNT PORTAL - ASP.NET Core MVC (.NET 8) + SQL Server LocalDB
================================================================

1. Unzip. Open Visual Studio 2022 > "Open a project or solution" and pick:
       AccountPortal\AccountPortal.csproj
   (Needs the "ASP.NET and web development" workload and the .NET 8 SDK.)

2. Press F5 (green "http" Start button). The browser opens at http://localhost:5080.
   NuGet restores Microsoft.Data.SqlClient automatically on first build.

3. The "AccountPortal" database and Users table are created automatically in
   SQL Server LocalDB the first time you sign up or log in.

To use a different database, edit ConnectionStrings:AccountDb in appsettings.json.
