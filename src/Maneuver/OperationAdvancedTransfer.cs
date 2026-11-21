using System;
using KRPC.Service.Attributes;
using KRPC.Utils;

namespace KRPC.MechJeb.Maneuver
{
    /// <summary>
    /// An interplanetary transfer to the target body. The departure and transfer time come
    /// from a search, chosen as <see cref="Mode"/> sets.
    /// </summary>
    /// <remarks>
    /// MechJeb searches in the background. The first call to <c>MakeNodes</c> starts the
    /// search, and throws an <see cref="OperationException"/> until it finishes. See
    /// <see cref="Searching"/>.
    /// </remarks>
    [KRPCClass (Service = "MechJeb")]
    public sealed class OperationAdvancedTransfer : Operation
    {
        static Field<object, int> selectionMode;
        static Field<object, double> maxArrivalTime;
        static Field<object, bool> includeCaptureBurn;
        static Field<object, double> periapsisHeight;
        static Field<object, object> worker;
        static Field<object, global::CelestialBody> lastTargetCelestial;
        static Func<object, global::Orbit, double, object, double?, double?, double?, double?, double?, string> compute;
        static Func<object, global::Orbit, global::Orbit, bool> isStale;
        static Func<global::CelestialBody, double> defaultPeriapsis;
        static Func<object, bool> finished;
        static Func<object, int> progress;
        static Field<object, bool> stop;
        static Field<object, bool> targetInclinationFlag;
        static Field<object, double> targetInclination;
        static Func<object, bool> isFinished;
        static Func<object, bool> hasFailed;
        static Action<object> selectLowestDeltaV;
        static Action<object> selectAsap;
        static Func<object, double, double, bool> selectTimes;
        static TryGetSelection tryGetSelection;

        delegate bool TryGetSelection (object instance, out double departureTime, out double duration, out double deltaV);

        internal static new void Load ()
        {
            var type = Bindings.Type ("MuMech.OperationAdvancedTransfer");
            selectionMode = type.Field<object, int> ("_selectionMode");
            maxArrivalTime = Bindings.EditableDouble (type, "MaxArrivalTime");
            includeCaptureBurn = type.Field<object, bool> ("_includeCaptureBurn");
            periapsisHeight = Bindings.EditableDouble (type, "_periapsisHeight");
            worker = type.Field<object, object> ("_worker");
            lastTargetCelestial = type.Field<object, global::CelestialBody> ("_lastTargetCelestial");
            compute = type.Method<Func<object, global::Orbit, double, object, double?, double?, double?, double?, double?, string>> ("Compute");
            isStale = type.Method<Func<object, global::Orbit, global::Orbit, bool>> ("IsStale");
            defaultPeriapsis = type.Method<Func<global::CelestialBody, double>> ("DefaultPeriapsisKm");
            var calculator = Bindings.Type ("MuMech.TransferCalculator");
            finished = calculator.Getter<object, bool> ("Finished");
            progress = calculator.Getter<object, int> ("Progress");
            stop = calculator.Field<object, bool> ("Stop");
            targetInclinationFlag = type.Field<object, bool> ("TargetInclinationFlag");
            targetInclination = Bindings.EditableDouble (type, "_targetInclination");
            isFinished = type.Getter<object, bool> ("IsFinished");
            hasFailed = type.Getter<object, bool> ("HasFailed");
            selectLowestDeltaV = type.Method<Action<object>> ("SelectLowestDv");
            selectAsap = type.Method<Action<object>> ("SelectAsap");
            selectTimes = type.Method<Func<object, double, double, bool>> ("SelectTimes");
            tryGetSelection = type.Method<TryGetSelection> ("TryGetSelection");
        }

        // The target orbit that MaxArrivalTime was set or computed for
        global::Orbit arrivalTarget;

        internal OperationAdvancedTransfer () : base ("OperationAdvancedTransfer")
        {
        }

        /// <summary>
        /// The latest arrival time, as a time from now, in seconds.
        /// </summary>
        /// <remarks>
        /// Applies in the <see cref="AdvancedTransferMode.LimitedTime"/> mode. <c>MakeNodes</c>
        /// computes a default for a new target, from the synodic period and the transfer time,
        /// unless it was set while the target was selected.
        /// </remarks>
        [KRPCProperty]
        public double MaxArrivalTime {
            get { return maxArrivalTime.Get (Instance); }
            set {
                var instance = Instance;
                if (maxArrivalTime.Get (instance) != value)
                    StopSearch (instance);
                maxArrivalTime.Set (instance, value);
                var targetController = MechJeb.TargetController;
                targetController.Update ();
                arrivalTarget = targetController.InternalTargetOrbit;
            }
        }

        /// <summary>
        /// Whether to plan a capture burn at the destination.
        /// </summary>
        [KRPCProperty]
        public bool IncludeCaptureBurn {
            get { return includeCaptureBurn.Get (Instance); }
            set {
                var instance = Instance;
                if (includeCaptureBurn.Get (instance) != value)
                    StopSearch (instance);
                includeCaptureBurn.Set (instance, value);
            }
        }

        /// <summary>
        /// The altitude of the periapsis at the destination, in meters.
        /// </summary>
        /// <remarks>
        /// <c>MakeNodes</c> sets a default for a new target body, 10 km above its atmosphere or
        /// 100 km, unless it was set while the body was the target.
        /// </remarks>
        [KRPCProperty]
        public double PeriapsisHeight {
            // MechJeb holds it in kilometers
            get { return periapsisHeight.Get (Instance) * 1000d; }
            set {
                // Only the nodes depend on the periapsis, so the search goes on
                var instance = Instance;
                periapsisHeight.Set (instance, value / 1000d);
                // MechJeb's window keeps the value while the target body stays the same
                var targetController = MechJeb.TargetController;
                targetController.Update ();
                var body = targetController.Target as global::CelestialBody;
                if (body != null)
                    lastTargetCelestial.Set (instance, body);
            }
        }

        /// <summary>
        /// Whether MechJeb is searching for the departure.
        /// </summary>
        [KRPCProperty]
        public bool Searching {
            get {
                var calculator = worker.Get (Instance);
                return calculator != null && !finished (calculator);
            }
        }

        /// <summary>
        /// The progress of the search for the departure, as a percentage.
        /// </summary>
        [KRPCProperty]
        public int SearchProgress {
            get {
                var calculator = worker.Get (Instance);
                return calculator == null ? 0 : progress (calculator);
            }
        }

        /// <summary>
        /// How the search chooses the departure and transfer time.
        /// </summary>
        /// <remarks>
        /// Setting it stops a search, so the next <c>MakeNodes</c> starts one.
        /// </remarks>
        [KRPCProperty]
        public AdvancedTransferMode Mode {
            get { return (AdvancedTransferMode)selectionMode.Get (Instance); }
            set {
                var instance = Instance;
                if (selectionMode.Get (instance) != (int)value)
                    StopSearch (instance);
                selectionMode.Set (instance, (int)value);
            }
        }

        /// <summary>
        /// Whether to aim for <see cref="TargetInclination"/> at the target.
        /// </summary>
        [KRPCProperty]
        public bool TargetInclinationFlag {
            get { return targetInclinationFlag.Get (Instance); }
            set { targetInclinationFlag.Set (Instance, value); }
        }

        /// <summary>
        /// The inclination of the orbit at the target, in degrees.
        /// </summary>
        /// <remarks>Requires <see cref="TargetInclinationFlag"/>.</remarks>
        [KRPCProperty]
        public double TargetInclination {
            get { return targetInclination.Get (Instance); }
            set { targetInclination.Set (Instance, value); }
        }

        /// <summary>
        /// Whether the search finished and found no transfer.
        /// </summary>
        [KRPCProperty]
        public bool SearchFailed {
            get { return hasFailed (Instance); }
        }

        void CheckPorkchop (object instance)
        {
            if (Mode != AdvancedTransferMode.Porkchop)
                throw new InvalidOperationException ("The mode is not porkchop");
            if (!isFinished (instance))
                throw new InvalidOperationException ("The search has not finished");
        }

        /// <summary>
        /// Select the departure and transfer time that need the least delta-v. The search
        /// selects them when it finishes. Throws an exception if <see cref="Mode"/> is not
        /// <see cref="AdvancedTransferMode.Porkchop"/>, or the search has not finished.
        /// </summary>
        [KRPCMethod]
        public void SelectLowestDeltaV ()
        {
            var instance = Instance;
            CheckPorkchop (instance);
            selectLowestDeltaV (instance);
        }

        /// <summary>
        /// Select the earliest departure, with the transfer time that needs the least delta-v
        /// for it. Throws an exception if <see cref="Mode"/> is not
        /// <see cref="AdvancedTransferMode.Porkchop"/>, or the search has not finished.
        /// </summary>
        [KRPCMethod]
        public void SelectAsap ()
        {
            var instance = Instance;
            CheckPorkchop (instance);
            selectAsap (instance);
        }

        /// <summary>
        /// Select the departure and transfer time nearest the given ones. Throws an exception
        /// if <see cref="Mode"/> is not <see cref="AdvancedTransferMode.Porkchop"/>, the search
        /// has not finished, or there is no transfer near those times.
        /// </summary>
        /// <param name="departureTime">The universal time of the departure, in seconds.</param>
        /// <param name="duration">The transfer time, in seconds.</param>
        [KRPCMethod]
        public void Select (double departureTime, double duration)
        {
            var instance = Instance;
            CheckPorkchop (instance);
            if (!selectTimes (instance, departureTime, duration))
                throw new ArgumentException ("There is no transfer at those times");
        }

        /// <summary>
        /// The universal time of the selected departure, in seconds. Throws an exception if the
        /// search has not finished.
        /// </summary>
        [KRPCProperty]
        public double SelectedDepartureTime {
            get { return Selection ().Item1; }
        }

        /// <summary>
        /// The selected transfer time, in seconds. Throws an exception if the search has not
        /// finished.
        /// </summary>
        [KRPCProperty]
        public double SelectedDuration {
            get { return Selection ().Item2; }
        }

        /// <summary>
        /// The delta-v of the selected transfer that the search estimates, in <unit>m/s</unit>.
        /// Throws an exception if the search has not finished.
        /// </summary>
        [KRPCProperty]
        public double SelectedDeltaV {
            get { return Selection ().Item3; }
        }

        Tuple<double, double, double> Selection ()
        {
            double departureTime, duration, deltaV;
            if (!tryGetSelection (Instance, out departureTime, out duration, out deltaV))
                throw new InvalidOperationException ("The search has not finished");
            return Tuple.Create (departureTime, duration, deltaV);
        }

        // Stop the search, so the next MakeNodes starts one with the current settings. MechJeb's
        // window does this on any change to its settings, and we do it on a change to a
        // setting the search reads.
        static void StopSearch (object instance)
        {
            var calculator = worker.Get (instance);
            if (calculator == null)
                return;
            stop.Set (calculator, true);
            worker.Set (instance, null);
        }

        internal override void Prepare (object instance, global::Orbit orbit, double time)
        {
            // MechJeb's window records the target body, which sizes the capture orbit, and sets
            // a default periapsis when the body changes
            var body = MechJeb.TargetController.Target as global::CelestialBody;
            if (body == null)
                throw new OperationException ("The target must be a celestial body");
            if (lastTargetCelestial.Get (instance) != body) {
                periapsisHeight.Set (instance, defaultPeriapsis (body));
                lastTargetCelestial.Set (instance, body);
            }
            var targetController = MechJeb.TargetController;
            var target = targetController.InternalTargetOrbit;
            // A search from another orbit, or to another target, is restarted, as in MechJeb's window
            if (isStale (instance, orbit, target))
                StopSearch (instance);
            if (worker.Get (instance) != null)
                return;
            // MechJeb's Compute sets the default arrival time, unless given one. We give the
            // one set for this target.
            double? arrival = null;
            if (target == arrivalTarget)
                arrival = maxArrivalTime.Get (instance);
            var error = compute (instance, orbit, time, targetController.Instance, null, null, null, null, arrival);
            arrivalTarget = target;
            throw new OperationException (error ?? "Started computation");
        }
    }

    /// <summary>
    /// How the advanced transfer chooses the departure and transfer time. See
    /// <see cref="OperationAdvancedTransfer.Mode"/>.
    /// </summary>
    [KRPCEnum (Service = "MechJeb")]
    public enum AdvancedTransferMode
    {
        /// <summary>
        /// The transfer that needs the least delta-v and arrives before
        /// <see cref="OperationAdvancedTransfer.MaxArrivalTime"/>.
        /// </summary>
        LimitedTime,

        /// <summary>
        /// A transfer from a grid of departures and transfer times, the porkchop plot of
        /// MechJeb's window. The one that needs the least delta-v, unless another is selected.
        /// </summary>
        Porkchop
    }
}
