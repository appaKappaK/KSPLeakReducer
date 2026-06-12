# NoMoreLeaks

NoMoreLeaks is a small Kerbal Space Program 1 mod that reduces stock event
callback memory leaks during scene changes, editor use, vessel unloading, and
other teardown paths.

It works alongside
[KSPCommunityFixes](https://github.com/KSPModdingLibs/KSPCommunityFixes).
KSPCommunityFixes detects and cleans many leaks after they occur; NoMoreLeaks
tries to prevent selected stock callbacks from being left behind in the first
place.

## Features

- Reduces callback leaks caused by stock KSP and Breaking Ground modules.
- Cleans up inventory callbacks during part creation, deletion, editor exit,
  and launch.
- Cleans callback-owning modules throughout child-part hierarchies during
  explicit subtree deletion.
- Handles several map, tracking station, navigation, vessel, and UI teardown
  cases.
- Runs lightweight cleanup during play and a broader cleanup during scene
  changes.
- Includes optional verbose logging for troubleshooting.

## Requirements

- Kerbal Space Program `1.12.x`
- Harmony 2
- KSPCommunityFixes is strongly recommended

## Installation

Copy the packaged `NoMoreLeaks` folder into your KSP `GameData` directory:

```text
GameData/
  000_Harmony/
    0Harmony.dll
  NoMoreLeaks/
    NoMoreLeaks.cfg
    NoMoreLeaks.version
    Plugins/
      NoMoreLeaks.dll
```

After starting KSP, `KSP.log` should contain:

```text
[NoMoreLeaks] Harmony patches applied
```

## Configuration

`GameData/NoMoreLeaks/NoMoreLeaks.cfg` contains:

```text
NoMoreLeaks
{
    VerboseDebugLogging = true
}
```

Set `VerboseDebugLogging` to `false` to reduce log output. Normal cleanup
continues to run when verbose logging is disabled.

## Compatibility

NoMoreLeaks targets Kerbal Space Program `1.12.x` and requires Harmony 2. It is
designed to complement KSPCommunityFixes: NoMoreLeaks tries to prevent selected
stock callbacks from being stranded, while KSPCommunityFixes detects and cleans
callbacks that remain.

No other gameplay or content mods are required. NoMoreLeaks includes a small
number of optional compatibility fixes for known mod interactions, but they are
used only when the relevant mod is installed.

NoMoreLeaks does not attempt broad third-party leak cleanup. Its primary scope
remains stock KSP and Breaking Ground lifecycle behavior.

## Troubleshooting

After starting KSP, confirm that `KSP.log` contains:

```text
[NoMoreLeaks] Harmony patches applied
```

This confirms that Harmony loaded every NoMoreLeaks patch successfully. If the
message is missing, confirm that Harmony 2 and NoMoreLeaks are installed
correctly, then check the surrounding log entries for a patching exception.

Cleanup messages appear only when NoMoreLeaks actually removes callbacks, so
they may not appear during every session:

```text
[NoMoreLeaks] Removed N destroyed callback owners
[NoMoreLeaks] Scene-unload removed N destroyed callback owners
```

When verbose logging is enabled, detailed cleanup messages begin with:

```text
[NoMoreLeaks:Debug]
```

Include `KSP.log` when reporting a problem.

## Project Information

- See [CHANGELOG.md](CHANGELOG.md) for release notes.
- See [summaries/README.md](summaries/README.md) for development, validation,
  and leak-history notes.
  
## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.
