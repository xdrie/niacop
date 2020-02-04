
# niacop

a cross platform self-tracker tool

niacop has been tested on windows and linux.

## config and paths

+ configuration file is located at `~/.config/niacop/niacop.conf`
+ data is divided into profiles, stored at `~/.local/share/niacomp/<profile>/`

## modes

### activity tracking

+ activity tracking mode will monitor active windows and applications and timestamps
+ run with `niacop activity`

### book

+ an interactive mode to list words
+ an option to recall recent entries, count is configurable
+ run with `niacop book`

## platform support

the window tracking features requires native support on some platforms. see the `external/` directory for sources.

## useful commands
```
# run niacop
niacop activity | tee ~/.log/niacop/niacop.log
# export niacop data
sqlite3 -header -csv ~/.local/share/niacop/profile_debugging/tracker/activity.db "select * from session;" > ~/.local/share/niacop/profile_debugging/analysis/activity.csv
```

