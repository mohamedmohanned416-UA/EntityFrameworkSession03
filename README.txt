Online Learning Platform - EF Core Assignment 3

Requirements:
.NET 8 SDK
Visual Studio 2022 with .NET desktop development workload or Visual Studio Code with C# Dev Kit
SQL Server Express LocalDB (Windows) or an accessible SQL Server instance

Visual Studio 2022:
1. Extract the zip.
2. Open OnlineLearningPlatform.csproj in Visual Studio.
3. Restore NuGet packages if prompted.
4. Install SQL Server Express LocalDB if it is not installed.
5. Press Ctrl+F5 to run.

Visual Studio Code:
1. Extract and open the OnlineLearningPlatform folder in VS Code.
2. Open Terminal > New Terminal.
3. Run: dotnet restore
4. Run: dotnet run

The program automatically applies two existing migrations:
202611220001_InitialCreate
202611220002_SeedData

The database is OnlineLearningPlatformDb on (localdb)\MSSQLLocalDB by default.
If using another SQL Server instance, set the LEARNING_PLATFORM_CONNECTION environment variable before running.

PowerShell example for SQL Server Express:
$env:LEARNING_PLATFORM_CONNECTION = 'Server=.\SQLEXPRESS;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;'
dotnet run

PowerShell example for an ordinary local SQL Server:
$env:LEARNING_PLATFORM_CONNECTION = 'Server=localhost;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;'
dotnet run

If you want to apply the migrations yourself before running:
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef database update

Do not run 'dotnet ef migrations add InitialCreate' or 'dotnet ef migrations add SeedData' because both migrations are already in the project.

The second execution skips Program.cs seed inserts when the relevant tables already have rows. CRUD demonstration methods execute on every run, so those methods can add or change data again.

The delete-course operation uses cascading deletion to remove linked enrollments, as explained in the printed output. The submitted code contains no comments.
