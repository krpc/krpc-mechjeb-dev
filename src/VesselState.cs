using System;
using KRPC.Utils;

namespace KRPC.MechJeb
{
    /// <summary>
    /// The vessel state of the active vessel's MechJeb.
    /// </summary>
    static class VesselState
    {
        static Func<object, object> instance;
        static Func<object, double> time;
        static Func<object, double> celestialLongitude;
        static Func<object, double> latitude;
        static Func<object, double> longitude;

        internal static void Load (MemberBinder core)
        {
            instance = core.Getter<object, object> ("VesselState");
            var type = Bindings.Type ("MuMech.VesselState");
            time = type.Getter<object, double> ("Time");
            celestialLongitude = type.Getter<object, double> ("CelestialLongitude");
            latitude = type.Getter<object, double> ("Latitude");
            longitude = type.Getter<object, double> ("Longitude");
        }

        static object Instance {
            get { return instance (MechJeb.Core); }
        }

        public static double Time {
            get { return time (Instance); }
        }

        public static double CelestialLongitude {
            get { return celestialLongitude (Instance); }
        }

        public static double Latitude {
            get { return latitude (Instance); }
        }

        public static double Longitude {
            get { return longitude (Instance); }
        }
    }
}
