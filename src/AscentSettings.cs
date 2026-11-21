using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The Ascent Settings window, which holds the settings shared by the ascent types.
    /// </summary>
    [KRPCClass (Service = "MechJeb")]
    public sealed class AscentSettings : ComputerModule
    {
        static Field<object, bool> forceRoll;
        static Field<object, double> verticalRoll;
        static Field<object, double> turnRoll;
        static Field<object, double> rollAltitude;
        static Field<object, bool> autoDeploySolarPanels;
        static Field<object, bool> autoDeployAntennas;
        static Field<object, bool> autostage;
        static Field<object, bool> autowarp;
        static Func<object, bool> limitingAoA;
        static Func<object, double> currentMaxAoA;

        internal static new void Load ()
        {
            var settings = Bindings.Type ("MuMech.MechJebModuleAscentSettings");
            forceRoll = settings.Field<object, bool> ("ForceRoll");
            verticalRoll = Bindings.EditableDouble (settings, "VerticalRoll");
            turnRoll = Bindings.EditableDouble (settings, "TurnRoll");
            rollAltitude = Bindings.EditableDouble (settings, "RollAltitude");
            autoDeploySolarPanels = settings.Field<object, bool> ("AutoDeploySolarPanels");
            autoDeployAntennas = settings.Field<object, bool> ("AutoDeployAntennas");
            autostage = settings.Field<object, bool> ("Autostage");
            autowarp = settings.Field<object, bool> ("Autowarp");
            limitingAoA = settings.Getter<object, bool> ("LimitingAoA");
            currentMaxAoA = settings.Getter<object, double> ("CurrentMaxAoA");
            AscentStaging.Load (settings);
            AscentThrustLimits.Load (settings);
        }

        internal AscentSettings () : base ("MechJebModuleAscentSettings")
        {
            Staging = new AscentStaging ();
            Thrust = new AscentThrustLimits ();
        }

        /// <summary>
        /// Whether the autopilot controls the roll.
        /// </summary>
        [KRPCProperty]
        public bool ForceRoll {
            get { return forceRoll.Get (Instance); }
            set { forceRoll.Set (Instance, value); }
        }

        /// <summary>
        /// The roll during the vertical climb, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="ForceRoll"/>.</remarks>
        [KRPCProperty]
        public double VerticalRoll {
            get { return verticalRoll.Get (Instance); }
            set { verticalRoll.Set (Instance, value); }
        }

        /// <summary>
        /// The roll during the turn, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="ForceRoll"/>.</remarks>
        [KRPCProperty]
        public double TurnRoll {
            get { return turnRoll.Get (Instance); }
            set { turnRoll.Set (Instance, value); }
        }

        /// <summary>
        /// The altitude above which the autopilot controls the roll, in meters.
        /// </summary>
        /// <remarks>Requires <see cref="ForceRoll"/>.</remarks>
        [KRPCProperty]
        public double RollAltitude {
            get { return rollAltitude.Get (Instance); }
            set { rollAltitude.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the ascent keeps the solar panels retracted in the atmosphere, and extends
        /// them above it.
        /// </summary>
        [KRPCProperty]
        public bool AutoDeploySolarPanels {
            get { return autoDeploySolarPanels.Get (Instance); }
            set { autoDeploySolarPanels.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the ascent keeps the antennas retracted in the atmosphere, and extends them
        /// above it.
        /// </summary>
        [KRPCProperty]
        public bool AutoDeployAntennas {
            get { return autoDeployAntennas.Get (Instance); }
            set { autoDeployAntennas.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the autopilot stages when the current stage runs out of fuel.
        /// </summary>
        /// <remarks><see cref="Staging"/> holds the staging settings.</remarks>
        [KRPCProperty]
        public bool Autostage {
            get { return autostage.Get (Instance); }
            set { autostage.Set (Instance, value); }
        }

        /// <summary>
        /// The staging settings of the ascent, which apply in place of
        /// <see cref="Utilities.Staging"/> while the autopilot is enabled.
        /// </summary>
        [KRPCProperty]
        public AscentStaging Staging { get; private set; }

        /// <summary>
        /// The throttle limits of the ascent, which apply in place of
        /// <see cref="Utilities.Thrust"/> while the autopilot is enabled.
        /// </summary>
        [KRPCProperty]
        public AscentThrustLimits Thrust { get; private set; }

        /// <summary>
        /// Whether to warp time during the launch countdown and the coasts of the ascent.
        /// </summary>
        [KRPCProperty]
        public bool Autowarp {
            get { return autowarp.Get (Instance); }
            set { autowarp.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the angle of attack limit is limiting the steering.
        /// </summary>
        [KRPCProperty]
        public bool LimitingAngleOfAttack {
            get { return limitingAoA (Instance); }
        }

        /// <summary>
        /// The largest angle of attack the limit allows now, in degrees.
        /// </summary>
        [KRPCProperty]
        public double CurrentMaxAngleOfAttack {
            get { return currentMaxAoA (Instance); }
        }
    }
}
