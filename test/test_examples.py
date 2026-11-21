"""The Python examples of the docs, run against the game as a reader runs them."""

import os
import subprocess
import sys
import unittest

from krpctools.game.ksp import TestCase

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SCRIPTS = os.path.join(ROOT, "doc", "src", "scripts")


class ExampleTestCase(TestCase):
    mods = ["MechJeb"]
    craft = ""

    @classmethod
    def setUpClass(cls):
        cls.vessel = cls.launch(cls.craft)
        cls.conn = cls.connect()
        cls.wait_until(lambda: cls.conn.mech_jeb.ready)

    def run_example(self, path, timeout):
        env = dict(
            os.environ, PYTHONPATH=os.path.join(ROOT, "build", "stubs", "python")
        )
        result = subprocess.run(
            [sys.executable, os.path.join(SCRIPTS, path)],
            check=False,
            env=env,
            timeout=timeout,
            capture_output=True,
            text=True,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        return result.stdout


class TestMechJebExample(ExampleTestCase):
    craft = "Kerbal X"

    def test_example(self):
        output = self.run_example("api/mechjeb/MechJebExample.py", timeout=900)
        self.assertIn("Launch complete", output)
        orbit = self.conn.space_center.active_vessel.orbit
        self.assertGreater(orbit.periapsis_altitude, 70000)


class TestLaunchToMinmus(ExampleTestCase):
    craft = "PT Series Munsplorer"

    def test_tutorial(self):
        self.run_example("tutorials/mechjeb/LaunchToMinmus.py", timeout=3600)
        vessel = self.conn.space_center.active_vessel
        self.assertEqual("Minmus", vessel.orbit.body.name)
        self.assertEqual(
            self.conn.space_center.VesselSituation.landed, vessel.situation
        )


if __name__ == "__main__":
    unittest.main()
