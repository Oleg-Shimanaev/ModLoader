# ModLoader FAQ / Справка

## Русский

### Как публикует ModLoader?

Два способа (выбор в мастере — две крупные кнопки):

1. **Через Steam-клиент (без пароля)** — основной. ModLoader подключается к **уже запущенному Steam** через Steamworks и загружает мод тем же аккаунтом, в который вы вошли в клиенте. Пароль в ModLoader **не нужен**.
2. **Через steamcmd** — запасной. Отдельный логин: пароль и Steam Guard — **в окне steamcmd**. Сессию из Steam-клиента в steamcmd **передать нельзя** (поэтому бывает Invalid Password, если путать эти два входа).

Игра при обоих способах **не запускается**.

Нужны: запущенный Steam (для клиента — ещё вход в аккаунт), право публиковать в Workshop игры, AppID игры.

### Публикация через Steam-клиент (официальный путь KillHouse / Door Kickers 2)

Для upload **через Steam-клиент / игру** (официальный путь KillHouse) мод должен лежать в папке **`mods`** или **`mods_upload`**.

- **Предпочтительно `mods_upload`** (шаблон):  
  `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\<имя_мода>\`  
  (не храните единственную копию только в `mods_upload` внутри установки игры — слетит при апдейте).
- Допустимо и **`mods`**:  
  `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods\<имя_мода>\`
- Также встречается рядом с игрой (буква диска у каждого своя — в примерах её нет):  
  `steamapps\common\DoorKickers2\mods\<имя_мода>\`  
  `steamapps\common\DoorKickers2\mods_upload\<имя_мода>\`

**В мастере ModLoader и в проводнике выбирайте подпапку мода** с файлом **`mod.xml`**, а не корень `mods` / `mods_upload`.

В игре (официальный путь): меню Mods → у мода **синяя стрелка Upload**.

По шагам (`readme_how_to_mod.txt` в DK2):
1. Скопируйте папку мода (с `mod.xml`) в `mods_upload` (предпочтительно LocalAppData).
2. Откройте игру → Mods → синяя стрелка Upload.

| Папка | Назначение |
|-------|------------|
| `%LOCALAPPDATA%\…\mods\` | Свои моды для игры; для клиентского upload тоже подходит |
| `%LOCALAPPDATA%\…\mods_upload\` | Официально для upload в игре (предпочтительно) |
| `steamapps\common\DoorKickers2\mods\` / `mods_upload\` | Вариант рядом с установкой; в `mods` часто ещё шаблоны KillHouse |

### ModLoader: Steamworks и steamcmd (наш путь)

В приложении при загрузке через **Steamworks** или **steamcmd** поле **`contentfolder`** = **папка выбранного мода** (та, где `mod.xml`), не корень `mods`.

Для Door Kickers 2 при **клиентском / официальном** upload KillHouse по-прежнему ждут мод в **`mods`** или **`mods_upload`** (см. выше). Удобно держать мод там же и указывать эту подпапку в мастере.

### Превью

В шаблонах KillHouse превью часто называется **`mod_image.jpg`** рядом с `mod.xml`.  
ModLoader подставляет его автоматически, если файл есть.

### Новый мод vs обновление

- **Новый** — создаётся новый Workshop item.
- **Обновить** — Workshop ID из ссылки `...?id=XXXX`.

### steamcmd

Нужен файл **`steamcmd.exe`**.

**Уже ставили, но не знаете куда:**
1. В ModLoader нажмите **«Указать папку steamcmd…»** и выберите папку, где лежит `steamcmd.exe` (или папку уровнем выше — ищем и на 1 уровень вглубь).
2. Или в Проводнике: поиск по диску → `steamcmd.exe` → укажите эту папку в ModLoader.

**Автоустановка из ModLoader:** кнопка **«Скачать и установить steamcmd»**.  
Сначала спросит **куда установить** (по умолчанию `%LOCALAPPDATA%\ModLoader\steamcmd\`, можно выбрать другую папку) → скачает zip во temp → распакует → `steamcmd.exe +quit` → путь подставится сам.

**Автопоиск** смотрит типичные места: `%LOCALAPPDATA%\ModLoader\steamcmd\`, рядом с приложением, `%ProgramFiles%\SteamCMD\`, `%LOCALAPPDATA%\steamcmd\`, `%USERPROFILE%\steamcmd\`. Любой другой путь — только через **«Указать папку steamcmd…»**.

Вручную: [SteamCMD (Valve Wiki)](https://developer.valvesoftware.com/wiki/SteamCMD).  
При способе steamcmd: логин в UI; пароль и 2FA — только в окне steamcmd.

### Не достучались до Steam

Приложение **не молчит**: красный статус + окно «Не получилось» (часто с «Открыть FAQ?»).

| Ситуация | Что сделать |
|----------|-------------|
| Steam не запущен | Запустить Steam, войти в аккаунт |
| Init failed / клиент не отвечает | Перезапустить Steam; проверить интернет, VPN, firewall |
| Нет связи (таймаут) | Интернет / VPN / firewall; повторить |
| Нет `steam_api64.dll` | Переустановить ModLoader или выбрать путь **steamcmd** |

### steamcmd: консоль не открывается

| Ситуация | Что сделать |
|----------|-------------|
| `steamcmd.exe` не найден | «Указать папку…» или «Скачать и установить steamcmd» |
| Process.Start / консоль не открылась | Антивирус, путь, права; запустить `steamcmd.exe` вручную |
| Сразу закрылся | Антивирус/блокировка; см. также **http error 0** ниже |
| Таймаут без ответа | Сеть/антивирус; запуск вручную |

### steamcmd: Download failed http error 0

В логе bootstrap (`steamcmd +quit`) бывает:

`Download failed: http error 0` / `Package file … missing or incorrect size` / `Download of package … failed after 0 bytes`.

Это **не** ошибка путей ModLoader. Типичный сбой скачивания пакета с CDN Valve (сеть, firewall, прокси, антивирус, временный CDN).

Что попробовать:
1. Проверить интернет; отключить/сменить VPN; разрешить steamcmd в firewall и антивирусе.
2. Повторить позже (CDN Valve иногда «лежит»).
3. Запустить `steamcmd.exe` вручную из папки установки ещё раз — окно само докачает пакеты.
4. Запуск от администратора обычно **не** нужен, но иногда помогает.
5. Корпоративный прокси/фильтр может блокировать CDN — спросите у админов или попробуйте другую сеть.

### AppID

| Игра | AppID |
|------|-------|
| Door Kickers | 248610 |
| Door Kickers 2 | **1239080** |

### «Нет лицензии»

Игра не на аккаунте или неверный AppID. См. библиотеку Steam.

### FAQ рядом с exe

Файл `FAQ.md` лежит рядом с `ModLoader.exe`. В приложении — кнопка **FAQ**.

---

## English

### How ModLoader uploads

Two options in the wizard (two large buttons):

1. **Via Steam client (no password)** — primary. Uses Steamworks against the **running Steam client** (same logged-in account). No password in ModLoader.
2. **Via steamcmd** — fallback. Separate login; password / Steam Guard in the **steamcmd** window. You **cannot** pass the Steam client session into steamcmd.

The game process is **not** launched.

### Publishing via Steam client (official KillHouse / Door Kickers 2 path)

For upload **via Steam client / in-game** (official KillHouse path), the mod must live under **`mods`** or **`mods_upload`**.

- **Prefer `mods_upload`** (template):  
  `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\<mod_name>\`  
  (don’t keep the only copy inside the game install `mods_upload` — updates wipe it).
- Also fine under **`mods`**:  
  `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods\<mod_name>\`
- Also seen next to the game install (drive letter varies — omitted on purpose):  
  `steamapps\common\DoorKickers2\mods\<mod_name>\`  
  `steamapps\common\DoorKickers2\mods_upload\<mod_name>\`

**In ModLoader (and Explorer) pick the mod subfolder** that contains **`mod.xml`**, not the root `mods` / `mods_upload`.

In-game (official path): Mods menu → **blue Upload arrow**.

From DK2 `readme_how_to_mod.txt`:
1. Copy the mod folder (with `mod.xml`) into `mods_upload` (prefer LocalAppData).
2. Launch the game → Mods → blue Upload arrow.

| Folder | Role |
|--------|------|
| `%LOCALAPPDATA%\…\mods\` | Your mods for play; also OK for client upload |
| `%LOCALAPPDATA%\…\mods_upload\` | Official in-game upload location (preferred) |
| `steamapps\common\DoorKickers2\mods\` / `mods_upload\` | Beside install; `mods` often also has KillHouse templates |

### ModLoader: Steamworks and steamcmd (our path)

In the app, for **Steamworks** or **steamcmd**, **`contentfolder`** = the **selected mod folder** (the one with `mod.xml`), not the `mods` root.

For Door Kickers 2 **client / official** KillHouse upload, the game still expects the mod under **`mods`** or **`mods_upload`** (see above). Keep the mod there and point the wizard at that subfolder.

Preview file is often **`mod_image.jpg`**.

### steamcmd

Need **`steamcmd.exe`**.

**Already installed, path unknown:** use **“Указать папку steamcmd…”** (folder or parent — we look one level deep), or search Explorer for `steamcmd.exe`.

**In-app install:** **“Скачать и установить steamcmd”** → ask destination folder (default `%LOCALAPPDATA%\ModLoader\steamcmd\`) → temp zip → extract → `steamcmd +quit` → path saved.

Also auto-checks LocalAppData ModLoader folder, app folder, `%ProgramFiles%\SteamCMD\`, profile `steamcmd`. Any other location: use the folder picker.

### Can’t reach Steam

The app shows a **red status** + dialog (often with **Open FAQ?**).

| Case | What to do |
|------|------------|
| Steam not running | Start Steam and sign in |
| Init failed / client not responding | Restart Steam; check internet, VPN, firewall |
| Timeout / no connection | Network / VPN / firewall; retry |
| Missing `steam_api64.dll` | Reinstall ModLoader or use **steamcmd** path |

### steamcmd console won’t open

| Case | What to do |
|------|------------|
| `steamcmd.exe` not found | Pick folder or in-app install |
| Process.Start / no console | Antivirus, path, permissions; run `steamcmd.exe` manually |
| Exits immediately | Antivirus; also see **http error 0** |
| Timeout with no output | Network/AV; run manually |

### steamcmd: Download failed http error 0

Bootstrap log may show `http error 0` / package missing or incorrect size. This is a **CDN/network/firewall/proxy/AV** fail, not a ModLoader path bug. Retry later, check VPN/firewall/AV, run `steamcmd.exe` again from the install folder, try another network if behind a corporate proxy. Admin rights usually not required.

### AppID

Door Kickers 2 = **1239080**.
