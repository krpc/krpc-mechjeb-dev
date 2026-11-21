using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The throttle limits of the ascent, from the Ascent Settings window.
    /// </summary>
    /// <remarks>
    /// The ascent starts with a copy of <see cref="Utilities.Thrust"/>, and keeps its own
    /// limits after that. The <see cref="AscentType.PSG"/> ascent ignores
    /// <see cref="LimitThrottle"/> and <see cref="ElectricThrottle"/>.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class AscentThrustLimits
    {
        static Func<object, object> instance;
        static Field<object, bool> limitToPreventOverheats;
        static Field<object, bool> limitDynamicPressure;
        static Field<object, double> maxDynamicPressure;
        static Field<object, bool> limitAcceleration;
        static Field<object, double> maxAcceleration;
        static Field<object, bool> limitThrottle;
        static Field<object, double> maxThrottle;
        static Field<object, bool> limiterMinThrottle;
        static Field<object, double> minThrottle;
        static Field<object, bool> electricThrottle;
        static Field<object, double> electricThrottleLo;
        static Field<object, double> electricThrottleHi;
        static Field<object, bool> limitToPreventUnstableIgnition;
        static Field<object, bool> autoRCSUllaging;

        internal static void Load (MemberBinder settings)
        {
            instance = settings.Getter<object, object> ("ThrustLimits");
            var type = Bindings.Type ("MuMech.ThrustLimitSet");
            limitToPreventOverheats = type.Field<object, bool> ("LimitToPreventOverheats");
            limitDynamicPressure = type.Field<object, bool> ("LimitDynamicPressure");
            maxDynamicPressure = Bindings.EditableDouble (type, "MaxDynamicPressure");
            limitAcceleration = type.Field<object, bool> ("LimitAcceleration");
            maxAcceleration = Bindings.EditableDouble (type, "MaxAcceleration");
            limitThrottle = type.Field<object, bool> ("LimitThrottle");
            maxThrottle = Bindings.EditableDouble (type, "MaxThrottle");
            limiterMinThrottle = type.Field<object, bool> ("LimiterMinThrottle");
            minThrottle = Bindings.EditableDouble (type, "MinThrottle");
            electricThrottle = type.Field<object, bool> ("ElectricThrottle");
            electricThrottleLo = Bindings.EditableDouble (type, "ElectricThrottleLo");
            electricThrottleHi = Bindings.EditableDouble (type, "ElectricThrottleHi");
            limitToPreventUnstableIgnition = type.Field<object, bool> ("LimitToPreventUnstableIgnition");
            autoRCSUllaging = type.Field<object, bool> ("AutoRCSUllaging");
        }

        static object Instance {
            get { return instance (AscentGuidance.SettingsModule); }
        }

        /// <summary>
        /// Whether to limit the throttle to keep parts from overheating.
        /// </summary>
        [KRPCProperty]
        public bool LimitToPreventOverheats {
            get { return limitToPreventOverheats.Get (Instance); }
            set { limitToPreventOverheats.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to limit the dynamic pressure.
        /// </summary>
        [KRPCProperty]
        public bool LimitDynamicPressure {
            get { return limitDynamicPressure.Get (Instance); }
            set { limitDynamicPressure.Set (Instance, value); }
        }

        /// <summary>
        /// The largest dynamic pressure, in pascals.
        /// </summary>
        /// <remarks>Requires <see cref="LimitDynamicPressure"/>.</remarks>
        [KRPCProperty]
        public double MaxDynamicPressure {
            get { return maxDynamicPressure.Get (Instance); }
            set { maxDynamicPressure.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to limit the acceleration.
        /// </summary>
        [KRPCProperty]
        public bool LimitAcceleration {
            get { return limitAcceleration.Get (Instance); }
            set { limitAcceleration.Set (Instance, value); }
        }

        /// <summary>
        /// The largest acceleration, in <unit>m/s^2</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="LimitAcceleration"/>.</remarks>
        [KRPCProperty]
        public double MaxAcceleration {
            get { return maxAcceleration.Get (Instance); }
            set { maxAcceleration.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to limit the throttle.
        /// </summary>
        [KRPCProperty]
        public bool LimitThrottle {
            get { return limitThrottle.Get (Instance); }
            set { limitThrottle.Set (Instance, value); }
        }

        /// <summary>
        /// The largest throttle, between 0 and 1.
        /// </summary>
        /// <remarks>Requires <see cref="LimitThrottle"/>.</remarks>
        [KRPCProperty]
        public double MaxThrottle {
            get { return maxThrottle.Get (Instance); }
            set { maxThrottle.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to keep the throttle above <see cref="MinThrottle"/>.
        /// </summary>
        [KRPCProperty]
        public bool LimiterMinThrottle {
            get { return limiterMinThrottle.Get (Instance); }
            set { limiterMinThrottle.Set (Instance, value); }
        }

        /// <summary>
        /// The smallest throttle, between 0 and 1.
        /// </summary>
        /// <remarks>Requires <see cref="LimiterMinThrottle"/>.</remarks>
        [KRPCProperty]
        public double MinThrottle {
            get { return minThrottle.Get (Instance); }
            set { minThrottle.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to limit the throttle of electric engines to the available power.
        /// </summary>
        [KRPCProperty]
        public bool ElectricThrottle {
            get { return electricThrottle.Get (Instance); }
            set { electricThrottle.Set (Instance, value); }
        }

        /// <summary>
        /// The charge level at which electric engines stop, between 0 and 1.
        /// </summary>
        /// <remarks>Requires <see cref="ElectricThrottle"/>.</remarks>
        [KRPCProperty]
        public double ElectricThrottleLo {
            get { return electricThrottleLo.Get (Instance); }
            set { electricThrottleLo.Set (Instance, value); }
        }

        /// <summary>
        /// The charge level at which electric engines run at full throttle, between 0 and 1.
        /// </summary>
        /// <remarks>Requires <see cref="ElectricThrottle"/>.</remarks>
        [KRPCProperty]
        public double ElectricThrottleHi {
            get { return electricThrottleHi.Get (Instance); }
            set { electricThrottleHi.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to hold the throttle at zero while the engines' fuel is unstable.
        /// </summary>
        /// <remarks>
        /// MechJeb shows this setting only with Real Fuels, as stock engines have stable fuel.
        /// </remarks>
        [KRPCProperty]
        public bool LimitToPreventUnstableIgnition {
            get { return limitToPreventUnstableIgnition.Get (Instance); }
            set { limitToPreventUnstableIgnition.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to fire the RCS forward to settle the fuel before the engines ignite.
        /// </summary>
        /// <remarks>
        /// MechJeb shows this setting only with Real Fuels, as stock engines have stable fuel.
        /// </remarks>
        [KRPCProperty]
        public bool AutoRCSUllaging {
            get { return autoRCSUllaging.Get (Instance); }
            set { autoRCSUllaging.Set (Instance, value); }
        }
    }
}
