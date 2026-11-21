import unittest

from mechjeb_test_case import MechJebTestCase


class TestAscentGuidance(MechJebTestCase):
    @classmethod
    def setUpClass(cls):
        super().setUpClass()
        cls.ascent = cls.mj.ascent_guidance

    def test_settings(self):
        ascent = self.ascent
        settings = ascent.settings
        classic = ascent.classic_path
        psg = ascent.psg_settings
        self.assert_settings(
            [
                (ascent, "desired_orbit_altitude", 123000),
                (ascent, "desired_inclination", 12),
                (ascent, "periapsis_altitude", 123000),
                (ascent, "apoapsis_altitude", 234000),
                (ascent, "burnout_altitude_flag", True),
                (ascent, "burnout_altitude", 123000),
                (ascent, "flight_path_angle", 2.5),
                (ascent, "argument_of_periapsis_flag", True),
                (ascent, "argument_of_periapsis", 12.0),
                (ascent, "desired_lan", 12),
                (ascent, "override_warp_to_plane", True),
                (ascent, "launch_lan_difference", 12),
                (ascent, "warp_count_down", 12),
                (settings, "force_roll", False),
                (settings, "vertical_roll", 12),
                (settings, "turn_roll", 12),
                (settings, "roll_altitude", 1234),
                (settings, "auto_deploy_solar_panels", False),
                (settings, "auto_deploy_antennas", False),
                (settings, "autostage", False),
                (settings, "autowarp", False),
                (classic, "turn_start_altitude", 1234),
                (classic, "turn_start_velocity", 123),
                (classic, "turn_end_altitude", 12345),
                (classic, "turn_end_angle", 12),
                (classic, "turn_shape_exponent", 0.5),
                (classic, "auto_path", False),
                (classic, "auto_turn_percent", 0.5),
                (classic, "auto_turn_speed_factor", 12),
                (classic, "corrective_steering", True),
                (classic, "corrective_steering_gain", 1.5),
                (classic, "limit_angle_of_attack", False),
                (classic, "max_angle_of_attack", 12),
                (classic, "angle_of_attack_limit_fadeout_pressure", 1234),
                (classic, "skip_circularization", True),
                (psg, "pitch_start_height", 123),
                (psg, "pitch_rate", 0.75),
                (psg, "drag_coefficient", 0.75),
                (psg, "reference_area", 12),
                (psg, "auto_aero", False),
                (psg, "limit_q_alpha", 1234.5),
                (psg, "min_delta_v", 12.5),
                (psg, "max_coast", 123),
                (psg, "min_coast", 12),
                (psg, "ullage_lead_time", 12),
                (psg, "spinup_lead_time", 12),
                (psg, "spinup_angular_velocity", 0.5),
            ]
        )

    def test_staging_and_thrust(self):
        staging = self.ascent.settings.staging
        thrust = self.ascent.settings.thrust
        self.assert_settings(
            [
                (staging, "autostage_pre_delay", 1.5),
                (staging, "autostage_post_delay", 1.5),
                (staging, "autostage_limit", 2),
                (staging, "fairing_max_dynamic_pressure_enabled", False),
                (staging, "fairing_max_dynamic_pressure", 1234),
                (staging, "fairing_min_altitude_enabled", False),
                (staging, "fairing_min_altitude", 12345),
                (staging, "fairing_max_aerothermal_flux_enabled", False),
                (staging, "fairing_max_aerothermal_flux", 1234),
                (staging, "clamp_auto_stage_thrust_pct", 0.5),
                (staging, "hot_staging", True),
                (staging, "hot_staging_lead_time", 1.5),
                (staging, "drop_solids", True),
                (staging, "drop_solids_twr_pct", 0.25),
                (thrust, "limit_to_prevent_overheats", True),
                (thrust, "limit_dynamic_pressure", True),
                (thrust, "max_dynamic_pressure", 12345),
                (thrust, "limit_acceleration", True),
                (thrust, "max_acceleration", 12),
                (thrust, "limit_throttle", True),
                (thrust, "max_throttle", 0.5),
                (thrust, "limiter_min_throttle", True),
                (thrust, "min_throttle", 0.25),
                (thrust, "electric_throttle", True),
                (thrust, "electric_throttle_lo", 0.25),
                (thrust, "electric_throttle_hi", 0.5),
                (thrust, "limit_to_prevent_unstable_ignition", True),
                (thrust, "auto_rcs_ullaging", False),
            ]
        )

    def test_own_thrust_limits(self):
        # The ascent's limits are separate from the general ones
        ascent = self.ascent.settings.thrust
        general = self.mj.utilities.thrust
        old = general.max_acceleration
        try:
            general.max_acceleration = old + 1
            self.assertNotEqual(general.max_acceleration, ascent.max_acceleration)
        finally:
            general.max_acceleration = old

    def test_read_only(self):
        ascent = self.ascent
        classic = ascent.classic_path
        psg = ascent.psg_settings
        self.assertFalse(ascent.enabled)
        self.assertEqual(self.mj.AscentLaunchMode.normal, ascent.launch_mode)
        self.assertGreater(classic.auto_turn_start_altitude, 0)
        self.assertGreater(classic.auto_turn_start_velocity, 0)
        self.assertGreater(classic.auto_turn_end_altitude, 0)
        self.assertFalse(ascent.settings.limiting_angle_of_attack)
        self.assertIsInstance(ascent.settings.current_max_angle_of_attack, float)
        self.assertFalse(psg.has_solution)
        self.assertIsInstance(psg.guidance_status, str)

    def test_ascent_type(self):
        ascent = self.ascent
        types = self.mj.AscentType
        self.assertEqual(types.classic, ascent.ascent_type)
        ascent.ascent_type = types.psg
        self.assertEqual(types.psg, ascent.ascent_type)
        ascent.ascent_type = types.classic
        self.assertEqual(types.classic, ascent.ascent_type)

    def test_enable(self):
        ascent = self.ascent
        status = ascent.status
        ascent.enabled = True
        try:
            self.assertTrue(ascent.enabled)
            self.wait_until(lambda: ascent.status != status)
        finally:
            ascent.enabled = False
        self.assertFalse(ascent.enabled)

    def test_type_change_disables(self):
        ascent = self.ascent
        ascent.enabled = True
        try:
            ascent.ascent_type = self.mj.AscentType.psg
            self.assertFalse(ascent.enabled)
            ascent.enabled = True
            self.assertTrue(ascent.enabled)
            ascent.ascent_type = self.mj.AscentType.classic
            self.assertFalse(ascent.enabled)
        finally:
            ascent.enabled = False
            ascent.ascent_type = self.mj.AscentType.classic

    def test_timed_launch(self):
        ascent = self.ascent
        modes = self.mj.AscentLaunchMode
        # The Mun's orbit is equatorial, so a launch into its plane can go at once
        self.sc.target_body = self.sc.bodies["Minmus"]
        try:
            target = self.mj.target_controller
            self.wait_until(lambda: target.normal_target_exists)
            ascent.launch_to_target_plane()
            self.assertEqual(modes.target_plane, ascent.launch_mode)
            self.assertGreater(ascent.time_to_launch, 0)
            ascent.abort_timed_launch()
            self.assertEqual(modes.normal, ascent.launch_mode)
        finally:
            self.sc.clear_target()
        with self.assertRaisesRegex(RuntimeError, "no target"):
            ascent.launch_to_target_plane()

    def test_lan_launches(self):
        ascent = self.ascent
        modes = self.mj.AscentLaunchMode
        with self.assertRaisesRegex(RuntimeError, "PSG"):
            ascent.launch_to_lan()
        ascent.ascent_type = self.mj.AscentType.psg
        self.sc.target_body = self.sc.bodies["Mun"]
        try:
            ascent.launch_to_lan()
            self.assertEqual(modes.lan, ascent.launch_mode)
            ascent.abort_timed_launch()
            self.assertEqual(modes.normal, ascent.launch_mode)

            target = self.mj.target_controller
            self.wait_until(lambda: target.normal_target_exists)
            ascent.launch_to_target_lan()
            self.assertEqual(modes.target_lan, ascent.launch_mode)
            ascent.abort_timed_launch()
            self.assertEqual(modes.normal, ascent.launch_mode)
        finally:
            ascent.ascent_type = self.mj.AscentType.classic
            self.sc.clear_target()


class TestAscentGuidancePSG(MechJebTestCase):
    """A PSG ascent of the stock Kerbal X to an 80 km circular orbit."""

    @classmethod
    def setUpClass(cls):
        cls.vessel = cls.launch("Kerbal X")
        cls.conn = cls.connect()
        cls.mj = cls.conn.mech_jeb
        cls.sc = cls.conn.space_center
        cls.wait_until(lambda: cls.mj.ready)

    def test_launch_to_orbit(self):
        ascent = self.mj.ascent_guidance
        ascent.ascent_type = self.mj.AscentType.psg
        ascent.periapsis_altitude = 80000
        ascent.apoapsis_altitude = 0
        ascent.desired_inclination = 0
        ascent.settings.autostage = True
        ascent.enabled = True
        try:
            self.wait_until(lambda: ascent.psg_settings.has_solution, timeout=60)
            # The autopilot waits on the pad for the first stage
            self.vessel.control.activate_next_stage()
            # The autopilot disengages once the vessel is in orbit
            self.wait_until(lambda: not ascent.enabled, timeout=600)
            orbit = self.vessel.orbit
            self.assertAlmostEqual(80000, orbit.periapsis_altitude, delta=5000)
            self.assertAlmostEqual(80000, orbit.apoapsis_altitude, delta=10000)
        finally:
            ascent.enabled = False
            ascent.ascent_type = self.mj.AscentType.classic


if __name__ == "__main__":
    unittest.main()
