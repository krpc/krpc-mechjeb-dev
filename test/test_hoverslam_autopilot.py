import math
import unittest

from mechjeb_test_case import MechJebTestCase


class TestHoverslamAutopilot(MechJebTestCase):
    def test_settings(self):
        hoverslam = self.mj.hoverslam_autopilot
        self.assert_settings(
            [
                (hoverslam, "ignition_lead", 1.5),
                (hoverslam, "touchdown_speed", 1.5),
                (hoverslam, "autowarp", False),
                (hoverslam, "hold_upright", True),
                (hoverslam, "autostage", False),
                (hoverslam, "pwm_pulse_width", 0.25),
                (hoverslam, "map_landing_prediction", False),
                (hoverslam, "recalculation_interval", 2.5),
                (hoverslam, "vertical_authority", 0.75),
                (hoverslam, "vertical_altitude", 123),
            ]
        )

    def test_read_only(self):
        hoverslam = self.mj.hoverslam_autopilot
        self.assertFalse(hoverslam.enabled)
        self.assertEqual(self.mj.HoverslamState.disabled, hoverslam.state)
        self.assertFalse(hoverslam.has_prediction)
        self.assertEqual("N/A", hoverslam.landing_biome)


class TestHoverslamLanding(MechJebTestCase):
    """The Basic craft lands on the Mun, falling from 20 km with little horizontal speed."""

    @classmethod
    def setUpClass(cls):
        super().setUpClass()
        epoch = cls.sc.ut
        # The apoapsis of an orbit 220 km from the Mun's center, 77 m/s sideways
        eccentricity = 0.98
        cls.set_orbit(
            "Mun", 220000 / (1 + eccentricity), eccentricity, 0, 0, 0, math.pi, epoch
        )
        cls.vessel.control.activate_next_stage()

    def test_landing(self):
        hoverslam = self.mj.hoverslam_autopilot
        self.wait_until(lambda: hoverslam.has_prediction, timeout=60)
        self.assertGreater(hoverslam.time_to_ignition, 0)
        self.assertGreater(hoverslam.time_to_landing, hoverslam.time_to_ignition)
        self.assertGreater(hoverslam.delta_v, 0)
        hoverslam.enabled = True
        try:
            situation = self.sc.VesselSituation
            self.wait_until(
                lambda: self.vessel.situation == situation.landed, timeout=600
            )
            self.assertEqual("Mun", self.vessel.orbit.body.name)
        finally:
            hoverslam.enabled = False


if __name__ == "__main__":
    unittest.main()
