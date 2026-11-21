using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb.Maneuver
{
    /// <summary>
    /// Return from a moon to the body it orbits.
    /// </summary>
    [KRPCClass (Service = "MechJeb")]
    public sealed class OperationMoonReturn : Operation
    {
        static Field<object, double> periapsis;
        static Field<object, bool> inclinationFlag;
        static Field<object, double> inclination;

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.OperationMoonReturn");
            periapsis = Bindings.EditableDouble (type, "Periapsis");
            inclinationFlag = type.Field<object, bool> ("InclinationFlag");
            inclination = Bindings.EditableDouble (type, "Inclination");
        }

        internal OperationMoonReturn () : base ("OperationMoonReturn")
        {
        }

        /// <summary>
        /// The approximate periapsis altitude at the body, in meters.
        /// </summary>
        [KRPCProperty]
        public double MoonReturnAltitude {
            get { return periapsis.Get (Instance); }
            set { periapsis.Set (Instance, value); }
        }

        /// <summary>
        /// Whether to aim for <see cref="Inclination"/>.
        /// </summary>
        [KRPCProperty]
        public bool InclinationFlag {
            get { return inclinationFlag.Get (Instance); }
            set { inclinationFlag.Set (Instance, value); }
        }

        /// <summary>
        /// The inclination of the orbit around the body after the return, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="InclinationFlag"/>.</remarks>
        [KRPCProperty]
        public double Inclination {
            get { return inclination.Get (Instance); }
            set { inclination.Set (Instance, value); }
        }
    }
}
