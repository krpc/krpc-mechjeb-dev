.. currentmodule:: MechJeb

MechJeb
=======

`MechJeb <https://github.com/MuMech/MechJeb2>`_ is an autopilot. The :doc:`MechJeb service
</python/api/mechjeb>` follows MechJeb's windows: a script sets what a player would set in a
window, then engages the autopilot. This tutorial walks through a script that flies the stock
PT Series Munsplorer from the launch pad to the surface of Minmus. MechJeb adds itself to every
command pod, so the craft needs no extra part.

The script is in Python, and you can :download:`download the whole program
</scripts/tutorials/mechjeb/LaunchToMinmus.py>`. It starts with its helpers. ``wait_for`` waits
until a condition is true, with an event. ``print_steps`` prints each step of an autopilot, from
a stream of its status. ``print_orbit`` prints the vessel's orbit:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :end-before: # The target orbit, set as in

Ascent to Orbit
---------------

The first part of the script launches the vessel into a 100 km circular orbit with Ascent
Guidance, and prints each step of the autopilot as it climbs:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # The target orbit, set as in
   :end-at: print_orbit()

The script sets the target orbit on :class:`AscentGuidance`, and the settings of MechJeb's
Ascent Settings window on :attr:`AscentGuidance.settings`. With :attr:`AscentSettings.autostage`
on, MechJeb stages the vessel once the first stage is activated. The autopilot disengages once
it has circularized the orbit.

:attr:`AscentGuidance.ascent_type` picks the ascent path. The classic path flies a turn whose
shape :attr:`AscentGuidance.classic_path` sets. PSG, Polynomial SQP Guidance, computes its own
path, and is the one for Realism Overhaul. Its settings are on
:attr:`AscentGuidance.psg_settings`.

Transfer to Minmus
------------------

The second part takes the vessel from its orbit of Kerbin to a 20 km orbit of Minmus. It plans
each maneuver with the Maneuver Planner, and has MechJeb execute it:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: planner = mj.maneuver_planner
   :end-at: print_orbit()

Each operation of :class:`ManeuverPlanner` is an object with the settings of the planner's
window. Its ``make_nodes`` method, such as :meth:`OperationPlane.make_nodes`, adds the
maneuver nodes, which the planner then executes:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: def execute_nodes():
   :end-at: wait_for(lambda: not planner.executing_node)

:attr:`ManeuverPlanner.autowarp` has MechJeb warp time to each burn. While a node executes,
:attr:`ManeuverPlanner.executing_node` is true.

:attr:`AscentSettings.autostage` applies only during the ascent. For the burns, the script
turns on the autostaging of MechJeb's Utilities window, :attr:`Utilities.staging`, which stages
the vessel once a stage burns out:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Stage once a stage burns out
   :end-at: mj.utilities.staging.enabled = True

MechJeb plans the transfer to its own target, which the game's target sets. MechJeb reads the
target on its next update, so the script waits for :attr:`TargetController.normal_target_exists`:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Target Minmus, and wait
   :end-before: # Match planes with Minmus

Minmus orbits Kerbin at an inclination of 6 degrees, and the vessel's orbit is equatorial. The
plane change comes first, at the node where the two orbits cross:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Match planes with Minmus
   :end-before: # The Hohmann transfer

Each operation picks the time of its maneuver with its :class:`TimeSelector`.
:attr:`TimeReference.rel_nearest_ad` is the nearer of the relative ascending and descending
nodes.

The Hohmann transfer burns once to reach Minmus' orbit. Minmus has a sphere of influence, so
the transfer stops at the encounter, with :attr:`OperationTransfer.capture` off:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # The Hohmann transfer
   :end-before: # Fine tune the encounter

:attr:`TimeReference.computed` has MechJeb compute the time of the burn, at the phase angle
that reaches Minmus.

The transfer orbit passes Minmus at some distance. A course correction moves the periapsis at
Minmus to 20 km:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Fine tune the encounter
   :end-before: # Warp into Minmus

Last, the script warps into Minmus' sphere of influence, and circularizes the orbit at its
periapsis:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Warp into Minmus
   :end-before: sc.clear_target()

.. note::

   To launch straight into Minmus' plane, and skip the plane change, target Minmus, enable the
   autopilot, and call :meth:`AscentGuidance.launch_to_target_plane` in place of activating the
   first stage. MechJeb warps to the launch time and stages the vessel itself.

Landing on Minmus
-----------------

The last part lands the vessel on Minmus' Great Flats with Landing Guidance, and prints the
autopilot's steps as it descends:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Stage until the next stage

The landing legs are on the lander, above the transfer stage. The legs'
:attr:`SpaceCenter.Part.decouple_stage` is the stage that drops the lander. The script stages
until that stage is next, which drops the transfer stage and activates the lander's engine.
It first turns off MechJeb's autostaging, so the script alone stages the lander:

.. literalinclude:: /scripts/tutorials/mechjeb/LaunchToMinmus.py
   :language: python
   :start-at: # Stage until the next stage
   :end-before: # Land on Minmus' Great Flats

MechJeb keeps the throttle at 5% or more. At full thrust, the lander's engine is too strong for
Minmus' weak gravity, and the lander climbs even at 5% throttle. A thrust limit of 30% lets the
lowest throttle leave the lander falling. :attr:`ThrustLimiter.min_throttle` sets that lowest
throttle.

:meth:`LandingGuidance.set_target` sets the landing target, as a latitude and longitude. The
autopilot burns to deorbit over the target, then brakes to a hover 200 m above it. Once it has
stopped drifting, it descends at :attr:`LandingGuidance.touchdown_speed`, and disengages once
the vessel has landed.

.. seealso::

   :doc:`/python/api/mechjeb/migration` for scripts written for kRPC's MechJeb service.
