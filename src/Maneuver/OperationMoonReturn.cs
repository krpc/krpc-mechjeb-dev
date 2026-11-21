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

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.OperationMoonReturn");
            periapsis = Bindings.EditableDouble (type, "Periapsis");
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
    }
}
