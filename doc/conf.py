"""Sphinx configuration of the krpc-mechjeb-dev docs.

scripts/docs.py generates the API pages into build/doc/src, copies the hand-written pages
beside them, and builds this configuration there.
"""

import os
import re
import tomllib

ROOT = os.environ["KRPC_MECHJEB_DEV_ROOT"]

with open(os.path.join(ROOT, "pins.toml"), "rb") as f:
    _pins = tomllib.load(f)
with open(os.path.join(ROOT, "src", "KRPC.MechJeb.csproj"), encoding="utf-8") as f:
    _match = re.search(r"<Version>(.*)</Version>", f.read())
assert _match is not None, "No <Version> in KRPC.MechJeb.csproj"
version = _match[1]
release = version
_krpc = _pins["krpc"]["version"]
_mechjeb = _pins["mechjeb"]["version"]

project = "kRPC MechJeb dev"
html_title = "kRPC MechJeb dev v%s" % release
copyright = "kRPC Org"
master_doc = "index"
source_suffix = ".rst"
primary_domain = "py"

extensions = [
    "sphinx.ext.intersphinx",
    "sphinx.ext.mathjax",
    "sphinx_design",
    "sphinx_tabs.tabs",
    "sphinx_lua_ls",
    "sphinx_csharp",
    "sphinx_java",
    "sphinxcontrib.httpdomain",
]

# The SpaceCenter types the service uses are documented on kRPC's site
intersphinx_mapping = {"krpc": ("https://krpc.github.io/krpc/%s/" % _krpc, None)}

rst_epilog = """
.. |MechJeb tested| replace:: Tested with MechJeb %s and kRPC %s.
.. |mechjeb_version| replace:: %s
.. |krpc_version| replace:: %s
""" % (_mechjeb, _krpc, _mechjeb, _krpc)

# The Lua pages are written by krpc-docgen, so the Lua language server is not used. The
# extension warns that its apidoc then has nothing to run on, on every build.
lua_ls_backend = "disable"
suppress_warnings = ["lua-ls"]
lua_ls_default_options = {"using": "krpc"}

html_theme = "pydata_sphinx_theme"
html_static_path = ["scripts"]
html_theme_options = {
    "icon_links": [
        {
            "name": "GitHub",
            "url": "https://github.com/krpc/krpc-mechjeb-dev",
            "icon": "fa-brands fa-github",
        }
    ],
}

# The pages name the SpaceCenter types as kRPC's own pages do, relative to the client's
# namespace, which resolves inside kRPC's site. kRPC's inventory holds the full names.
_namespaces = {
    "cs": "KRPC.Client.Services.",
    "java": "krpc.client.services.",
    "cpp": "krpc::services::",
}


def _qualify_spacecenter(app, env, node, contnode):
    from sphinx.ext import intersphinx

    prefix = _namespaces.get(node.get("refdomain"))
    target = node.get("reftarget", "")
    if prefix is None or not target.startswith("SpaceCenter"):
        return None
    qualified = node.deepcopy()
    qualified["reftarget"] = prefix + target
    return intersphinx.missing_reference(app, env, qualified, contnode)


def setup(app):
    app.connect("missing-reference", _qualify_spacecenter, priority=600)
