# CrossApp
Наскрізний проєкт з крос-платформного програмування.

**Студент:** Сах Маркіян, група ФЕІ-34  
**Предметна область:** Бібліотека (`Book`, `BookCopy`, `Reader`, `Loan`)

---

## Предметна область: Бібліотека
- **Назва:** Бібліотека.
- **Сутності:** `Book` (видання), `BookCopy` (примірник), `Reader` (читач), `Loan` (видача).
- **Призначення:** введення обліку видачі примірників книг читачам та контроль повернень.

## Запуск (Лабораторна робота 1)
```bash
dotnet build
dotnet run --project src/Cli
dotnet run --project src/Cli -- --json
```

## Середовище розробки
- **.NET SDK:** 8.0, 9.0
- **ОС:** Windows 11 x64
- **RID:** win-x64
- **IDE:** Visual Studio Code (з розширенням C# Dev Kit)

---

## Лабораторна робота 2: Solution, Core + Cli, Multi-targeting, Публікація

### 1. Архітектура та структура рішення
- `src/Core` — бібліотека класів (Class Library), що акумулює спільну модель, DTO та збір системних даних. Налаштовано multi-targeting (`net8.0;net9.0`).
- `src/Cli` — консольний клієнт, підключений до `Core` через `ProjectReference`.
- Організація каталогів у `Core`:
  - `Core/Dto/` — record-типи даних.
  - `Core/Domain/` — сутності домену «Бібліотека».
  - `Core/Storage/` — реалізації сховищ.

### 2. Порівняння режимів публікації

| Режим публікації | RID / Прапорці | Реальний розмір | Потрібен .NET Runtime |
| :--- | :--- | :--- | :--- |
| **Self-contained** | `win-x64` (`--self-contained true`) | **70.73 МБ** | **Ні** (CLR вшито у випуск) |
| **Framework-dependent** | `win-x64` (`--self-contained false`) | **0.20 МБ** | **Так** (потрібен встановлений .NET 8) |
| **PublishSingleFile** | `win-x64` (`-p:PublishSingleFile=true`) | **64.44 МБ** | **Ні** (монолітний файл .exe) |
| **PublishTrimmed** | `win-x64` (`-p:PublishTrimmed=true -p:PublishSingleFile=true`) | **11.82 МБ** | **Ні** (невикористаний код видалено) |
| **Linux Self-contained** | `linux-x64` | **~72.5 МБ** | **Ні** (протестовано та успішно запущено в Linux WSL) |

### 3. Різниця між Self-contained та Framework-dependent
- **Framework-dependent** містить лише скомпільований код застосунку (~0.20 МБ), але вимагає встановленого .NET Runtime на машині клієнта.
- **Self-contained** постачається разом із копією середовища виконання, гарантуючи запуск на будь-якому ПК без попередніх налаштувань ціною більшого розміру.

### 4. Основні команди розробки та запуску

# Збірка всього рішення
dotnet build

# Запуск під різними версіями .NET (Multi-targeting)
dotnet run --project src/Cli -f net8.0
dotnet run --project src/Cli -f net9.0

# Прямий запуск скомпільованого бінарника
.\publish\self-contained\Cli.exe

# Запуск із виводом структурованого JSON
.\publish\single-file\Cli.exe --json

# Тестування під Linux (WSL)
cd publish/linux-x64
wsl
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 ./Cli

## Лабораторна робота 3: Формат вхідних даних
- Роздільник колонок: крапка з комою (`;`)
- Кодування файлів: UTF-8
- Структура CSV: `id;isbn;title;year;[author]`
- Підтримувані розширення: `.csv`, `.json`, а також змішані файли з префіксами рядків `B;...` та `R;...