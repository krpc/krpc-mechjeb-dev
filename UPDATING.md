# Updating to a New MechJeb Build

MechJeb publishes several dev builds a day. We move the service to the latest one now and then,
on a branch, and release it. `scripts/update.py` does the mechanical steps.

## Check and Pin

See how far the pin is behind:

```
.venv/bin/python scripts/update.py status
```

Pin the latest build, or a given one:

```
.venv/bin/python scripts/update.py bump
.venv/bin/python scripts/update.py bump dev-2.16.0.0-2125
```

The new pin fetches the build from MechJeb until its release mirrors it.

## Test in Game

Run the whole suite against the new pin:

```
KSP_DIR=/path/to/KSP .venv/bin/pytest
```

`test_bindings.py` lists the members the new build no longer has. A failure in another test is
either a change in MechJeb's behavior or a flaky test, as listed in PORTING.md. Re-run the test
alone, and with `--mechjeb=release`, to tell them apart.

## Review the Changes

Write MechJeb's commits and C# diff since the last release into `build/update/`:

```
.venv/bin/python scripts/update.py diff
```

`log.txt` holds the commits, and `api.diff` the C# changes. Look for bound members that were
renamed or removed, changes in what an exposed feature does, and new features worth exposing.

## Change the Service

Fix the bindings, and add the features chosen, with tests and examples. Record each user-facing
change under a `## [Unreleased]` section of CHANGELOG.md, and each upstream problem in
PORTING.md.

## Release

Set the version, in a commit `Release vX.Y.Z` at the end of the branch:

```
.venv/bin/python scripts/update.py release 1.1.0
```

It heads the changelog section with the version and the pins, and points the pin at the
release's mirror of the build. Take a patch version for a new pin alone, a minor version for
additions, and a major one for a breaking change.

Once the pull request merges, tag main and build the release assets:

```
git tag v1.1.0 && git push origin v1.1.0
.venv/bin/python scripts/build_service.py
.venv/bin/python scripts/docs.py
.venv/bin/python scripts/mirror.py
.venv/bin/python scripts/update.py notes
gh release create v1.1.0 --title v1.1.0 --notes-file build/release-notes.md \
  build/krpc-mechjeb-dev-*.zip build/MechJeb2-*.zip
```

`mirror.py` and the MechJeb assets apply only to a release that pins a new build. Without them,
the pin still fetches from the release that mirrored its build.
