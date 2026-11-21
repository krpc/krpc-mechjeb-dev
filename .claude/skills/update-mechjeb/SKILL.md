---
name: update-mechjeb
description: Move the service to the latest MechJeb dev build - pin it, test in game, scan MechJeb's changes for API to bind or add, and prepare the pull request and release
---

Follow UPDATING.md, on a branch `mechjeb-<build number>` off main.

1. `.venv/bin/python scripts/update.py status`. Stop if there is no newer build.
2. `.venv/bin/python scripts/update.py bump`, and commit `Pin MechJeb <build>`.
3. Run the in-game suite, `.venv/bin/pytest`, in the background. Start the scan in step 4
   while it runs.
4. Run `.venv/bin/python scripts/update.py diff`, then spawn an agent with the brief below.
   It writes `build/update/scan.md`. If `api.diff` is small enough to read at a glance, write
   the scan yourself to the same brief.
5. Show the user the failures and the scan. Ask which new features to add.
6. Fix the bindings and the failures, then add the chosen features. Give each its tests, its
   doc examples and a CHANGELOG.md entry under `## [Unreleased]`. Re-run the affected tests.
   Make one commit per change.
7. Propose a version, as UPDATING.md describes. Run
   `.venv/bin/python scripts/update.py release <version>`, and commit `Release v<version>`.
8. Ask before pushing the branch and opening the pull request. The user publishes the release.

## Scan Brief

> MechJeb moved from one dev build to another. `build/update/log.txt` has its commits, and
> `build/update/api.diff` its C# changes. This repository's service, in `src/`, binds MechJeb
> members by name. `Bindings.Type ("MuMech.<Class>")` gives a type, and the calls on it and
> `Bindings.EditableDouble` and `EditableInt` name the members. Write
> `build/update/scan.md` with three sections:
>
> 1. **Bindings.** Each bound type or member that the diff renames, removes, or changes in type
>    or signature. Give the `src/` file and line, and the MechJeb change.
> 2. **Behavior.** Each change to a feature the service exposes that may change its result,
>    default or units. These may need a test, doc or changelog change.
> 3. **New API.** Each new user-facing feature or setting the service could expose, such as a
>    new `[Persistent]` or `EditableDouble` field, a module, a maneuver operation or an
>    autopilot mode. Give the MechJeb class and member names, and say what the user sees in
>    MechJeb's window.
>
> Leave out UI layout, localization and refactors that keep the bound names. Say "None" for an
> empty section. Cite each item with its commit hash.
