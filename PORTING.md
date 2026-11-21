# Port to MechJeb 2.16

The work list for the port, from the first test runs on 2026-11-21. Remove each entry as it is
ported.

## Baseline

kRPC v0.8.0's service, built here with `dotnet`, against MechJeb 2.15.4.0
(`pytest --mechjeb=release`): 78 of 79 tests pass, and every member binds.
`TestManeuverPlannerInOrbit.test_advanced_transfer` failed once, with MechJeb's
`PlaceManeuverNode: bad dV: [NaN, NaN, NaN]`, and passed on its own.

## Against dev-2.16.0.0-2120

70 members fail to bind (`test_bindings.py`). 14 other tests and 85 subtests fail, all but three
on one of those members. The tests of the docking, landing, rendezvous and rover autopilots,
Smart A.S.S., the target controller and Translatron pass.

| Area | Missing in the dev build | Failing tests |
| --- | --- | --- |
| Aircraft autopilot | `MechJebModuleAirplaneAutopilot` | `AircraftAutopilot` settings (27) |
| Aircraft approach | `MechJebModuleSpaceplaneAutopilot`, `Runway` | `test_autoland`, `test_runways`, settings (5) |
| Ascent | `MechJebModuleAscentBaseAutopilot` (2 bindings), `MechJebModuleGuidanceController`, `MechJebModuleAscentMenu.TimeToPhaseAngle`, 46 members of `MechJebModuleAscentSettings` | 7 of 9 ascent tests, settings of `AscentGuidance`, `AscentSettings`, `ClassicPath`, `PVGSettings` (40) |
| Attitude | `MechJebModuleAttitudeController.RCS_auto`, `.activeController` | `test_default_controller`, settings (2) |
| Utilities | `MechJebModuleDeployableController.autoDeploy` | settings (2) |
| Maneuver planner | `OperationAdvancedTransfer` (6 members), `OperationInterplanetaryTransfer`, `OperationGeneric.Rendezvous`, `OperationCourseCorrection.CourseCorrectFinalPeA`, `OperationMoonReturn.MoonReturnAltitude` | `test_advanced_transfer`, `test_transfer_without_capture`, `test_transfer_with_capture`, settings (7) |
| Vessel state | `VesselState.time`, `.latitude`, `.longitude`, `.celestialLongitude` | None directly |

The three failures with every member bound:

* `SmartRCS.rcs_throttle` and `ThrustLimiter.limiter_min_throttle` now default to false, the value
  the setting tests set.
* `test_solar_panels`: the panels extend, but `all_retracted` stays false for 60 s after
  `retract_all`.

`build/dev.log` from `pytest -v -rfE` has each error.
