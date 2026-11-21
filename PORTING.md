# Port to MechJeb 2.16

The state of the port to dev-2.16.0.0-2120. `CHANGELOG.md` lists what changed for users.

## Baseline

kRPC v0.8.0's service, built here with `dotnet`, against MechJeb 2.15.4.0
(`pytest --mechjeb=release`): 78 of 79 tests pass, and every member binds.
`TestManeuverPlannerInOrbit.test_advanced_transfer` failed once, with MechJeb's
`PlaceManeuverNode: bad dV: [NaN, NaN, NaN]`, and passed on its own.

## Ported

Every member binds against the dev build (`test_bindings.py`), from 70 missing at the start:
the renames, the removed aircraft autopilot and interplanetary transfer, and the PSG ascent.

The docs still link to the removed members from `doc/api/mechjeb/migration.tmpl`, kRPC's page
for moving from KRPC.MechJeb 0.7. Phase 4 replaces it with a page for moving from kRPC's
service.

## Upstream

* `OperationTransfer` with `Capture` off makes a node with no encounter.
  `test_transfer_without_capture` fails. MechJeb's "Flyby/Impact" mode passes `rendezvous: false`
  to `DeltaVAndTimeForHohmannTransfer` (`Capture && !MatchOrbit`), so the burn reaches the
  target's orbit but not the target. 2.15 passed `Rendezvous`, true by default.
* A PSG ascent of the stock Kerbal X to an 80 km circular orbit ended once with a periapsis
  of about 39 km, and reached the orbit on the next run. In the miss, guidance converged and
  flew all five stages to `TERMINAL -> FINISHED`, as the terminal check compares only the
  angular momentum, which matched the target. The eccentricity came out at about 0.06.
  `TestAscentGuidancePSG` may fail now and then.
* The hoverslam autopilot lands the Basic craft from a near-vertical fall. From a 31 km
  apoapsis with 560 m/s of horizontal speed, it touches down at 25 m/s sideways and the craft
  breaks up. `TestHoverslamLanding` flies the near-vertical fall.

## New in 2.16

Added: the hoverslam autopilot and its prediction, the stationary orbit operation, the porkchop
selection and target inclination of the advanced transfer, the inclination of the course
correction and moon return, the node executor's warp and alignment settings, the landing's own
autowarp and minimum throttle, its jettison before entry, and the new autostaging options.

Not added:

* PSG's stage plan, edited in MechJeb's PSG Stages window. kOS does not expose it either.
* The porkchop search window: the departure and transfer time ranges, which `Compute` takes.
  The search uses MechJeb's defaults.
