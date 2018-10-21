
# niacop

a cross platform self-tracker tool

## `niacop.sh` script

for convenience, a script is provided to call `dotnet niacop.dll`. This script can be symlinked as needed to get `niacop` in PATH.

## config and paths

+ configuration file is located at `~/.config/niacop/niacop.conf`
+ data is divided into profiles, stored at `~/.local/share/niacomp/<profile>/`

## modes

### activity tracking

+ activity tracking mode will monitor active windows and applications and timestamps
+ run with `niacop activity`

### state tracking

TODO


## platform support

the window tracking features requires native support on some platforms. see the `external/` directory for sources.
