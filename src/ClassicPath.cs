using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The settings of the classic ascent path, from the Ascent Path Editor window and the
    /// Ascent Settings window.
    /// </summary>
    [KRPCClass (Service = "MechJeb")]
    public sealed class ClassicPath : ComputerModule
    {
        static Field<object, double> turnStartAltitude;
        static Field<object, double> turnStartVelocity;
        static Field<object, double> turnEndAltitude;
        static Field<object, double> turnEndAngle;
        static Field<object, double> turnShapeExponent;
        static Field<object, bool> autoPath;
        static Field<object, float> autoTurnPerc;
        static Field<object, float> autoTurnSpdFactor;
        static Func<object, double> autoTurnStartAltitude;
        static Func<object, double> autoTurnStartVelocity;
        static Func<object, double> autoTurnEndAltitude;
        static Field<object, bool> correctiveSteering;
        static Field<object, double> correctiveSteeringGain;
        static Field<object, bool> limitAoA;
        static Field<object, double> maxAoA;
        static Field<object, double> aoaLimitFadeoutPressure;
        static Field<object, bool> skipCircularization;

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.MechJebModuleAscentClassicAutopilot");
            turnStartAltitude = Bindings.EditableDouble (type, "TurnStartAltitude");
            turnStartVelocity = Bindings.EditableDouble (type, "TurnStartVelocity");
            turnEndAltitude = Bindings.EditableDouble (type, "TurnEndAltitude");
            turnEndAngle = Bindings.EditableDouble (type, "TurnEndAngle");
            turnShapeExponent = Bindings.EditableDouble (type, "TurnShapeExponent");
            autoPath = type.Field<object, bool> ("AutoPath");
            autoTurnPerc = type.Field<object, float> ("AutoTurnPerc");
            autoTurnSpdFactor = type.Field<object, float> ("AutoTurnSpdFactor");
            autoTurnStartAltitude = type.Getter<object, double> ("AutoTurnStartAltitude");
            autoTurnStartVelocity = type.Getter<object, double> ("AutoTurnStartVelocity");
            autoTurnEndAltitude = type.Getter<object, double> ("AutoTurnEndAltitude");
            correctiveSteering = type.Field<object, bool> ("CorrectiveSteering");
            correctiveSteeringGain = Bindings.EditableDouble (type, "CorrectiveSteeringGain");
            limitAoA = type.Field<object, bool> ("LimitAoA");
            maxAoA = Bindings.EditableDouble (type, "MaxAoA");
            aoaLimitFadeoutPressure = Bindings.EditableDouble (type, "AOALimitFadeoutPressure");
            skipCircularization = type.Field<object, bool> ("SkipCircularization");
        }

        internal ClassicPath () : base ("MechJebModuleAscentClassicAutopilot")
        {
        }

        /// <summary>
        /// The altitude at which the turn starts, in meters.
        /// </summary>
        [KRPCProperty]
        public double TurnStartAltitude {
            get { return turnStartAltitude.Get (Instance); }
            set { turnStartAltitude.Set (Instance, value); }
        }

        /// <summary>
        /// The speed at which the turn starts, in <unit>m/s</unit>.
        /// </summary>
        [KRPCProperty]
        public double TurnStartVelocity {
            get { return turnStartVelocity.Get (Instance); }
            set { turnStartVelocity.Set (Instance, value); }
        }

        /// <summary>
        /// The altitude at which the turn ends, in meters.
        /// </summary>
        [KRPCProperty]
        public double TurnEndAltitude {
            get { return turnEndAltitude.Get (Instance); }
            set { turnEndAltitude.Set (Instance, value); }
        }

        /// <summary>
        /// The final flight path angle, in degrees.
        /// </summary>
        [KRPCProperty]
        public double TurnEndAngle {
            get { return turnEndAngle.Get (Instance); }
            set { turnEndAngle.Set (Instance, value); }
        }

        /// <summary>
        /// The steepness of the turn, between 0 and 1.
        /// </summary>
        [KRPCProperty]
        public double TurnShapeExponent {
            get { return turnShapeExponent.Get (Instance); }
            set { turnShapeExponent.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the autopilot chooses the turn altitudes and speed.
        /// </summary>
        [KRPCProperty]
        public bool AutoPath {
            get { return autoPath.Get (Instance); }
            set { autoPath.Set (Instance, value); }
        }

        /// <summary>
        /// The altitude at which an automatic turn starts, as a fraction of the height of the
        /// atmosphere.
        /// </summary>
        /// <remarks>Requires <see cref="AutoPath"/>.</remarks>
        [KRPCProperty]
        public float AutoTurnPercent {
            get { return autoTurnPerc.Get (Instance); }
            set { autoTurnPerc.Set (Instance, value); }
        }

        /// <summary>
        /// The factor that sets the speed at which an automatic turn starts.
        /// </summary>
        /// <remarks>Requires <see cref="AutoPath"/>.</remarks>
        [KRPCProperty]
        public float AutoTurnSpeedFactor {
            get { return autoTurnSpdFactor.Get (Instance); }
            set { autoTurnSpdFactor.Set (Instance, value); }
        }

        /// <summary>
        /// The altitude at which an automatic turn starts, in meters.
        /// </summary>
        [KRPCProperty]
        public double AutoTurnStartAltitude {
            get { return autoTurnStartAltitude (Instance); }
        }

        /// <summary>
        /// The speed at which an automatic turn starts, in <unit>m/s</unit>.
        /// </summary>
        [KRPCProperty]
        public double AutoTurnStartVelocity {
            get { return autoTurnStartVelocity (Instance); }
        }

        /// <summary>
        /// The altitude at which an automatic turn ends, in meters.
        /// </summary>
        [KRPCProperty]
        public double AutoTurnEndAltitude {
            get { return autoTurnEndAltitude (Instance); }
        }

        /// <summary>
        /// Whether the autopilot steers by the velocity vector rather than the position vector.
        /// </summary>
        /// <remarks>It is more accurate, though large craft may do better without it.</remarks>
        [KRPCProperty]
        public bool CorrectiveSteering {
            get { return correctiveSteering.Get (Instance); }
            set { correctiveSteering.Set (Instance, value); }
        }

        /// <summary>
        /// The gain of the corrective steering.
        /// </summary>
        /// <remarks>Requires <see cref="CorrectiveSteering"/>.</remarks>
        [KRPCProperty]
        public double CorrectiveSteeringGain {
            get { return correctiveSteeringGain.Get (Instance); }
            set { correctiveSteeringGain.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to limit the angle of attack.
        /// </summary>
        [KRPCProperty]
        public bool LimitAngleOfAttack {
            get { return limitAoA.Get (Instance); }
            set { limitAoA.Set (Instance, value); }
        }

        /// <summary>
        /// The largest angle of attack, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="LimitAngleOfAttack"/>.</remarks>
        [KRPCProperty]
        public double MaxAngleOfAttack {
            get { return maxAoA.Get (Instance); }
            set { maxAoA.Set (Instance, value); }
        }

        /// <summary>
        /// The dynamic pressure below which the angle of attack limit fades out, in pascals.
        /// </summary>
        /// <remarks>Requires <see cref="LimitAngleOfAttack"/>.</remarks>
        [KRPCProperty]
        public double AngleOfAttackLimitFadeoutPressure {
            get { return aoaLimitFadeoutPressure.Get (Instance); }
            set { aoaLimitFadeoutPressure.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to skip the circularization burn.
        /// </summary>
        [KRPCProperty]
        public bool SkipCircularization {
            get { return skipCircularization.Get (Instance); }
            set { skipCircularization.Set (Instance, value); }
        }
    }
}
