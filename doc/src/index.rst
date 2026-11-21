kRPC MechJeb Dev
================

A replacement of `kRPC <https://krpc.github.io/krpc/>`_'s MechJeb service for MechJeb's dev
builds. kRPC's own service supports the latest MechJeb release, and does not work with the dev
builds. |MechJeb tested|

.. note::

   This is a best-effort project. Report problems on
   `krpc-mechjeb-dev <https://github.com/krpc/krpc-mechjeb-dev/issues>`_, not on kRPC's tracker.

Install
-------

Install kRPC |krpc_version| and MechJeb |mechjeb_version|. Then unzip
``krpc-mechjeb-dev-<version>.zip``, from the service's
`releases <https://github.com/krpc/krpc-mechjeb-dev/releases>`_, into the KSP folder. It replaces kRPC's MechJeb files in ``GameData/kRPC``. A kRPC
update puts kRPC's own service back, so unzip the release again after each one.

The service is called ``MechJeb``, as kRPC's is, so scripts use it with no change of name.

Contents
--------

* :doc:`clients` says how each client picks up the service's API.
* :doc:`api` has the API in each client language.
* :doc:`tutorials/mechjeb` flies a craft to Minmus with the service, in Python.

.. toctree::
   :hidden:

   Clients <clients>
   API Reference <api>
   Tutorial <tutorials/mechjeb>
