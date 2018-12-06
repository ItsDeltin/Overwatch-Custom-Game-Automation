# Overwatch Custom Game Automation
A library for automating Overwatch custom games.

## Prerequisites
- [Overwatch](http://playoverwatch.com/en-us/)
- .Net Framework 4.7.1 or later.

## Getting Started
Add a reference to `Deltin.CustomGameAutomation` and create a `CustomGame` object.
```C#
using Deltin.CustomGameAutomation;
...
CustomGame cg = new CustomGame();
```
#### Usage Example:
```C#
// Writes to the console the number of players in the custom game server.
int allPlayers = cg.AllCount;
Console.WriteLine("Total player count: " + allPlayers);

// Writes to the console the number of players in the blue team.
int blueCount = cg.BlueCount;
Console.WriteLine("Players in blue: " + blueCount);

// Sends a message to the chat.
cg.Chat.SendChatMessage("Welcome to my custom game!");

// Moves all players in blue to red.
var slots = cg.GetSlots(SlotFlags.Blue);
foreach (int slot in slots)
{
	cg.Interact.SwapToRed(slot);
}

// Adds 3 hard AI Bastions to the red team.
cg.AI.AddAI(AIHero.Bastion, Difficulty.Hard, Team.Red, 3);

// Sets the selected map to Volskaya Industries, Hollywood, and Busan.
cg.ToggleMap(
	modesEnabled: Gamemode.Assault | Gamemode.AssaultEscort | Gamemode.Control | Gamemode.Escort,
	currentEvent: OWEvent.None,
	toggleAction: ToggleAction.DisableAll,
	Map.A_VolskayaIndustries, Map.AE_Hollywood, Map.C_Busan
	);
	
// Invites a player to the game.
cg.InvitePlayer("Tracer#1234", Team.BlueAndRed);

// ...And more!
```
#### Not working?
Overwatch's colorblind, gamma, and brightness settings must be set to default. Contrast must be set to the minimum.

The default method for capturing the Overwatch screen is BitBlt. If the library doesn't work for you, use ScreenCopy.
```C#
CustomGame cg = new CustomGame(
	new CustomGameBuilder()
	{
		ScreenshotMethod = ScreenshotMethod.ScreenCopy
	});
```

---
<details>
<summary>The library can...</summary>

- Get the player count.
- Get the slots filled.
- Get the current Overwatch event.
- Change the enabled maps.
- Get the current map.
- Get the enabled modes.
- Invite a player to the game using their battletag.
- Get the slots that are invited to the game.
- Start the map, start the gamemode, restart, and send the custom game server to the lobby. 
- Set the hero roster.
- Set the hero settings.
- Swap 2 players' slots.
- Swap both teams.
- Swap a player's team.
- Swap a player to spectators.
- Check if a slot is an AI.
- Add an AI.
- Remove an AI.
- Remove all AIs.
- Edit an AI.
- Get an AI's difficulty.
- Send a chat message.
- Join a chat channel. (All,Match,Team,Group,PM)
- Listen for a chat command.
- Pause/unpause the game
- Get the dead players' slots.
- Get the hero a slot is playing.
- Check if a slot is a friend.
- Check if a slot chose a hero.
- Check if a slot's ultimate ability is ready.
- Get the max player count.
- Get the slot the moderator is in.
- Load a preset.
- Set the game name.
- Set the max players.
- Set a team's name.
- Track when a slot updates.
- Get the current game state.
</details>


## Examples
- [ZombieBot]()