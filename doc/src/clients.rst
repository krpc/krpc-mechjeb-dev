Clients
=======

Each kRPC client ships stubs of kRPC's MechJeb service, the API for MechJeb 2.15. This service
has the API for MechJeb's dev builds. Each `release
<https://github.com/krpc/krpc-mechjeb-dev/releases>`_ of the service has an archive of the stub
for each client, ``krpc-mechjeb-dev-<client>-<version>.zip``, which takes the place of the
client's. Lua needs none.

Python
------

The client builds the service from the server when it connects, so a script runs without the
stub. The stub, ``krpc_mechjeb_dev.py``, gives the types of the dev API, and its enumerations.
Put it next to your script, and connect with a connection type that has the stub's
``MechJebConnection`` in place of the client's ``KSPConnection``:

.. code-block:: python

   import krpc
   from krpc.ksp.spacecenter import SpaceCenterConnection
   from krpc.services.krpc import KRPCConnection
   from krpc_mechjeb_dev import MechJebConnection


   class Connection(KRPCConnection, SpaceCenterConnection, MechJebConnection):
       """A connection with the MechJeb module of krpc-mechjeb-dev."""


   conn = krpc.connect[Connection]()

A connection type that also has ``KSPConnection`` runs, but type checkers reject it, as both
declare ``mech_jeb``.

C#
--

The client compiles kRPC's stub into ``KRPC.Client.dll``. Add ``MechJeb.cs`` to your project.
Its types take the place of the client's, so turn off the warning that says so, in the project
file:

.. code-block:: xml

   <NoWarn>CS0436</NoWarn>

The extension method on the connection is declared by both, so ``conn.MechJeb ()`` is
ambiguous. Call it as a static method:

.. code-block:: csharp

   using KRPC.Client.Services.MechJeb;

   var mj = ExtensionMethods.MechJeb (conn);

C++ and C-nano
--------------

The service's code is in its header, ``krpc/services/mech_jeb.hpp`` or
``krpc_cnano/services/mech_jeb.h``. Extract the archive, and put its ``include`` folder before the
client's on the include path, so the compiler takes the stub's header:

.. code-block:: console

   $ unzip krpc-mechjeb-dev-cpp-<version>.zip -d mechjeb
   $ g++ -I mechjeb/include -I /usr/local/include ...

Java
----

The client has kRPC's stub in ``krpc-java.jar``. Compile the archive's ``MechJeb.java`` against
the client, and put the classes before the client on the classpath, so the Java runtime loads
them:

.. code-block:: console

   $ javac -cp krpc-java.jar:protobuf-java.jar:javatuples.jar -d mechjeb \
       krpc/client/services/MechJeb.java
   $ java -cp mechjeb:krpc-java.jar:protobuf-java.jar:javatuples.jar ...

JavaScript
----------

The client builds the service from the server when it connects. The stub, ``mechjeb.ts``,
gives the types of the dev API, and declares its enumerations and exceptions in place of the
client's. Import it, and give the service its type:

.. code-block:: typescript

   import { connect } from "@krpc-org/krpc";
   import * as MechJeb from "./mechjeb.js";

   const conn = await connect();
   const mj = conn.mechJeb as unknown as MechJeb.Service;

The stub leaves out the service's types in server-side expressions, which the client already
declares for kRPC's stub.

Lua
---

The client builds the service from the server when it connects, and has no stub to replace.
