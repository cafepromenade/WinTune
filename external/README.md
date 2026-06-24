# external/ — vendored upstream sources (git submodules)

Each open-source project referenced by a feature [handoff](../docs/handoffs/) is registered here as a
**git submodule**, pinned to the upstream's `HEAD` commit at the time of registration. The submodules are
**not fetched yet** (registered only) so the WinTune repo stays small — fetch the ones you need on demand.

## Fetch a submodule's source

```bash
# one project
git submodule update --init --depth 1 external/<name>

# everything (large — many big repos)
git submodule update --init --recursive --depth 1
```

All entries are marked `shallow = true`, so updates pull a shallow clone by default.

## Why registered-only (not full clones)

Cloning every upstream (Blender, LibreOffice, VS Code, VLC, Wireshark, PowerToys, …) up front would be
tens of gigabytes. Registering them as pinned submodules gives the exact structure and commit references
without the download; `git submodule update --init` materialises whichever you actually work on.

## Mapping

Submodule folder names follow `external/<name>`; each corresponds to a handoff in `docs/handoffs/`
(see that folder's `README.md` index). Notable mappings:

- `external/veracrypt` → handoff **05 (HuiCrypt vault)** — HuiCrypt is a VeraCrypt fork; upstream VeraCrypt is
  vendored here as the reference. The shipped module must be de-branded (no "Hui"/"VeraCrypt"/"TrueCrypt" names).
- `external/paper` → handoff **22 (Minecraft server setupper)** — PaperMC server source.
- `external/powertoys` → handoff **49 (FancyZones)** (and useful for other PowerToys-style features).
- `external/amulet-map-editor` → handoff **14** — a prebuilt zip is also at
  `C:\Users\cntow\Downloads\amulet_map_editor.zip`.

## Not registered (no public git repo)

- **FileZilla** — distributed from <https://download.filezilla-project.org/client/> (no GitHub repo).
  Handoff **33** recommends a native FTP/SFTP client (FluentFTP + SSH.NET) rather than vendoring FileZilla.
- **Android SDK tools** (handoff 36) and **WebView2** (handoff 01) are SDK/NuGet components, not single repos.
- **Thunderbird** is vendored via `external/thunderbird` (mozilla `releases-comm-central` mirror); the
  recommended approach is still a native MailKit client.
