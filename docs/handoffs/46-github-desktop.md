# Handoff: GitHub Desktop features (port UX into Git & GitHub module)

| | |
|---|---|
| **Status** | Not started |
| **Source** | https://github.com/desktop/desktop (Electron/TypeScript) |
| **License** | MIT (open source) — UX/behavior is a reference only; do not copy code |
| **Proposed module** | Extend existing **Git & GitHub** (`Pages/GitHubModule.xaml`) · Developer / Tools group · existing Tag `module.github` |
| **Effort** | L — visual diff, branch graph, and hunk staging are real UI work; git/gh plumbing already exists |

## What the user asked for
Bring GitHub Desktop's best workflows into WinTune. Rather than ship the Electron app, port its UX — visual diff, commit history, branch graph, hunk-level staging, clone dialog, and one-click PR creation — into the existing `GitHubModule`. Offer launching the real GitHub Desktop (`winget GitHub.GitHubDesktop`) only as a fallback.

## Recommended approach
**Native C# clone, extending the existing module.** GitHub Desktop is a thick Electron app, but its value is the UX over `git` and `gh`, both of which WinTune already shells out to via `ShellRunner` and `Catalog/GitHubOperations.cs`. Per the global strategy we do NOT wrap the Electron binary — we reimplement the workflows natively in WinUI. Realistic v1: a repo picker, a changes/diff pane, a commit-history list, and a clone dialog — all driven by `git` porcelain output parsed in a new `GitDeskService`. PR creation rides on the `gh` CLI (already an install target). Honest limits: a polished animated branch *graph* with curved lane routing is the hardest part — render a simplified vertical lane graph from `git log --graph --pretty` in v1, refine later. Provide `EngineBars.AutoInstallButton("GitHub.GitHubDesktop", ...)` so users can install the real app as a fallback.

## Features to implement (v1 → later)
- v1: Repo picker (FileDialogs folder pick + recent list); Changes tab with file list and a side-by-side/unified text diff (`git diff` parsed, added/removed line coloring); commit box (summary + description) with stage-all / commit / push; commit-history list (`git log`) with per-commit changed-files + diff; Clone dialog (URL or `owner/repo` + target folder); "Open PR" via `gh pr create`.
- later: Hunk-level and line-level staging (`git apply --cached` against selected hunks); visual branch graph with lanes; branch create/switch/merge UI; conflict resolver; fetch/pull with ahead/behind badges; stash list; image-diff for binary files.

## Integration plan (WinTune specifics)
- New files: `Services/GitDeskService.cs` (parse `git status --porcelain=v2`, `git diff`, `git log`, stage/commit/push helpers, all via `ShellRunner.Capture`); extend `Pages/GitHubModule.xaml(.cs)` with a TabView (Changes / History / Branches); reuse/extend `Catalog/GitHubOperations.cs` for tweak-style one-shot ops (init, set remote, prune).
- Nav wiring: module already exists (Tag `module.github`, `ModuleRegistry`, `MapType`, `NavView_SelectionChanged`) — no new nav item needed; just confirm `ApplyStartPage` handles `--page github`.
- Engine/install: `git` and `gh` are the working dependencies (existing engine bars). Add `EngineBars.AutoInstallButton("GitHub.GitHubDesktop", "Install GitHub Desktop", "安裝 GitHub Desktop", recheck, rescan)` as a fallback launcher row.
- Key APIs/CLIs: `git status/diff/log/add/commit/push/clone/branch/checkout`, `gh pr create`, `gh repo clone`. Always use `FileDialogs` for folder/repo selection, never WinRT pickers.

## Dependencies & risks
- Parsing `git` porcelain is version-sensitive — pin to `--porcelain=v2` and `--pretty=format:` with explicit fields; handle CRLF and non-ASCII paths.
- Hunk staging is fiddly (patch byte-offsets) — defer to "later" to keep v1 shippable.
- Diff rendering performance on large files — virtualize the line list.
- `gh` requires auth; surface a clear bilingual prompt when `gh auth status` fails.

## Acceptance criteria
- Builds clean (Debug + Release x64); Git & GitHub module shows the new Changes/History tabs; user can pick a repo, see a colored diff, commit, push, view history, and clone; PR creation works via `gh`; GitHub Desktop install fallback present; every string bilingual (English + 粵語); folder/repo selection uses `FileDialogs` (no WinRT pickers).
