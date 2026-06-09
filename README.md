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
- Cleans up editor inventory callbacks during part deletion and launch.
- Handles several map, tracking station, navigation, vessel, and UI teardown
  cases.
- Runs lightweight cleanup during play and a broader cleanup during scene
  changes.
- Includes optional verbose logging for troubleshooting.

NoMoreLeaks intentionally focuses on stock KSP callback owners. It does not
attempt to fix every leak reported from third-party mods.

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

NoMoreLeaks patches stock KSP lifecycle and callback behavior with Harmony. It
does not replace KSPCommunityFixes and is designed to be used with it.

Third-party callback cleanup is currently outside this mod's scope.

## Troubleshooting

Confirm that Harmony 2 is installed and check `KSP.log` for:

```text
[NoMoreLeaks] Harmony patches applied
[NoMoreLeaks] Removed N destroyed callback owners
```

When verbose logging is enabled, additional entries begin with:

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
