## English

**v1.1.0** — Steam Workshop upload via **Steam client (Steamworks)** or **steamcmd**, without launching the game.

### Highlights
- Wizard step to choose upload method: **Steam client** (no password) vs **steamcmd**
- Primary path: Steamworks against the running Steam session
- Fallback: VDF + `steamcmd +workshop_build_item` (create / update)
- Bilingual **FAQ** (in-app button) and steamcmd install / locate helpers
- Door Kickers 2 AppID **1239080**; validator requires a mod subfolder with `mod.xml`

### Artifact
- `ModLoader-win-x64.zip` — extract and run `ModLoader.exe` (self-contained Windows x64; includes `steam_api64.dll`, `FAQ.md`)

### Requirements
- Windows 10/11
- Steam (for client mode: running and logged in)
- SteamCMD only if you choose the steamcmd path
- Mod folder with `mod.xml`

---

## Русский

**v1.1.0** — загрузка в Steam Workshop через **Steam-клиент (Steamworks)** или **steamcmd**, без запуска игры.

### Главное
- Выбор способа: **Steam-клиент** (без пароля) или **steamcmd**
- Основной путь: Steamworks к уже запущенному Steam
- Запасной: VDF + `steamcmd +workshop_build_item` (создание / обновление)
- Двуязычный **FAQ** и помощники установки / поиска steamcmd
- AppID Door Kickers 2 **1239080**; нужна подпапка мода с `mod.xml`

### Артефакт
- `ModLoader-win-x64.zip` — распаковать и запустить `ModLoader.exe` (self-contained Windows x64; рядом `steam_api64.dll`, `FAQ.md`)

### Требования
- Windows 10/11
- Steam (для режима клиента — запущен и выполнен вход)
- SteamCMD — только для пути steamcmd
- Папка мода с `mod.xml`
