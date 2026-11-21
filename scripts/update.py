"""Move the service to a newer MechJeb dev build, and prepare its release.

UPDATING.md gives the order to run these in:

* status: the pinned build, and the dev builds MechJeb has published since.
* bump [BUILD]: pin a dev build, the latest by default. The pin fetches from upstream until
  the release that mirrors the build is published.
* diff [--from REF]: MechJeb's commits and C# changes from the build pinned at REF, the last
  release tag by default, to the pinned build, into build/update/.
* release VERSION: set the version, head the changelog's unreleased section with it, and point
  the pin at the release's mirror of the build.
* notes: the release notes of the current version, into build/release-notes.md.

GITHUB_TOKEN, when set, authenticates the GitHub API calls.
"""

import argparse
import json
import os
import re
import subprocess
import sys
import tomllib
import urllib.request

import build_service
import fetch

ROOT = fetch.ROOT
PINS = os.path.join(ROOT, "pins.toml")
CHANGELOG = os.path.join(ROOT, "CHANGELOG.md")
UPDATE = os.path.join(build_service.BUILD, "update")
UPSTREAM = "MuMech/MechJeb2"
REPOSITORY = "krpc/krpc-mechjeb-dev"


def api(path):
    request = urllib.request.Request("https://api.github.com/" + path)
    request.add_header("Accept", "application/vnd.github+json")
    if "GITHUB_TOKEN" in os.environ:
        request.add_header("Authorization", "Bearer " + os.environ["GITHUB_TOKEN"])
    with urllib.request.urlopen(request, timeout=60) as response:
        return json.load(response)


def build_number(tag):
    """The build number of a dev build's tag, e.g. 2120 for dev-2.16.0.0-2120."""
    match = re.fullmatch(r"dev-[\d.]+-(\d+)", tag)
    return int(match.group(1)) if match else None


def dev_builds(since):
    """The dev builds after build number since, oldest first."""
    builds = []
    page = 1
    while True:
        releases = api("repos/%s/releases?per_page=100&page=%d" % (UPSTREAM, page))
        numbered = [(build_number(r["tag_name"]), r) for r in releases]
        builds += [(n, r) for n, r in numbered if n is not None and n > since]
        # Releases come newest first, so a page reaching the pinned build is the last
        if not releases or any(n is not None and n <= since for n, _ in numbered):
            break
        page += 1
    return [r for _, r in sorted(builds, key=lambda b: b[0])]


def set_pin(section, values):
    """Set keys of a pins.toml section, keeping its comments and layout."""
    with open(PINS) as f:
        lines = f.read().splitlines(keepends=True)
    current = None
    remaining = dict(values)
    for i, line in enumerate(lines):
        header = re.fullmatch(r"\[(\w+)\]\s*", line)
        if header:
            current = header.group(1)
            continue
        key = re.match(r"(\w+) = ", line)
        if current == section and key and key.group(1) in remaining:
            lines[i] = '%s = "%s"\n' % (key.group(1), remaining.pop(key.group(1)))
    if remaining:
        sys.exit("pins.toml has no %s in [%s]" % (", ".join(remaining), section))
    with open(PINS, "w") as f:
        f.writelines(lines)


def mirror_url(version, build):
    return "https://github.com/%s/releases/download/v%s/MechJeb2-%s.zip" % (
        REPOSITORY,
        version,
        build,
    )


def git(*args, cwd=ROOT):
    return subprocess.check_output(["git", *args], cwd=cwd, text=True)


def status(args):
    pin = fetch.pins()["mechjeb"]
    print("pinned:", pin["version"])
    builds = dev_builds(build_number(pin["version"]))
    if not builds:
        print("no newer dev builds")
        return
    print("%d newer dev builds:" % len(builds))
    for release in builds:
        print("  %s  %s" % (release["tag_name"], release["published_at"][:10]))


def bump(args):
    pin = fetch.pins()["mechjeb"]
    if args.build:
        release = api("repos/%s/releases/tags/%s" % (UPSTREAM, args.build))
    else:
        builds = dev_builds(build_number(pin["version"]))
        if not builds:
            sys.exit("%s is the latest dev build" % pin["version"])
        release = builds[-1]
    tag = release["tag_name"]
    name = "MechJeb2-%s.zip" % tag
    assets = [a for a in release["assets"] if a["name"] == name]
    if not assets:
        sys.exit("%s has no %s" % (tag, name))
    upstream = assets[0]["browser_download_url"]

    ref = api("repos/%s/git/ref/tags/%s" % (UPSTREAM, tag))["object"]
    if ref["type"] == "tag":
        ref = api("repos/%s/git/tags/%s" % (UPSTREAM, ref["sha"]))["object"]

    os.makedirs(fetch.DEPS, exist_ok=True)
    archive = os.path.join(fetch.DEPS, "mechjeb.zip")
    fetch.get(upstream, archive)
    set_pin(
        "mechjeb",
        {
            "version": tag,
            "commit": ref["sha"],
            "url": upstream,
            "upstream": upstream,
            "sha256": fetch.sha256(archive),
        },
    )
    print("pinned %s, was %s" % (tag, pin["version"]))


def diff(args):
    base = args.base or git("describe", "--tags", "--abbrev=0", "--match", "v*").strip()
    old = tomllib.loads(git("show", base + ":pins.toml"))["mechjeb"]
    new = fetch.pins()["mechjeb"]
    if old["commit"] == new["commit"]:
        sys.exit("%s pins the same MechJeb build, %s" % (base, new["version"]))

    # A clone with no file contents, which git fetches as the diff reads them
    source = os.path.join(UPDATE, "MechJeb2")
    if not os.path.exists(source):
        os.makedirs(UPDATE, exist_ok=True)
        subprocess.check_call(
            [
                "git",
                "clone",
                "--quiet",
                "--filter=blob:none",
                "--no-checkout",
                "https://github.com/%s.git" % UPSTREAM,
                source,
            ]
        )
    else:
        git("fetch", "--quiet", "origin", cwd=source)
    # A commit on no branch, as a dev build can be, is fetched by its hash
    for commit in (old["commit"], new["commit"]):
        if subprocess.call(["git", "cat-file", "-e", commit], cwd=source) != 0:
            git("fetch", "--quiet", "origin", commit, cwd=source)

    span = "%s..%s" % (old["commit"], new["commit"])
    header = "MechJeb %s (%s) to %s (%s)\n\n" % (
        old["version"],
        old["commit"],
        new["version"],
        new["commit"],
    )
    outputs = {
        "log.txt": git("log", "--reverse", "--stat", span, cwd=source),
        "api.diff": git("diff", span, "--", "*.cs", cwd=source),
    }
    for name, text in outputs.items():
        with open(os.path.join(UPDATE, name), "w") as f:
            f.write(header + text)
    commits = git("rev-list", "--count", span, cwd=source).strip()
    files = git("diff", "--stat", span, "--", "*.cs", cwd=source).splitlines()
    print(header.strip())
    print("%s commits, %s" % (commits, files[-1].strip() if files else "no C# changes"))
    for name in outputs:
        print(os.path.relpath(os.path.join(UPDATE, name), ROOT))


def release(args):
    version = args.version
    with open(build_service.PROJECT) as f:
        project = f.read()
    with open(build_service.PROJECT, "w") as f:
        f.write(
            re.sub(
                r"<Version>[^<]*</Version>", "<Version>%s</Version>" % version, project
            )
        )

    pins = fetch.pins()
    with open(CHANGELOG) as f:
        changelog = f.read()
    heading = "## [v%s]\n\nBuilt against kRPC v%s, for MechJeb %s.\n" % (
        version,
        pins["krpc"]["version"],
        pins["mechjeb"]["version"],
    )
    if "## [Unreleased]\n" in changelog:
        changelog = changelog.replace("## [Unreleased]\n", heading, 1)
    else:
        # A release that only moves the pin
        first = changelog.index("## [")
        changelog = changelog[:first] + heading + "\n" + changelog[first:]
    with open(CHANGELOG, "w") as f:
        f.write(changelog)

    # A build pinned since the last release is mirrored by this one
    mechjeb = pins["mechjeb"]
    if mechjeb["url"] == mechjeb["upstream"]:
        set_pin("mechjeb", {"url": mirror_url(version, mechjeb["version"])})
    print("version %s, for MechJeb %s" % (version, mechjeb["version"]))


def changes(version):
    """The changelog section of a version, with its wrapped lines joined for GitHub."""
    with open(CHANGELOG) as f:
        text = f.read()
    match = re.search(
        r"^## \[v%s\]\n(.*?)(?=^## \[|\Z)" % re.escape(version), text, re.M | re.S
    )
    if not match:
        sys.exit("CHANGELOG.md has no [v%s] section" % version)
    lines = []
    for line in match.group(1).strip().splitlines():
        if line.startswith("Built against "):
            continue
        if re.match(r" +[^ -]", line) and lines and lines[-1]:
            lines[-1] += " " + line.strip()
        else:
            lines.append(line)
    return "\n".join(lines).strip() or "No changes to the service."


NOTES = """\
A replacement of kRPC's MechJeb service for MechJeb's dev builds. This is a best-effort \
project: report problems here, not on kRPC's tracker.

Built against kRPC v{krpc}, and tested with MechJeb {mechjeb}.

**Install.** Install kRPC v{krpc} and MechJeb {mechjeb}, then unzip \
`krpc-mechjeb-dev-{version}.zip` into the KSP folder. It replaces kRPC's MechJeb files in \
`GameData/kRPC`, so unzip it again after each kRPC update.

**Clients.** Each `krpc-mechjeb-dev-<client>-{version}.zip` holds the stub of the service \
for that client. The [Clients](https://krpc.github.io/krpc-mechjeb-dev/clients.html) page says \
how each client picks it up. Lua needs none.

{copy}

## Changes

{changes}
"""

MIRRORED = """\
**MechJeb.** `MechJeb2-{mechjeb}.zip` is a copy of the MechJeb build this release is tested \
with, as MechJeb removes old dev builds. MechJeb is under the GPLv3, and \
`MechJeb2-{mechjeb}-source.zip` is its source, from MechJeb's commit \
[{short}](https://github.com/MuMech/MechJeb2/commit/{commit})."""

MIRRORED_BEFORE = """\
**MechJeb.** The [v{mirror} release](https://github.com/krpc/krpc-mechjeb-dev/releases/tag/\
v{mirror}) has a copy of MechJeb {mechjeb}, as MechJeb removes old dev builds, with its source."""


def notes(args):
    version = build_service.version()
    pins = fetch.pins()
    mechjeb = pins["mechjeb"]
    mirror = re.search(r"/releases/download/v([^/]+)/", mechjeb["url"])
    if mirror is None:
        sys.exit("pins.toml fetches MechJeb from upstream: run update.py release first")
    values = {
        "version": version,
        "krpc": pins["krpc"]["version"],
        "mechjeb": mechjeb["version"],
        "commit": mechjeb["commit"],
        "short": mechjeb["commit"][:7],
        "mirror": mirror.group(1),
    }
    template = MIRRORED if mirror.group(1) == version else MIRRORED_BEFORE
    path = os.path.join(build_service.BUILD, "release-notes.md")
    with open(path, "w") as f:
        f.write(
            NOTES.format(
                copy=template.format(**values), changes=changes(version), **values
            )
        )
    print(os.path.relpath(path, ROOT))


def main():
    parser = argparse.ArgumentParser(description=(__doc__ or "").split("\n\n")[0])
    commands = parser.add_subparsers(required=True)
    commands.add_parser("status").set_defaults(run=status)
    command = commands.add_parser("bump")
    command.add_argument(
        "build", nargs="?", help="the dev build's tag (default: latest)"
    )
    command.set_defaults(run=bump)
    command = commands.add_parser("diff")
    command.add_argument(
        "--from",
        dest="base",
        help="the git ref of the old pin (default: the last release tag, v*)",
    )
    command.set_defaults(run=diff)
    command = commands.add_parser("release")
    command.add_argument("version", help="the release version, e.g. 1.1.0")
    command.set_defaults(run=release)
    commands.add_parser("notes").set_defaults(run=notes)
    args = parser.parse_args()
    args.run(args)


if __name__ == "__main__":
    main()
