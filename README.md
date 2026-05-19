# MarketplaceOOP 
Курсовий проєкт

Студент: Артьомов Артемій Ігорович

Варіант №1 | Маркетплейс для продажу товарів

Файли проєкту знаходяться на гілці master

---

## Запуск локально

Потрібні **дві консолі**.

### 1. Backend

```bash
cd Marketplace.API
dotnet run
```

- API: http://localhost:5077
- Swagger: http://localhost:5077/swagger

При першому запуску застосовуються міграції EF Core і seed демо-даних.

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

- UI: http://localhost:5173
- Запити `/api/*` проксуються на `http://localhost:5077` (див. `frontend/vite.config.js`)

--- 

## Можливості
- Реєстрація та вхід (JWT)
- Каталог товарів і категорії (ієрархія)
- Оформлення замовлень, статуси, ТТН
- Кабінет продавця (товари, замовлення)
- Панель адміністратора (користувачі, модерація, експорт/імпорт каталогу JSON)
- Відгуки та модерація
- Блокування користувачів, lockout при невдалому вході
---
## Стек
| Частина  | Технології |
|----------|------------|
| Backend  | .NET 8, ASP.NET Core Web API, EF Core 8 |
| БД       | SQLite (`marketplace.db`) |
| Frontend | React 18, Vite, React Router |
| Auth     | JWT Bearer |
| Тести    | xUnit (`Marketplace.Tests`) |
---
## Структура проєкту

```
MarketplaceOOP/
|-- Marketplace.sln
|-- Marketplace.API/                  -- backend
|   |-- Controllers/                  -- HTTP API
|   |-- Models/                       -- предметна область
|   |-- Interfaces/                   -- контракти сервісів
|   |-- Services/                     -- бізнес-логіка
|   |-- Repositories/                 -- доступ до БД
|   |-- Data/                         -- DbContext, міграції
|   |-- DTOs/                         -- моделі для API
|   |-- Serialization/                -- JSON import/export
|   |-- Validation/
|   |-- Program.cs                    -- DI, JWT, міграції
|-- Marketplace.Tests/                -- Unit-тести
|-- frontend/                         -- React + Vite
    |-- src/pages, components, context
```

---
