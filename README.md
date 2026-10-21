# ModLoader

> **GitHub About (EN):** Windows wizard that uploads a mod folder to Steam Workshop via the game’s `-publish` flow (Door Kickers–style).

Minimal Windows app: a step-by-step wizard to publish a mod folder to the Steam Workshop.

Based on the [Door Kickers mod publishing guide](https://steamcommunity.com/sharedfiles/filedetails/?l=english&id=441616640): the game is launched through Steam with `-publish "mod folder"`, then Steam / the game client handles the Workshop upload.

---

## English

### Requirements

1. **Windows 10/11**
2. **Steam** (account allowed to publish to the chosen game’s Workshop)
3. The game installed in Steam (Door Kickers AppID: `248610`)
4. A ready **mod folder** (for Door Kickers, `mod.xml` is recommended)
5. To **build from source**: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Prebuilt releases are self-contained and do **not** need the SDK.

### How to use the wizard

1. **Select the mod folder** → Next  
2. **Pick a game** or enter an AppID → Next  
3. Enter **title** and **description** → Next  
4. **Preview image** (or Skip)  
5. Review → **Upload**  
   If Steam is not running, the app will ask permission to start it.

### Build & run (from source)

```powershell
dotnet restore
dotnet build
dotnet run --project ModLoader
```

### Publish a Windows executable

```powershell
dotnet publish ModLoader -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

### Tests

```powershell
dotnet test
```

Covered (business logic, not UI buttons):

- mod folder validation
- upload parameters (AppID, title, description, preview)
- VDF generation for steamcmd
- Steam check/launch via `ISteamClient` (mocked in tests)
- wizard step transitions

### Solution layout

| Project | Role |
|---------|------|
| `ModLoader` | Thin WinForms UI (wizard) |
| `ModLoader.Core` | Validation, Steam, VDF, wizard logic |
| `ModLoader.Tests` | xUnit + Moq + FluentAssertions |

### How upload works

**Primary path:** `steam.exe -applaunch <AppID> -publish "C:\path\to\mod"`  
(as in the Door Kickers guide).

**Fallback sketch:** generate a `.vdf` for `steamcmd +workshop_build_item` (see `WorkshopVdfGenerator` / `PublishService.PrepareSteamCmdUpload`).

### Limitations

- Full Steamworks API (CreateItem / SubmitItemUpdate) is not wired yet — for Door Kickers, `-publish` is enough.
- Updating an already published item via steamcmd needs a `publishedfileid` (can be passed manually in code/API when preparing VDF).
- If the Windows username contains a space, the Door Kickers guide recommends not placing the mod on `C:` — keep that in mind when choosing a folder.

---

## Русский

### Требования

1. **Windows 10/11**
2. **Steam** (аккаунт с правом публиковать в Workshop выбранной игры)
3. Игра установлена в Steam (для Door Kickers — AppID `248610`)
4. Готовая **папка мода** (для Door Kickers желателен файл `mod.xml`)
5. Для **сборки из исходников**: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Готовые релизы self-contained — **SDK не нужен**.

### Как пользоваться мастером

1. **Выбрать папку мода** → «Далее»
2. **Выбрать игру** кнопкой или ввести AppID → «Далее»
3. Ввести **название** и **описание** → «Далее»
4. **Картинка превью** (можно «Пропустить»)
5. Проверить данные → «Загрузить»  
   Если Steam не запущен — приложение спросит разрешение на запуск.

### Сборка и запуск (из исходников)

```powershell
dotnet restore
dotnet build
dotnet run --project ModLoader
```

### Публикация exe для Windows

```powershell
dotnet publish ModLoader -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

### Тесты

```powershell
dotnet test
```

Покрыта бизнес-логика (не кнопки UI):

- проверка папки мода
- параметры загрузки (AppID, название, описание, превью)
- генерация VDF для steamcmd
- проверка/запуск Steam через интерфейс `ISteamClient` (в тестах — моки)
- переходы шагов мастера

### Структура решения

| Проект | Назначение |
|--------|------------|
| `ModLoader` | Тонкий WinForms UI (wizard) |
| `ModLoader.Core` | Логика: валидация, Steam, VDF, мастер |
| `ModLoader.Tests` | xUnit + Moq + FluentAssertions |

### Как устроена загрузка

**Основной путь:** `steam.exe -applaunch <AppID> -publish "C:\path\to\mod"`  
(как в гайде Door Kickers).

**Запасной каркас:** генерация `.vdf` для `steamcmd +workshop_build_item` (см. `WorkshopVdfGenerator` / `PublishService.PrepareSteamCmdUpload`).

### Ограничения

- Полноценный Steamworks API (CreateItem / SubmitItemUpdate) пока не подключён — для Door Kickers достаточно `-publish`.
- Для обновления уже опубликованного мода через steamcmd нужен `publishedfileid` (сейчас при подготовке VDF можно передать вручную в коде/API сервиса).
- Если в имени пользователя Windows есть пробел, гайд Door Kickers рекомендует класть мод не на диск `C:` — учтите это при выборе папки.
