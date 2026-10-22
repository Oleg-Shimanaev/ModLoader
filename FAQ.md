# ModLoader FAQ / Справка

## Русский

### Как публикует ModLoader?

Два способа (выбор в мастере — две крупные кнопки):

1. **Через Steam-клиент (без пароля)** — основной. ModLoader подключается к **уже запущенному Steam** через Steamworks и загружает мод тем же аккаунтом, в который вы вошли в клиенте. Пароль в ModLoader **не нужен**.
2. **Через steamcmd** — запасной. Отдельный логин: пароль и Steam Guard — **в окне steamcmd**. Сессию из Steam-клиента в steamcmd **передать нельзя** (поэтому бывает Invalid Password, если путать эти два входа).

Игра при обоих способах **не запускается**.

Нужны: запущенный Steam (для клиента — ещё вход в аккаунт), право публиковать в Workshop игры, AppID игры.

### Официальный способ KillHouse (альтернатива)

По `readme_how_to_mod.txt` в Door Kickers 2:

1. Рабочая папка для upload: **`%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\`**  
   (не храните моды только в `mods_upload` внутри установки игры — слетит при апдейте).
2. Скопируйте **папку мода** (с `mod.xml`) в `mods_upload`.
3. Откройте игру → меню Mods → у мода **синяя стрелка Upload**.

### Куда класть мод и что выбирать в мастере

| Папка | Назначение |
|-------|------------|
| `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods\` | Установка/игра своих модов (не Steam Workshop upload) |
| `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\` | Официально для upload в игре |
| `steamapps\common\DoorKickers2\mods\` (внутри установки Steam) | Шаблоны KillHouse (ZIP, localization), обычно **не** ваши моды |

**В мастере выбирайте подпапку одного мода**, где лежит **`mod.xml`**.  
Не выбирайте корень `mods` или `mods_upload`.

Пример структуры:  
`%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods\<имя_мода>\mod.xml`

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

### Official KillHouse path (alternative)

From DK2 `readme_how_to_mod.txt`:

1. Prefer `%LOCALAPPDATA%\KillHouseGames\DoorKickers2\mods_upload\`
2. Copy the mod folder (with `mod.xml`) there
3. Launch the game → Mods → **blue Upload arrow**

### What folder to pick

Pick a **single mod subfolder** with `mod.xml`, not the root `mods` / `mods_upload`.  
Install `steamapps\common\DoorKickers2\mods` usually has templates only. User mods often live under LocalAppData `mods\<mod_name>\`.

Preview file is often **`mod_image.jpg`**.

### steamcmd

Need **`steamcmd.exe`**.

**Already installed, path unknown:** use **“Указать папку steamcmd…”** (folder or parent — we look one level deep), or search Explorer for `steamcmd.exe`.

**In-app install:** **“Скачать и установить steamcmd”** → ask destination folder (default `%LOCALAPPDATA%\ModLoader\steamcmd\`) → temp zip → extract → `steamcmd +quit` → path saved.

Also auto-checks LocalAppData ModLoader folder, app folder, `%ProgramFiles%\SteamCMD\`, profile `steamcmd`. Any other location: use the folder picker.

### steamcmd: Download failed http error 0

Bootstrap log may show `http error 0` / package missing or incorrect size. This is a **CDN/network/firewall/proxy/AV** fail, not a ModLoader path bug. Retry later, check VPN/firewall/AV, run `steamcmd.exe` again from the install folder, try another network if behind a corporate proxy. Admin rights usually not required.

### AppID

Door Kickers 2 = **1239080**.
