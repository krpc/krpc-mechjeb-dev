# kRPC MechJeb dev service

A replacement for [kRPC](https://github.com/krpc/krpc)'s MechJeb service, for MechJeb dev
builds. kRPC's own service supports the latest MechJeb release only.

This is a best-effort project. Report problems with it here, not on kRPC's tracker.

## Install

Install the kRPC release and the MechJeb dev build listed in [pins.toml](pins.toml), then unzip
`krpc-mechjeb-dev-<version>.zip` into the KSP folder. It replaces kRPC's MechJeb files in
`GameData/kRPC`. Updating kRPC puts its own MechJeb service back, so unzip this release again
after each kRPC update.

The service is called `MechJeb`, as kRPC's is, so scripts use it with no change of name.

## Build

Needs the .NET 10 SDK and Python 3.14:

```
python -m venv .venv
.venv/bin/pip install -r requirements.txt
.venv/bin/python scripts/build_service.py
```

`scripts/fetch.py` downloads the releases in `pins.toml` into `build/deps`, and
`scripts/build_service.py` builds the service against them.

## Test

The tests drive KSP 1.12.5, through `krpctools.game`. They install the MechJeb dev build in
`pins.toml`, and the pinned kRPC release with the built service in place of kRPC's:

```
KSP_DIR=/path/to/KSP .venv/bin/pytest test/test_smart_ass.py
```

Each run builds the service first. `test/test_bindings.py` lists the MechJeb members the service
fails to bind, and the other tests run with those members missing. `--mechjeb=release` tests
against the MechJeb release kRPC pins in place of the dev build.

## Source

Copied from `service/MechJeb` of krpc/krpc v0.8.0
(2784aa03e4caba11ad152018e939c7e77f7c4518). The code is adapted from
[KRPC.MechJeb](https://github.com/Genhis/KRPC.MechJeb) by Genhis, and is under the GPLv3.
