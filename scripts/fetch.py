"""Download and extract the releases in pins.toml into build/deps.

build/deps/<name>.zip is the archive, checked against its sha256, and build/deps/<name>/ its
contents. An archive already there with the right digest is not downloaded again. A pin with
an upstream URL falls back to it while its own URL is missing, as the mirror of a new MechJeb
build exists only once its release is published.
"""

import hashlib
import os
import shutil
import sys
import tomllib
import urllib.error
import urllib.request
import zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEPS = os.path.join(ROOT, "build", "deps")


def pins():
    with open(os.path.join(ROOT, "pins.toml"), "rb") as f:
        return tomllib.load(f)


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as f:
        for block in iter(lambda: f.read(1 << 20), b""):
            digest.update(block)
    return digest.hexdigest()


def get(url, path):
    print("downloading", url)
    with urllib.request.urlopen(url, timeout=60) as response:
        with open(path, "wb") as f:
            shutil.copyfileobj(response, f)
    return url


def download(pin, path):
    """Download a pin to path, from its URL or else its upstream URL. Returns the URL used."""
    urls = [pin["url"]]
    if pin.get("upstream", pin["url"]) != pin["url"]:
        urls.append(pin["upstream"])
    for url in urls[:-1]:
        try:
            return get(url, path)
        except urllib.error.HTTPError as e:
            if e.code != 404:
                raise
            print(url, "not found")
    return get(urls[-1], path)


def fetch(name, pin):
    archive = os.path.join(DEPS, name + ".zip")
    if not os.path.exists(archive) or sha256(archive) != pin["sha256"]:
        partial = archive + ".part"
        url = download(pin, partial)
        digest = sha256(partial)
        if digest != pin["sha256"]:
            os.remove(partial)
            sys.exit(
                "%s has sha256 %s, pins.toml gives %s" % (url, digest, pin["sha256"])
            )
        os.replace(partial, archive)
    directory = os.path.join(DEPS, name)
    shutil.rmtree(directory, ignore_errors=True)
    with zipfile.ZipFile(archive) as f:
        f.extractall(directory)
    return archive


def main():
    os.makedirs(DEPS, exist_ok=True)
    for name, pin in pins().items():
        fetch(name, pin)


if __name__ == "__main__":
    main()
