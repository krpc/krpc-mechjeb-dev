import krpc
from krpc.ksp.spacecenter import SpaceCenterConnection
from krpc.services.krpc import KRPCConnection
from krpc_mechjeb_dev import MechJebConnection


class Connection(KRPCConnection, SpaceCenterConnection, MechJebConnection):
    """A connection with the MechJeb module of krpc-mechjeb-dev."""


conn = krpc.connect[Connection](name="MechJeb Example")
vessel = conn.space_center.active_vessel
ascent = conn.mech_jeb.ascent_guidance

ascent.desired_orbit_altitude = 100000
ascent.desired_inclination = 6
ascent.settings.autostage = True
ascent.enabled = True

vessel.control.activate_next_stage()

event = conn.add_event(lambda: not ascent.enabled)
with event.condition:
    event.wait()

print("Launch complete")
