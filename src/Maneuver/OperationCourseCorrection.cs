using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb.Maneuver
{
    /// <summary>
    /// Fine tune the closest approach to the target.
    /// </summary>
    [KRPCClass (Service = "MechJeb")]
    public sealed class OperationCourseCorrection : Operation
    {
        static Field<object, double> periapsis;
        static Field<object, bool> inclinationFlag;
        static Field<object, double> inclination;
        static Field<object, double> interceptDistance;

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.OperationCourseCorrection");
            periapsis = Bindings.EditableDouble (type, "Periapsis");
            inclinationFlag = type.Field<object, bool> ("InclinationFlag");
            inclination = Bindings.EditableDouble (type, "Inclination");
            interceptDistance = Bindings.EditableDouble (type, "InterceptDistance");
        }

        internal OperationCourseCorrection () : base ("OperationCourseCorrection")
        {
        }

        /// <summary>
        /// The periapsis altitude at a target body, in meters.
        /// </summary>
        [KRPCProperty]
        public double FinalPeriapsisAltitude {
            get { return periapsis.Get (Instance); }
            set { periapsis.Set (Instance, value); }
        }

        /// <summary>
        /// The closest approach distance to a target vessel, in meters.
        /// </summary>
        [KRPCProperty]
        public double InterceptDistance {
            get { return interceptDistance.Get (Instance); }
            set { interceptDistance.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to aim for <see cref="Inclination"/> at the target.
        /// </summary>
        /// <remarks>Applies when the target is a celestial body.</remarks>
        [KRPCProperty]
        public bool InclinationFlag {
            get { return inclinationFlag.Get (Instance); }
            set { inclinationFlag.Set (Instance, value); }
        }

        /// <summary>
        /// The inclination of the orbit at the target, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="InclinationFlag"/>.</remarks>
        [KRPCProperty]
        public double Inclination {
            get { return inclination.Get (Instance); }
            set { inclination.Set (Instance, value); }
        }
    }
}
