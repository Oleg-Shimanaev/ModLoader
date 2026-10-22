# ModLoader

> **GitHub About (EN):** Windows wizard that uploads a mod folder to Steam Workshop via **Steam client (Steamworks)** or **steamcmd** (no game launch).

Minimal Windows app: step-by-step wizard → Steamworks upload **or** VDF + `steamcmd +workshop_build_item`.

See **[CHANGELOG.md](CHANGELOG.md)** for version history. Help & paths: **[FAQ.md](FAQ.md)**.

---

## English

### Requirements

1. **Windows 10/11**
2. **Steam** (account allowed to publish; for Steam-client mode, Steam must be running and logged in)
3. **SteamCMD** only if you choose the steamcmd upload path ([Valve Wiki](https://developer.valvesoftware.com/wiki/SteamCMD))
4. A **mod folder** with `mod.xml` (Door Kickers 2 AppID: `1239080`)
5. To **build from source**: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### How to use

1. Select the **mod subfolder** (with `mod.xml`) → Next  
2. Pick a game / AppID → Next  
3. Choose upload method: **Steam client** (no password) or **steamcmd**  
4. New or update (Workshop ID) → title / description → preview  
5. **Upload** (steamcmd mode: login in UI; password / Guard in the steamcmd window)

Details: for **Steam-client / official KillHouse** upload the mod must be under **`mods`** or **`mods_upload`** (prefer `mods_upload`); pick the **mod subfolder** with `mod.xml`. In ModLoader, Steamworks/steamcmd `contentfolder` = that mod folder. Full paths & blue Upload arrow: **[FAQ.md](FAQ.md)**.

### Build & publish exe

```powershell
dotnet restore
dotnet test
dotnet publish ModLoader -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

Output: `./publish/win-x64/ModLoader.exe` (+ `steam_api64.dll`, `FAQ.md` beside it).

### How upload works

**Primary:** Steamworks against the running Steam client — **no password in ModLoader**, game is **not** launched.  
**Fallback:** generate VDF → `steamcmd +login … +workshop_build_item …` — game is **not** launched.  
**Alternative (official KillHouse):** put the mod under `mods` or `mods_upload` (prefer `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\<mod_name>\`) and use the blue Upload arrow in-game (see FAQ).

---

## Русский

### Требования

1. **Windows 10/11**
2. **Steam** (для режима клиента — запущен и выполнен вход)
3. **SteamCMD** — только если выбран путь steamcmd
4. Папка мода с **`mod.xml`** (Door Kickers 2 = AppID `1239080`)
5. Для сборки: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Как пользоваться

1. Выберите **подпапку мода** с `mod.xml` (не корень `mods` / `mods_upload`) → Далее  
2. Игра / AppID → способ загрузки: **Steam-клиент** или **steamcmd**  
3. Новый или обновление → название → превью → **Загрузить**  
   (в режиме steamcmd: логин в UI; пароль и Guard — в окне steamcmd)

Для **официального пути KillHouse / Steam-клиент** мод должен лежать в **`mods`** или **`mods_upload`** (лучше `mods_upload`). В приложении `contentfolder` = папка мода. Подробности: **[FAQ.md](FAQ.md)**. История: **[CHANGELOG.md](CHANGELOG.md)**.

### Сборка

```powershell
dotnet test
dotnet publish ModLoader -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

### Как устроена загрузка

**Основной путь:** Steamworks через запущенный Steam-клиент, **без пароля в UI**.  
**Запасной:** steamcmd + VDF, **без запуска игры**.  
**Официальная альтернатива KillHouse:** мод в `mods` или `mods_upload` (лучше `mods_upload`) + синяя стрелка Upload в игре (см. FAQ).
