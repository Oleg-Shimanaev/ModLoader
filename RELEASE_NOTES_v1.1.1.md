## English

**v1.1.1** — clearer alerts when Steam is unreachable or steamcmd console won’t open + FAQ for `mods` / `mods_upload`.

### Highlights
- Detect Steam not running / Init failed / timeout / missing Steamworks DLL → red status + «Open FAQ?»
- Detect steamcmd not found / Process.Start fail / immediate exit → clear RU message + FAQ
- FAQ: official KillHouse path — mod under **`mods`** or **`mods_upload`** (prefer `mods_upload`)

### Artifact
- `ModLoader-win-x64.zip` — extract and run `ModLoader.exe` (self-contained Windows x64; includes `steam_api64.dll`, `FAQ.md`; no shipped `steam_appid.txt`)

---

## Русский

**v1.1.1** — понятные оповещения, если до Steam не достучались или консоль steamcmd не открылась + FAQ про `mods` / `mods_upload`.

### Главное
- Steam не запущен / Init failed / таймаут / нет DLL → красный статус + «Открыть FAQ?»
- steamcmd не найден / не стартует / сразу закрылся → ясное сообщение + FAQ
- FAQ: официальный путь KillHouse — мод в **`mods`** или **`mods_upload`** (лучше `mods_upload`)

### Артефакт
- `ModLoader-win-x64.zip` — распаковать и запустить `ModLoader.exe` (self-contained Windows x64; рядом `steam_api64.dll`, `FAQ.md`; без тестового `steam_appid.txt`)
