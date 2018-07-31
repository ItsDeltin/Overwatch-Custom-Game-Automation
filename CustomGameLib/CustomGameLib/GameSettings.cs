using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        internal bool? GetHighlightedSettingValue(bool waitForScrollAnimation)
        {
            if (waitForScrollAnimation)
                Thread.Sleep(150);

            int min = 35;
            int max = 155;

            updateScreen();
            for (int y = 110; y < 436; y++)
                if (CompareColor(652, y, new int[] { 127, 127, 127 }, 20)
                    && CompareColor(649, y, CALData.WhiteColor, 20))
                {
                    int checkY = y + 2;

                    bool? settingValue = null;

                    // If the setting is set to DISABLED
                    if (CompareColor(564, checkY, new int[] { min, min, min }, new int[] { max, max, max }))
                        settingValue = false;

                    // If the setting is set to ENABLED
                    else if (CompareColor(599, checkY, new int[] { min, min, min }, new int[] { max, max, max }))
                        settingValue = true;

                    // If the setting is set to OFF
                    else if (CompareColor(589, checkY, new int[] { min, min, min }, new int[] { max, max, max }))
                        settingValue = false;

                    // If the setting is set to ON
                    else if (CompareColor(588, checkY, new int[] { min, min, min }, new int[] { max, max, max }))
                        settingValue = true;

                    if (settingValue != null)
                    {
                        return settingValue;
                    }
                }

            return null;
        }

        /// <summary>
        /// Setting game settings in Overwatch.
        /// </summary>
        public CG_Settings GameSettings;
        /// <summary>
        /// Setting game settings in Overwatch.
        /// </summary>
        public class CG_Settings
        {
            private CustomGame cg;

            private int numPresets = -1;

            internal CG_Settings(CustomGame cg)
            { this.cg = cg; }
            

            /// <summary>
            /// Loads a preset saved in Overwatch, 0 being the first saved preset.
            /// </summary>
            /// <param name="preset">Preset to load.</param>
            /// <param name="maxWaitTime">Maximum time to wait for the preset to show up.</param>
            // Loads a preset in Overwatch Custom Games
            public bool LoadPreset(int preset, int maxWaitTime = 5000)
            {
                if (preset < 0)
                    throw new ArgumentOutOfRangeException("preset", preset, "Argument preset must be equal or greater than 0.");

                Point presetLocation = GetPresetLocation(preset);

                cg.GoToSettings();
                cg.LeftClick(Points.SETTINGS_PRESETS, 2000); // Clicks "Preset" button

                Stopwatch wait = new Stopwatch();
                wait.Start();

                if (numPresets == -1)
                {
                    while (true)
                    {
                        cg.updateScreen();

                        if (cg.CompareColor(presetLocation, new int[] { 126, 128, 134 }, 40))
                        {
                            break;
                        }
                        else if (wait.ElapsedMilliseconds >= maxWaitTime)
                        {
                            cg.GoBack(2);
                            cg.ResetMouse();
                            return false;
                        }

                        Thread.Sleep(100);
                    }
                }
                else
                {
                    Point finalPresetLocation = GetPresetLocation(numPresets);
                    while (true)
                    {
                        cg.updateScreen();

                        if (cg.CompareColor(finalPresetLocation, CALData.LoadablePreset, 40))
                        {
                            break;
                        }
                        else if (wait.ElapsedMilliseconds >= maxWaitTime)
                        {
                            cg.GoBack(2);
                            cg.ResetMouse();
                            return false;
                        }

                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(250);

                cg.LeftClick(presetLocation);
                cg.LeftClick(Points.PRESETS_CONFIRM);

                // Go back to lobby
                cg.GoBack(2);
                cg.ResetMouse();
                return true;
            }


            /// <summary>
            /// Informs library of total number of saved presets. 
            /// May make preset loading faster or more accurate.
            /// </summary>
            /// <param name="num">Number of saved presets the host has.</param>
            public void SetNumPresets(int num)
            {
                numPresets = num;
            }


            private Point GetPresetLocation(int preset)
            {
                int x = 0;
                int y = 155;

                // Number of presets in a row is 4.
                while (preset > 3)
                {
                    preset = preset - 4;
                    y += 33; // Increment row by 1. Space between rows is 33 pixels.
                }
                if (preset == 0) x = 146; // Column 1
                else if (preset == 1) x = 294; // Column 2
                else if (preset == 2) x = 440; // Column 3
                else if (preset == 3) x = 590; // Column 4

                return new Point(x, y);
            }

            /// <summary>
            /// Changes who can join.
            /// </summary>
            /// <param name="setting">Join setting to select.</param>
            public void SetJoinSetting(Join setting)
            {
                cg.LeftClick(Points.LOBBY_JOIN_DROPDOWN);
                if (setting == Join.Everyone) cg.LeftClick(Points.LOBBY_JOIN_EVERYONE);
                if (setting == Join.FriendsOnly) cg.LeftClick(Points.LOBBY_JOIN_FRIENDS);
                if (setting == Join.InviteOnly) cg.LeftClick(Points.LOBBY_JOIN_INVITE);
                cg.ResetMouse();
            }

            /// <summary>
            /// Changes the custom game's name.
            /// </summary>
            /// <param name="name">Name to change game name to.</param>
            // Changes the game's name
            public void SetGameName(string name)
            {
                if (name.Length < 3)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too low, needs to be at least 3.");
                if (name.Length > 64)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too high, needs to be 64 or lower.");
                cg.LeftClick(Points.LOBBY_GAME_NAME); // click on game's name
                cg.TextInput(name);
                cg.KeyPress(Keys.Return);
                Thread.Sleep(500);
            }

            /// <summary>
            /// Changes a team's name.
            /// </summary>
            /// <param name="team">Team to change name.</param>
            /// <param name="name">Name to change to.</param>
            /// <returns></returns>
            // changes Team1/Team2's name
            public void SetTeamName(PlayerTeam team, string name)
            {
                if (name.Length < 1)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too low, needs to be at least 1.");
                if (name.Length > 15)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too high, needs to be 15 or lower.");
                if (team == PlayerTeam.Blue) cg.LeftClick(Points.LOBBY_BLUE_NAME);
                if (team == PlayerTeam.Red) cg.LeftClick(Points.LOBBY_RED_NAME);
                cg.TextInput(name);
                cg.KeyPress(Keys.Return);
                Thread.Sleep(500);
            }

            /// <summary>
            /// Sets the max player count for blue team, red team, free for all, or spectators.
            /// </summary>
            /// <param name="blueCount">Maximum number of blue players. Must be in the range of 1-6. Set to null to ignore.</param>
            /// <param name="redCount">Maximum number of red players. Must be in the range of 1-6. Set to null to ignore.</param>
            /// <param name="ffaCount">Maximum number of FFA players. Must be in the range of 1-12. Set to null to ignore.</param>
            /// <param name="spectatorCount">Maximum number of spectators. Must be in the range of 0-12. Set to null to ignore.</param>
            public void SetMaxPlayers(int? blueCount, int? redCount, int? ffaCount, int? spectatorCount)
            {
                cg.GoToSettings();
                cg.LeftClick(Points.SETTINGS_LOBBY, 100); // Click "lobby" option

                if (blueCount < 0 || blueCount > 6)
                    throw new ArgumentOutOfRangeException("blueCount", blueCount, "blueCount is out of range. Value must be greater or equal to 1 and less than or equal to 6.");

                if (redCount < 0 || redCount > 6)
                    throw new ArgumentOutOfRangeException("redCount", redCount, "redCount is out of range. Value must be greater or equal to 1 and less than or equal to 6.");

                if (ffaCount < 0 || ffaCount > 12)
                    throw new ArgumentOutOfRangeException("ffaCount", ffaCount, "ffaCount is out of range. Value must be greater or equal to 1 and less than or equal to 12.");

                if (spectatorCount < 0 || spectatorCount > 12)
                    throw new ArgumentOutOfRangeException("spectatorCount", spectatorCount, "spectatorCount is out of range. Value must be greater or equal to 0 and less than or equal to 12.");

                if (blueCount != null)
                {
                    cg.LeftClick(Points.SETTINGS_LOBBY_BLUE_MAX_PLAYERS, 100);
                    cg.TextInput(blueCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (redCount != null)
                {
                    cg.LeftClick(Points.SETTINGS_LOBBY_RED_MAX_PLAYERS, 100);
                    cg.TextInput(redCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (ffaCount != null)
                {
                    cg.LeftClick(Points.SETTINGS_LOBBY_FFA_MAX_PLAYERS, 100);
                    cg.TextInput(ffaCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (spectatorCount != null)
                {
                    cg.LeftClick(Points.SETTINGS_LOBBY_MAX_SPECTATORS, 100);
                    cg.TextInput(spectatorCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                cg.GoBack(3);
                Thread.Sleep(150);
            }

            /*
            public void SetSettings(Settings settings)
            {
                settings.SetSettings(cg);
            }
            */
        }
    }

    public enum LobbyTeam
    {
        Blue,
        Red,
        FFA,
        Spectator
    }

}
