# Port to MechJeb 2.16

The work list for the port, from the test runs against dev-2.16.0.0-2120. Remove each entry as it
is ported.

## Baseline

kRPC v0.8.0's service, built here with `dotnet`, against MechJeb 2.15.4.0
(`pytest --mechjeb=release`): 78 of 79 tests pass, and every member binds.
`TestManeuverPlannerInOrbit.test_advanced_transfer` failed once, with MechJeb's
`PlaceManeuverNode: bad dV: [NaN, NaN, NaN]`, and passed on its own.

## To Port

54 members fail to bind (`test_bindings.py`), down from 70 at the start.

| Area | Missing in the dev build | Failing tests |
| --- | --- | --- |
| Aircraft autopilot | `MechJebModuleAirplaneAutopilot` | `AircraftAutopilot` settings (27) |
| Aircraft approach | `MechJebModuleSpaceplaneAutopilot`, `Runway` | `test_autoland`, `test_runways`, settings (5) |
| Ascent | `MechJebModuleAscentBaseAutopilot` (2 bindings), `MechJebModuleGuidanceController`, `MechJebModuleAscentMenu.TimeToPhaseAngle`, 46 members of `MechJebModuleAscentSettings` | 7 of 9 ascent tests, settings of `AscentGuidance`, `AscentSettings`, `ClassicPath`, `PVGSettings` (40) |
| Interplanetary transfer | `OperationInterplanetaryTransfer`, removed in MechJeb | `OperationInterplanetaryTransfer` setting (1) |

## Upstream

* `OperationTransfer` with `Capture` off makes a node with no encounter.
  `test_transfer_without_capture` fails. MechJeb's "Flyby/Impact" mode passes `rendezvous: false`
  to `DeltaVAndTimeForHohmannTransfer` (`Capture && !MatchOrbit`), so the burn reaches the
  target's orbit but not the target. 2.15 passed `Rendezvous`, true by default.

## New in 2.16

Members the service could add once the port is done:

* `Inclination` and `InclinationFlag` on `OperationCourseCorrection` and `OperationMoonReturn`.
* `OperationAdvancedTransfer`: the porkchop mode, through `Compute`, `SelectTimes` and
  `TryGetSelection`, and the target inclination.
* `OperationStationaryOrbit`, a new operation.
* Per-autopilot throttle limits, `IThrustLimits` through `MechJebModuleThrustController.Limits`.

`build/dev-step1.log` from `pytest -v -rfE` has each error.
