# Changelog

All notable changes to this project are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
versioning: [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

---

## [1.1.0] — 2026-10-22

### Added / Добавлено

- **EN/RU:** Wizard step **«Как загрузить»** with two large buttons: **Steam client** (Steamworks, no password) vs **steamcmd** (login / password in steamcmd window).
- **EN:** Primary Workshop upload via **Steamworks** (`ISteamUGC` / Facepunch) against the running Steam client session.
- **RU:** Основная загрузка через **Steam-клиент** (Steamworks), без пароля в UI.
- **EN/RU:** Fallback **steamcmd** + VDF `workshop_build_item` (create and update). Game process is **not** launched.
- **EN/RU:** Bilingual `FAQ.md` + in-app **FAQ** button (tips removed from wizard screens).
- **EN/RU:** SteamCMD folder picker (**«Указать папку steamcmd…»**) — searches selected folder + 1 nested level for `steamcmd.exe`.
- **EN/RU:** In-app **«Скачать и установить steamcmd»**: ask destination folder first (default `%LOCALAPPDATA%\ModLoader\steamcmd\`) → zip to temp → extract → `steamcmd +quit` → auto-set path.
- **EN/RU:** Auto-locate steamcmd under LocalAppData ModLoader, app directory, Program Files\SteamCMD, profile `steamcmd` (via SpecialFolder only — no machine-specific drives).
- **EN/RU:** Steam login field only for steamcmd mode (password / Guard in steamcmd window).
- **EN/RU:** VDF fields: `appid`, `publishedfileid`, `contentfolder`, `previewfile`, `title`, `description`, `visibility`, `changenote`.
- **EN/RU:** Default folder picker candidates: `%LOCALAPPDATA%\…\mods_upload`, `%LOCALAPPDATA%\…\mods`, and `steamapps\common\DoorKickers2\mods` via Steam `libraryfolders.vdf` (no machine-specific paths in docs).
- **EN/RU:** Auto-detect `mod_image.jpg` as Workshop preview.
- **EN/RU:** FAQ: session cannot be passed from Steam client to steamcmd; use Steam client upload instead.
- **EN/RU:** FAQ documents official KillHouse in-game Upload (blue arrow / `mods_upload`) as alternative.
- **EN/RU:** FAQ section for steamcmd bootstrap **`http error 0`** (CDN/network/firewall); UI offers FAQ after failed install.

### Changed / Изменено

- **EN:** Replaced `-applaunch … -publish` as the main path (legacy kept off UI).
- **RU:** Вместо `-applaunch -publish` — Steamworks / steamcmd; legacy не в UI.
- **EN/RU:** Short wizard labels; long help → FAQ.
- **EN/RU:** Validator rejects selecting root `mods` / `mods_upload` (need mod subfolder with `mod.xml`).

### Fixed / Исправлено

- **EN/RU:** Door Kickers 2 AppID **1239080**.
- **EN/RU:** Steam-running check before upload.
- **EN/RU:** Update existing item by Workshop ID.
- **EN/RU:** Removed any machine-specific absolute paths from docs/examples.
- **EN/RU:** steamcmd bootstrap `http error 0` (CDN/network): detect from output, one retry with pause, short RU message + FAQ offer in UI; FAQ section added.

---

## [1.0.0] — 2026-10

### Added / Добавлено

- **EN:** First release: Windows wizard for Steam Workshop (Door Kickers–style `-publish`).
- **RU:** Первый релиз: мастер публикации через `-publish`.
- Known games, validation, preview, unit tests, self-contained `win-x64` publish.

[Unreleased]: https://github.com/Oleg-Shimanaev/ModLoader/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/Oleg-Shimanaev/ModLoader/releases/tag/v1.1.0
[1.0.0]: https://github.com/Oleg-Shimanaev/ModLoader/releases/tag/v1.0.0
