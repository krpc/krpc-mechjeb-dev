"""The mirror of the pinned MechJeb dev build, for the release: the build as MechJeb releases it,
and its source, into build/.

MechJeb deletes old dev builds, and their tags with them. MechJeb is under the GPLv3, so a
release that carries the build carries its source too, from the commit in pins.toml.
"""

import os
import shutil
import urllib.request

import build_service
import fetch


def main():
    pin = fetch.pins()["mechjeb"]
    build = os.path.join(build_service.BUILD, "MechJeb2-%s.zip" % pin["version"])
    # From upstream, as the mirror is what this makes
    shutil.copy(fetch.fetch("mechjeb", dict(pin, url=pin["upstream"])), build)
    source = os.path.join(
        build_service.BUILD, "MechJeb2-%s-source.zip" % pin["version"]
    )
    url = "https://github.com/MuMech/MechJeb2/archive/%s.zip" % pin["commit"]
    with urllib.request.urlopen(url, timeout=60) as response, open(source, "wb") as f:
        shutil.copyfileobj(response, f)
    for path in (build, source):
        print(os.path.relpath(path, build_service.ROOT))


if __name__ == "__main__":
    main()
