using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The Ascent Guidance window, which flies the vessel to orbit.
    /// </summary>
    /// <remarks>
    /// See the <a href="https://github.com/MuMech/MechJeb2/wiki/Ascent-Guidance">MechJeb wiki</a>
    /// for how to set up this autopilot.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class AscentGuidance : KRPCComputerModule
    {
        const string settingsName = "MechJebModuleAscentSettings";
        const string classicName = "MechJebModuleAscentClassicAutopilot";
        const string psgName = "MechJebModulePSGAutopilot";
        const string simulationName = "MechJebModulePSGSimulation";
        const string countdownName = "MechJebModuleLaunchCountdown";
        // The values of MechJeb's AscentType
        const int classic = 0;
        const int psg = 1;

        static Func<object, object> autopilot;
        static Func<object, object> status;
        static Func<object, string> statusText;

        static Field<object, int> ascentType;
        static Field<object, double> desiredOrbitAltitude;
        static Field<object, double> desiredInclination;
        static Field<object, double> launchLANDifference;
        static Field<object, int> warpCountDown;
        static Field<object, bool> overrideWarpToPlane;
        static Func<object, bool> launchingToPlane;
        static Func<object, bool> launchingToMatchLan;
        static Func<object, bool> launchingToLan;

        static Field<object, double> periapsisAltitude;
        static Field<object, double> apoapsisAltitude;
        static Field<object, bool> burnoutAltitudeFlag;
        static Field<object, double> burnoutAltitude;
        static Field<object, double> flightPathAngle;
        static Field<object, bool> argumentOfPeriapsisFlag;
        static Field<object, double> argumentOfPeriapsis;
        static Field<object, double> lan;

        static Func<object, bool> countdownActive;
        static Func<object, double> tMinus;
        static Func<object, double> startToPlaneOfTarget;
        static Func<object, double> startToTargetLan;
        static Func<object, double> startToLan;
        static Action<object> abort;

        internal static new void Load ()
        {
            var settings = Bindings.Type ("MuMech." + settingsName);
            autopilot = settings.Getter<object, object> ("AscentAutopilot");
            status = Bindings.Type ("MuMech.IAscentAutopilot").Getter<object, object> ("Status");
            statusText = Bindings.Type ("MuMech.AscentStatus").Getter<object, string> ("Text");

            ascentType = settings.Field<object, int> ("AscentType");
            desiredOrbitAltitude = Bindings.EditableDouble (settings, "DesiredOrbitAltitude");
            desiredInclination = Bindings.EditableDouble (settings, "DesiredInclination");
            launchLANDifference = Bindings.EditableDouble (settings, "LaunchLANDifference");
            warpCountDown = Bindings.EditableInt (settings, "WarpCountDown");
            overrideWarpToPlane = settings.Field<object, bool> ("OverrideWarpToPlane");
            launchingToPlane = settings.Getter<object, bool> ("LaunchingToPlane");
            launchingToMatchLan = settings.Getter<object, bool> ("LaunchingToMatchLan");
            launchingToLan = settings.Getter<object, bool> ("LaunchingToLan");

            var simulation = Bindings.Type ("MuMech." + simulationName);
            periapsisAltitude = Bindings.EditableDouble (simulation, "PeriapsisAltitude");
            apoapsisAltitude = Bindings.EditableDouble (simulation, "ApoapsisAltitude");
            burnoutAltitudeFlag = simulation.Field<object, bool> ("BurnoutAltitudeFlag");
            burnoutAltitude = Bindings.EditableDouble (simulation, "BurnoutAltitude");
            flightPathAngle = Bindings.EditableDouble (simulation, "FPA");
            argumentOfPeriapsisFlag = simulation.Field<object, bool> ("ArgPFlag");
            argumentOfPeriapsis = Bindings.EditableDouble (simulation, "ArgP");
            lan = Bindings.EditableDouble (simulation, "LAN");

            var countdown = Bindings.Type ("MuMech." + countdownName);
            countdownActive = countdown.Getter<object, bool> ("Active");
            tMinus = countdown.Getter<object, double> ("TMinus");
            startToPlaneOfTarget = countdown.Method<Func<object, double>> ("StartToPlaneOfTarget");
            startToTargetLan = countdown.Method<Func<object, double>> ("StartToTargetLan");
            startToLan = countdown.Method<Func<object, double>> ("StartToLan");
            abort = countdown.Method<Action<object>> ("Abort");
        }

        internal AscentGuidance () : base (settingsName)
        {
            Settings = new AscentSettings ();
            PSGSettings = new PSGSettings ();
            ClassicPath = new ClassicPath ();
        }

        /// <summary>
        /// MechJeb's ascent settings, which hold the settings both ascent types share.
        /// </summary>
        internal static object SettingsModule {
            get { return MechJeb.Module (settingsName); }
        }

        static object Simulation {
            get { return MechJeb.Module (simulationName); }
        }

        static object Countdown {
            get { return MechJeb.Module (countdownName); }
        }

        /// <summary>
        /// The autopilot of the selected ascent type.
        /// </summary>
        internal override object Instance {
            get { return autopilot (SettingsModule); }
        }

        /// <summary>
        /// Engage the autopilot of the selected ascent type, or disengage the autopilots of
        /// both types.
        /// </summary>
        internal override void SetEnabled (bool value)
        {
            if (value) {
                SetEnabled (Instance, true);
            } else {
                // The type may have changed since the autopilot was enabled
                SetEnabled (MechJeb.Module (classicName), false);
                SetEnabled (MechJeb.Module (psgName), false);
            }
        }

        /// <summary>
        /// The autopilot status, which depends on the selected ascent type.
        /// </summary>
        [KRPCProperty]
        public string Status {
            get { return statusText (status (Instance)); }
        }

        /// <summary>
        /// The ascent type.
        /// </summary>
        /// <remarks>
        /// Setting it disengages the autopilot of the other type.
        /// </remarks>
        [KRPCProperty]
        public AscentType AscentType {
            get { return ascentType.Get (SettingsModule) == psg ? AscentType.PSG : AscentType.Classic; }
            set { ascentType.Set (SettingsModule, value == AscentType.PSG ? psg : classic); }
        }

        /// <summary>
        /// The Ascent Settings window.
        /// </summary>
        [KRPCProperty]
        public AscentSettings Settings { get; private set; }

        /// <summary>
        /// The settings of the <see cref="AscentType.PSG"/> ascent, from the Ascent Settings
        /// window and the PSG Stages window.
        /// </summary>
        [KRPCProperty]
        public PSGSettings PSGSettings { get; private set; }

        /// <summary>
        /// The Ascent Path Editor window, with the settings of the
        /// <see cref="AscentType.Classic"/> ascent.
        /// </summary>
        [KRPCProperty]
        public ClassicPath ClassicPath { get; private set; }

        /// <summary>
        /// The altitude of the final circular orbit, in meters.
        /// </summary>
        /// <remarks>Applies to the <see cref="AscentType.Classic"/> ascent.</remarks>
        [KRPCProperty]
        public double DesiredOrbitAltitude {
            get { return desiredOrbitAltitude.Get (SettingsModule); }
            set { desiredOrbitAltitude.Set (SettingsModule, value); }
        }

        /// <summary>
        /// The inclination of the final orbit, in degrees.
        /// </summary>
        [KRPCProperty]
        public double DesiredInclination {
            get { return desiredInclination.Get (SettingsModule); }
            set { desiredInclination.Set (SettingsModule, value); }
        }

        /// <summary>
        /// The altitude of the periapsis of the target orbit, in meters.
        /// </summary>
        /// <remarks>Applies to the <see cref="AscentType.PSG"/> ascent.</remarks>
        [KRPCProperty]
        public double PeriapsisAltitude {
            get { return periapsisAltitude.Get (Simulation); }
            set { periapsisAltitude.Set (Simulation, value); }
        }

        /// <summary>
        /// The altitude of the apoapsis of the target orbit, in meters.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="AscentType.PSG"/> ascent. A value below
        /// <see cref="PeriapsisAltitude"/>, such as 0, gives a circular orbit, and a negative
        /// value a hyperbolic one.
        /// </remarks>
        [KRPCProperty]
        public double ApoapsisAltitude {
            get { return apoapsisAltitude.Get (Simulation); }
            set { apoapsisAltitude.Set (Simulation, value); }
        }

        /// <summary>
        /// Whether to reach the target orbit at <see cref="BurnoutAltitude"/>.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="AscentType.PSG"/> ascent. When every stage has a fixed burn
        /// time, the ascent always burns out at <see cref="BurnoutAltitude"/>.
        /// </remarks>
        [KRPCProperty]
        public bool BurnoutAltitudeFlag {
            get { return burnoutAltitudeFlag.Get (Simulation); }
            set { burnoutAltitudeFlag.Set (Simulation, value); }
        }

        /// <summary>
        /// The altitude at which the ascent reaches the target orbit, in meters. MechJeb's window
        /// calls it the attach altitude.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="AscentType.PSG"/> ascent. See
        /// <see cref="BurnoutAltitudeFlag"/>.
        /// </remarks>
        [KRPCProperty]
        public double BurnoutAltitude {
            get { return burnoutAltitude.Get (Simulation); }
            set { burnoutAltitude.Set (Simulation, value); }
        }

        /// <summary>
        /// The flight path angle at burnout, in degrees.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="AscentType.PSG"/> ascent, when every stage has a fixed burn
        /// time.
        /// </remarks>
        [KRPCProperty]
        public double FlightPathAngle {
            get { return flightPathAngle.Get (Simulation) * (180d / Math.PI); }
            set { flightPathAngle.Set (Simulation, value * (Math.PI / 180d)); }
        }

        /// <summary>
        /// Whether to aim for <see cref="ArgumentOfPeriapsis"/>.
        /// </summary>
        /// <remarks>Applies to the <see cref="AscentType.PSG"/> ascent.</remarks>
        [KRPCProperty]
        public bool ArgumentOfPeriapsisFlag {
            get { return argumentOfPeriapsisFlag.Get (Simulation); }
            set { argumentOfPeriapsisFlag.Set (Simulation, value); }
        }

        /// <summary>
        /// The argument of periapsis of the target orbit, in degrees.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="AscentType.PSG"/> ascent. Requires
        /// <see cref="ArgumentOfPeriapsisFlag"/>.
        /// </remarks>
        [KRPCProperty]
        public double ArgumentOfPeriapsis {
            get { return argumentOfPeriapsis.Get (Simulation) * (180d / Math.PI); }
            set { argumentOfPeriapsis.Set (Simulation, value * (Math.PI / 180d)); }
        }

        /// <summary>
        /// The longitude of the ascending node of a launch to a LAN, in degrees.
        /// See <see cref="LaunchToLan"/>.
        /// </summary>
        [KRPCProperty]
        public double DesiredLan {
            get { return lan.Get (Simulation); }
            set { lan.Set (Simulation, value); }
        }

        /// <summary>
        /// The difference from the target's longitude of the ascending node of a launch into the
        /// target's plane, in degrees.
        /// </summary>
        [KRPCProperty]
        public double LaunchLANDifference {
            get { return launchLANDifference.Get (SettingsModule); }
            set { launchLANDifference.Set (SettingsModule, value); }
        }

        /// <summary>
        /// Whether a timed launch launches at once, rather than at the time it computes.
        /// </summary>
        [KRPCProperty]
        public bool OverrideWarpToPlane {
            get { return overrideWarpToPlane.Get (SettingsModule); }
            set { overrideWarpToPlane.Set (SettingsModule, value); }
        }

        /// <summary>
        /// The time before a timed launch at which time warp stops, in seconds.
        /// </summary>
        [KRPCProperty]
        public int WarpCountDown {
            get { return warpCountDown.Get (SettingsModule); }
            set { warpCountDown.Set (SettingsModule, value); }
        }

        /// <summary>
        /// The launch mode, which gives whether a timed launch is counting down.
        /// </summary>
        [KRPCProperty]
        public AscentLaunchMode LaunchMode {
            get {
                if (!countdownActive (Countdown))
                    return AscentLaunchMode.Normal;
                var settings = SettingsModule;
                if (launchingToPlane (settings))
                    return AscentLaunchMode.TargetPlane;
                if (launchingToMatchLan (settings))
                    return AscentLaunchMode.TargetLan;
                return AscentLaunchMode.Lan;
            }
        }

        /// <summary>
        /// The time until a timed launch, in seconds.
        /// </summary>
        /// <remarks>
        /// Requires a <see cref="LaunchMode"/> other than <see cref="AscentLaunchMode.Normal"/>.
        /// </remarks>
        [KRPCProperty]
        public double TimeToLaunch {
            get { return tMinus (Countdown); }
        }

        /// <summary>
        /// Abort a timed launch.
        /// </summary>
        [KRPCMethod]
        public void AbortTimedLaunch ()
        {
            abort (Countdown);
        }

        // MechJeb's target, updated from the game's target
        static void CheckTarget ()
        {
            var targetController = MechJeb.TargetController;
            targetController.Update ();
            var target = targetController.InternalTargetOrbit;
            if (!targetController.NormalTargetExists || target == null)
                throw new InvalidOperationException ("There is no target");
            if (target.referenceBody != FlightGlobals.ActiveVessel.mainBody)
                throw new InvalidOperationException ("The target does not orbit the body the vessel is on");
        }

        /// <summary>
        /// Count down to a launch into the plane of the target, and set
        /// <see cref="DesiredInclination"/> to match. Throws an exception if there is no target
        /// orbiting the body the vessel is on.
        /// </summary>
        /// <remarks>
        /// The countdown runs while the autopilot is enabled.
        /// </remarks>
        [KRPCMethod]
        public void LaunchToTargetPlane ()
        {
            CheckTarget ();
            AbortTimedLaunch ();
            startToPlaneOfTarget (Countdown);
        }

        /// <summary>
        /// Count down to a launch into the plane with the target's longitude of the ascending
        /// node, less <see cref="LaunchLANDifference"/>, and inclination
        /// <see cref="DesiredInclination"/>. Throws an exception if there is no target orbiting
        /// the body the vessel is on, or the ascent type is not <see cref="AscentType.PSG"/>.
        /// </summary>
        /// <remarks>
        /// The countdown runs while the autopilot is enabled.
        /// </remarks>
        [KRPCMethod]
        public void LaunchToTargetLan ()
        {
            CheckTarget ();
            CheckPSG ();
            AbortTimedLaunch ();
            startToTargetLan (Countdown);
        }

        /// <summary>
        /// Count down to a launch into the plane with longitude of the ascending node
        /// <see cref="DesiredLan"/> and inclination <see cref="DesiredInclination"/>. Throws an
        /// exception if the ascent type is not <see cref="AscentType.PSG"/>.
        /// </summary>
        /// <remarks>
        /// The countdown runs while the autopilot is enabled.
        /// </remarks>
        [KRPCMethod]
        public void LaunchToLan ()
        {
            CheckPSG ();
            AbortTimedLaunch ();
            startToLan (Countdown);
        }

        // MechJeb's window offers the LAN launches for the PSG ascent only
        void CheckPSG ()
        {
            if (AscentType != AscentType.PSG)
                throw new InvalidOperationException ("The ascent type must be PSG");
        }
    }

    /// <summary>
    /// The launch mode of the ascent autopilot. See <see cref="AscentGuidance.LaunchMode"/>.
    /// </summary>
    [KRPCEnum (Service = "MechJeb")]
    public enum AscentLaunchMode
    {
        /// <summary>
        /// No timed launch is counting down.
        /// </summary>
        Normal,

        /// <summary>
        /// A timed launch into the plane of the target. See
        /// <see cref="AscentGuidance.LaunchToTargetPlane"/>.
        /// </summary>
        TargetPlane,

        /// <summary>
        /// A timed launch to the target's longitude of the ascending node. See
        /// <see cref="AscentGuidance.LaunchToTargetLan"/>.
        /// </summary>
        TargetLan,

        /// <summary>
        /// A timed launch to a longitude of the ascending node. See
        /// <see cref="AscentGuidance.LaunchToLan"/>.
        /// </summary>
        Lan
    }

    /// <summary>
    /// The ascent type. See <see cref="AscentGuidance.AscentType"/>.
    /// </summary>
    [KRPCEnum (Service = "MechJeb")]
    public enum AscentType
    {
        /// <summary>
        /// The classic ascent path. See <see cref="AscentGuidance.ClassicPath"/>.
        /// </summary>
        Classic,

        /// <summary>
        /// Polynomial SQP Guidance, an optimized ascent, for RSS/RO. See
        /// <see cref="AscentGuidance.PSGSettings"/>.
        /// </summary>
        PSG
    }
}
