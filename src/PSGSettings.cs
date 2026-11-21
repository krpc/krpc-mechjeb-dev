using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The settings of the Polynomial SQP Guidance ascent, from the Ascent Settings window and
    /// the PSG Stages window, and the state of its guidance.
    /// </summary>
    /// <remarks>
    /// PSG plans which stages burn and coast from the vessel. The target orbit is set in
    /// <see cref="AscentGuidance"/>.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class PSGSettings
    {
        const string simulationName = "MechJebModulePSGSimulation";
        const string autopilotName = "MechJebModulePSGAutopilot";

        static Field<object, double> pitchStartHeight;
        static Field<object, double> pitchRate;
        static Field<object, double> spinupLeadTime;
        static Field<object, double> spinupAngularVelocity;
        static Field<object, double> dragCoefficient;
        static Field<object, double> referenceArea;
        static Field<object, bool> autoAero;
        static Field<object, double> limitQa;
        static Field<object, double> minDeltaV;
        static Field<object, double> maxCoast;
        static Field<object, double> minCoast;
        static Field<object, double> ullageLeadTime;
        static Func<object, bool> hasSolution;
        static Func<object, string> guidanceStatus;
        static Func<object, double> tgo;
        static Func<object, double> vgo;
        static Func<object, double> pitch;
        static Func<object, double> heading;

        internal static void Load ()
        {
            var settings = Bindings.Type ("MuMech.MechJebModuleAscentSettings");
            pitchStartHeight = Bindings.EditableDouble (settings, "PitchStartHeight");
            pitchRate = Bindings.EditableDouble (settings, "PitchRate");
            spinupLeadTime = Bindings.EditableDouble (settings, "SpinupLeadTime");
            spinupAngularVelocity = Bindings.EditableDouble (settings, "SpinupAngularVelocity");
            var simulation = Bindings.Type ("MuMech." + simulationName);
            dragCoefficient = Bindings.EditableDouble (simulation, "Cd");
            referenceArea = Bindings.EditableDouble (simulation, "Aref");
            autoAero = simulation.Field<object, bool> ("AutoAero");
            limitQa = Bindings.EditableDouble (simulation, "LimitQa");
            minDeltaV = Bindings.EditableDouble (simulation, "MinDeltaV");
            maxCoast = Bindings.EditableDouble (simulation, "MaxCoast");
            minCoast = Bindings.EditableDouble (simulation, "MinCoast");
            var autopilot = Bindings.Type ("MuMech." + autopilotName);
            ullageLeadTime = Bindings.EditableDouble (autopilot, "UllageLeadTime");
            hasSolution = autopilot.Getter<object, bool> ("HasSolution");
            guidanceStatus = autopilot.Getter<object, string> ("GuidanceStatus");
            tgo = autopilot.Getter<object, double> ("Tgo");
            vgo = autopilot.Getter<object, double> ("Vgo");
            pitch = autopilot.Getter<object, double> ("Pitch");
            heading = autopilot.Getter<object, double> ("Heading");
        }

        static object Settings {
            get { return AscentGuidance.SettingsModule; }
        }

        static object Simulation {
            get { return MechJeb.Module (simulationName); }
        }

        static object Autopilot {
            get { return MechJeb.Module (autopilotName); }
        }

        /// <summary>
        /// The height above the launch site at which the pitch program starts, in meters.
        /// </summary>
        [KRPCProperty]
        public double PitchStartHeight {
            get { return pitchStartHeight.Get (Settings); }
            set { pitchStartHeight.Set (Settings, value); }
        }

        /// <summary>
        /// The pitch rate of the pitch program, in <unit>deg/s</unit>.
        /// </summary>
        [KRPCProperty]
        public double PitchRate {
            get { return pitchRate.Get (Settings); }
            set { pitchRate.Set (Settings, value); }
        }

        /// <summary>
        /// The drag coefficient guidance plans with.
        /// </summary>
        /// <remarks>Ignored with <see cref="AutoAero"/>.</remarks>
        [KRPCProperty]
        public double DragCoefficient {
            get { return dragCoefficient.Get (Simulation); }
            set { dragCoefficient.Set (Simulation, value); }
        }

        /// <summary>
        /// The reference area guidance plans with, in <unit>m^2</unit>.
        /// </summary>
        /// <remarks>Ignored with <see cref="AutoAero"/>.</remarks>
        [KRPCProperty]
        public double ReferenceArea {
            get { return referenceArea.Get (Simulation); }
            set { referenceArea.Set (Simulation, value); }
        }

        /// <summary>
        /// Whether guidance works out <see cref="DragCoefficient"/> and
        /// <see cref="ReferenceArea"/> from the vessel before each solve.
        /// </summary>
        [KRPCProperty]
        public bool AutoAero {
            get { return autoAero.Get (Simulation); }
            set { autoAero.Set (Simulation, value); }
        }

        /// <summary>
        /// The limit on dynamic pressure times angle of attack, in <unit>Pa.rad</unit>.
        /// </summary>
        /// <remarks>0 turns the limit off. MechJeb clamps it to at most 10000.</remarks>
        [KRPCProperty]
        public double LimitQAlpha {
            get { return limitQa.Get (Simulation); }
            set { limitQa.Set (Simulation, value); }
        }

        /// <summary>
        /// The smallest delta-v of a stage that guidance plans with, in <unit>m/s</unit>.
        /// </summary>
        [KRPCProperty]
        public double MinDeltaV {
            get { return minDeltaV.Get (Simulation); }
            set { minDeltaV.Set (Simulation, value); }
        }

        /// <summary>
        /// The longest coast guidance plans, in seconds.
        /// </summary>
        [KRPCProperty]
        public double MaxCoast {
            get { return maxCoast.Get (Simulation); }
            set { maxCoast.Set (Simulation, value); }
        }

        /// <summary>
        /// The shortest coast guidance plans, in seconds.
        /// </summary>
        [KRPCProperty]
        public double MinCoast {
            get { return minCoast.Get (Simulation); }
            set { minCoast.Set (Simulation, value); }
        }

        /// <summary>
        /// The time before an ignition at which RCS settles the propellant, in seconds.
        /// </summary>
        [KRPCProperty]
        public double UllageLeadTime {
            get { return ullageLeadTime.Get (Autopilot); }
            set { ullageLeadTime.Set (Autopilot, value); }
        }

        /// <summary>
        /// The time before staging at which the spin-up starts, in seconds.
        /// </summary>
        [KRPCProperty]
        public double SpinupLeadTime {
            get { return spinupLeadTime.Get (Settings); }
            set { spinupLeadTime.Set (Settings, value); }
        }

        /// <summary>
        /// The angular velocity of the spin-up, in <unit>rad/s</unit>.
        /// </summary>
        /// <remarks>MechJeb's window shows it in revolutions per minute.</remarks>
        [KRPCProperty]
        public double SpinupAngularVelocity {
            get { return spinupAngularVelocity.Get (Settings); }
            set { spinupAngularVelocity.Set (Settings, value); }
        }

        /// <summary>
        /// Whether guidance has a solution.
        /// </summary>
        [KRPCProperty]
        public bool HasSolution {
            get { return hasSolution (Autopilot); }
        }

        /// <summary>
        /// The status of guidance.
        /// </summary>
        [KRPCProperty]
        public string GuidanceStatus {
            get { return guidanceStatus (Autopilot) ?? string.Empty; }
        }

        /// <summary>
        /// The time to the end of the current burn of the solution, in seconds.
        /// </summary>
        /// <remarks>Requires <see cref="HasSolution"/>.</remarks>
        [KRPCProperty]
        public double TimeToGo {
            get { return tgo (Autopilot); }
        }

        /// <summary>
        /// The delta-v left in the solution, in <unit>m/s</unit>.
        /// </summary>
        /// <remarks>Requires <see cref="HasSolution"/>.</remarks>
        [KRPCProperty]
        public double VelocityToGo {
            get { return vgo (Autopilot); }
        }

        /// <summary>
        /// The pitch guidance commands, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="HasSolution"/>.</remarks>
        [KRPCProperty]
        public double Pitch {
            get { return pitch (Autopilot); }
        }

        /// <summary>
        /// The heading guidance commands, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="HasSolution"/>.</remarks>
        [KRPCProperty]
        public double Heading {
            get { return heading (Autopilot); }
        }
    }
}
