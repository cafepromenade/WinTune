# WinTune Roadmap · 路線圖

**EN —** The living, auto-growing backlog for the WinTune suite. The 30-minute build loop reads this file, **discovers new real features and appends them here**, then builds the next unchecked chunk depth-first and ticks it off. Every item lists its real mechanism (registry key, command, deep link, API, or engine to wrap).

**粵語 —** WinTune 套件嘅活動式、會自己生長嘅待辦清單。每 30 分鐘嘅建置迴圈會讀呢個檔、**發掘新嘅真實功能再加落嚟**，跟住深度建置下一個未剔嘅項目再剔走。每項都附上真實機制（登錄檔、指令、深層連結、API 或者要包嘅引擎）。

## ✅ Done · 已完成
- [x] **Windows 11 control module** · Windows 11 控制模組 — 169 tweaks / 13 categories
- [x] **Git & GitHub module** · Git 與 GitHub 模組 — repo ops, chunked uploader, 111 git/gh operations
- [x] **Maintenance & Diagnostics module** · 維護與診斷模組 — 102 real ops (services, disk health, SFC/DISM, drivers, updates, event logs, power reports)
- [x] **Archives module** · 壓縮檔模組 — 7-Zip create/extract/list/test/benchmark + 100 advanced operations
- [x] **Developer & Terminal module** · 開發與終端機模組 — 100 ops (winget/docker/runtimes/env·ports/CLIs)
- [x] **Browser Control module** · 瀏覽器控制模組 — 100 ops (Chrome/Edge launch·flags·policies·profiles)
- [x] **Encryption & Vault module** · 加密與保險庫模組 — 100 ops (BitLocker/VeraCrypt/EFS/certs/Defender)
- [x] **Windows 11 Advanced module** · Windows 11 進階模組 — 100 ops (input/storage/perf/Explorer/Settings links)
- [x] **Network Pro module** · 網絡進階模組 — 100 ops (adapters/IP·DNS/Wi-Fi/firewall/diagnostics)
- [x] **Recipes (one-click)** · 一鍵流程 — 12 multi-step bundles (cleanup/privacy/gaming/dev/network/perf/…)
- [x] **Per-feature Markdown docs** · 逐項功能 Markdown 文件 — `--export-docs` writes one .md per feature (997 files)
- [x] **Settings import/export** · 設定匯入／匯出 — export/import WinTune settings as JSON (Settings page)

## 🎯 Requested big features (queued) · 已點名嘅大功能（排緊隊）
- [ ] **Settings & Control Panel hub** · 設定與控制台總匯 — bilingual searchable launcher for every `ms-settings:` page + every Control Panel applet (`control /name`, `*.cpl`).
- [x] **Interactive Registry Editor** · 互動式登錄編輯器 — bespoke in-app tree browser + value view/add/edit/delete over RegistryHelper, bilingual. (no regedit.exe redirect)

> **Design rule (user directive):** everything must run **purely in-app, NOT as redirects** to external
> Windows UI. Replace "Open X.msc / start ms-settings: / launch editor" features with native in-app
> equivalents (in-app services manager, device list, scheduled-tasks list, settings toggles, etc.).
> **設計守則（使用者指示）：** 所有嘢要**純粹喺 app 內**做，唔好跳去外部 Windows 介面。將「開 X.msc／
> start ms-settings:／叫出編輯器」嘅功能，換成 app 內嘅原生等價物。
- [ ] **Forum pain-points** · 論壇痛點 — features people complain about (from /deepresearch), appended here then built.

## 🔭 Discovered backlog · 發掘待辦（175 items / 項）

### Windows 11  (13)
- [ ] **Disable Wallpaper JPEG Compression (Import Quality 100)** · 熄咗桌布JPEG壓縮(質素調到最高100)
  - _HKCU\Control Panel\Desktop -> DWORD JPEGImportQuality = 100 (valid 60-100; default behaviour ~85). Disables Windows' automatic recompression of JPG wallpapers. After writing, re-apply the image via SystemParametersInfo SPI_SETDESKWALLPAPER (or re-set the wallpaper) so the new quality takes effect. PNG/BMP wallpapers are unaffected._
- [ ] **Enable Verbose Startup/Shutdown Status Messages** · 開啟開關機詳細狀態訊息
  - _HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System -> DWORD VerboseStatus = 1. Shows detailed 'Applying settings / Stopping service X' text during boot and shutdown instead of the spinner. Requires admin (HKLM)._
- [ ] **Toggle Cloud Clipboard Sync Across Devices** · 開定熄雲端剪貼簿跨裝置同步
  - _HKCU\Software\Microsoft\Clipboard -> DWORD EnableClipboardHistory = 1 (Win+V history) and CloudClipboardAutomaticUpload = 1 (roam via Microsoft account). Deep link ms-settings:clipboard._
- [ ] **Configure Storage Sense Cadence & Recycle Bin Purge** · 設定儲存感知清理週期同回收筒清空
  - _HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy -> DWORDs: 01 (enable)=1, 2048 (run cadence: 0=low disk space,1=daily,7=weekly,30=monthly), 256 (Recycle Bin retention days: 0/1/14/30/60), 512 (Downloads retention days). Deep link ms-settings:storagesense._
- [ ] **Disable Mouse Pointer Acceleration (Enhance Pointer Precision)** · 熄咗滑鼠加速(精準指標)
  - _HKCU\Control Panel\Mouse -> string MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0 unchecks 'Enhance pointer precision' (mouse acceleration). Apply live via SystemParametersInfo SPI_SETMOUSE with the 3-element accel array. Deep link ms-settings:mousetouchpad._
- [ ] **Set Keyboard Repeat Delay & Repeat Rate to Fastest** · 將鍵盤重複延遲同速率調到最快
  - _HKCU\Control Panel\Keyboard -> string KeyboardDelay=0 (0-3, 0=shortest) and KeyboardSpeed=31 (0-31, 31=fastest). Apply live via SystemParametersInfo SPI_SETKEYBOARDDELAY / SPI_SETKEYBOARDSPEED. Deep link ms-settings:easeofaccess-keyboard._
- [ ] **Enable Filter Keys / Slow Keys for Accessibility** · 開啟篩選鍵(慢速鍵)輔助功能
  - _HKCU\Control Panel\Accessibility\Keyboard Response -> string Flags (e.g. 27 off / 59 on) plus DWORDs DelayBeforeAcceptance, AutoRepeatDelay, AutoRepeatRate, BounceTime (ms). Apply live via SystemParametersInfo SPI_SETFILTERKEYS. Deep link ms-settings:easeofaccess-keyboard._
- [ ] **Export / Import Default App Associations (machine-wide)** · 匯出匯入預設程式關聯
  - _Wrap DISM: 'dism /Online /Export-DefaultAppAssociations:C:\assoc.xml' to capture current defaults, and 'dism /Online /Import-DefaultAppAssociations:C:\assoc.xml' (admin) to apply the template for new users/login. Per-user single-extension changes need the protected UserChoice hash, so wrap SetUserFTA.exe or open ms-settings:defaultapps for manual confirmation._
- [ ] **Toggle Notifications / Configure Focus & Quiet-Hours Rules** · 開熄通知同設定專注模式(勿擾)規則
  - _Global toasts: HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings -> DWORD NOC_GLOBAL_SETTING_TOASTS_ENABLED = 0/1. Per-app toasts under ...\Settings\<AppUserModelID> -> Enabled. Focus session / automatic quiet-hours rules are configured via ms-settings:quiethours (Focus assist); deep link ms-settings:notifications._
- [ ] **Tune Snap Assist & Snap Layout Behavior** · 調整貼齊版面(Snap)行為
  - _HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced -> DWORDs SnapAssist, EnableSnapAssistFlyout, SnapFill, JointResize (1/0); HKCU\Control Panel\Desktop -> DWORD WindowArrangementActive ('1'/'0' string) master toggle. Deep link ms-settings:multitasking._
- [ ] **Change Regional First Day of Week & Short Date Format** · 改地區設定一週起始日同短日期格式
  - _HKCU\Control Panel\International -> strings iFirstDayOfWeek (0=Mon..6=Sun), sShortDate (e.g. yyyy-MM-dd), sLongDate, sShortTime. PowerShell Set-Culture / Set-WinHomeLocation for locale. Deep link ms-settings:regionformatting._
- [ ] **Enable 'End Task' on Taskbar Right-Click** · 開啟工作列右掣直接結束工作
  - _HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings -> DWORD TaskbarEndTask = 1 (Win11 22631.2715+). Adds 'End Task' to taskbar app context menus (kills the process tree). Restart explorer.exe to apply._
- [ ] **Restore Classic (Win10) Right-Click Context Menu in Explorer** · 還原傳統完整右掣選單
  - _reg add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve (empty default value), then restart explorer.exe. Masks the Win11 compact-menu COM object so the classic full context menu shows directly. Delete the CLSID key to revert._

### Encryption · 🆕 new module / 新模組  (15)
- [ ] **Create Volume (silent, scripted)** · 靜雞雞整個加密磁碟區
  - _"VeraCrypt Format.exe" /create <path> /size <e.g. 200M|max> /encryption <AES|Serpent|Twofish|Camellia|Kuznyechik|AES(Twofish)...> /hash <sha-512|sha-256|whirlpool|blake2s|streebog> /filesystem <NTFS|exFAT|FAT|ReFS|None> /pim 0 /dynamic /quick /silent — drives the VeraCrypt Format.exe wizard binary to build a file container with no UI; password supplied at runtime via /password, never stored._
- [ ] **Mount All Favorites on demand** · 一次過開晒啲常用磁碟區
  - _VeraCrypt.exe /a favorites /q (optionally /m ro for read-only). /a favorites is the documented batch-mount switch; it mounts every entry in Configuration\Favorite Volumes.xml under %APPDATA%\VeraCrypt. Confirmed against VeraCrypt Windows command-line docs._
- [ ] **Mount with keyfile + PIM** · 用鎖匙檔加 PIM 嚟開
  - _VeraCrypt.exe /v <volume> /l <driveLetter> /k <keyfilePathOrFolder> /pim <n> /m ro /q /silent — /k passes a keyfile (or a folder of keyfiles), /pim sets the Personal Iterations Multiplier; add /tokenlib <pkcs11.dll> for security-token keyfiles._
- [ ] **Dismount all / force dismount** · 全部熄晒，鎖死都照熄
  - _VeraCrypt.exe /d /q dismounts every mounted volume (omit the drive letter = all); add /force (/f) to dismount even when an app is holding the volume open. Confirmed against VeraCrypt Windows command-line docs and the Normal vs Force Dismount page._
- [ ] **Change volume password / PIM** · 改咗加密磁碟區嘅密碼
  - _VeraCrypt.exe has no headless change-password switch, so wrap it as a GUI deep action: launch VeraCrypt.exe /v <volume> and surface Volume > Change Volume Password. BitLocker equivalent is fully headless: manage-bde -changepassword <drive:>._
- [ ] **Backup volume header (raw copy)** · 備份磁碟區個 header
  - _Header backup is GUI-only (Tools > Backup Volume Header) — wrap by launching VeraCrypt.exe to that tool. As a true CLI fallback, snapshot the first 131072 bytes of the container with PowerShell [IO.File] read/write (captures the embedded primary header); label clearly as a raw header copy, not VeraCrypt's official backup-header format._
- [ ] **BitLocker: pause / resume encryption** · 暫停咗 / 繼續 BitLocker 加密
  - _manage-bde -pause <drive:> and manage-bde -resume <drive:> — pause/resume an in-progress encrypt or decrypt sweep without changing protection state. Documented manage-bde subcommands._
- [ ] **BitLocker: backup recovery key to AD / Entra ID** · BitLocker 救命金鎖匙備份咗去網域
  - _manage-bde -protectors -get <drive:> to read the numerical recovery password + its key-protector ID, then manage-bde -protectors -adbackup <drive:> -id {GUID} for Active Directory, or -aadbackup for Microsoft Entra ID. PowerShell equivalents: Backup-BitLockerKeyProtector. Confirmed against manage-bde -protectors docs._
- [ ] **BitLocker: force recovery on next boot** · 下次開機逼出 BitLocker 救命畫面
  - _manage-bde -forcerecovery <drive:> — sets the volume to demand the recovery key at next boot (useful before firmware/TPM changes). Real and not undoable until you supply the key; gate behind explicit confirmation._
- [ ] **BitLocker: repair a damaged encrypted drive** · 救返爛咗嘅 BitLocker 磁碟
  - _repair-bde <inputVolume> <outputVolumeOrImage> [-rp <48-digit recovery password> | -rk <key.bek> | -kp <keyPackageFile>] [-lf <log>] [-f] — reconstructs and decrypts salvageable data from a corrupted BitLocker volume to a clean target (the -kp key package comes from AD DS / the BitLocker Recovery Password Viewer). Confirmed against the Microsoft repair-bde reference. Output is overwritten, so require a separate empty target and confirmation._
- [ ] **EFS: wipe free space (cipher /w)** · 抹乾淨啲冇用嘅空間
  - _cipher /w:<directory|drive> — overwrites all unallocated/slack space on the hosting volume (zeros, then 0xFF, then random), erasing remnants of deleted plaintext. Per Microsoft docs, all other cipher params are ignored when /w is used._
- [ ] **EFS: backup encryption certificate + key** · 備份 EFS 證書同私鑰
  - _cipher /x[:<efsFile>] <outFile.pfx> — exports the user's EFS certificate and private key to a .pfx so encrypted files stay recoverable after a profile/OS reinstall; cipher /y prints the current EFS cert thumbprint. Confirmed against the Microsoft cipher reference._
- [ ] **EFS: generate Data Recovery Agent cert** · 整個 EFS 救援代理證書
  - _cipher /r:<fileName> [/smartcard] — generates an EFS recovery-agent key pair, writing <fileName>.cer (public cert) and <fileName>.pfx (cert + private key); the .cer is then added to the EFS recovery policy so an admin can recover any user's files. Confirmed against the Microsoft cipher reference._
- [ ] **Encrypt folder with EFS (per-file)** · 用 EFS 加密成個資料夾
  - _cipher /e /s:<folder> marks a tree for EFS encryption (new files inherit); cipher /d /s:<folder> decrypts. Per-file via the FILE_ATTRIBUTE_ENCRYPTED flag (PowerShell (Get-Item x).Encrypt() / Win32 EncryptFile). Distinct from container/volume encryption._
- [ ] **Defender: toggle Attack Surface Reduction rules** · 開熄 Defender 攻擊面收窄規則
  - _Add-MpPreference -AttackSurfaceReductionRules_Ids <ruleGUID> -AttackSurfaceReductionRules_Actions <Enabled|AuditMode|Disabled|Warn>; read current state via (Get-MpPreference).AttackSurfaceReductionRules_Ids/_Actions. Resolve human-readable rule names (e.g. 'Block credential stealing from LSASS') from Microsoft's ASR reference at runtime rather than hardcoding GUIDs._

### ViveTool · 🆕 new module / 新模組  (15)
- [ ] **Feature flag searchbar (query all states)** · 搵功能旗仔嘅搜尋框（查晒狀態）
  - _Wrap ViVeTool.exe /query to dump the local Feature Store (every feature ID with State, Priority, Type=Experiment/Override) into a searchable list; pair with a bundled community feature-name dictionary so users search by human-readable name. The live store is the source of truth - no fabricated IDs._
- [ ] **Enable feature by human-readable name** · 用睇得明嘅名開功能
  - _ViVeTool.exe /enable /id:<id> where <id> is resolved at runtime by matching the user-picked human name (e.g. 'File Explorer tabs', 'New Start menu') against the on-disk dictionary. IDs must be confirmed against the running build at runtime - never hard-coded; show the resolved numeric id before applying. Requires admin + reboot._
- [ ] **Disable / reset a feature flag** · 熄咗或者重設功能旗仔
  - _ViVeTool.exe /disable /id:<id> to force-off, or ViVeTool.exe /reset /id:<id> to clear the override back to the Windows default state. Both take the runtime-resolved id. Reboot to apply._
- [ ] **Full reset (clear all overrides)** · 全部重設（清晒自己改嘅嘢）
  - _ViVeTool.exe /fullreset removes every custom feature configuration and returns the Feature Store to defaults. Gate behind an explicit confirm dialog since it wipes all prior toggles; reboot after._
- [ ] **Export / import flag profiles** · 匯出同匯入旗仔設定檔
  - _ViVeTool.exe /export <file> writes current custom configurations to a file; ViVeTool.exe /import <file> applies a saved profile (both are real verbs in the thebookisclosed/ViVe command reference). Lets a power user snapshot a known-good toggle set and restore or share it across machines._
- [ ] **Show Last Known Good rollback status** · 睇 Last Known Good 回滾狀態
  - _ViVeTool.exe /lkgstatus prints the current 'Last Known Good' rollback system status, so a power user can tell whether Windows has armed a safe-config rollback before applying experimental flags. Read-only; surface the raw status in the UI._
- [ ] **Toggle File Explorer tabs / duplicate tab** · 開檔案總管分頁同複製分頁
  - _ViVeTool.exe /enable /id:<id> for the 'File Explorer tabs' experiment; the 'tab bar' / 'duplicate tab' sub-features have their own ids. All ids must be queried at runtime against your build - resolve by human name from the dictionary, do not assume historical values._
- [ ] **Toggle new Start menu (scrollable, categories, Phone panel)** · 開新版開始選單（捲動、分類、手機側欄）
  - _ViVeTool.exe /enable /id:<id>[,<id>...] for the 'New Start menu redesign' (single scrollable surface, category grid, right-side Phone Link panel). It is a multi-ID feature group whose members vary by build - enumerate via /query and resolve by human name at runtime; never hard-code the set._
- [ ] **Toggle modern context menus / command bar** · 開新式右鍵選單
  - _ViVeTool.exe /enable /id:<id> for the Windows 11 'modern context menu' experiment and related command-bar surfaces. Resolve id by human-readable name at runtime; restart explorer.exe to apply._
- [ ] **Toggle taskbar 'End Task' on right-click** · 開工作列右鍵嘅「結束工作」
  - _ViVeTool.exe /enable /id:<id> for the taskbar 'End Task' experiment (mirrors the developer setting). Once enabled, the toggle lives at ms-settings:developers. Id confirmed at runtime by name._
- [ ] **Toggle seconds in the system clock** · 開系統時鐘嘅秒數
  - _ViVeTool.exe /enable /id:<id> for the 'show seconds in clock / Notification Center' experiment on builds where the ShowSecondsInSystemClock registry route is not honored. Resolve id by human name at runtime; reboot._
- [ ] **Toggle new Snap Layouts / suggested groupings** · 開新版貼齊版面同建議排版
  - _ViVeTool.exe /enable /id:<id> for the updated 'Snap Layouts' / suggested-snap-groups experiment (enhanced flyout, drag-to-top bar). Id resolved at runtime by human-readable name from the dictionary._
- [ ] **Toggle desktop / always-on Energy Saver** · 開枱機版慳電模式
  - _ViVeTool.exe /enable /id:<id> for the 'Energy Saver' experiment that surfaces the desktop/always-on toggle and Quick Settings tile. Resolve id by human name at runtime; complements ms-settings:batterysaver._
- [ ] **Toggle AI actions / Click to Do surfaces** · 開 AI 動作（Click to Do）功能
  - _ViVeTool.exe /enable /id:<id>[,<id>...] for the 'AI actions in File Explorer / Click to Do' experiment group. Multi-ID and build-specific - enumerate and resolve by human name at runtime. Note: some surfaces are also server-gated, so a local toggle may not fully light up._
- [ ] **Scan available-but-disabled experiments + restart-explorer helper** · 掃描未開嘅實驗，順手重啟 explorer
  - _Diff ViVeTool.exe /query output against the bundled name dictionary to surface features present on THIS build but sitting at Default/Disabled ('available to try'); after any /enable|/disable|/reset offer a soft apply via 'taskkill /f /im explorer.exe && start explorer.exe' for shell-only features or 'shutdown /r /t 0' for store-level ones. User confirms - no destructive default._

### Media · 🆕 new module / 新模組  (15)
- [ ] **Normalize loudness to broadcast standard (EBU R128)** · 校正音量去廣播標準（EBU R128）
  - _Two-pass ffmpeg loudnorm. Pass 1 measures: ffmpeg -i in.mp4 -af loudnorm=I=-16:TP=-1.5:LRA=11:print_format=json -f null - ; parse measured_I/measured_TP/measured_LRA/measured_thresh from stderr JSON, then pass 2: ffmpeg -i in.mp4 -af loudnorm=I=-16:TP=-1.5:LRA=11:measured_I=...:measured_TP=...:measured_LRA=...:measured_thresh=...:linear=true -c:v copy out.mp4 (all measured_* params + print_format confirmed present in this build)_
- [ ] **Auto-trim silence from start/end and gaps** · 自動剪走頭尾同中間嘅靜音
  - _ffmpeg -i in.mp3 -af silenceremove=start_periods=1:start_silence=0.1:start_threshold=-50dB:stop_periods=-1:stop_silence=0.3:stop_threshold=-50dB:detection=peak out.mp3 (silenceremove filter + all listed options confirmed in this build)_
- [ ] **Make high-quality GIF (two-pass palette)** · 整靚 GIF（兩步調色板）
  - _Two-pass palettegen/paletteuse for clean colors. Pass1: ffmpeg -i in.mp4 -vf "fps=15,scale=480:-1:flags=lanczos,palettegen=stats_mode=diff" palette.png ; Pass2: ffmpeg -i in.mp4 -i palette.png -lavfi "fps=15,scale=480:-1:flags=lanczos[x];[x][1:v]paletteuse=dither=bayer:bayer_scale=3" out.gif_
- [ ] **Stabilize shaky video (vidstab two-pass)** · 整定鏡頭、減震（vidstab 兩步）
  - _Two-pass libvidstab (vidstabdetect/vidstabtransform confirmed). Pass1 detect: ffmpeg -i in.mp4 -vf vidstabdetect=shakiness=8:accuracy=15:result=transforms.trf -f null - ; Pass2 transform: ffmpeg -i in.mp4 -vf vidstabtransform=input=transforms.trf:smoothing=30:zoom=0,unsharp=5:5:0.8:3:3:0.4 -c:a copy out.mp4_
- [ ] **Auto-detect and crop black bars** · 自動偵測、剷走黑邊
  - _ffmpeg -ss 60 -i in.mp4 -vframes 200 -vf cropdetect=round=2 -f null - to read the suggested crop=w:h:x:y from stderr, then ffmpeg -i in.mp4 -vf crop=w:h:x:y -c:a copy out.mp4 (cropdetect + round option confirmed)_
- [ ] **Lossless cut on keyframes (no re-encode)** · 唔重新編碼、喺關鍵幀剪片
  - _Stream-copy trim: ffmpeg -ss 00:01:30 -to 00:02:45 -i in.mp4 -c copy -avoid_negative_ts make_zero out.mp4 (instant, no quality loss; snaps to nearest keyframe). Pair with ffprobe -select_streams v -show_frames -show_entries frame=pts_time,key_frame to list keyframes._
- [ ] **Concat / join clips without re-encoding** · 唔重新編碼咁駁埋幾段片
  - _concat demuxer: write list.txt with lines like file 'C:/clip1.mp4' then ffmpeg -f concat -safe 0 -i list.txt -c copy out.mp4 (requires same codec/params; fall back to concat filter ffmpeg -i a -i b -filter_complex "[0:v][0:a][1:v][1:a]concat=n=2:v=1:a=1" when they differ)_
- [ ] **GPU hardware encode with NVENC** · 用顯示卡硬件編碼（NVENC）
  - _ffmpeg -i in.mp4 -c:v hevc_nvenc -preset p5 -tune hq -rc vbr -cq 26 -b:v 0 -c:a copy out.mp4 (h264_nvenc / hevc_nvenc / av1_nvenc all confirmed present in this build). Gate on detecting an NVIDIA GPU first._
- [ ] **Two-pass target-size encode (Discord/email cap)** · 兩步壓到指定大細（夾返上限）
  - _Compute video bitrate V = (targetMB*8388.608/durationSec) - audioKbps, then x264 two-pass on Windows: pass1 ffmpeg -y -i in.mp4 -c:v libx264 -b:v {V}k -pass 1 -an -f mp4 NUL && pass2 ffmpeg -i in.mp4 -c:v libx264 -b:v {V}k -pass 2 -c:a aac -b:a {A}k out.mp4 (duration from ffprobe -show_entries format=duration; note audio is -b:a not -b:v)_
- [ ] **Burn-in or soft-mux subtitles (SRT/ASS)** · 燒字幕入畫面 或 軟掛字幕（SRT/ASS）
  - _Burn-in (hardsub): ffmpeg -i in.mp4 -vf "subtitles='subs.srt':force_style='FontName=Microsoft JhengHei,FontSize=22'" out.mp4 (libass via subtitles filter, confirmed). Soft-mux (toggleable): ffmpeg -i in.mp4 -i subs.srt -c copy -c:s mov_text -metadata:s:s:0 language=yue out.mp4 (mov_text encoder confirmed)_
- [ ] **Extract chapters and split video by chapter** · 抽章節、按章節分割片段
  - _List: ffprobe -i in.mkv -show_chapters -print_format json (reads chapter start/end times). Split each: ffmpeg -i in.mkv -ss {start} -to {end} -c copy "Chapter NN.mkv" per chapter entry._
- [ ] **Contact sheet / storyboard thumbnails** · 整縮圖總表（storyboard）
  - _ffmpeg -i in.mp4 -vf "select='not(mod(n\,300))',scale=320:-1,tile=4x5" -frames:v 1 -qscale:v 3 contact_sheet.jpg (select + tile filters confirmed). Or one representative frame per scene with the thumbnail filter._
- [ ] **Convert HEIC/JPEG-XL photos to JPG/PNG (batch)** · 批次轉 HEIC/JXL 相做 JPG/PNG
  - _This build has libjxl decoder + hevc decoders. Per file: ffmpeg -i photo.heic -frames:v 1 -q:v 2 photo.jpg ; ffmpeg -i photo.jxl out.png . Loop a folder in PowerShell over *.heic/*.jxl. (ImageMagick magick mogrify -format jpg *.heic as alternative if installed.)_
- [ ] **Strip EXIF/GPS metadata from photos** · 洗走相片 EXIF／GPS 資料
  - _ffmpeg -i in.jpg -map_metadata -1 -c:v copy clean.jpg (drops EXIF/GPS without re-encoding the JPEG). For full ICC/XMP scrub use ImageMagick magick in.jpg -strip clean.jpg if present._
- [ ] **Make animated WebP from video (smaller than GIF)** · 由片整動態 WebP（細過 GIF）
  - _ffmpeg -i in.mp4 -vf "fps=20,scale=600:-1:flags=lanczos" -c:v libwebp_anim -lossless 0 -q:v 70 -loop 0 -an out.webp (libwebp_anim encoder confirmed in this build)_

### Maintenance · 🆕 new module / 新模組  (15)
- [ ] **Services Manager (start/stop/startup type)** · 服務管理員（開／熄、改自動或手動）
  - _Wrap sc.exe / PowerShell: list via 'Get-Service' or 'sc.exe query type= service state= all'; control via 'sc.exe start <name>' / 'sc.exe stop <name>'; change startup type via 'sc.exe config <name> start= auto|demand|disabled|delayed-auto'; read current config with 'sc.exe qc <name>'. Elevate via existing no-UAC scheduled-task launcher._
- [ ] **SMART / disk health & wear counters** · 睇硬碟健康同損耗狀態
  - _PowerShell Storage cmdlets: 'Get-PhysicalDisk | Get-StorageReliabilityCounter' for Wear, ReadErrorsTotal, Temperature, PowerOnHours; 'Get-PhysicalDisk | Select FriendlyName,HealthStatus,OperationalStatus,MediaType'. Get-StorageReliabilityCounter confirmed present. Requires elevation. Fallback: 'wmic diskdrive get model,status'._
- [ ] **Retrim SSD / optimize drives (TRIM)** · 幫SSD做TRIM、整理返隻碟
  - _Wrap defrag.exe / Optimize-Volume (ReTrim/Analyze/Defrag params confirmed present): 'defrag C: /L' issues retrim/TRIM on SSDs; 'defrag C: /O' picks the right op per media; 'defrag C: /A' analyzes only. PowerShell: 'Optimize-Volume -DriveLetter C -ReTrim -Verbose'. Requires elevation._
- [ ] **Create / list restore points** · 整還原點同睇返舊嘅
  - _PowerShell (Enable-ComputerRestore/Checkpoint-Computer/Get-ComputerRestorePoint confirmed present): 'Enable-ComputerRestore -Drive "C:\"', then 'Checkpoint-Computer -Description "WinTune" -RestorePointType MODIFY_SETTINGS'; list with 'Get-ComputerRestorePoint'. Resize cache via 'vssadmin Resize ShadowStorage'. The 24h gate is the DWORD 'SystemRestorePointCreationFrequency' under HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore (set 0 to lift). Requires elevation._
- [ ] **Pause / resume Windows Update** · 暫停咗或繼續Windows更新
  - _Registry HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings: set 'PauseUpdatesExpiryTime' (REG_SZ, ISO8601 UTC) plus 'PauseFeatureUpdatesStartTime'/'PauseFeatureUpdatesEndTime' and 'PauseQualityUpdatesStartTime'/'PauseQualityUpdatesEndTime' to pause; delete those values to resume. These values are absent until first paused. Trigger scan/download/install via 'UsoClient.exe StartScan' / 'StartDownload' / 'StartInstall' (UsoClient.exe confirmed present)._
- [ ] **Driver list / export / rollback hints** · 睇驅動程式、匯出同提示點回復
  - _Wrap pnputil.exe (subcommands confirmed from /?): 'pnputil /enum-drivers' lists third-party oem*.inf packages with provider/date/version; 'pnputil /export-driver <oem#.inf|*> <dir>' backs them up; 'pnputil /delete-driver <oem#.inf> /uninstall' removes a bad package (rollback = re-add the exported prior package). Device-level: 'pnputil /enum-devices', 'pnputil /restart-device'. Requires elevation._
- [ ] **Scheduled-task browser (query / run / disable)** · 排程工作清單（執行、停咗佢）
  - _Wrap schtasks.exe (Query/Run/End/Change confirmed from /?): 'schtasks /Query /FO LIST /V' lists all with Last/Next-Run-Time and Status; 'schtasks /Run /TN <name>' runs on demand; 'schtasks /End /TN <name>' stops; 'schtasks /Change /TN <name> /DISABLE' or '/ENABLE' toggles. PowerShell alt: Get-ScheduledTask / Get-ScheduledTaskInfo._
- [ ] **Event-log error/warning digest** · 事件記錄錯誤摘要
  - _Wrap wevtutil.exe (confirmed at System32) with XPath: 'wevtutil qe System /q:"*[System[(Level=1 or Level=2) and TimeCreated[timediff(@SystemTime)<=86400000]]]" /f:text /c:50 /rd:true'; repeat for Application. Boot/shutdown durations from Microsoft-Windows-Diagnostics-Performance/Operational (EventIDs 100-110). PowerShell alt: 'Get-WinEvent -FilterHashtable @{LogName='System';Level=1,2;StartTime=(Get-Date).AddDays(-1)}'._
- [ ] **Startup impact / autoruns audit** · 開機啟動項影響分析
  - _Enumerate Run keys under HKLM/HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run(+RunOnce) and the StartupApproved\Run blob (Explorer\StartupApproved\Run confirmed present; first byte 0x02/0x03 = enabled, 0x03 with high bytes = disabled per Task Manager); also list shell:startup and shell:common startup folders. Toggle by writing the StartupApproved binary value. Read-only inventory via 'Get-CimInstance Win32_StartupCommand' (confirmed working)._
- [ ] **Generate energy / battery / sleep report** · 出電池同耗電報告
  - _Wrap powercfg.exe: 'powercfg /energy /output <path>.html /duration 60' (efficiency problems); 'powercfg /batteryreport /output <path>.html' (capacity history & cycle estimate); 'powercfg /sleepstudy /output <path>.html' (modern-standby drain). Open the resulting HTML in-app. Requires elevation._
- [ ] **Diagnose what blocks sleep / wakes the PC** · 搵乜嘢阻住部機瞓覺、又整醒佢
  - _Wrap powercfg.exe (all confirmed real): 'powercfg /requests' shows active power requests blocking sleep per process/driver; 'powercfg /lastwake' shows the last wake source; 'powercfg /waketimers' enumerates scheduled wake timers; 'powercfg /devicequery wake_armed' lists wake-capable devices (confirmed returns list) — disarm with 'powercfg /devicedisablewake "<name>"'. /requests and /waketimers require elevation._
- [ ] **Component store cleanup (WinSxS / ResetBase)** · 清理元件儲存（WinSxS）
  - _Wrap DISM (Dism.exe confirmed at System32): 'Dism /Online /Cleanup-Image /AnalyzeComponentStore' reports reclaimable size & recommendation; 'Dism /Online /Cleanup-Image /StartComponentCleanup' trims superseded components; add '/ResetBase' to also drop superseded update backups (surface the warning that this blocks uninstalling installed updates). Requires elevation._
- [ ] **System file & image integrity repair** · 修復系統檔同系統映像
  - _Wrap built-ins (sfc.exe & Dism.exe confirmed present): 'sfc /scannow' verifies/repairs protected system files; 'Dism /Online /Cleanup-Image /CheckHealth' quick-checks, '/ScanHealth' deep-scans, '/RestoreHealth' repairs from Windows Update. Parse %WINDIR%\Logs\CBS\CBS.log for sfc results. Requires elevation._
- [ ] **Bulk update all apps (winget upgrade)** · 一次過更新晒啲App
  - _Wrap winget (v1.28 confirmed): 'winget upgrade' lists upgradable packages; 'winget upgrade --all --include-unknown --accept-source-agreements --accept-package-agreements' updates everything; per-app 'winget upgrade --id <Pkg.Id>'. Pin volatile apps via 'winget pin add --id <Pkg.Id>' (winget pin sub-command confirmed present)._
- [ ] **Reset / re-register a stuck Store app** · 重設或重新註冊死咗嘅Store App
  - _PowerShell Appx: re-register a broken package via 'Get-AppxPackage <name> | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register "$($_.InstallLocation)\AppXManifest.xml"}'; list with 'Get-AppxPackage -AllUsers'. Soft reset = the documented Reset button reached by the 'ms-settings:appsfeatures' deep link; hard clear = remove the data folder under %LocalAppData%\Packages\<PackageFamilyName>._

### Dev & Terminal · 🆕 new module / 新模組  (15)
- [ ] **Kill process on port** · 殺咗佔住個 port 嗰個程式
  - _Get-NetTCPConnection -LocalPort <n> -State Listen | Select -Expand OwningProcess | ForEach-Object { Stop-Process -Id $_ -Force }. Resolve names via Get-Process -Id; build the full listener table by joining Get-NetTCPConnection -State Listen to Get-Process on OwningProcess._
- [ ] **Manage PATH entries (user/system)** · 整理 PATH 入面嘅路徑
  - _Read/write registry: HKCU\Environment value 'Path' (user, REG_EXPAND_SZ) and HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment value 'Path' (system). After editing, broadcast WM_SETTINGCHANGE via SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 'Environment') so new shells pick it up. Dedupe/reorder in the UI._
- [ ] **Edit user & system environment variables** · 改環境變數
  - _Get/set arbitrary vars via [Environment]::GetEnvironmentVariable(name,'User'/'Machine') and SetEnvironmentVariable(name,value,scope) (writes HKCU\Environment / HKLM Session Manager\Environment); broadcast WM_SETTINGCHANGE 'Environment' after. Open the OS dialog with rundll32 sysdm.cpl,EditEnvironmentVariables. Machine scope needs elevation (route via existing no-UAC scheduled-task launcher)._
- [ ] **Export & restore winget package set** · 匯出同還原 winget 套件清單
  - _winget export -o packages.json --include-versions; on a new machine winget import -i packages.json --accept-package-agreements --accept-source-agreements --ignore-versions._
- [ ] **Upgrade all outdated packages** · 一次過更新晒啲套件
  - _winget upgrade --all --include-unknown --silent; for scoop: scoop update; scoop update *. Show the pending list first via winget upgrade (table) and scoop status._
- [ ] **Docker container & image dashboard** · 睇住 Docker 容器同 image
  - _docker ps -a --format '{{json .}}' and docker images --format '{{json .}}' for the grid; row actions docker start/stop/restart/rm <id>, docker logs -f <id>, docker exec -it <id> sh into the terminal panel; reclaim space with docker system df then docker system prune -f._
- [ ] **Switch Node version (per-shell)** · 切換 Node 版本
  - _Wrap fnm (winget install Schniz.fnm, ID verified): fnm list, fnm install <ver>, fnm use <ver>; emit fnm env --use-on-cd for the panel's shell init. Fallback to nvm-windows: nvm list / nvm install <ver> / nvm use <ver>._
- [ ] **Enable Corepack for pnpm/yarn** · 開咗 Corepack 用 pnpm/yarn
  - _corepack enable (ships with Node); pin a manager via corepack prepare pnpm@latest --activate or yarn@stable; verify with corepack --version. No global npm install needed._
- [ ] **Add Windows Defender dev-folder exclusions** · 幫開發資料夾加 Defender 例外
  - _Add-MpPreference -ExclusionPath '<repo dir>' and -ExclusionProcess 'node.exe','docker.exe' to stop real-time scans slowing builds; list with Get-MpPreference | Select -Expand ExclusionPath; remove via Remove-MpPreference -ExclusionPath. Needs elevation (route through the no-UAC scheduled-task launcher)._
- [ ] **Run the real Claude / Codex / OpenCode CLI** · 喺度行 Claude / Codex / OpenCode CLI
  - _Spawn the installed agent binaries in the embedded ConPTY terminal: claude (Anthropic Claude Code), codex (OpenAI Codex CLI), opencode; detect with Get-Command claude/codex/opencode and offer install via npm i -g @anthropic-ai/claude-code, @openai/codex, opencode-ai. Pass cwd = selected repo._
- [ ] **Generate & copy SSH key for Git** · 整條 SSH key 俾 Git 用
  - _ssh-keygen -t ed25519 -C '<email>' -f $env:USERPROFILE\.ssh\id_ed25519 -N ''; enable the agent with Set-Service ssh-agent -StartupType Automatic; Start-Service ssh-agent; ssh-add; copy the public key via Get-Content id_ed25519.pub | Set-Clipboard; optionally register with gh ssh-key add id_ed25519.pub._
- [ ] **Open WSL distro management** · 管理 WSL 發行版
  - _wsl --list --verbose for state/version; wsl --set-default-version 2; wsl --update (kernel); install from wsl --list --online via wsl --install -d <Distro>; per-distro wsl --terminate <name> / wsl --unregister <name>; launch one into the embedded terminal with wsl -d <name>._
- [ ] **Widen ephemeral ports & tune TIME_WAIT** · 調闊 ephemeral port 同縮短 TIME_WAIT
  - _netsh int ipv4 set dynamicport tcp start=10000 num=55000 to widen the ephemeral range (show current with netsh int ipv4 show dynamicport tcp). Optionally set HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters DWORD 'TcpTimedWaitDelay' (valid 30-300 s). Registry write + reboot/restart needs elevation._
- [ ] **Tunnel a local port (share dev server)** · 開隧道分享本機 port
  - _Wrap Cloudflare cloudflared (winget install Cloudflare.cloudflared, ID verified): cloudflared tunnel --url http://localhost:<port> prints a public https URL to copy. Alt engine: ngrok http <port> if installed._
- [ ] **Clean dev caches (npm/pnpm/pip/docker)** · 清咗開發快取
  - _npm cache clean --force; pnpm store prune; yarn cache clean; pip cache purge; docker builder prune -f; nuget locals all -clear. Show reclaimable sizes first by sizing %LocalAppData%\npm-cache, the pnpm store path (pnpm store path), %LocalAppData%\pip\Cache, and docker system df._

### Home Assistant · 🆕 new module / 新模組  (14)
- [ ] **Render a Jinja template against live state** · 攞實時狀態嚟跑 Jinja 範本
  - _POST /api/template with JSON {"template":"..."} (e.g. {{ states('sensor.temperature') }}); returns the rendered string as plain text. Lets a power user test templates against live entity state without opening HA's Developer Tools UI._
- [ ] **Validate config before restarting HA** · 重啟 HA 之前先驗下個 config
  - _POST /api/config/core/check_config (empty body) returns {"result":"valid"|"invalid","errors":...}. Run it, and only if valid call POST /api/services/homeassistant/restart. Prevents bricking HA on a bad configuration.yaml._
- [ ] **Plot 24h entity history sparkline** · 畫返廿四個鐘嘅實體歷史走勢
  - _GET /api/history/period/<ISO-8601 start timestamp>?filter_entity_id=<entity_id>&end_time=<ISO end>&minimal_response returns an array of state objects with last_changed; render as an inline sparkline for any sensor (temperature, power, etc.)._
- [ ] **Reload one integration without a full restart** · 唔使重啟成個 HA，淨係 reload 個整合
  - _POST /api/services/homeassistant/reload_config_entry with JSON {"entry_id":"<config_entry_id>"}; or domain reloads like POST /api/services/automation/reload, /api/services/template/reload, /api/services/scene/reload. Far faster than a full restart._
- [ ] **Set a custom in-memory state on any entity** · 幫實體寫返個自訂狀態屬性
  - _POST /api/states/<entity_id> with JSON {"state":"<value>","attributes":{...}}. Creates or overrides a virtual/dashboard-flag state directly via REST. Non-destructive: only affects in-memory state, not config (DELETE /api/states/<entity_id> removes it again)._
- [ ] **Snapshot a camera frame to disk** · 影低部 camera 嘅一格畫面存落本機
  - _GET /api/camera_proxy/<camera entity_id> returns the current JPEG bytes; stream them to a local file. Lets the user grab a still without the HA mobile app._
- [ ] **Run a scene or script on demand** · 即刻跑個場景或者腳本
  - _POST /api/services/scene/turn_on {"entity_id":"scene.<name>"} or POST /api/services/script/<object_id> (script entities expose themselves as a service). List candidates first via GET /api/states filtered to scene.* / script.*._
- [ ] **Fire a custom event into automations** · 掟個自訂事件出去俾自動化接
  - _POST /api/events/<event_type> with an optional JSON event-data body. Triggers any automation using an 'event' trigger (e.g. fire 'wintune_pc_locked' from a Windows session hook). Bridges Windows events into HA automations._
- [ ] **Browse today's calendar events** · 睇返今日 calendar 有咩節目
  - _GET /api/calendars lists calendar entities, then GET /api/calendars/<calendar entity_id>?start=<ISO>&end=<ISO> returns events in that window. Surfaces HA-linked calendars (Google/CalDAV) inside WinTune._
- [ ] **Tail the HA error log** · 睇實 HA 嘅錯誤 log
  - _GET /api/error_log returns the plaintext of the current session's home-assistant.log. Poll and diff to surface new WARNING/ERROR lines; pair with a 'copy to clipboard' for troubleshooting._
- [ ] **Set light brightness and colour temperature** · 校返盞燈嘅光暗同色溫
  - _POST /api/services/light/turn_on with JSON {"entity_id":"light.<x>","brightness_pct":0-100,"color_temp_kelvin":2000-6500} (or "rgb_color":[r,g,b]). Full dimming/tuning, not just an on/off toggle._
- [ ] **Set thermostat target temp and HVAC mode** · 校冷氣嘅目標溫度同運作模式
  - _POST /api/services/climate/set_temperature {"entity_id":"climate.<x>","temperature":n} and POST /api/services/climate/set_hvac_mode {"entity_id":"climate.<x>","hvac_mode":"heat"|"cool"|"off"|...}. Thermostat control from the desktop._
- [ ] **Push a notification to phones** · 推個通知去手機度
  - _POST /api/services/notify/<target> (e.g. notify.mobile_app_<device> or notify.notify) with JSON {"title":"...","message":"..."}. Discover targets via GET /api/services (notify domain). Push a desktop event (build done, backup finished) to the user's phone via HA._
- [ ] **Trigger a parameterized voice intent by text** · 用文字觸發個語音指令意圖
  - _POST /api/intent/handle with JSON {"name":"<IntentName>","data":{...slots}} (the intent must be registered in HA, e.g. the built-in HassTurnOn). Lets WinTune invoke an Assist/conversation intent with a structured slot payload instead of free text._

### Archives · 🆕 new module / 新模組  (14)
- [ ] **Encrypt archive headers (hide file names)** · 加密壓縮檔個檔頭（連檔名都收埋）
  - _7z.exe a -t7z archive.7z <files> -p{pwd} -mhe=on — AES-256 encrypts the archive header so even the file LIST is hidden, not just contents. Verified on 7-Zip 26.01: -mhe=on is the real header-encryption switch (only valid for .7z)._
- [ ] **Hash files / folders (CRC32, CRC64, SHA-256, SHA-1, BLAKE2sp, XXH64)** · 計檔案雜湊值（CRC32、SHA-256、BLAKE2sp 等）
  - _7z.exe h -scrcSHA256 <path> (also -scrcCRC32 / -scrcCRC64 / -scrcSHA1 / -scrcBLAKE2sp / -scrcXXH64 / -scrc* for all at once). The 'h' command hashes files on disk without archiving — verified BLAKE2sp, SHA256, XXH64 all output correctly on 7-Zip 26.01._
- [ ] **Benchmark compression / crypto codecs (MIPS rating)** · 跑分測試壓縮同加密速度（MIPS）
  - _7z.exe b [dictSize] [numIterations] — the 'b' benchmark tests LZMA compression/decompression speed and reports MIPS and MB/s; first positional arg sets dictionary size (e.g. 7z.exe b 24 for ~16MB). Add -mm=LZMA2 / -mm=Deflate / etc. to benchmark a specific codec. (No -mm=* extended-sweep switch — that is not real.)_
- [ ] **Update archive (refresh only changed / newer files)** · 更新壓縮檔（淨係加啲改咗或者新嘅檔）
  - _7z.exe u archive.7z <files> — the 'u' (update) command refreshes an existing archive, adding new files and replacing newer versions. Fine-tune with the single-token update-options switch -u{...}, e.g. -uq0 (don't copy missing-from-disk files), per 7-Zip's -u[-][p#][q#][r#][x#][y#][z#] syntax verified in switch help._
- [ ] **Delete files from inside an archive without re-packing** · 喺壓縮檔入面直接刪檔（唔使拆返出嚟）
  - _7z.exe d archive.7z <names_or_masks> — the 'd' (delete) command removes matching entries directly inside the archive. Combine with -r and include/exclude masks (e.g. 7z.exe d archive.7z *.log -r) to prune junk from a large archive in place._
- [ ] **Split into volumes / re-join (multi-part archive)** · 拆做分卷 / 砌返埋（多卷壓縮檔）
  - _Create: 7z.exe a archive.7z <files> -v100m (or -v700m, -v4480m for DVD). Re-join + extract: 7z.exe x archive.7z.001 reads all .00x parts automatically. Verified switch: -v{Size}[b|k|m|g]._
- [ ] **Make self-extracting EXE (SFX)** · 整自解壓 EXE（SFX）
  - _7z.exe a archive.exe <files> -sfx — verified switch -sfx[{name}]; defaults to the 7zCon.sfx console stub, or -sfx7z.sfx for the GUI stub shipped beside 7z.exe. Output must end in .exe; recipient runs it without 7-Zip installed._
- [ ] **Delete source files after successful packing (move-to-archive)** · 壓縮成功之後自動刪走原檔（等於搬入壓縮檔）
  - _7z.exe a archive.7z <files> -sdel — verified switch -sdel deletes the source files only after the archive is written successfully, turning a copy-into-archive into a true move. Pair with -t after for a safety integrity check._
- [ ] **Test archive integrity** · 驗壓縮檔完唔完整
  - _7z.exe t archive.7z -p{pwd} — the 't' command CRC-tests every entry without extracting and returns a non-zero exit code on corruption. For RAR: bundle unrar.exe and run unrar t archive.rar._
- [ ] **List archive contents with technical detail** · 列出壓縮檔內容（連技術細節）
  - _7z.exe l archive.7z -slt — verified switch -slt (show technical info) dumps per-file size, packed size, modified time, attributes, CRC, method and encryption flag. For RAR bare list: unrar lb archive.rar._
- [ ] **Repair corrupted RAR via recovery record** · 用復原記錄修整壞咗嘅 RAR
  - _Bundle RARLAB unrar.exe: unrar r archive.rar repairs a RAR using its embedded recovery record / recovery volumes (.rev). Pair with unrar x -kb archive.rar to keep partially-extracted broken files (-kb = keep broken, real WinRAR switch). 7-Zip cannot repair RAR, so this requires the unrar CLI._
- [ ] **Set LZMA2 dictionary & word size for max ratio** · 調字典大細同字長（榨到最盡個壓縮比）
  - _7z.exe a archive.7z <files> -m0=LZMA2 -md=256m -mfb=273 -mx=9 -ms=on — -md sets dictionary size, -mfb sets fast-bytes / word size (max 273), -mx=9 ultra level, -ms=on enables solid mode for best ratio on many small files. All real -m method parameters._
- [ ] **Filter by file mask & exclude junk into archive** · 用檔名樣式篩選同排除入壓縮檔
  - _7z.exe a archive.7z -ir!*.jpg -xr!*.tmp -xr!node_modules <root> — verified recursive include/exclude switches -i[r[-|0]]!wildcard and -x[r]!wildcard let you archive only matching files and skip junk folders._
- [ ] **Preserve NTFS timestamps & don't bump Last-Access** · 保留 NTFS 時間，唔好郁到最後存取時間
  - _7z.exe a archive.7z <files> -mtc=on -mta=on -mtm=on -ssp — -mtc/-mta/-mtm store Created/Accessed/Modified times in the .7z; verified switch -ssp stops Windows from updating the source files' Last-Access-Time while 7-Zip reads them. Add -sni to also store NT security info._

### Communications · 🆕 new module / 新模組  (14)
- [ ] **Compose Outlook draft (no auto-send)** · 整封 Outlook 草稿（唔會自動寄）
  - _Launch classic Outlook with new-message switches: "%ProgramFiles%\Microsoft Office\root\Office16\OUTLOOK.EXE" /c ipm.note /m "someone@example.com?subject=...&cc=...&body=...". /c ipm.note forces a new mail item; /m carries an RFC 6068 mailto query. Resolve the exact OUTLOOK.EXE path at runtime (Office16 path varies by install/bitness; new Outlook has no /c switch). Opens a draft only, never sends._
- [ ] **Open mailto: compose in default mail app** · 用預設信件 App 開 mailto: 寫信
  - _Start-Process "mailto:someone@example.com?subject=...&body=..." routes through whatever owns the mailto protocol (new Outlook, Thunderbird, web). RFC 6068 keys: subject, cc, bcc, body (URL-encoded). Honors the registered mailto UserChoice handler instead of hardcoding Outlook._
- [ ] **Attach files to a new mail (drag-pick)** · 揀檔案落新郵件做附件
  - _Classic Outlook: OUTLOOK.EXE /a "C:\path\file.pdf" opens a fresh message with that file staged as an attachment; combine with /m to pre-address. /a accepts one file path; loop-launch or zip first for multiple. No send. Classic Outlook only._
- [ ] **Jump to an Outlook folder on launch** · 跳去指定 Outlook 資料夾
  - _Classic Outlook: OUTLOOK.EXE /select outlook:Calendar (also outlook:Inbox, outlook:Contacts, outlook:Tasks, outlook:Notes, outlook:Drafts). The /select switch with an outlook: namespace path opens that store folder on launch._
- [ ] **Open a Discord channel / server** · 直接開個 Discord 頻道或者 server
  - _Start-Process "discord://-/channels/<guildId>/<channelId>" (omit channelId to land on the last viewed channel; use discord://-/channels/@me for DMs home). The installed handler is Discord.exe --url -- "%1" (verified in HKCU\Software\Classes\discord\shell\open\command), so the OS resolves the discord:// URL. IDs are user-supplied; no login._
- [ ] **Open an existing Discord DM thread** · 開返個 Discord 私訊對話
  - _Start-Process "discord://-/channels/@me/<dmChannelId>" opens an existing DM channel by its numeric channel id (the @me route is the documented DM namespace mirrored from the web app). User-supplied id only; no credential entry. (Profile-by-userId and settings/<pane> deep links are NOT exposed by the discord:// scheme and were dropped.)_
- [ ] **Start a Teams 1:1 / group chat** · 開個 Teams 一對一或者群組傾偈
  - _Start-Process "https://teams.microsoft.com/l/chat/0/0?users=joe@contoso.com,bob@contoso.com&topicName=...&message=..." — the official Teams deep-link form (the https l/chat URL launches the desktop client via the registered ms-teams/msteams handler and falls back to web). users is a comma list of UPNs; message pre-fills, does not auto-send._
- [ ] **Schedule a new Teams meeting** · 排個新嘅 Teams 會議
  - _Start-Process "https://teams.microsoft.com/l/meeting/new?subject=...&attendees=user1@x.com,user2@x.com&startTime=<ISO8601>&endTime=<ISO8601>&content=...". Official deep link; opens the meeting-scheduling form pre-filled, user clicks Send. No auto-send._
- [ ] **Open a Teams call deep link** · 撳一下打 Teams 電話
  - _Start-Process "https://teams.microsoft.com/l/call/0/0?users=joe@contoso.com" — documented Teams l/call deep link; opens the desktop client and starts a call to the given UPN(s). Replaces the unsupported Graph presence-write item, which needs OAuth credentials the suite never enters._
- [ ] **Share a URL/text to Telegram** · 分享條 link 去 Telegram
  - _Start-Process "tg://msg_url?url=<urlencoded>&text=<urlencoded>" opens Telegram Desktop's chat-picker with the URL+text staged in the compose box (documented at core.telegram.org/api/links). User picks the chat and sends. Plain text-only share: tg://msg?text=<urlencoded>._
- [ ] **Open a Telegram chat by username** · 開個 Telegram 傾偈
  - _Start-Process "tg://resolve?domain=<username>" opens that public chat/channel; add &post=<id> to jump to a specific channel post. Documented tg:// links only (the &videochat/&voicechat join params are not public and were dropped). Username only; no login._
- [ ] **Open a Slack channel / DM** · 開個 Slack 頻道或者私訊
  - _Start-Process "slack://channel?team=<TXXXX>&id=<CXXXX>" opens that channel in the Slack desktop client; slack://user?team=<TXXXX>&id=<UXXXX> opens a DM, and slack://open?team=<TXXXX> just focuses the workspace. Documented slack:// deep links; team/channel/user IDs are user-supplied; no login._
- [ ] **Call / text via Phone Link** · 用 Phone Link 打電話或者傳 SMS
  - _Start-Process "tel:+18005551234" and Start-Process "sms:+18005551234?body=<urlencoded>" route through the registered tel:/sms: handler (Phone Link / Your Phone when a phone is paired). RFC 3966 tel and the sms: body query; opens the dialer/compose, never auto-dials or auto-sends._
- [ ] **Pick the default mail / protocol handler** · 揀邊個做預設信件 App
  - _Start-Process "ms-settings:defaultapps" opens the Default apps Settings page so the user reassigns mailto and the discord/tg/msteams/slack scheme handlers. Windows 10+ blocks programmatic UserChoice writes, so the suite deep-links the page for the user to confirm rather than forcing a handler._

### Browser Control · 🆕 new module / 新模組  (14)
- [ ] **Launch site as desktop app window** · 用 App 模式開個網站做獨立視窗
  - _Spawn msedge.exe / chrome.exe with --app=https://<url> for a chromeless standalone window (no tabs/omnibox). Optionally add --window-size=W,H and --window-position=X,Y. Real exe paths confirmed at C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe and C:\Program Files\Google\Chrome\Application\chrome.exe (resolved via HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths)._
- [ ] **Open in incognito / InPrivate window** · 開個無痕視窗瀏覽
  - _msedge.exe --inprivate <url> for Edge; chrome.exe --incognito <url> for Chrome. Both are documented switches._
- [ ] **Launch full-screen kiosk URL** · 開全螢幕 Kiosk 鎖死喺個網度
  - _msedge.exe --kiosk https://<url> --edge-kiosk-type=fullscreen --kiosk-idle-timeout-minutes=0 (Edge kiosk switches); chrome.exe --kiosk https://<url>. Assigned-access-free kiosk launch._
- [ ] **Pick and launch a specific browser profile** · 揀個 Profile 嚟開
  - _Enumerate profiles by reading 'User Data\Local State' JSON (key profile.info_cache maps folder dir to display name; confirmed present for both Edge and Chrome). Launch chosen one with msedge.exe --profile-directory="Default" (or "Profile 1"). No registry, real on-disk profile dirs._
- [ ] **List and launch installed PWAs** · 列晒啲裝咗嘅 PWA 出嚟開
  - _PWAs install as .lnk shortcuts under %APPDATA%\Microsoft\Windows\Start Menu\Programs whose targets are msedge.exe/chrome.exe --profile-directory=... --app-id=<AppId>. Enumerate via shell:AppsFolder or parse the .lnk targets; launch with --app-id=<AppId>. App IDs must be read at runtime from existing shortcut targets, never fabricated._
- [ ] **Open the Windows default-apps picker for a browser** · 開預設 App 設定揀返個瀏覽器
  - _Windows 11 blocks silent default changes; deep-link the user to ms-settings:defaultapps (the per-app subpage / registeredAppUser anchor is build-dependent, so land on the page and let the user pick). For unattended provisioning, build an XML and apply with DISM /Online /Import-DefaultAppAssociations:assoc.xml. No fabricated keys._
- [ ] **Open internal flags & policy pages** · 開 edge://flags 同 policy 內部頁
  - _Launch the browser with an internal page as the URL arg: msedge.exe edge://flags | edge://policy | edge://version | edge://settings/profiles ; chrome.exe chrome://flags | chrome://policy | chrome://components | chrome://net-export . All real internal URLs._
- [ ] **Clear browsing cache for a profile** · 清返個 Profile 嘅 cache
  - _With the browser fully closed, delete on-disk caches: %LOCALAPPDATA%\Microsoft\Edge\User Data\<Profile>\Cache and \Code Cache (Cache dir confirmed present), plus the Chrome equivalent under Google\Chrome. For interactive scope, deep-link edge://settings/clearBrowserData or chrome://settings/clearBrowserData. No undocumented flag._
- [ ] **Set per-launch proxy server** · 開個 session 行 proxy
  - _msedge.exe/chrome.exe --proxy-server="socks5://127.0.0.1:1080" (or http://host:port) with optional --proxy-bypass-list="*.local;127.0.0.1". Real Chromium network switch; pair with --user-data-dir to keep it isolated._
- [ ] **Launch isolated throwaway browser sandbox** · 開個獨立 user-data 沙盒用完即棄
  - _msedge.exe/chrome.exe --user-data-dir="%TEMP%\wintune-sandbox\<guid>" creates a brand-new isolated profile tree (own cookies/history/extensions). Delete the dir afterward for a clean throwaway session. Documented switch._
- [ ] **Force-enable a hidden browser feature flag** · 夾硬開個隱藏功能 flag
  - _msedge.exe --enable-features=<FeatureName> / --disable-features=<FeatureName> on the command line. Feature names are real Chromium/Edge strings; the suite should let the user type/select a name confirmed from edge://flags rather than hard-coding a fabricated id._
- [ ] **Apply enterprise browser policy** · 落 policy 落去個瀏覽器
  - _Write real ADMX-backed policies under HKLM\SOFTWARE\Policies\Microsoft\Edge (e.g. HomepageLocation REG_SZ, RestoreOnStartup REG_DWORD, ExtensionInstallBlocklist) and HKLM\SOFTWARE\Policies\Google\Chrome. Verify applied state at edge://policy / chrome://policy. Documented policy keys, not invented._
- [ ] **Open URL with remote debugging port** · 開個 remote debugging port 俾自動化用
  - _msedge.exe/chrome.exe --remote-debugging-port=9222 --user-data-dir=<isolated dir> <url>. Exposes the DevTools/CDP JSON endpoint at http://127.0.0.1:9222/json for Playwright/Puppeteer attach. Switch requires a non-default user-data-dir on current Chromium._
- [ ] **Install/update a browser via winget** · 用 winget 裝/升級瀏覽器
  - _winget install --id Google.Chrome -e --silent / winget install --id Microsoft.Edge -e ; winget upgrade --id Google.Chrome -e --silent . Alternates Mozilla.Firefox, Brave.Brave. Real winget package identifiers._

### Config & Backup · 🆕 new module / 新模組  (14)
- [ ] **Export all suite settings to a portable bundle** · 匯出成個套件嘅設定做一個檔案
  - _Serialize WinTune's own settings store (the app's settings JSON/INI under %LOCALAPPDATA%\WinTune) plus a manifest into a single .zip via System.IO.Compression.ZipFile.CreateFromDirectory; bundle includes a version stamp so import can validate. File IO + ZipFile._
- [ ] **Import a settings bundle and re-apply tweaks** · 匯入設定檔案，再套返晒啲調整
  - _ZipFile.ExtractToDirectory of the exported .zip into a temp dir, validate manifest version, then replay each tweak through the existing Windows-11 module apply pipeline. File IO + ZipFile._
- [ ] **Init local git snapshot repo for config history** · 起一個本地 git 倉庫，儲低設定歷史
  - _git init in %LOCALAPPDATA%\WinTune\snapshots; on each snapshot write the current settings export into the working tree, then `git add -A` and `git commit -m "<timestamp>"`. Real git CLI._
- [ ] **Browse snapshot history (log)** · 睇返啲快照歷史 (log)
  - _git -C <snapshotsDir> log --pretty=format:%H%x09%ad%x09%s --date=iso, parse the tab-separated output into a list view. Real git CLI._
- [ ] **Restore settings to an earlier snapshot** · 還原返去之前一個快照
  - _git -C <snapshotsDir> restore --source=<commit> . (or `git checkout <commit> -- .`) to repopulate the working tree, then re-run import/apply. Non-destructive: stash the current state first with `git stash`. Real git CLI._
- [ ] **Diff current config against a snapshot** · 比較而家同舊快照嘅設定差異
  - _git -C <snapshotsDir> diff <commit> -- settings.json (text settings serialized deterministically so diffs are meaningful). Real git CLI._
- [ ] **Schedule automatic daily backup** · 排好每日自動備份
  - _Register a Task Scheduler task via schtasks /Create /SC DAILY /TN "WinTune Backup" /TR "<WinTune.exe> --snapshot" /ST 03:00 /RL LIMITED (reuses the suite's existing no-UAC scheduled-task pattern). Real schtasks CLI._
- [ ] **Export tweaked registry keys to a .reg file** · 匯出改過嘅登錄機碼做 .reg 檔
  - _For each key the Windows-11 module is known to touch, run reg export "<HKCU\...\key>" "<out>.reg" /y and concatenate into one human-reviewable, re-importable backup. Real reg.exe._
- [ ] **Capture installed-app list with winget export** · 用 winget 匯出而家裝咗嘅程式清單
  - _winget export -o apps.json --include-versions --accept-source-agreements; store the JSON in the snapshot so a rebuild can `winget import apps.json`. Real winget CLI (flags confirmed in winget v1.28)._
- [ ] **Back up taskbar pins and Start layout files** · 備份工作列嘅固定捷徑同開始選單排版
  - _reg export HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband for the reliable taskbar part, plus a raw file copy of %LOCALAPPDATA%\Packages\Microsoft.Windows.StartMenuExperienceHost_cw5n1h2txyewy\LocalState\start2.bin (Win11 filename). NOTE: copying start2.bin back is best-effort only — Microsoft removed supported Start-layout import on Win11, so restore is not guaranteed across builds/machines; store it for reference and re-pin if needed. File copy + reg export._
- [ ] **Mirror backups to a folder or network share** · 將備份鏡像去一個資料夾或者網絡共享
  - _robocopy "<snapshotsDir>" "<destOrUNC>" /MIR /R:2 /W:2 /NP /LOG:<log> for resilient incremental mirroring to an external drive or \\server\share. Real robocopy._
- [ ] **Package a snapshot as a single git bundle file** · 將一個快照打包成單一個 git bundle 檔
  - _git -C <snapshotsDir> bundle create wintune-config.bundle --all to produce one transportable file containing full history; restore elsewhere with `git clone wintune-config.bundle`. Real git CLI._
- [ ] **Prune snapshot history to reclaim space** · 清走舊快照，慳返啲空間
  - _Drop refs older than a chosen date, then git -C <snapshotsDir> reflog expire --expire=now --all && git gc --prune=now --aggressive to compact the repo. Real git CLI._
- [ ] **Verify backup integrity with hashes** · 用雜湊驗一驗備份冇壞
  - _Get-FileHash -Algorithm SHA256 over each file in the bundle, write a checksums.txt manifest; on restore recompute and compare, and run `git -C <snapshotsDir> fsck --full` to validate the repo objects. PowerShell Get-FileHash + git fsck._

### Capture Studio · 🆕 new module / 新模組  (3)
- [ ] **Region screen-record to MP4/GIF** · 錄起螢幕一忽嘅片，轉MP4或者GIF
  - _Wrap bundled ffmpeg: ffmpeg -f gdigrab -framerate 30 -offset_x X -offset_y Y -video_size WxH -i desktop out.mp4. For GIF use two-pass palette: ffmpeg -i out.mp4 -vf "fps=15,scale=720:-1:flags=lanczos,palettegen" pal.png then ffmpeg -i out.mp4 -i pal.png -filter_complex paletteuse out.gif. Region chosen via a transparent overlay window that supplies the offset/video_size args._
- [ ] **Instant rectangular snip to clipboard** · 即刻㩒個矩形截圖入剪貼簿
  - _Invoke the built-in Snipping Tool clip mode via its registered URL protocol (HKCR\ms-screenclip): Process.Start("ms-screenclip:") or explorer.exe ms-screenclip:. Optionally bind a global hotkey that SendInput-simulates Win+Shift+S. The resulting PNG is read back with Clipboard.GetImage()._
- [ ] **OCR text from any image or screen region** · 由圖或者螢幕嗰忽認返啲字出嚟
  - _WinRT Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages(); decode the bitmap with BitmapDecoder, await engine.RecognizeAsync(softwareBitmap), join OcrResult.Lines and copy to clipboard. For Cantonese/Chinese check OcrEngine.AvailableRecognizerLanguages for a zh-Hant/zh-Hans recognizer and prompt to add the language pack if absent._

### DNS & Hosts Manager · 🆕 new module / 新模組  (2)
- [ ] **Hosts file editor with one-click block/redirect** · 改hosts檔，一㩒就封站或者轉址
  - _Read/write %SystemRoot%\System32\drivers\etc\hosts (needs elevation). Back up to hosts.bak first, append rows like '0.0.0.0 example.com', toggle a row by prefixing '#', then run ipconfig /flushdns to apply._
- [ ] **Switch DNS server (Cloudflare/Google/auto)** · 轉DNS伺服器（Cloudflare/Google/自動）
  - _PowerShell DnsClient module: Set-DnsClientServerAddress -InterfaceIndex N -ServerAddresses ('1.1.1.1','1.0.0.1'); reset to DHCP with -ResetServerAddresses. Enumerate adapters via Get-DnsClientServerAddress; flush with Clear-DnsClientCache._

### Clipboard & QR Toolkit · 🆕 new module / 新模組  (2)
- [ ] **Generate QR code from clipboard text/URL** · 由剪貼簿啲字或者網址整個QR碼
  - _Encode locally with the QRCoder NuGet library: QRCodeGenerator.CreateQrCode(text, ECCLevel.Q) then new PngByteQRCode(data).GetGraphic(20). Save the PNG and/or place the bitmap on the clipboard. No network call._
- [ ] **Clipboard history viewer with pin & paste-as-plain** · 睇返剪貼簿歷史，可以釘住同貼純文字
  - _Enable history first: HKCU\Software\Microsoft\Clipboard value EnableClipboardHistory (REG_DWORD)=1 (or open ms-settings:clipboard). At runtime use Windows.ApplicationModel.DataTransfer.Clipboard.GetHistoryItemsAsync(); re-copy a chosen entry with SetHistoryItemAsContent; 'paste plain' builds a fresh DataPackage with SetText only to drop rich formats._

### Battery & Thermal Dashboard · 🆕 new module / 新模組  (2)
- [ ] **Battery health & wear report** · 睇電池健康同耗損報告
  - _Run powercfg /batteryreport /output report.html and parse DESIGN CAPACITY vs FULL CHARGE CAPACITY to compute wear %; run powercfg /energy for power-draw warnings. Live charge level via the Win32_Battery WMI class (EstimatedChargeRemaining, BatteryStatus)._
- [ ] **Live CPU/GPU temperature & fan monitor** · 即時睇CPU/GPU溫度同風扇
  - _Wrap the LibreHardwareMonitorLib NuGet: new Computer { IsCpuEnabled = true, IsGpuEnabled = true }.Open(), then traverse Hardware[].Sensors where SensorType is Temperature/Fan/Load. Fallback to the MSAcpi_ThermalZoneTemperature WMI class for a coarse thermal-zone reading without admin drivers._

### WSL & VM Launcher · 🆕 new module / 新模組  (2)
- [ ] **WSL distro manager (install/export/set-default)** · 管WSL發行版（裝、出檔、設預設）
  - _Shell out to wsl.exe (C:\Windows\System32\wsl.exe): list with wsl --list --verbose; browse with wsl --list --online; install wsl --install -d <Distro>; back up wsl --export <Distro> <file.tar>; restore wsl --import <Name> <dir> <file.tar>; default wsl --set-default <Distro>; reclaim RAM with wsl --shutdown._
- [ ] **Launch Windows Sandbox with a prebuilt .wsb config** · 用現成.wsb設定開Windows沙盒
  - _Emit a .wsb XML (<Configuration><MappedFolders><MappedFolder><HostFolder>...</HostFolder><ReadOnly>true</ReadOnly></MappedFolder></MappedFolders><Networking>Disable</Networking></Configuration>) and start it with WindowsSandbox.exe <file.wsb>. Enable the feature first via DISM /Online /Enable-Feature /FeatureName:Containers-DisposableClientVM /All (the documented Windows Sandbox feature name)._

### Color Lab · 🆕 new module / 新模組  (1)
- [ ] **System-wide color picker / eyedropper** · 全螢幕㩒色嘅滴管
  - _P/Invoke Gdi32: read the pixel under the cursor with GetCursorPos + GetPixel(GetDC(IntPtr.Zero), x, y); convert to HEX/RGB/HSL/HSV and copy the chosen format to clipboard. Draw a zoom loupe from a BitBlt of the screen DC around the cursor._

### Font Manager · 🆕 new module / 新模組  (1)
- [ ] **Install / preview / uninstall fonts in bulk** · 一次過裝、睇同移除啲字型
  - _Per-user install (no UAC): copy .ttf/.otf to %LOCALAPPDATA%\Microsoft\Windows\Fonts and add a value under HKCU\Software\Microsoft\Windows NT\CurrentVersion\Fonts named '<Face> (TrueType)' = the file path, then broadcast WM_FONTCHANGE. Machine-wide variant: copy to %WINDIR%\Fonts + the HKLM equivalent. Preview by rendering a sample string per face._

### Hotkey & Macro Runner · 🆕 new module / 新模組  (1)
- [ ] **Global hotkey -> action macro runner** · 設全域熱鍵跑自訂動作
  - _Register chords with user32 RegisterHotKey and pump WM_HOTKEY; map each id to an action: Process.Start an app, run a PowerShell snippet, or replay input via SendInput. Persist bindings to JSON; optionally bundle AutoHotkey v2 (AutoHotkey64.exe script.ahk) for richer key remaps._

### OneDrive · 🆕 new module / 新模組  (1)
- [ ] **Pin / Dehydrate OneDrive Files-On-Demand & Set Auto-Free Threshold** · 設定OneDrive檔案隨選同自動釋放空間
  - _OneDrive.exe /shutdown to pause sync. Per file/folder: 'attrib +U -P <path>' marks online-only (dehydrate via cloud-filter pin state), 'attrib +P -U <path>' pins it always-local. Auto-dehydration age set by DWORD ConfigStorageSenseCloudContentDehydrationThreshold under ...\StorageSense\Parameters\StoragePolicy._

### Time & Unit Tools · 🆕 new module / 新模組  (1)
- [ ] **World clock & timezone converter** · 睇世界時鐘同換時區
  - _Enumerate zones with TimeZoneInfo.GetSystemTimeZones() (or tzutil /l) and convert with TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, srcId, dstId). Read the machine's current zone via tzutil /g; show a multi-city board updating each second. No network needed; zones come from the OS ICU/registry data._

### Voice & Read-Aloud · 🆕 new module / 新模組  (1)
- [ ] **Read selected text aloud (TTS) / export WAV** · 讀返揀咗嘅字出聲，或者出WAV檔
  - _.NET: Add-Type -AssemblyName System.Speech; $s = New-Object System.Speech.Synthesis.SpeechSynthesizer; choose a voice from $s.GetInstalledVoices(); $s.SpeakAsync(text) to play, or $s.SetOutputToWaveFile(path) then $s.Speak(text); $s.Dispose() flushes/closes the WAV._

## 🌱 Newly discovered — iteration 1 · 第 1 次迭代新發掘 (12)

### Windows 11 / Maintenance (newly found · 新搵到)
- [ ] **Free up Reserved Storage (~7 GB)** · 釋放保留儲存空間（約 7 GB）
  - _DISM /Online /Set-ReservedStorageState /State:Disabled (admin). Reclaims the space Windows reserves for updates._
- [ ] **Processor scheduling: Programs vs Background** · 處理器排程：程式定背景服務
  - _HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl -> DWORD Win32PrioritySeparation = 0x26 (programs) / 0x18 (background). admin._
- [ ] **Disable USB selective suspend** · 熄咗 USB 選擇性暫停
  - _powercfg /SETACVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0 then powercfg /setactive SCHEME_CURRENT. admin._
- [ ] **Set active network profile to Private** · 將目前網絡設為私人
  - _Set-NetConnectionProfile -NetworkCategory Private (admin) — enables sharing & lowers firewall strictness on trusted nets._
- [ ] **Toggle Hyper-V hypervisor launch** · 開熄 Hyper-V 虛擬化啟動
  - _bcdedit /set hypervisorlaunchtype auto (on) / off. admin, reboot. Off can help some anti-cheat games; on needed for WSL2/sandbox._
- [ ] **Rebuild performance counters** · 重建效能計數器
  - _lodctr /R (admin) — fixes broken Task Manager / PerfMon counters._
- [ ] **Disable/Enable RAM memory compression** · 開熄記憶體壓縮
  - _Disable-MMAgent -MemoryCompression / Enable-MMAgent -MemoryCompression (admin, reboot)._
- [ ] **Hardware-accelerated GPU scheduling (HAGS)** · 硬件加速 GPU 排程
  - _HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers -> DWORD HwSchMode = 2 (on) / 1 (off). admin, reboot._
- [ ] **Set crash dump to small (minidump)** · 將當機傾印設為小型 (minidump)
  - _HKLM\SYSTEM\CurrentControlSet\Control\CrashControl -> DWORD CrashDumpEnabled = 3. admin. Saves disk vs full dumps._
- [ ] **Disable SysMain (Superfetch) service** · 停用 SysMain (Superfetch) 服務
  - _sc config SysMain start= disabled then sc stop SysMain (admin) — can reduce disk thrash on some SSD systems._
- [ ] **GPU TDR delay (fix display-driver timeouts)** · GPU TDR 延遲（修顯示驅動逾時）
  - _HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers -> DWORD TdrDelay = 8 (seconds). admin, reboot._
- [ ] **Rebuild Windows Search index** · 重建 Windows 搜尋索引
  - _HKLM\SOFTWARE\Microsoft\Windows Search -> DWORD SetupCompletedSuccessfully = 0, then restart the WSearch service (admin)._

## 🌱 Newly discovered — iteration 2 · 第 2 次迭代新發掘 (6)

- [ ] **Compare two folders/archives (hash diff)** · 比較兩個資料夾／壓縮檔（雜湊對比）
  - _Get-FileHash on each side (or 7z h) then diff the hash lists to find changed/missing files. Pure local, no extra engine._
- [ ] **Mount / dismount ISO & VHD** · 掛載／卸載 ISO 同 VHD　🆕 Disk Image module
  - _Mount-DiskImage -ImagePath "x.iso" / Dismount-DiskImage -ImagePath "x.iso"; works for .iso/.vhd/.vhdx natively. Get-DiskImage to query._
- [ ] **Create ISO from a folder** · 由資料夾整 ISO　🆕 Disk Image module
  - _Wrap oscdimg.exe (Windows ADK Deployment Tools): oscdimg -m -u2 "C:\src" "C:\out.iso". Detect ADK install; otherwise guide the user._
- [ ] **Network speed test** · 網絡測速　🆕 Network extras
  - _Wrap the Ookla Speedtest CLI: winget install Ookla.Speedtest.CLI, then run "speedtest"; fallback to a timed download via Invoke-WebRequest._
- [ ] **Window snapping zones (FancyZones-style)** · 視窗分區貼齊　🆕 Window Manager module
  - _Launch/configure PowerToys FancyZones (winget install Microsoft.PowerToys; start the FancyZones editor), or move/resize windows directly via Win32 SetWindowPos._
- [ ] **Process detail: open handles & loaded DLLs** · 程序詳情：開啟嘅控制代碼同 DLL
  - _PowerShell: Get-Process -Id <pid> -Module lists loaded modules; tasklist /m for DLLs; wrap Sysinternals handle.exe (winget install Microsoft.Sysinternals.Handle) for open handles._

## 🌱 Newly discovered — iteration 3 · 第 3 次迭代新發掘 (forum pain-points · 論壇痛點)

_Source: xda-developers "Your Windows 11 complaints have solutions" + r/Windows11 threads._
- [ ] **Turn off Windows Copilot** · 熄 Windows Copilot
  - _HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot -> DWORD TurnOffWindowsCopilot = 1. Removes Copilot integration people find intrusive._
- [ ] **Disable Bing & web results in Start search** · 熄開始搜尋嘅 Bing／網頁結果
  - _HKCU\Software\Policies\Microsoft\Windows\Explorer -> DWORD DisableSearchBoxSuggestions = 1; and HKCU\Software\Microsoft\Windows\CurrentVersion\Search -> DWORD BingSearchEnabled = 0. Makes search app-focused & instant (top complaint)._
- [ ] **Disable Search Highlights** · 熄搜尋醒目提示
  - _HKCU\Software\Microsoft\Windows\CurrentVersion\SearchSettings -> DWORD IsDynamicSearchBoxEnabled = 0. Removes the rotating doodles/ads in the search box._
- [ ] **Remove "Ask Copilot" / extra entries from right-click** · 右鍵移除「Ask Copilot」等項目
  - _Restore the compact-menu CLSID block (already have classic-menu toggle) + TurnOffWindowsCopilot; per-app verbs (Clipchamp/Notepad) are blocked via their CLSID under HKCU\Software\Classes\...\shell. Build as in-app toggles._
- [x] **In-app Services Manager** · App 內服務管理員　(replaces services.msc redirect) — DONE: live list of 315 services, search, start/stop/restart + set startup type, native bilingual, no redirect.

## 🌱 Newly discovered — iteration 4 · 第 4 次迭代新發掘 (forum pain-points · 論壇痛點)

_Source: windowsforum.com + thewindowsclub telemetry/Task-Scheduler threads._
- [x] **In-app Scheduled-Tasks Manager** · App 內排程工作管理員　(replaces taskschd.msc) — DONE this iteration: Get-ScheduledTask list, run/stop/enable/disable, native bilingual, no redirect.
- [ ] **Disable telemetry scheduled tasks (one-click)** · 一鍵停用遙測排程工作
  - _Disable-ScheduledTask for \Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser, \...\ProgramDataUpdater, \Autochk\Proxy (UsbCeip), \Customer Experience Improvement Program\Consolidator + UsbCeip. Add as a Recipe + individual toggles in the Tasks manager._
- [ ] **Disable Compatibility Appraiser via registry too** · 用登錄檔停埋相容性評估
  - _HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\... and the task; pair with the task disable for completeness._

## 🌱 Newly discovered — iteration 5 · 第 5 次迭代新發掘

_Source: windowscentral "Top 10 open-source Windows 11 apps recommended by Reddit" (PowerToys etc.)._
- [x] **In-app Devices Manager** · App 內裝置管理員　(replaces devmgmt.msc) — DONE this iteration: Get-PnpDevice present list, enable/disable, native bilingual, no redirect.
- [ ] **Batch file rename (PowerRename-style)** · 批次改檔名（PowerRename 式）　🆕 in-app module
  - _Pure C# file IO + System.Text.RegularExpressions: pick a folder, preview find/replace or regex rename across files, apply. No external engine, fully in-app._
- [ ] **In-app screen recorder** · App 內螢幕錄影
  - _Wrap ffmpeg: ffmpeg -f gdigrab -framerate 30 -i desktop -c:v libx264 -preset ultrafast out.mp4; start/stop from a native panel. Part of a future Capture Studio module._
- [ ] **Startup-apps manager (enable/disable)** · 開機程式管理（啟用／停用）
  - _List from HKCU/HKLM ...\Run + Startup folders + Win32_StartupCommand; toggle via the Explorer\StartupApproved\Run binary blob (byte0 0x02=enabled / 0x03=disabled). In-app, replaces Task Manager's Startup tab._

## 🌱 Newly discovered — iteration 6 · 第 6 次迭代新發掘

- [x] **In-app Startup-apps Manager** · App 內開機程式管理員　(replaces Task Manager Startup tab) — DONE this iteration: lists Run keys + Startup folders, enable/disable via the StartupApproved blob, native bilingual, no redirect.
- [ ] **One-click "trim startup bloat"** · 一鍵清開機臃腫
  - _Recipe that disables common non-essential startup entries (Steam, Epic, OEM updaters) by writing 0x03 to their StartupApproved\Run value — reversible. Pair with the Startup manager._
- [ ] **Show boot time / last boot duration** · 顯示開機時間
  - _Get-WinEvent -LogName 'Microsoft-Windows-Diagnostics-Performance/Operational' -Id 100 for boot duration (ms); surface in System Info / Startup module._

## 🌱 Newly discovered — iteration 7 · 第 7 次迭代新發掘

_Source: Microsoft Learn (PowerRename) + bulkrenameutility.co.uk — confirms no native Explorer bulk-rename._
- [x] **In-app Batch Rename** · App 內批次改名　(PowerRename-style, pure C#) — DONE this iteration: pick a folder, find/replace with regex + live preview + conflict detection, apply. No external tool.
- [ ] **Batch rename: case transforms & auto-number** · 批次改名：大小寫轉換同自動編號
  - _Extend the rename engine: UPPER/lower/Title case, prepend/append text, sequential numbering ({n} token). Pure C# (System.IO + Regex)._
- [ ] **Bulk file operations** · 批次檔案操作　🆕 in-app module
  - _Move/copy files matching a wildcard/regex, flatten nested folders, delete-by-pattern (with confirm). Pure C# File/Directory APIs, in-app preview._

## 🌱 Newly discovered — iteration 8 · 第 8 次迭代新發掘

- [x] **In-app Media module (ffmpeg)** · App 內媒體模組 — DONE this iteration: input/output pickers, quick convert/extract/GIF/compress/mute/info + 60 advanced ffmpeg/ffprobe ops, native bilingual, no redirect.
- [ ] **In-app screen recorder** · App 內螢幕錄影
  - _ffmpeg -f gdigrab -framerate 30 -i desktop -c:v libx264 -preset ultrafast out.mp4; start as a tracked background process, stop by sending 'q' / killing. Add to the Media module._
- [ ] **Audio recorder (mic)** · 收音錄音
  - _ffmpeg -f dshow -i audio="<mic name>" out.wav (enumerate devices via ffmpeg -list_devices true -f dshow -i dummy). In-app panel._

## 🌱 Newly discovered — iteration 9 · 第 9 次迭代新發掘 (forum pain-points · 論壇痛點)

_Source: windowsnews.ai / windowsforum / makeuseof / howtogeek — "Make Windows 11 less intrusive" 2025._
- [x] **Debloat & Annoyances category** · 去煩擾分類 — DONE this iteration: ~30 in-app registry/command toggles for the most-complained-about Win11 annoyances (Copilot, Recall, Bing/web search, Search Highlights, lock-screen tips, Start/Explorer/Settings ads, setup nags…). No redirect.
- [ ] **Disable Recall snapshots** · 停用 Recall 快照
  - _HKCU\Software\Policies\Microsoft\Windows\WindowsAI -> DWORD DisableAIDataAnalysis = 1 (and the HKLM policy). Stops on-screen snapshotting._
- [ ] **Stop lock-screen Spotlight tips/ads** · 熄鎖機畫面 Spotlight 提示／廣告
  - _HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager -> RotatingLockScreenOverlayEnabled = 0, SubscribedContent-338387Enabled = 0._

## 🌱 Newly discovered — iteration 10 · 第 10 次迭代新發掘

_Source: howtogeek / neowin / windowsforum — Explorer bulk-ops, organization & dual-pane still barebones._
- [x] **In-app Bulk File Operations** · App 內批次檔案操作 — DONE this iteration: pick a folder, match by wildcard/regex/extension (recursive), preview, then Copy/Move/Recycle/Flatten/Organize-by-extension. Pure C#, recycle via SHFileOperation (reversible).
- [ ] **Duplicate file finder** · 重複檔案搜尋
  - _Pure C#: group files by size then SHA-256 hash to find exact duplicates; preview groups; recycle the extras. No external tool._
- [ ] **Empty-folder cleaner** · 清空資料夾
  - _Pure C#: recursively find directories with no files (and no non-empty subdirs); preview; remove. In-app._

## 🌱 Newly discovered — iteration 11 · 第 11 次迭代新發掘

_Source: Microsoft Community Hub — Windows has NO built-in duplicate finder; 3rd-party tools are slow / high false-positives._
- [x] **In-app Duplicate File Finder** · App 內重複檔案搜尋 — DONE this iteration: size pre-filter then SHA-256 content hash (zero false positives), grouped results, one-click recycle of redundant copies, shows wasted space. Pure C#, no external tool.
- [ ] **Largest files finder** · 最大檔案搜尋
  - _Pure C#: enumerate a folder recursively, sort by size desc, show the top N biggest files with a recycle action. Helps reclaim space fast._
- [ ] **Folder size analyser** · 資料夾大細分析
  - _Pure C#: compute total size per immediate subfolder (recursive sum), show a sorted bar list to spot what's eating disk. In-app._

## 🌱 Newly discovered — iteration 12 · 第 12 次迭代新發掘

_Source: windirstat.net — Windows' built-in storage breakdown is limited; users reach for WinDirStat/TreeSize._
- [x] **In-app Disk Analyser** · App 內磁碟分析 — DONE this iteration: per-folder recursive size with %-bars + drill-in, and a "largest files" mode. Pure C#, off-UI-thread scan, recycle action. Covers the queued largest-files + folder-size items.
- [ ] **Drive overview** · 磁碟機總覽
  - _DriveInfo.GetDrives(): show each drive's used/free/total with a bar. Quick entry point into the analyser. Pure C#._
- [ ] **Treemap visualisation** · 樹狀圖視覺化
  - _Render folder sizes as nested rectangles (squarified treemap) on a Canvas for an at-a-glance view, like WinDirStat. Pure C# drawing._

---
_Auto-grown by the WinTune build loop · 由 WinTune 建置迴圈自動擴充_
