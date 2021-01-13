
![icon](media/icon.png)

# niacop
a cross platform automatic time-tracking tool

**niacop** runs in the background as a daemon and gradually gathers data on
your computer usage. all data collection is done locally and data never leaves
the host computer. the usage data is compiled into a local database, which can
later be queried in such a way as to allow you to figure out what applications
you were using at any time in the past. it can also optionally count key events
as a metric of engagement with an application.

supports windows via Win32 and linux via X11.
platform support is extensible, so theoretically more platforms could also be added.

extensively tested on an Arch Linux install, using the i3 window manager.

## dependencies

### linux
+ `xprintidle`
+ `xprop`
+ `xdotool`
+ `xinput`
+ `xmodmap`

some of these can be found in source form in `external/`.

### windows

the niacop windows backend is built on the Win32 API, so no external dependencies are needed.

## build

install [.NET SDK](https://dotnet.microsoft.com/download/dotnet/current), then:

```
cd src/niacop
dotnet build -c Release
```

this will create a binary `niacop`, which you should symlink to somewhere in your path as `niacop`.
then, you can use `niacop` to invoke the program.

## config/paths

+ configuration file is located at `~/.config/niacop/niacop.conf` (linux) or in `AppData` (windows)
+ data is divided into profiles, stored at `~/.local/share/niacomp/<profile>/` (linux)

sample config:
```
[profile]
name="test1"
 
[tracker]
keycounter = true # count key presses
    # tags used to categorize sessions
    [[tracker.tags]]
    name = "GAME"
    match = [
        "lutris",
        "steam",
    ]
    [[tracker.tags]]
    name = "WWEB"
    match = [
        "firefox",
    ]

[log]
verbosity = "trace"
```
for reference, see the [config model](src/niacop/Nia/Config.cs).

## modes

### activity tracking/daemon

+ activity tracking mode will monitor active windows and applications and timestamps
+ run with `niacop activity`
+ ideally, fork it to the background

### summary

+ automatically tag all sessions within a time range (using config `tracker.tags`)
+ display a ratio graph, breaking down activity by category
+ run with `niacop summary`

### time machine

+ time query for finding what applications were in use at a given time
+ run with ex. `niacop timemachine "may 22 at 7 PM"`

### book

+ an interactive mode to just type words, they're split up by spaces
+ an option to recall recent entries, count is configurable
+ run with `niacop book`

see [book](doc/book.md)

## useful commands

run niacop with a log file
```
niacop activity | tee ~/.log/niacop/niacop.log
```

export niacop data to CSV
```
sqlite3 -header -csv ~/.local/share/niacop/profile_xxxx/tracker/activity.db "select * from session;" > ~/.local/share/niacop/profile_xxxx/analysis/activity.csv
```
