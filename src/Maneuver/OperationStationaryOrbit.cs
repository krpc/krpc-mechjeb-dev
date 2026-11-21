using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb.Maneuver
{
    /// <summary>
    /// A transfer to the stationary orbit of the body, above <see cref="TargetLongitude"/>.
    /// </summary>
    /// <remarks>
    /// The transfer is a Hohmann transfer with a second node to circularize. Throws an
    /// <see cref="OperationException"/> from <c>MakeNodes</c> if the stationary orbit is outside
    /// the body's sphere of influence.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class OperationStationaryOrbit : Operation
    {
        static Field<object, double> targetLongitude;

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.OperationStationaryOrbit");
            targetLongitude = type.Field<object, double> ("targetLongitude");
        }

        internal OperationStationaryOrbit () : base ("OperationStationaryOrbit")
        {
        }

        /// <summary>
        /// The surface longitude to stay above, in degrees.
        /// </summary>
        [KRPCProperty]
        public double TargetLongitude {
            get { return targetLongitude.Get (Instance); }
            set { targetLongitude.Set (Instance, value); }
        }
    }
}
