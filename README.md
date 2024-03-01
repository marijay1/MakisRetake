[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/) [![Github All Releases](https://img.shields.io/github/downloads/marijay1/MakisRetake/total.svg)](https://github.com/marijay1/MakisRetake/releases)
# Maki's Retakes

A CS2 Retakes plugin, created using CounterStrikeSharp.
This plugin was created for my own server, so I will not go out of my way to add requested features.


## Features

- Ability to manage spawns from in-game
- VIPs have priority in queue
- Autoplant
- Retakes config


## Roadmap

- [ ] More Configuration

- [ ] Edit mode for configuring map spawns

- [x] Translations

- [ ] Bombsite announcement image 

- [x] Bombsite announcement announcer

- [ ] Add better feedback to the player/admin

- [ ] Add better Logging


## Installation

- Download the [latest release](https://github.com/marijay1/MakisRetake/releases)
- Extract the folder into the **plugins** folder in the **counterstrikesharp** directory
## Configuration

A file in the **configs/MakisRetake** folder in the **counterstrikesharp** directory named `MakisRetake.json` will be generated on the first load of the plugin. It will contain the following configurations:

| Config                           | Description                                                       | Default |
|----------------------------------|-------------------------------------------------------------------|---------|
| MaxPlayers                       | The maximum number of players allowed in the game at any time.    | 9       |
| TerroristRatio                   | The percentage of the total players that should be Terrorists.    | 0.45    |
| RoundsToScramble                 | The number of rounds won in a row before the teams are scrambled. | 5       |

## Commands

| Command         | Arguments                          | Description                                                                 | Permissions |
|-----------------|------------------------------------|-----------------------------------------------------------------------------|-------------|
| css_addspawn    | [T/CT] [A/B] [Y/N (planter spawn)] | Adds a spawn in your current location. This includes where you are looking  | @css/admin  |
| css_removespawn |                                    | Removes the nearest spawn point                                             | @css/admin  |

## Acknowledgements

I used the below repos for inspiration:
 - [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes)
 - [yonilerner/cs2-retakes-allocator](https://github.com/matiassingers/awesome-readme)

