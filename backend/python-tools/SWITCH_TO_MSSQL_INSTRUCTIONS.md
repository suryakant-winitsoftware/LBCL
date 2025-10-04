# Switch from PostgreSQL to MSSQL - Instructions

## Changes needed in `/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs`

### For Employee Login to work with MSSQL:

**1. Line 212 - Auth (Login)**

```csharp
// OLD (PostgreSQL):
.AddTransient<Winit.Modules.Auth.DL.Interfaces.IAuthDL, Winit.Modules.Auth.DL.Classes.PGSQLAuthDL>();

// NEW (MSSQL):
.AddTransient<Winit.Modules.Auth.DL.Interfaces.IAuthDL, Winit.Modules.Auth.DL.Classes.MSSQLAuthDL>();
```

**2. Line 958 - Employee**

```csharp
// OLD (PostgreSQL):
_ = services.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpDL>();

// NEW (MSSQL):
_ = services.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpDL>();
```

**3. Line 968 - Employee Info**

```csharp
// OLD (PostgreSQL):
.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpInfoDL>();

// NEW (MSSQL):
.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpInfoDL>();
```

**4. Line 976 - Employee Org Mapping**

```csharp
// OLD (PostgreSQL):
.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpOrgMappingDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpOrgMappingDL>();

// NEW (MSSQL):
.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpOrgMappingDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpOrgMappingDL>();
```

## Connection String

Make sure `appsettings.Development.json` has ONLY the MSSQL connection string:

```json
"ConnectionStrings": {
  "MSSQL": "Server=10.20.53.175;Database=LBSSFADev;User Id=lbssfadev;Password=lbssfadev;TrustServerCertificate=True;"
}
```

## After making changes:

1. Save `Startup.cs`
2. Rebuild the project
3. Restart the API
4. Test login endpoint: `POST /api/Auth/GetToken`

## Note:

The MSSQL classes (`MSSQLAuthDL`, `MSSQLEmpDL`, etc.) already exist in your codebase, so you only need to change the dependency injection registrations.
