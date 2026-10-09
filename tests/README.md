# Тесты проекта

## 1. xUnit: `tests/ProjectOne.Tests`

| Папка | Что проверяет | Нужна БД |
|---|---|---|
| `Domain/` | Правила домена: бронь/освобождение ячейки, валидация цены и размеров, изменение договора | нет |
| `Application/` | `ReserveStorageCellHandler` на фейковых репозиториях: цена считается на сервере, проверка дат, 409 при занятой ячейке и при гонке | нет |
| `Integration/` | Настоящее API в памяти + PostgreSQL: вход/выход и роли, права доступа, контракт ответов для фронта, бронирование, 409, параллельные брони, чужие аренды (IDOR), удаление ячейки с договором | да |

Запуск:

```bash
dotnet sln add tests/ProjectOne.Tests/ProjectOne.Tests.csproj   # один раз
dotnet test tests/ProjectOne.Tests
```

База для интеграционных тестов:
- по умолчанию поднимается контейнер `postgres:16-alpine` через Testcontainers (нужен запущенный Docker);
- либо задайте свою БД: `TEST_DB_CONNECTION="Server=localhost;Port=5432;Database=projectone_tests;User Id=postgres;Password=postgres;"`
  (миграции и админ создаются при старте приложения; данные тесты создают уникальные, чистить БД между запусками не нужно).

Только быстрые тесты без БД: `dotnet test tests/ProjectOne.Tests --filter "FullyQualifiedName!~Integration"`.

## 2. Playwright E2E: `e2e`

Проверяют связку «настоящий браузер + фронт + бэк + БД»: вход, регистрация, бронирование, отмена, права администратора, изоляция аренд между клиентами и регрессии (NaN в окне брони, дубли POST /reserve).

```bash
docker compose up -d        # PostgreSQL
cd e2e
npm install
npx playwright install chromium
npx playwright test
```

Фронт (`npx serve -l 5173 ../front`) и бэк (`dotnet run --launch-profile https`) Playwright запускает сам,
либо использует уже запущенные. Адрес API также прописан константой `API` в `front/app.js`
(по умолчанию `https://localhost:7269`); если меняете порт, поправьте и её, и `API_URL`.
Первый запуск бэка по https: `dotnet dev-certs https --trust`.
