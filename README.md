
# niacop

a cross platform self-tracker tool

niacop has been tested on windows and linux.

## config and paths

+ configuration file is located at `~/.config/niacop/niacop.conf`
+ data is divided into profiles, stored at `~/.local/share/niacomp/<profile>/`

## dependencies

on linux, niacop requires:
+ `xprintidle`
+ `xprop`
+ `xdotool`
+ `xinput`
+ `xmodmap`

## build

```
cd src/niacop
dotnet build -c Release
```

## modes

### activity tracking

+ activity tracking mode will monitor active windows and applications and timestamps
+ run with `niacop activity`

### time machine

+ time query for finding what applications were in use at a given time
+ run with ex. `niacop timemachine "7:15 PM on May 22"`

### book

+ an interactive mode to just type words, they're split up by spaces
+ an option to recall recent entries, count is configurable
+ run with `niacop book`

see [book](doc/book.md)

## platform support

the window tracking features requires native support on some platforms. see the `external/` directory for sources.

## useful commands
```
# run niacop
niacop activity | tee ~/.log/niacop/niacop.log
# export niacop data
sqlite3 -header -csv ~/.local/share/niacop/profile_debugging/tracker/activity.db "select * from session;" > ~/.local/share/niacop/profile_debugging/analysis/activity.csv
```
