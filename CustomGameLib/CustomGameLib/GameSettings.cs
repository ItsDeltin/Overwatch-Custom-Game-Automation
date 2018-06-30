using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        public bool? GetHighlightedSettingValue(bool waitForScrollAnimation)
        {
            if (waitForScrollAnimation)
                Thread.Sleep(150);

            updateScreen();
            for (int y = 110; y < 436; y++)
                if (CompareColor(652, y, new int[] { 127, 127, 127 }, 20)
                    && CompareColor(649, y, CALData.WhiteColor, 20))
                {
                    for (int checkY = y - 5; checkY < y + 5; checkY++)
                    {
                        bool? settingValue = null;

                        // If the setting is set to DISABLED
                        if (CompareColor(564, checkY, new int[] { 81, 81, 81 }, 40))
                            settingValue = false;

                        // If the setting is set to ENABLED
                        else if (CompareColor(599, checkY, new int[] { 127, 127, 127 }, 40))
                            settingValue = true;

                        // If the setting is set to OFF
                        else if (CompareColor(589, checkY, new int[] { 127, 127, 127 }, 40))
                            settingValue = false;

                        // If the setting is set to ON
                        else if (CompareColor(588, checkY, new int[] { 127, 127, 127 }, 40))
                            settingValue = true;

                        if (settingValue != null)
                        {
                            return settingValue;
                        }
                    }
                }

            return null;
        }

        public CG_Settings GameSettings;
        public class CG_Settings
        {
            private CustomGame cg;
            internal CG_Settings(CustomGame cg)
            { this.cg = cg; }

            /// <summary>
            /// Loads a preset saved in Overwatch, 0 being the first saved preset.
            /// </summary>
            /// <param name="preset">Preset to load.</param>
            // Loads a preset in Overwatch Custom Games
            public void LoadPreset(int preset)
            {
                if (preset < 0)
                    throw new ArgumentOutOfRangeException("preset", preset, "Argument preset must be greater than 0.");

                int x = 0;
                int y = 155;

                // Number of presets in a row is 4.
                while (preset > 3)
                {
                    preset = preset - 4;
                    y = y + 33; // Increment row by 1. Space between rows is 33 pixels.
                }
                if (preset == 0) x = 146; // Column 1
                else if (preset == 1) x = 294; // Column 2
                else if (preset == 2) x = 440; // Column 3
                else if (preset == 3) x = 590; // Column 4

                cg.GoToSettings();
                cg.LeftClick(103, 183, 2000); // Clicks "Preset" button

                cg.updateScreen();
                while (cg.CompareColor(91, 174, new int[] { 188, 143, 77 }, 10)) { cg.updateScreen(); Thread.Sleep(100); }
                cg.LeftClick(x, y); // Clicks the preset
                cg.LeftClick(480, 327); // Clicks confirm

                // Go back to lobby
                cg.GoBack(2);
            }

            /// <summary>
            /// Changes who can join.
            /// </summary>
            /// <param name="setting">Join setting to select.</param>
            public void SetJoinSetting(Join setting)
            {
                cg.LeftClick(280, 198);
                if (setting == Join.Everyone) cg.LeftClick(280, 220); // Click "Everyone"
                if (setting == Join.FriendsOnly) cg.LeftClick(280, 240); // Click "Friends Only"
                if (setting == Join.InviteOnly) cg.LeftClick(280, 260); // Click "Invite Only"
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
                cg.LeftClick(209, 165); // click on game's name
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
                if (team == PlayerTeam.Blue) cg.LeftClick(159, 229); // Clicks team1's name
                if (team == PlayerTeam.Red) cg.LeftClick(458, 230); // Clicks team2's name
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
                cg.LeftClick(297, 183, 100); // Click "lobby" option

                if (blueCount < 1 || blueCount > 6)
                    throw new ArgumentOutOfRangeException("blueCount", blueCount, "blueCount is out of range. Value must be greater or equal to 1 and less than or equal to 6.");

                if (redCount < 1 || redCount > 6)
                    throw new ArgumentOutOfRangeException("redCount", redCount, "redCount is out of range. Value must be greater or equal to 1 and less than or equal to 6.");

                if (ffaCount < 1 || ffaCount > 12)
                    throw new ArgumentOutOfRangeException("ffaCount", ffaCount, "ffaCount is out of range. Value must be greater or equal to 1 and less than or equal to 12.");

                if (spectatorCount < 0 || spectatorCount > 12)
                    throw new ArgumentOutOfRangeException("spectatorCount", spectatorCount, "spectatorCount is out of range. Value must be greater or equal to 0 and less than or equal to 12.");

                if (blueCount != null)
                {
                    cg.LeftClick(500, 269, 100);
                    cg.TextInput(blueCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (redCount != null)
                {
                    cg.LeftClick(500, 290, 100);
                    cg.TextInput(redCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (ffaCount != null)
                {
                    cg.LeftClick(500, 311, 100);
                    cg.TextInput(ffaCount.ToString());
                    cg.KeyPress(Keys.Enter);
                }

                if (spectatorCount != null)
                {
                    cg.LeftClick(500, 333, 100);
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

    /*

    public class Settings
    {
        private Settings() { }

        internal void SetSettings(CustomGame cg)
        {
            Navigate(cg);

            object[] values = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Select(v => v.GetValue(this)).ToArray();

            int waitTime = 10;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    if (values[i] is bool)
                    {
                        bool option = (bool)values[i];
                        bool value = (bool)cg.GetHighlightedSettingValue(true);
                        if (option != value)
                        {
                            cg.KeyPress(Keys.Space);
                            Thread.Sleep(waitTime);
                        }
                    }
                    else if (values[i] is int)
                    {
                        var set = cg.GetNumberKeys((int)values[i]);

                        for (int k = 0; k < set.Length; k++)
                        {
                            cg.KeyDown(set[k]);
                            Thread.Sleep(waitTime);
                        }
                        cg.KeyDown(Keys.Enter);
                        Thread.Sleep(waitTime);
                    }
                    else if (values[i] is Enum)
                    {
                        int set = (int)values[i];
                        int length = Enum.GetNames(values[i].GetType()).Length;

                        cg.KeyPress(Keys.Space);
                        Thread.Sleep(waitTime);
                        for (int a = 0; a < length; a++)
                        {
                            cg.KeyPress(Keys.Up);
                            Thread.Sleep(waitTime);
                        }
                        for (int a = 0; a < set; a++)
                        {
                            cg.KeyPress(Keys.Down);
                            Thread.Sleep(waitTime);
                        }
                        cg.KeyPress(Keys.Space);
                        Thread.Sleep(waitTime);
                    }
                }

                cg.KeyPress(Keys.Down);
                Thread.Sleep(waitTime);
            }

            Return(cg);
        }

        internal virtual void Navigate(CustomGame cg) { throw new NotImplementedException(); }
        internal virtual void Return(CustomGame cg) { throw new NotImplementedException(); }

        public class Settings_Modes_All : Settings
        {
            public Settings_Modes_All(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    EnemyHealthBars = true;
                    GameModeStart = Game_Mode_Start.All_Slots_Filled;
                    HealthPackRespawnTimeScalar = 100;
                    KillCam = true;
                    KillFeed = true;
                    Skins = true;
                    SpawnHealthPacks = Spawn_Health_Packs.Determined_By_Mode;

                    AllowHeroSwitching = true;
                    HeroLimit = Hero_Limit.One_Per_Team;
                    LimitRoles = Limit_Roles.Off;
                    RespawnAsRandomHero = false;
                    RespawnTimeScalar = 100;
                };
            }

            // Settings
            public bool? EnemyHealthBars = null;
            public Game_Mode_Start? GameModeStart = null;
            public int? HealthPackRespawnTimeScalar = null;
            public bool? KillCam = null;
            public bool? KillFeed = null;
            public bool? Skins = null;
            public Spawn_Health_Packs? SpawnHealthPacks = null;

            public bool? AllowHeroSwitching = null;
            public Hero_Limit? HeroLimit = null;
            public Limit_Roles? LimitRoles = null;
            public bool? RespawnAsRandomHero = null;
            public int? RespawnTimeScalar = null;
            // /Settings

            public enum Game_Mode_Start { All_Slots_Filled, Immediately, Manual }
            public enum Spawn_Health_Packs { Determined_By_Mode, Enabled, Disabled }
            public enum Hero_Limit { Off, One_Per_Team, Two_Per_Team, One_Per_Game, Two_Per_Game }
            public enum Limit_Roles { Off, Two_Of_Each_Role_Per_Team }

            internal override void Navigate(CustomGame cg)
            {
                cg.GoToSettings();
                cg.LeftClick(494, 178);
                cg.LeftClick(98, 177);
                cg.KeyPress(Keys.Down, Keys.Up, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3);
            }
        }

        public class Settings_Modes_Assault : Settings
        {
            public Settings_Modes_Assault(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = true;
                    CaptureSpeedModifier = 100;
                    CompetitiveRules = false;
                }
            }

            public bool? Enabled = null;
            public int? CaptureSpeedModifier = null;
            public bool? CompetitiveRules = null;

            internal override void Navigate(CustomGame cg)
            {
                cg.GoToSettings();
                cg.LeftClick(494, 178);
                cg.LeftClick(245, 175);
                cg.KeyPress(Keys.Up, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }
    }

    */
}
