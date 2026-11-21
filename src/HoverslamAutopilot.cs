using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The hoverslam autopilot, which lands with a single burn that ends just above the ground,
    /// then descends vertically. The Hoverslam Info window shows it and its prediction.
    /// </summary>
    /// <remarks>
    /// The prediction runs whenever the vessel is in flight, so it is available with the
    /// autopilot disabled.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class HoverslamAutopilot : ComputerModule
    {
        const string simulationName = "MechJebModuleHoverslamSimulation";

        static Field<object, bool> enabled;
        static Func<object, string> state;
        static Field<object, double> ignitionLead;
        static Field<object, double> touchdownSpeed;
        static Field<object, bool> autowarp;
        static Field<object, bool> holdUpright;
        static Field<object, bool> autostage;
        static Field<object, double> pwmPulseWidth;
        static Field<object, bool> mapLandingPrediction;
        static Field<object, double> recalculationInterval;
        static Field<object, double> verticalAuthority;
        static Field<object, double> verticalAltitude;
        static Func<object, double> ignitionTime;
        static Func<object, double> landingTime;
        static Func<object, double> timeToIgnition;
        static Func<object, double> timeToLanding;
        static Func<object, double> latitude;
        static Func<object, double> longitude;
        static Func<object, string> biome;
        static Func<object, double> terrainAltitude;
        static Func<object, double> slope;
        static Func<object, double> finalDescentSpeed;
        static Func<object, double> finalThrustAcceleration;
        static Func<object, double> deltaV;

        internal static new void Load ()
        {
            enabled = Bindings.Type ("MuMech.ComputerModule").Field<object, bool> ("Enabled");
            var type = Bindings.Type ("MuMech.MechJebModuleHoverslamAutopilot");
            state = type.Getter<object, string> ("HoverslamState");
            ignitionLead = Bindings.EditableDouble (type, "IgnitionLead");
            touchdownSpeed = Bindings.EditableDouble (type, "TouchdownSpeed");
            autowarp = type.Field<object, bool> ("AutoWarp");
            holdUpright = type.Field<object, bool> ("HoldUpright");
            autostage = type.Field<object, bool> ("Autostage");
            pwmPulseWidth = Bindings.EditableDouble (type, "PWMPulseWidth");
            var simulation = Bindings.Type ("MuMech." + simulationName);
            mapLandingPrediction = simulation.Field<object, bool> ("MapLandingPrediction");
            recalculationInterval = Bindings.EditableDouble (simulation, "SimRecalcInterval");
            verticalAuthority = Bindings.EditableDouble (simulation, "VerticalAuthority");
            verticalAltitude = Bindings.EditableDouble (simulation, "VerticalAltitude");
            ignitionTime = simulation.Getter<object, double> ("IgnitionUT");
            landingTime = simulation.Getter<object, double> ("LandingUT");
            timeToIgnition = simulation.Getter<object, double> ("IgnitionCountdown");
            timeToLanding = simulation.Getter<object, double> ("LandingCountdown");
            latitude = simulation.Getter<object, double> ("Lat");
            longitude = simulation.Getter<object, double> ("Lng");
            biome = simulation.Method<Func<object, string>> ("Biome");
            terrainAltitude = simulation.Getter<object, double> ("TerrainAltitude");
            slope = simulation.Getter<object, double> ("Slope");
            finalDescentSpeed = simulation.Getter<object, double> ("FinalDescentSpeed");
            finalThrustAcceleration = simulation.Getter<object, double> ("FinalThrustAccel");
            deltaV = simulation.Getter<object, double> ("DeltaV");
        }

        internal HoverslamAutopilot () : base ("MechJebModuleHoverslamAutopilot")
        {
        }

        static object Simulation {
            get { return MechJeb.Module (simulationName); }
        }

        /// <summary>
        /// Whether the autopilot is enabled.
        /// </summary>
        /// <remarks>
        /// The autopilot can be enabled before there is a prediction, and starts flying once
        /// there is one. Setting it throws an exception if the autopilot is
        /// <see cref="HoverslamState.Locked"/>.
        /// </remarks>
        [KRPCProperty]
        public bool Enabled {
            get { return enabled.Get (Instance); }
            set {
                var instance = Instance;
                enabled.Set (instance, value);
                if (value && !enabled.Get (instance))
                    throw new InvalidOperationException (
                        "The hoverslam autopilot is locked, as it needs a research or a tracking station upgrade");
            }
        }

        /// <summary>
        /// The phase of the autopilot.
        /// </summary>
        [KRPCProperty]
        public HoverslamState State {
            get { return (HoverslamState)Enum.Parse (typeof (HoverslamState), state (Instance)); }
        }

        /// <summary>
        /// The time before the computed ignition at which the burn starts, in seconds.
        /// </summary>
        /// <remarks>
        /// It makes up for staging or engine spool-up delays. For a margin against the
        /// terrain, raise <see cref="VerticalAltitude"/> instead.
        /// </remarks>
        [KRPCProperty]
        public double IgnitionLead {
            get { return ignitionLead.Get (Instance); }
            set { ignitionLead.Set (Instance, value); }
        }

        /// <summary>
        /// The downward speed at touchdown, in <unit>m/s</unit>.
        /// </summary>
        [KRPCProperty]
        public double TouchdownSpeed {
            get { return touchdownSpeed.Get (Instance); }
            set { touchdownSpeed.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to warp time to the ignition.
        /// </summary>
        [KRPCProperty]
        public bool Autowarp {
            get { return autowarp.Get (Instance); }
            set { autowarp.Set (Instance, value); }
        }

        /// <summary>
        /// Whether Smart A.S.S. holds the vessel upright after touchdown.
        /// </summary>
        [KRPCProperty]
        public bool HoldUpright {
            get { return holdUpright.Get (Instance); }
            set { holdUpright.Set (Instance, value); }
        }

        /// <summary>
        /// Whether enabling the autopilot enables autostaging, and disabling it disables
        /// autostaging.
        /// </summary>
        [KRPCProperty]
        public bool Autostage {
            get { return autostage.Get (Instance); }
            set { autostage.Set (Instance, value); }
        }

        /// <summary>
        /// The shortest pulse of the throttle, in seconds.
        /// </summary>
        /// <remarks>
        /// The autopilot pulses the throttle of engines that cannot throttle low enough to
        /// hover, such as with Real Fuels.
        /// </remarks>
        [KRPCProperty]
        public double PWMPulseWidth {
            get { return pwmPulseWidth.Get (Instance); }
            set { pwmPulseWidth.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to mark the predicted landing site on the map.
        /// </summary>
        [KRPCProperty]
        public bool MapLandingPrediction {
            get { return mapLandingPrediction.Get (Simulation); }
            set { mapLandingPrediction.Set (Simulation, value); }
        }

        /// <summary>
        /// The time between updates of the prediction, in seconds.
        /// </summary>
        [KRPCProperty]
        public double RecalculationInterval {
            get { return recalculationInterval.Get (Simulation); }
            set { recalculationInterval.Set (Simulation, value); }
        }

        /// <summary>
        /// The fraction of the thrust above hover that the burn plans to use, between 0 and 1.
        /// </summary>
        /// <remarks>
        /// 0.5 leaves an equal margin for overshoot and undershoot. A higher value uses less
        /// delta-v, with less margin.
        /// </remarks>
        [KRPCProperty]
        public double VerticalAuthority {
            get { return verticalAuthority.Get (Simulation); }
            set { verticalAuthority.Set (Simulation, value); }
        }

        /// <summary>
        /// The altitude above the terrain at which the burn ends and the vertical descent
        /// starts, in meters.
        /// </summary>
        [KRPCProperty]
        public double VerticalAltitude {
            get { return verticalAltitude.Get (Simulation); }
            set { verticalAltitude.Set (Simulation, value); }
        }

        /// <summary>
        /// Whether there is a prediction of the landing.
        /// </summary>
        [KRPCProperty]
        public bool HasPrediction {
            get { return !double.IsNaN (ignitionTime (Simulation)); }
        }

        /// <summary>
        /// The universal time of the ignition, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double IgnitionTime {
            get { return ignitionTime (Simulation); }
        }

        /// <summary>
        /// The universal time of the touchdown, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double LandingTime {
            get { return landingTime (Simulation); }
        }

        /// <summary>
        /// The time until the ignition, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double TimeToIgnition {
            get { return timeToIgnition (Simulation); }
        }

        /// <summary>
        /// The time until the touchdown, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double TimeToLanding {
            get { return timeToLanding (Simulation); }
        }

        /// <summary>
        /// The latitude of the predicted landing site, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double LandingLatitude {
            get { return latitude (Simulation); }
        }

        /// <summary>
        /// The longitude of the predicted landing site, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double LandingLongitude {
            get { return longitude (Simulation); }
        }

        /// <summary>
        /// The biome of the predicted landing site, or <c>N/A</c> with no prediction.
        /// </summary>
        [KRPCProperty]
        public string LandingBiome {
            get { return biome (Simulation); }
        }

        /// <summary>
        /// The altitude above sea level of the terrain at the predicted landing site, in meters.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double TerrainAltitude {
            get { return terrainAltitude (Simulation); }
        }

        /// <summary>
        /// The slope of the terrain at the predicted landing site, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double Slope {
            get { return slope (Simulation); }
        }

        /// <summary>
        /// The speed of the vertical descent, in <unit>m/s</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double FinalDescentSpeed {
            get { return finalDescentSpeed (Simulation); }
        }

        /// <summary>
        /// The acceleration from thrust during the vertical descent, in <unit>m/s^2</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double FinalThrustAcceleration {
            get { return finalThrustAcceleration (Simulation); }
        }

        /// <summary>
        /// The delta-v of the landing burn, in <unit>m/s</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="HasPrediction"/>.</remarks>
        [KRPCProperty]
        public double DeltaV {
            get { return deltaV (Simulation); }
        }
    }

    /// <summary>
    /// The phase of the hoverslam autopilot. See <see cref="HoverslamAutopilot.State"/>.
    /// </summary>
    [KRPCEnum (Service = "MechJeb")]
    public enum HoverslamState
    {
        /// <summary>
        /// The autopilot needs a research or a tracking station upgrade.
        /// </summary>
        Locked,

        /// <summary>
        /// The autopilot is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The autopilot is waiting for a prediction.
        /// </summary>
        Init,

        /// <summary>
        /// The autopilot is turning the vessel to the burn.
        /// </summary>
        Align,

        /// <summary>
        /// The autopilot is coasting to the ignition.
        /// </summary>
        Coast,

        /// <summary>
        /// The autopilot is burning to slow down.
        /// </summary>
        Burn,

        /// <summary>
        /// The autopilot is descending vertically to touchdown.
        /// </summary>
        Vertical,

        /// <summary>
        /// The vessel has landed.
        /// </summary>
        Finished
    }
}
