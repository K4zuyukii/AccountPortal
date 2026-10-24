using AccountPortal.Models;
using Microsoft.Data.SqlClient;

namespace AccountPortal.Data;

// Plain ADO.NET against SQL Server LocalDB. The database and Users table are
// created automatically the first time the app talks to them.
public class UserRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AccountDb")
            ?? @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=AccountPortal;Integrated Security=True;TrustServerCertificate=True";
    }

    private async Task<SqlConnection> OpenAsync()
    {
        await EnsureDatabaseAsync();
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private async Task EnsureDatabaseAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;

            // 1) create the database if missing (connect to master)
            builder.InitialCatalog = "master";
            await using (var master = new SqlConnection(builder.ConnectionString))
            {
                await master.OpenAsync();
                await using var cmd = master.CreateCommand();
                cmd.CommandText = "IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE [' + @name + ']')";
                cmd.Parameters.AddWithValue("@name", databaseName);
                await cmd.ExecuteNonQueryAsync();
            }

            // 2) create the Users table if missing
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
IF OBJECT_ID('dbo.Users', 'U') IS NULL
CREATE TABLE dbo.Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100)  NOT NULL,
    Phone        NVARCHAR(30)   NULL,
    Email        NVARCHAR(256)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256)  NOT NULL,
    CreatedAt    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);";
                await cmd.ExecuteNonQueryAsync();
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = await OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Phone, Email, PasswordHash, CreatedAt FROM dbo.Users WHERE Email = @email";
        cmd.Parameters.AddWithValue("@email", email);
        return await ReadOneAsync(cmd);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = await OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Phone, Email, PasswordHash, CreatedAt FROM dbo.Users WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        return await ReadOneAsync(cmd);
    }

    public async Task<int> CreateAsync(User user)
    {
        await using var connection = await OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Users (Name, Phone, Email, PasswordHash)
VALUES (@name, @phone, @email, @hash);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private static async Task<User?> ReadOneAsync(SqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
            Email = reader.GetString(3),
            PasswordHash = reader.GetString(4),
            CreatedAt = DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Utc)
        };
    }
}
