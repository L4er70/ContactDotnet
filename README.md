# ContactBook Setup

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Database Setup
1. Start SQL Server in Docker:
   - On Linux/Mac/Windows (with Docker installed):
     ```
     docker-compose up -d
     ```
   - Or, run manually:
     ```
     docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong@Passw0rd' -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2022-latest
     ```

## Run the App
1. Restore dependencies:
   ```
   dotnet restore
   ```
2. Apply migrations (if using EF Core):
   ```
   dotnet ef database update
   ```
3. Run the app:
   ```
   dotnet run
   ```

## Default Admin Login
- Email: `admin@contactbook.com`
- Password: `Admin123!`