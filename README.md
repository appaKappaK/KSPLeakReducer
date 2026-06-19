# 💧 KSPLeakReducer

[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](LICENSE) [![KSP Version](https://img.shields.io/badge/KSP-1.12.x-green.svg)](https://www.kerbalspaceprogram.com/) [![GitHub release](https://img.shields.io/github/v/release/appaKappaK/KSPLeakReducer.svg)](https://github.com/appaKappaK/KSPLeakReducer/releases)

> **KSPLeakReducer** is a small Kerbal Space Program 1 mod that reduces stock
> event callback memory leaks during scene changes, editor use, vessel
> unloading, and other teardown paths.

It works alongside
[KSPCommunityFixes](https://github.com/KSPModdingLibs/KSPCommunityFixes).
KSPCommunityFixes detects and cleans many leaks after they occur; KSPLeakReducer
tries to prevent selected stock callbacks from being left behind in the first
place.

## Features

- **Reduces callback leaks** caused by stock KSP and Breaking Ground modules.
- **Cleans up inventory callbacks** during part creation, deletion, editor
  exit, and launch.
- **Cleans callback-owning modules** throughout child-part hierarchies during
  explicit subtree deletion.
- **Handles several teardown cases**, including map, tracking station,
  navigation, vessel, and UI.
- **Runs lightweight cleanup** during play and a broader cleanup during scene
  changes.
- **Includes optional verbose logging** for troubleshooting.

## Requirements

- **Kerbal Space Program** `1.12.x`
- **Harmony 2**
- **KSPCommunityFixes** *(strongly recommended)*

## Installation

Copy the packaged `KSPLeakReducer` folder into your KSP `GameData` directory.

`KSPLeakReducer` was previously named `NoMoreLeaks`.
If you are upgrading, delete `GameData/NoMoreLeaks` first so KSP does not load both copies.

```text
GameData/
  000_Harmony/
    0Harmony.dll
  KSPLeakReducer/
    KSPLeakReducer.cfg
    KSPLeakReducer.version
    Plugins/
      KSPLeakReducer.dll
```

After starting KSP, your `KSP.log` should contain:

```text
[KSPLeakReducer] Harmony patches applied
```

## Configuration

`GameData/KSPLeakReducer/KSPLeakReducer.cfg` contains:

```text
KSPLEAKREDUCER
{
    VerboseDebugLogging = true
}
```

> **Tip:** Set `VerboseDebugLogging` to `false` to reduce log output. Normal
> cleanup continues to run when verbose logging is disabled.

## Compatibility

KSPLeakReducer targets Kerbal Space Program `1.12.x` and requires Harmony 2. It is
designed to complement **KSPCommunityFixes**: KSPLeakReducer tries to prevent
selected stock callbacks from being stranded, while KSPCommunityFixes detects
and cleans callbacks that remain.

No other gameplay or content mods are required. KSPLeakReducer includes a small
number of optional compatibility fixes for known mod interactions, but they are
used *only* when the relevant mod is installed.

KSPLeakReducer does not attempt broad third-party leak cleanup. Its primary scope
remains stock KSP and Breaking Ground lifecycle behavior.

## Troubleshooting

After starting KSP, confirm that `KSP.log` contains:

```text
[KSPLeakReducer] Harmony patches applied
```

This confirms that Harmony loaded every KSPLeakReducer patch successfully. If the
message is missing, confirm that Harmony 2 and KSPLeakReducer are installed
correctly, then check the surrounding log entries for a patching exception.

Cleanup messages appear only when KSPLeakReducer actually removes callbacks, so
they may not appear during every session:

```text
[KSPLeakReducer] Removed N destroyed callback owners
[KSPLeakReducer] Scene-unload removed N destroyed callback owners
```

When verbose logging is enabled, detailed cleanup messages begin with:

```text
[KSPLeakReducer:Debug]
```

**Please include `KSP.log` when reporting a problem.**

## Project Information

- See [CHANGELOG.md](CHANGELOG.md) for release notes.
- See [summaries/README.md](summaries/README.md) for development, validation,
  and leak-history notes.

## License

This project is licensed under the **GNU General Public License v3.0**. See the
[LICENSE](LICENSE) file for details.
