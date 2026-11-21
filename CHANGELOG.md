Changes relative to the MechJeb service of the kRPC release each version is built against.
The changes kRPC's service made before then are in kRPC's own changelog.

## [v1.0.0] - unreleased

Built against kRPC v0.8.0, for MechJeb dev-2.16.0.0-2120.

- Start from the MechJeb service of kRPC v0.8.0
- Bind the maneuver planner, attitude adjustment, solar panel, antenna and vessel state members
  MechJeb 2.16 renamed
- `RCSThrottle` and `LimiterMinThrottle` default to false, as in MechJeb 2.16
- `DeployableController.RetractAll` leaves parts that are still extending extended, as in
  MechJeb 2.16

- Removed in MechJeb 2.16
  - **Breaking:** Remove `AircraftAutopilot` and `AircraftApproach`, as MechJeb 2.16 has no
    aircraft autopilot
  - **Breaking:** Remove `OperationInterplanetaryTransfer`. Use `OperationAdvancedTransfer`

- Maneuver planner
  - **Breaking:** Replace `OperationTransfer.Rendezvous` with `MatchOrbit`, its opposite, as in
    MechJeb 2.16
