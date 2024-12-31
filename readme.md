<img src="./BingoLogo.png">

# Baphomet's Bingo

Baphomet's Bingo is a modification (mod) for ULTRAKILL that  adds a multiplayer competitive bingo-like gamemode that tasks teams of players to race through levels with the quickest time or highest style to claim them for their team, and to be the first team to claim a bingo by claiming a complete row, column or diagonal for their team color.

# Features

- A new multiplayer gamemode centered around bingo
- Support for Angry custom maps for a bigger map pool, with additional maps regularly being added
- Configuration of game settings and criteria (Time or style, requiring P-Ranks...)
- Allows for mid-game downloading of maps

# Download & Installation

Baphomet's Bingo is available for download on [Thunderstore](https://www.google.com) (recommended), and as a download package on GitHub.
#### Baphomet's Bingo requires the Angry Level Loader mod to function. Downloading Baphomet's Bingo via ThunderStore will automatically install it for you, however if you are having issues, you can manually download it [here](https://thunderstore.io/c/ultrakill/p/EternalsTeam/AngryLevelLoader/).

# FAQ

### Will there be additional maps added to the map pool?

The final remaining campaign levels will be added upon release, and additional Angry levels that are suitable for the gameplay flow will be added based on community suggestions and recommendations.

### Will there be support for [insert map loader] here?

I will investigate possibilities for support for trending map loader mods in future, however nothing is certain.

### Why is Baphomet's Bingo locked for me?
You may be in one of two scenarios:
- You have not yet completed 7-4 and obtained the full (current) arsenal the game has, which is required by virtually every custom level. Locking the mod behind this check ensures that players do not find themselves softlocked in custom levels due to missing equipment.
- You may be using a mod that is not whitelisted. To ensure fair play for all players in a game, a mod whitelist is active. If you wish to request a commonly used mod to be whitelisted, feel free to file an issue on the Github page or drop it over in the Baphomet's Bingo Discord.


# Troubleshooting

### I am unable to connect to the Baphomet's Bingo servers.
Please try using a VPN if possible. The Baphomet's Bingo servers are proxied behind Cloudflare, the latter of which may not allow some specific IP ranges for reasons I am not able to directly provide support for.
If you are still having problems, please join the Baphomet's Bingo Discord and mention your problem, and I will try to help you to the best of my ability.

### I am unable to join a game that says "This game is not accepting new players".
The host of the game you are trying to join has manually defined and set teams, which locks the lobby and prevents new players from joining. This is an intentional design choice to prevent abnormal behavior. If possible, ask the host of the game to reset the teams in order to unlock the lobby and allow new players to join.

### I am receiving an error when the mod attempts to download, or load a custom level.
Inform me of the level name you're attempting to load and I will investigate ASAP. This may point to the level bundle in question being moved or deleted from the Angry level catalogue and can no longer be used, and I will need to update the catalog used on the bingo servers.

### I am consistently losing connection to games in the middle of gameplay.
This may point to server-side errors occuring when a player performs a specific action. Inform me ASAP when this happens so I can investigate.

Additionally,  note the Baphomet's Bingo server automatically restarts every at 12PM and midnight GMT, which will disconnect all players and end all games that may be happening during this time. Please avoid starting any games during this period until after the restart has completed to avoid disruption.


# Map credits & usage
View [maps.md](maps.md) for full credits and information.

# Credits & Contributors
- Baphomet's Bingo created & maintained by Clearwater
- Angry integration support and consultation by EternalUnion

A **huge** thank you to the following members of the ULTRAKILL and Angry community who helped me test this mod out and helped me track down bugs and other problems: Frizou, FruitCircuit, JOE, LambCG, Lena, CEO of Gaming, Draghtnim, FNChannel, RedNova, yusufturk

### Baphomet's Bingo uses the following libraries and dependencies:

- Client:
  - [JSON.Net](https://github.com/JamesNK/Newtonsoft.Json) by [NewtonSoft](https://www.newtonsoft.com/json), licenced under the MIT Licence.
  - [WebSocketSharp](https://github.com/sta/websocket-sharp) by [sta](https://github.com/sta), licenced under the MIT Licence.
  - [Tommy](https://github.com/dezhidki/Tommy) by [dezhidki](https://github.com/dezhidki), licenced under the MIT Licence.
  -  [AngryLevelLoader](https://github.com/eternalUnion/AngryLevelLoader) by [EternalUnion](https://github.com/eternalUnion), licenced under the MIT Licence.

- Server:
  - [PHP-CLI-Colors](https://github.com/mikeerickson/php-cli-colors) by [mikeerickson](https://github.com/mikeerickson), licenced under the MIT Licence.
  - [Websocket-PHP](https://github.com/sirn-se/websocket-php) by [Sirn-se](https://github.com/sirn-se/), licenced under the ISC Licence.
  - [PHPDotEnv](https://github.com/vlucas/phpdotenv) by [VLucas](https://github.com/vlucas), licenced under the MIT Licence.