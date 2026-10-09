using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ProjectOne.Tests.Integration;

/// <summary>
/// Поднимает настоящее API в памяти (WebApplicationFactory) поверх настоящей PostgreSQL.
///
/// База берётся так:
///  1) если задана переменная окружения TEST_DB_CONNECTION - используется она
///     (например, "Server=localhost;Port=5432;Database=projectone_tests;User Id=postgres;Password=postgres;");
///  2) иначе запускается контейнер postgres:16-alpine через Testcontainers (нужен запущенный Docker).
///
/// Миграции и создание администратора выполняет сам Program.cs при старте.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public const string AdminEmail = "admin@projectone.local";
    public const string AdminPassword = "Admin123!";

    private PostgreSqlContainer? _container;
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION");
        string connectionString;

        if (!string.IsNullOrWhiteSpace(external))
        {
            connectionString = external;
        }
        else
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("projectone_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            connectionString = _container.GetConnectionString();
        }

        // Переменные окружения имеют приоритет над appsettings*.json (строка подключения читается из IConfiguration).
        Environment.SetEnvironmentVariable("ConnectionStrings__Db", connectionString);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

        // Принудительно стартуем хост: миграции + сидинг ролей и администратора.
        _ = _factory.Server;
    }

    /// <summary>
    /// Новый клиент со своим контейнером кук = отдельная "сессия браузера".
    /// BaseAddress с https нужен, потому что кука access_token помечена Secure.
    /// </summary>
    public ApiClient CreateClient()
    {
        var http = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        return new ApiClient(http);
    }

    /// <summary>Клиент, уже вошедший как администратор.</summary>
    public async Task<ApiClient> CreateAdminAsync()
    {
        var client = CreateClient();
        var response = await client.LoginAsync(AdminEmail, AdminPassword);
        response.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>Клиент с новым зарегистрированным пользователем (роль Client).</summary>
    public async Task<ApiClient> CreateRegisteredClientAsync(string? prefix = null)
    {
        var client = CreateClient();
        var email = $"{prefix ?? "user"}-{Guid.NewGuid():N}@test.local";
        var response = await client.RegisterAsync(email, "Passw0rd!", "Test User");
        response.EnsureSuccessStatusCode();
        client.Email = email;
        return client;
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        if (_container is not null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api"; // должно совпадать со строкой в [CollectionDefinition]
}
