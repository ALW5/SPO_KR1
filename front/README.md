# Фронтенд складских ячеек

Статичный фронтенд (HTML/CSS/JS), работает с ASP.NET Core API (`ProjectOne`) по HTTPS.
Авторизация через HttpOnly-куку `access_token`; роль и профиль берутся из `GET /Auth/me`.

## Запуск

1. База: `docker compose up -d` (PostgreSQL, БД `projectone`, пользователь/пароль `postgres`).
2. Бэк (профиль https, порт 7269):
   ```
   dotnet dev-certs https --trust
   dotnet run --project ProjectOne --launch-profile https
   ```
   Миграции и администратор (`admin@projectone.local` / `Admin123!`) создаются при старте.
3. Фронт нужно раздавать с адреса, разрешённого в CORS (`Cors:AllowedOrigins` в `appsettings.json`;
   по умолчанию `http://localhost:5173`, `http://localhost:5500`, `http://127.0.0.1:5500`):
   ```
   npx serve -l 5173 front
   ```
   Лучше использовать `localhost` (а не `127.0.0.1`): тогда фронт и API считаются одним сайтом и кука не блокируется.
4. Адрес API задаётся константой `API` в начале `app.js`
   (`https://localhost:7269` для профиля https, `https://localhost:44373` для IIS Express).

## Правила бизнес-логики (на стороне сервера)
- Стоимость брони считается сервером: цена ячейки × число дней; `totalPrice` из запроса игнорируется.
- Клиент видит и отменяет только свои аренды, администратор - все.
- Ячейку с договорами удалить нельзя (409).
