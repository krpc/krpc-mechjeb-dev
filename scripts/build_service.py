"""Build the service and its release archive, and the kRPC archive the tests install.

Outputs, under build/:

* bin/: KRPC.MechJeb.dll, .xml and .json, the service definitions from krpc-servicedefs.
* krpc-mechjeb-dev-<version>.zip: the release, the four MechJeb files of kRPC's archive under
  GameData/kRPC, to unzip over a kRPC install.
* krpc-<krpc version>-mechjeb-dev-<version>.zip: the pinned kRPC archive with its MechJeb files
  replaced by the built ones, as a user's install is after unzipping the release over it.
"""

import os
import subprocess
import sys
import zipfile

import fetch

ROOT = fetch.ROOT
BUILD = os.path.join(ROOT, "build")
BIN = os.path.join(BUILD, "bin")
PROJECT = os.path.join(ROOT, "src", "KRPC.MechJeb.csproj")
KRPC_DIR = os.path.join(fetch.DEPS, "krpc", "GameData", "kRPC")
MANAGED_DIR = os.path.join(fetch.DEPS, "ksp", "KSP_Data", "Managed")

# The services the service depends on, which krpc-servicedefs loads alongside it
DEPENDENCIES = ["KRPC.SpaceCenter.dll"]

# A fixed timestamp for every archive member, as in kRPC's archive
TIMESTAMP = (1980, 1, 1, 0, 0, 0)


def version():
    return subprocess.check_output(
        ["dotnet", "msbuild", PROJECT, "-getProperty:Version"], text=True
    ).strip()


def servicedefs():
    """The path of krpc-servicedefs, from the virtual environment running this script."""
    path = os.path.join(os.path.dirname(sys.executable), "krpc-servicedefs")
    return path if os.path.exists(path) else "krpc-servicedefs"


def build():
    subprocess.check_call(
        [
            "dotnet",
            "build",
            PROJECT,
            "-c",
            "Release",
            "-o",
            BIN,
            "-nologo",
            "-v",
            "quiet",
        ]
    )
    subprocess.check_call(
        [
            servicedefs(),
            "MechJeb",
            os.path.join(BIN, "KRPC.MechJeb.dll"),
            *[os.path.join(KRPC_DIR, name) for name in DEPENDENCIES],
            "--reference-dir",
            KRPC_DIR,
            "--reference-dir",
            MANAGED_DIR,
            "-o",
            os.path.join(BIN, "KRPC.MechJeb.json"),
        ]
    )


def service_files():
    """The files the release holds, by their path in the archive."""
    files = {
        "GameData/kRPC/KRPC.MechJeb." + ext: os.path.join(BIN, "KRPC.MechJeb." + ext)
        for ext in ("dll", "xml", "json")
    }
    files["GameData/kRPC/CHANGELOG.MechJeb.md"] = os.path.join(ROOT, "CHANGELOG.md")
    return files


def write(archive, name, data):
    info = zipfile.ZipInfo(name, TIMESTAMP)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.external_attr = 0o644 << 16
    archive.writestr(info, data)


def read(path):
    with open(path, "rb") as f:
        return f.read()


def package(version):
    files = service_files()
    release = os.path.join(BUILD, "krpc-mechjeb-dev-%s.zip" % version)
    with zipfile.ZipFile(release, "w") as archive:
        for name, path in sorted(files.items()):
            write(archive, name, read(path))
        write(archive, "LICENSE.KRPC.MechJeb", read(os.path.join(ROOT, "LICENSE")))

    krpc = fetch.pins()["krpc"]["version"]
    repacked = os.path.join(BUILD, "krpc-%s-mechjeb-dev-%s.zip" % (krpc, version))
    with zipfile.ZipFile(os.path.join(fetch.DEPS, "krpc.zip")) as src:
        with zipfile.ZipFile(repacked, "w") as dst:
            for info in src.infolist():
                if info.filename in files:
                    continue
                dst.writestr(info, src.read(info))
            for name, path in sorted(files.items()):
                write(dst, name, read(path))
    return release, repacked


def main():
    fetch.main()
    build()
    for path in package(version()):
        print(os.path.relpath(path, ROOT))


if __name__ == "__main__":
    main()
