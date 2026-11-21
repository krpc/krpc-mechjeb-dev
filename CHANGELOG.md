Changes relative to the MechJeb service of the kRPC release each version is built against.
The changes kRPC's service made before then are in kRPC's own changelog.

## [v1.0.0]

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

- Ascent
  - **Breaking:** Replace the PVG ascent with PSG, Polynomial SQP Guidance: `AscentType.PVG`
    with `AscentType.PSG`, and `PVGSettings` with `PSGSettings`. PSG plans the stages itself,
    so the PVG stage settings go
  - **Breaking:** Set the PSG target orbit with `AscentGuidance.PeriapsisAltitude`,
    `ApoapsisAltitude`, `BurnoutAltitude`, `FlightPathAngle` and `ArgumentOfPeriapsis`, in place
    of `DesiredApoapsis`, `AttachAltFlag`, `DesiredAttachAlt` and `DesiredFPA`
  - **Breaking:** Move `CorrectiveSteering`, `CorrectiveSteeringGain`, `LimitAngleOfAttack`,
    `MaxAngleOfAttack`, `AngleOfAttackLimitFadeoutPressure` and `SkipCircularization` from
    `AscentSettings` to `ClassicPath`, as they apply to the classic ascent only
  - **Breaking:** `AscentSettings.Staging` and `Thrust` are the ascent's own settings,
    `AscentStaging` and `AscentThrustLimits`, which apply in place of the general ones while
    the ascent flies
  - **Breaking:** `AscentSettings.Autowarp` is the ascent's own, no longer shared with the
    maneuver planner
  - **Breaking:** Rename `AutodeploySolarPanels` to `AutoDeploySolarPanels`
  - **Breaking:** Remove `LaunchToRendezvous`, `LaunchPhaseAngle`, and the `Rendezvous` and
    `Unknown` launch modes, as MechJeb 2.16 times every launch itself
  - **Breaking:** Rename `LaunchToMatchLan` and `AscentLaunchMode.MatchLan` to
    `LaunchToTargetLan` and `TargetLan`
  - Add `AscentGuidance.TimeToLaunch`, and the guidance state of the PSG ascent to
    `PSGSettings`

- Maneuver planner
  - **Breaking:** Replace `OperationTransfer.Rendezvous` with `MatchOrbit`, its opposite, as in
    MechJeb 2.16
  - **Breaking:** `OperationAdvancedTransfer` defaults to MechJeb's porkchop mode. Set `Mode`
    to `LimitedTime` for the search up to `MaxArrivalTime`
  - Add the porkchop selection to `OperationAdvancedTransfer`, with `SelectLowestDeltaV`,
    `SelectAsap`, `Select` and the selected departure, duration and delta-v
  - Add `TargetInclination` to `OperationAdvancedTransfer`, and `Inclination` to
    `OperationCourseCorrection` and `OperationMoonReturn`
  - Add `OperationStationaryOrbit`
  - Add `InitialWarpLeadTime`, `AlignedTolerance` and `WarpAlignedTolerance` to the node
    execution settings of `ManeuverPlanner`

- Add `HoverslamAutopilot`, MechJeb 2.16's single-burn landing, with its landing prediction
- **Breaking:** `LandingGuidance.Autowarp` is the landing's own, no longer shared with the
  maneuver planner
- Add `LimiterMinThrottle`, `MinThrottle` and `JettisonBeforeEntry` to `LandingGuidance`
- Add the fairing conditions and `DropSolids` to `Autostaging`
