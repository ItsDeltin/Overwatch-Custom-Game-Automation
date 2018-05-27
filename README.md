# Overwatch Custom Game Automation
A library for automating Overwatch custom games.
[Click here for tutorial and documentation](https://www.abyxa.net/Library/Library.html)

## Getting Started

### Prerequisites
- [Overwatch](http://playoverwatch.com/en-us/) 
- .Net Framework 4.7.1 or later.

### Usage

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
CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethods.BitBlt);
// or
CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethods.ScreenCopy);
```
- BitBlt does not require to be on top. You can have other windows over the Overwatch window and it will still work.
- ScreenCopy needs to be on top and have no windows overlapping the Overwatch window. Does not work with all border styles right now.

The Overwatch window the CustomGame class is using must have default colorblind, brightness, and gamma settings with contrast at the minimum. Some methods may not work unless you are the moderator of the custom game and/or you are in spectator.

It is possible to run your application and play Overwatch at the same time by starting 2 Overwatch processes directly from the Overwatch.exe file, which is by default at C:/Program Files (x86)/Overwatch/Overwatch.exe. Create the CustomGame object with the main window handle of the Overwatch process you want the CustomGame class to use. If you are using ScreenCopy, which requires the Overwatch window the CustomGame class is using to be on top, I recommend using a virtual machine.

### Slots
You can target a specific player on the server with their slot.

![](https://raw.githubusercontent.com/ItsDeltin/Overwatch-Custom-Game-Automation/master/Tutorial/Library/Assets/slots.jpg "slots")

`CustomGame.BlueSlots`, for the example above, will return a List<int> containing only the number 0.

`CustomGame.CG_Interact.Move(0, 8)` will move the player Deltin from slot 0 on the blue team to slot 8 on the red team.

`CustomGame.CG_PlayerInfo.IsUltimateReady(5)` will check if the player in slot 5's ultimate ability is ready.

### Examples

- [ZombieBot](https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation/tree/master/ZombieBot "ZombieBot")

- [1. Creating a game of tag](https://www.abyxa.net/Library/Tag.html "1. Creating a game of tag")

- [2. Map voting system](https://www.abyxa.net/Library/MapVoting.html "2. Map voting system")