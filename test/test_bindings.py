import unittest

from krpctools.game.ksp import TestCase


class TestBindings(TestCase):
    """The service binds every MechJeb member it uses. A failure lists the members the
    installed MechJeb is missing, or why the integration did not load."""

    mods = ["MechJeb"]

    def test_bindings(self):
        problems = dict(self.connect().testing_tools.mod_problems())
        problem = problems.get("MechJeb")
        if problem is None:
            return
        if not problem.startswith("missing "):
            self.fail("The MechJeb integration did not load: " + problem)
        members = sorted(problem.removeprefix("missing ").split(", "))
        self.fail(
            "MechJeb is missing %d members the service binds:\n  %s"
            % (len(members), "\n  ".join(members))
        )


if __name__ == "__main__":
    unittest.main()
