"""Generate the client stubs of the service into build/stubs, with krpc-clientgen, and an
archive of each for the release, build/krpc-mechjeb-dev-<client>-<version>.zip.

Each file but the Python one has the path and name of the stub the client package ships, so it
can take its place. The Clients page of the docs says how each client picks it up.
"""

import os
import shutil
import subprocess
import zipfile

import build_service
from build_service import tool

ROOT = build_service.ROOT
STUBS = os.path.join(build_service.BUILD, "stubs")

# The stub of each client, as its package ships it, and the include path of the SpaceCenter
# code it uses, for the C++ and C-nano generators
CLIENTS = {
    # A module of its own, as a krpc package ahead of the client's would hide the client
    "python": ("python/krpc_mechjeb_dev.py", None),
    "csharp": ("csharp/MechJeb.cs", None),
    "cpp": ("cpp/include/krpc/services/mech_jeb.hpp", "krpc/services/space_center.hpp"),
    "cnano": (
        "cnano/include/krpc_cnano/services/mech_jeb.h",
        "krpc_cnano/services/space_center.h",
    ),
    "java": ("java/krpc/client/services/MechJeb.java", None),
    "js": ("js/mechjeb.ts", None),
}


def strip_server_types(path):
    """Remove the TypeScript stub's declarations of the service's server types. The client
    package declares them for its own MechJeb stub, and TypeScript rejects a second declaration
    of the same names with other types."""
    with open(path, encoding="utf-8") as f:
        text = f.read()
    start = text.index('declare module "@krpc-org/krpc" {')
    end = text.index("\n}\n", start) + 3
    with open(path, "w", encoding="utf-8") as f:
        f.write(text[:start].rstrip() + "\n" + text[end:])


def package(language, path, version):
    """The release archive of a client's stub, at its path in the client's folder. Code
    krpctools generates for a client is under the client's license, as kRPC's LICENSE says, so
    the archive does not carry the service's."""
    archive = os.path.join(
        build_service.BUILD, "krpc-mechjeb-dev-%s-%s.zip" % (language, version)
    )
    with zipfile.ZipFile(archive, "w") as f:
        name = path.split("/", 1)[1]
        build_service.write(f, name, build_service.read(os.path.join(STUBS, path)))
    return archive


def main():
    build_service.main()
    version = build_service.version()
    shutil.rmtree(STUBS, ignore_errors=True)
    definitions = os.path.join(build_service.BIN, "KRPC.MechJeb.json")
    for language, (path, space_center) in CLIENTS.items():
        output = os.path.join(STUBS, path)
        os.makedirs(os.path.dirname(output), exist_ok=True)
        args = [tool("krpc-clientgen"), language, "MechJeb", definitions, "-o", output]
        if space_center is not None:
            args.append("--include=SpaceCenter=" + space_center)
        subprocess.check_call(args)
        if language == "js":
            strip_server_types(output)
        print(os.path.relpath(package(language, path, version), ROOT))


if __name__ == "__main__":
    main()
