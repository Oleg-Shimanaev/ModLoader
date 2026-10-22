## English

**v1.0.0 — First release** of ModLoader: a Windows wizard that publishes a mod folder to the Steam Workshop (Door Kickers–style `-publish` flow).

### What's included
- Self-contained Windows x64 build (`ModLoader-win-x64.zip`) — no .NET SDK required
- Step-by-step wizard: mod folder → game/AppID → title & description → preview → upload
- Steam launch / check before publishing

### Requirements
- Windows 10/11
- Steam (account allowed to publish to the game’s Workshop)
- Target game installed (e.g. Door Kickers AppID `248610`)
- Ready mod folder (for Door Kickers, `mod.xml` recommended)

### How to use
1. Extract the zip
2. Run `ModLoader.exe`
3. Follow the wizard steps and click Upload

### Source
Build from source with .NET 8 SDK: `dotnet build` / `dotnet run --project ModLoader` / `dotnet test`

---

## Русский

**v1.0.0 — Первый релиз** ModLoader: мастер для Windows, который публикует папку мода в Steam Workshop (путь Door Kickers: `-publish`).

### Что внутри
- Self-contained сборка Windows x64 (`ModLoader-win-x64.zip`) — .NET SDK не нужен
- Пошаговый мастер: папка мода → игра/AppID → название и описание → превью → загрузка
- Проверка/запуск Steam перед публикацией

### Требования
- Windows 10/11
- Steam (аккаунт с правом публиковать в Workshop игры)
- Игра установлена (например Door Kickers AppID `248610`)
- Готовая папка мода (для Door Kickers желателен `mod.xml`)

### Как пользоваться
1. Распакуйте zip
2. Запустите `ModLoader.exe`
3. Пройдите шаги мастера и нажмите «Загрузить»

### Исходники
Сборка с .NET 8 SDK: `dotnet build` / `dotnet run --project ModLoader` / `dotnet test`
