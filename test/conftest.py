"""The game the tests run against: KSP with the pinned kRPC release, this repository's MechJeb
service in place of kRPC's, and the pinned MechJeb dev build.

``--mechjeb=release`` installs the MechJeb release kRPC's registry pins in place of the dev
build, as the baseline for the service the dev build breaks.
"""

import os
import sys

from krpctools.game import install, register
from krpctools.game.ksp import KSP, Mod
from krpctools.game.ksp.game import source_digest

sys.path.insert(0, os.path.join(os.path.dirname(os.path.dirname(__file__)), "scripts"))

import build_service as build
import fetch


class MechJebDev(Mod):
    """The MechJeb dev build in pins.toml. It has no version, so it replaces every version of
    MechJeb in kRPC's registry."""

    name = "MechJeb"
    folders = ("MechJeb2",)

    def __init__(self, pin):
        super().__init__(pin["url"], pin["sha256"])

    def present(self, game, conn):
        return conn.mech_jeb.available

    def installed(self, game):
        if not game.has_install():
            return None
        name = "MechJeb2.dll"
        expected = source_digest(
            install.fetch(self.source, self.sha256), self.folders, name
        )
        return game.installed_digest(self, name) == expected


class MechJebDevKSP(KSP):
    """KSP with the service built from this repository."""

    def __init__(self, mechjeb):
        super().__init__(
            mods=[MechJebDev(fetch.pins()["mechjeb"])] if mechjeb == "dev" else []
        )

    def install(self, mods=()):
        if self.krpc is None:
            build.main()
        super().install(mods)

    def krpc_source(self):
        if self.krpc is not None:
            return self.krpc, None
        krpc = fetch.pins()["krpc"]["version"]
        path = os.path.join(
            build.BUILD, "krpc-%s-mechjeb-dev-%s.zip" % (krpc, build.version())
        )
        return path, None

    def mod_problems(self, conn):
        # The members the service fails to bind are reported by test_bindings.py. Leaving
        # them out here lets the tests of the other members run.
        problems = super().mod_problems(conn)
        problems.pop("MechJeb", None)
        return problems


def pytest_addoption(parser):
    parser.getgroup("krpc-mechjeb-dev").addoption(
        "--mechjeb",
        choices=["dev", "release"],
        default="dev",
        help="the MechJeb to test against: the dev build in pins.toml, or the release kRPC "
        "pins (default: dev)",
    )


def pytest_configure(config):
    register(MechJebDevKSP(config.getoption("mechjeb")))


def pytest_report_header(config):
    if config.getoption("mechjeb") == "dev":
        mechjeb = fetch.pins()["mechjeb"]["version"]
    else:
        mechjeb = "kRPC's pinned release"
    return "krpc-mechjeb-dev: MechJeb %s, kRPC %s" % (
        mechjeb,
        fetch.pins()["krpc"]["version"],
    )
