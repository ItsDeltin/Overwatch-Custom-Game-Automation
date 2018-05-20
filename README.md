# Overwatch Custom Game Automation
A library for automating Overwatch custom games.
[Click here for tutorial and documentation](https://www.abyxa.net/Library/Library.html)

## Getting Started

Add a reference to Deltin.CustomGameAutomation.
```C#
using Deltin.CustomGameAutomation;
```
Create a CustomGame object. By default, it uses the first Overwatch process it finds.
```C#
CustomGame cg = new CustomGame();
```
If you want it to use a specific Overwatch process, you can use the main window handle as an argument.
```C#
Process overwatchProcess = ...
CustomGame cg = new CustomGame(overwatchProcess.MainWindowHandle);
```
By default the CustomGame class uses BitBlt to capture screenshots of the Overwatch process. Since this method doesn't work on some systems, you can choose the screenshot method to use.
```C#
CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethod.BitBlt);
// or
CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethod.ScreenCopy);
```
- BitBlt does not require to be on top. You can have other windows over the Overwatch window and it will still work.
- ScreenCopy needs to be on top and have no windows overlapping the Overwatch window. Does not work with all border styles right now.

The Overwatch window the CustomGame class is using must have default colorblind, brightness, and gamma settings with contrast at the minimum. Some methods may not work unless you are the moderator of the custom game and/or you are in spectator.

## Examples

The code below will disable all maps except for Dorado and Eichenwalde.
```C#
CustomGame cg = new CustomGame();

// Tells the CustomGame class which modes are enabled in the custom game.
cg.ModesEnabled = new ModesEnabled()
{
	Assault = true,
	AssaultEscort = true,
	Control = true,
	Escort = true,
};

// In case of event exclusive maps, tells the CustomGame class that there are no Overwatch events currently.
cg.CurrentOverwatchEvent = Event.None;

// Disable all maps except for dorado.
cg.Maps.ToggleMap(ToggleAction.DisableAll, Map.E_Dorado, Map.AE_Eichenwalde);
```
[CustomGame.CG_Maps.ToggleMap()](https://www.abyxa.net/Library/CustomGame/Maps/ToggleMap.html "CustomGame.CG_Maps.ToggleMap()")

<hr>
- [ZombieBot](https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation/tree/master/ZombieBot "ZombieBot")
- [1. Creating a game of tag](https://www.abyxa.net/Library/Tag.html "1. Creating a game of tag")
- [2. Map voting system](https://www.abyxa.net/Library/MapVoting.html "2. Map voting system")