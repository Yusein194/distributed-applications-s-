# Fitness Rental System

## Информация за студента

- **Име:** Юсеин Мехмедов (Yusein Mehmedov)
- **Факултетен номер:** 2401321052

## Описание на проекта

**Fitness Rental System** е уеб-базирана система за управление на наеми на фитнес оборудване. Проектът е разработен на **ASP.NET Core 8.0** и се състои от две отделни приложения:

1. **FitnessRentalSystem.API** — RESTful Web API, който осигурява бизнес логиката и достъпа до базата данни. Използва JWT за автентикация и Entity Framework Core за работа с MS SQL Server.
2. **FitnessRental.Web** — MVC уеб клиент, който комуникира с API-то и предоставя потребителски интерфейс.

### Основни функционалности

- Регистрация и вход на потребители (с JWT токени)
- Управление на фитнес оборудване (CRUD операции)
- Създаване и проследяване на наеми на оборудване
- Управление на потребители
- Глобален middleware за обработка на грешки
- Swagger UI за документация и тестване на API

### Използвани технологии

- ASP.NET Core 8.0 (Web API + MVC)
- Entity Framework Core 8.0
- MS SQL Server
- JWT Bearer Authentication
- BCrypt.Net (хеширане на пароли)
- Swashbuckle / Swagger
- Bootstrap (във Web клиента)

## Структура на проекта

```
FitnessRentalSystem.API/
├── FitnessRentalSystem.sln          # Solution файл
├── FitnessRentalSystem.API/         # Web API проект
│   ├── Controllers/                 # AuthController, UsersController,
│   │                                # FitnessEquipmentsController,
│   │                                # EquipmentRentalsController
│   ├── Models/                      # User, FitnessEquipment, EquipmentRental
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Data/                        # AppDbContext
│   ├── Middleware/                  # GlobalExceptionMiddleware
│   ├── Migrations/                  # EF Core миграции
│   └── Program.cs
└── FitnessRental.Web/               # MVC уеб клиент
    ├── Controllers/
    ├── Views/
    ├── Models/
    ├── wwwroot/
    └── Program.cs
```

## Изисквания — необходими приложения

Преди да стартирате проекта, **задължително** трябва да имате инсталирани следните приложения:

### Задължителен софтуер

1. **.NET 8.0 SDK** — необходим за компилиране и стартиране на проекта
   - Линк за изтегляне: https://dotnet.microsoft.com/download/dotnet/8.0
   - Проверка след инсталация: `dotnet --version` (трябва да върне версия 8.0.x или по-нова)

2. **Microsoft SQL Server** — база данни за съхранение на потребители, оборудване и наеми
   - Препоръчителна версия: **SQL Server 2019 / 2022 Express Edition** (безплатна)
   - Линк за изтегляне: https://www.microsoft.com/sql-server/sql-server-downloads
   - Алтернатива: **SQL Server LocalDB** (идва с Visual Studio)

3. **SQL Server Management Studio (SSMS)** — за управление и преглед на базата данни
   - Линк за изтегляне: https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

4. **IDE / редактор на код** (изберете един):
   - **Visual Studio 2022** (препоръчително за Windows) — Community Edition е безплатна
     - https://visualstudio.microsoft.com/downloads/
     - При инсталация изберете workload-а **"ASP.NET and web development"**
   - **Visual Studio Code** + разширение **C# Dev Kit**
     - https://code.visualstudio.com/
   - **JetBrains Rider**
     - https://www.jetbrains.com/rider/

### Допълнителни инструменти

5. **dotnet-ef CLI tool** — за прилагане на миграциите към базата данни
   - Инсталация: `dotnet tool install --global dotnet-ef`

6. **Уеб браузър** — за достъп до уеб клиента и Swagger UI
   - Google Chrome, Microsoft Edge, Mozilla Firefox или друг съвременен браузър


## Инсталация

### 1. Клониране / разархивиране на проекта

Разархивирайте архива или клонирайте репозиторито в локална директория.

### 2. Конфигуриране на връзката с базата данни

Отворете файла `FitnessRentalSystem.API/appsettings.json` и редактирайте `DefaultConnection` според вашия SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=FitnessRentalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
}
```

> Заменете `YOUR_SERVER_NAME` с името на вашия SQL Server инстанс (напр. `(localdb)\\MSSQLLocalDB` или `.\\SQLEXPRESS`).

### 3. Възстановяване на NuGet пакетите

В главната папка на проекта (където е `FitnessRentalSystem.sln`) изпълнете:

```bash
dotnet restore
```

### 4. Прилагане на миграциите към базата данни

```bash
cd FitnessRentalSystem.API
dotnet ef database update
```

> Ако `dotnet-ef` не е инсталиран, изпълнете:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

## Стартиране

Проектът се състои от **две приложения**, които трябва да се стартират едновременно.

### Вариант 1: От Visual Studio

1. Отворете `FitnessRentalSystem.sln`.
2. Кликнете с десен бутон върху Solution → **Set Startup Projects** → **Multiple startup projects**.
3. Изберете **Start** за `FitnessRentalSystem.API` и `FitnessRental.Web`.
4. Натиснете **F5** (или зеления бутон Start).

### Вариант 2: От терминал (две конзоли)

**Конзола 1 — стартиране на API:**

```bash
cd FitnessRentalSystem.API
dotnet run
```

API ще е достъпно на: `https://localhost:7XXX` (порт според `launchSettings.json`).
Swagger UI: `https://localhost:7XXX/swagger`

**Конзола 2 — стартиране на Web клиента:**

```bash
cd FitnessRental.Web
dotnet run
```

Уеб клиентът ще е достъпен на: `https://localhost:7YYY` и ще зареди страницата за вход.

> Уверете се, че в `FitnessRental.Web/appsettings.json` стойността на `ApiSettings:BaseUrl` сочи към адреса на стартираното API.

## Използване

1. Отворете уеб клиента в браузъра.
2. Регистрирайте нов потребител или влезте с вече съществуващ.
3. След успешен вход получавате JWT токен и достъп до функционалностите за управление на оборудване и наеми.

За директно тестване на API endpoint-ите използвайте **Swagger UI** на адрес `/swagger`.


