using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The staging settings of the ascent, from the Ascent Settings window.
    /// </summary>
    /// <remarks>
    /// The ascent starts with a copy of <see cref="Utilities.Staging"/>, and keeps its own
    /// settings after that.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class AscentStaging
    {
        static Func<object, object> instance;
        static Field<object, double> autostagePreDelay;
        static Field<object, double> autostagePostDelay;
        static Field<object, int> autostageLimit;
        static Field<object, bool> fairingMaxDynamicPressureEnabled;
        static Field<object, double> fairingMaxDynamicPressure;
        static Field<object, bool> fairingMinAltitudeEnabled;
        static Field<object, double> fairingMinAltitude;
        static Field<object, bool> fairingMaxAerothermalFluxEnabled;
        static Field<object, double> fairingMaxAerothermalFlux;
        static Field<object, double> clampAutoStageThrustPct;
        static Field<object, bool> hotStaging;
        static Field<object, double> hotStagingLeadTime;
        static Field<object, bool> dropSolids;
        static Field<object, double> dropSolidsTwrPct;

        internal static void Load (MemberBinder settings)
        {
            instance = settings.Getter<object, object> ("StagingRules");
            var type = Bindings.Type ("MuMech.StagingRuleSet");
            autostagePreDelay = Bindings.EditableDouble (type, "AutostagePreDelay");
            autostagePostDelay = Bindings.EditableDouble (type, "AutostagePostDelay");
            autostageLimit = Bindings.EditableInt (type, "AutostageLimit");
            fairingMaxDynamicPressureEnabled = type.Field<object, bool> ("FairingMaxDynamicPressureEnabled");
            fairingMaxDynamicPressure = Bindings.EditableDouble (type, "FairingMaxDynamicPressure");
            fairingMinAltitudeEnabled = type.Field<object, bool> ("FairingMinAltitudeEnabled");
            fairingMinAltitude = Bindings.EditableDouble (type, "FairingMinAltitude");
            fairingMaxAerothermalFluxEnabled = type.Field<object, bool> ("FairingMaxAerothermalFluxEnabled");
            fairingMaxAerothermalFlux = Bindings.EditableDouble (type, "FairingMaxAerothermalFlux");
            clampAutoStageThrustPct = Bindings.EditableDouble (type, "ClampAutoStageThrustPct");
            hotStaging = type.Field<object, bool> ("HotStaging");
            hotStagingLeadTime = Bindings.EditableDouble (type, "HotStagingLeadTime");
            dropSolids = type.Field<object, bool> ("DropSolids");
            dropSolidsTwrPct = Bindings.EditableDouble (type, "DropSolidsTwrPct");
        }

        static object Instance {
            get { return instance (AscentGuidance.SettingsModule); }
        }

        /// <summary>
        /// The delay before each staging, in seconds.
        /// </summary>
        [KRPCProperty]
        public double AutostagePreDelay {
            get { return autostagePreDelay.Get (Instance); }
            set { autostagePreDelay.Set (Instance, value); }
        }

        /// <summary>
        /// The delay after each staging, in seconds.
        /// </summary>
        [KRPCProperty]
        public double AutostagePostDelay {
            get { return autostagePostDelay.Get (Instance); }
            set { autostagePostDelay.Set (Instance, value); }
        }

        /// <summary>
        /// The last stage the ascent activates.
        /// </summary>
        [KRPCProperty]
        public int AutostageLimit {
            get { return autostageLimit.Get (Instance); }
            set { autostageLimit.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to wait for <see cref="FairingMaxDynamicPressure"/> to jettison a fairing.
        /// </summary>
        [KRPCProperty]
        public bool FairingMaxDynamicPressureEnabled {
            get { return fairingMaxDynamicPressureEnabled.Get (Instance); }
            set { fairingMaxDynamicPressureEnabled.Set (Instance, value); }
        }

        /// <summary>
        /// The largest dynamic pressure at which the ascent jettisons a fairing, in
        /// pascals.
        /// </summary>
        /// <remarks>Requires <see cref="FairingMaxDynamicPressureEnabled"/>.</remarks>
        [KRPCProperty]
        public double FairingMaxDynamicPressure {
            get { return fairingMaxDynamicPressure.Get (Instance); }
            set { fairingMaxDynamicPressure.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to wait for <see cref="FairingMinAltitude"/> to jettison a fairing.
        /// </summary>
        [KRPCProperty]
        public bool FairingMinAltitudeEnabled {
            get { return fairingMinAltitudeEnabled.Get (Instance); }
            set { fairingMinAltitudeEnabled.Set (Instance, value); }
        }

        /// <summary>
        /// The lowest altitude at which the ascent jettisons a fairing, in meters.
        /// </summary>
        /// <remarks>Requires <see cref="FairingMinAltitudeEnabled"/>.</remarks>
        [KRPCProperty]
        public double FairingMinAltitude {
            get { return fairingMinAltitude.Get (Instance); }
            set { fairingMinAltitude.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to wait for <see cref="FairingMaxAerothermalFlux"/> to jettison a fairing.
        /// </summary>
        [KRPCProperty]
        public bool FairingMaxAerothermalFluxEnabled {
            get { return fairingMaxAerothermalFluxEnabled.Get (Instance); }
            set { fairingMaxAerothermalFluxEnabled.Set (Instance, value); }
        }

        /// <summary>
        /// The largest aerothermal flux at which the ascent jettisons a fairing, in
        /// <unit>W/m^2</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="FairingMaxAerothermalFluxEnabled"/>.</remarks>
        [KRPCProperty]
        public double FairingMaxAerothermalFlux {
            get { return fairingMaxAerothermalFlux.Get (Instance); }
            set { fairingMaxAerothermalFlux.Set (Instance, value); }
        }

        /// <summary>
        /// The fraction of full thrust at which the ascent releases the launch clamps.
        /// </summary>
        [KRPCProperty]
        public double ClampAutoStageThrustPct {
            get { return clampAutoStageThrustPct.Get (Instance); }
            set { clampAutoStageThrustPct.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the ascent ignites the next stage before the current one burns out.
        /// </summary>
        [KRPCProperty]
        public bool HotStaging {
            get { return hotStaging.Get (Instance); }
            set { hotStaging.Set (Instance, value); }
        }

        /// <summary>
        /// The time before burnout at which a hot staging happens, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HotStaging"/>.</remarks>
        [KRPCProperty]
        public double HotStagingLeadTime {
            get { return hotStagingLeadTime.Get (Instance); }
            set { hotStagingLeadTime.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the ascent drops the solid boosters of the next stage early, once their
        /// thrust no longer pays for their mass.
        /// </summary>
        [KRPCProperty]
        public bool DropSolids {
            get { return dropSolids.Get (Instance); }
            set { dropSolids.Set (Instance, value); }
        }

        /// <summary>
        /// The fraction of the vessel's thrust-to-weight ratio at which the solid boosters
        /// drop, between 0 and 1.
        /// </summary>
        /// <remarks>Requires <see cref="DropSolids"/>.</remarks>
        [KRPCProperty]
        public double DropSolidsTwrPct {
            get { return dropSolidsTwrPct.Get (Instance); }
            set { dropSolidsTwrPct.Set (Instance, value); }
        }
    }
}
