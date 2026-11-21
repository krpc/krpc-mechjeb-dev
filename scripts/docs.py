"""Build the docs into build/doc/html.

krpc-docgen writes the API pages of each client language from the templates in doc/api, into
build/doc/src/<language>/api. The hand-written pages in doc/src are copied beside them, and
Sphinx builds the lot with doc/conf.py. pyright first checks the examples against the Python
stub in build/stubs, as kRPC's docs check theirs against the client, without the checks for None that the
examples skip for brevity.
"""

import glob
import os
import shutil
import subprocess

import stubs

import build_service
from build_service import tool

ROOT = build_service.ROOT
DOC = os.path.join(ROOT, "doc")
OUT = os.path.join(build_service.BUILD, "doc")
SRC = os.path.join(OUT, "src")

# The client languages, and their titles
LANGUAGES = {
    "python": "Python",
    "csharp": "C#",
    "cpp": "C++",
    "cnano": "C-nano",
    "java": "Java",
    "js": "JavaScript",
    "lua": "Lua",
    "http": "HTTP",
}


def generate(language):
    templates = sorted(
        glob.glob(os.path.join(DOC, "api", "**", "*.tmpl"), recursive=True)
    )
    args = [
        tool("krpc-docgen"),
        language,
        os.path.join(DOC, "order.txt"),
        os.path.join(build_service.BIN, "KRPC.MechJeb.json"),
    ]
    for template in templates:
        page = os.path.splitext(os.path.relpath(template, DOC))[0] + ".rst"
        args += ["--page", template, os.path.join(SRC, language, page)]
    subprocess.check_call(args)


def check_examples():
    subprocess.check_call(
        [tool("pyright"), "-p", os.path.join(DOC, "pyrightconfig.json")]
    )


def main():
    stubs.main()
    check_examples()
    shutil.rmtree(OUT, ignore_errors=True)
    shutil.copytree(os.path.join(DOC, "src"), SRC)
    shutil.copy(os.path.join(DOC, "conf.py"), SRC)
    for language in LANGUAGES:
        generate(language)
    env = dict(os.environ, KRPC_MECHJEB_DEV_ROOT=ROOT)
    subprocess.check_call(
        [
            tool("sphinx-build"),
            "-W",
            "--keep-going",
            "-q",
            "-b",
            "html",
            SRC,
            os.path.join(OUT, "html"),
        ],
        env=env,
    )
    print(os.path.relpath(os.path.join(OUT, "html", "index.html"), ROOT))


if __name__ == "__main__":
    main()
