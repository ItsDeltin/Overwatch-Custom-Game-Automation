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
            // Look for the highlighted option
            for (int y = 110; y < 436; y++)
                if (CompareColor(652, y, new int[] { 127, 127, 127 }, 20)
                    && CompareColor(649, y, Colors.WHITE, 20))
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
        /// Custom Game settings in Overwatch.
        /// </summary>
        public Settings Settings;
    }
    /// <summary>
    /// Custom Game settings in Overwatch.
    /// </summary>
    /// <remarks>
    /// The Settings class is accessed in a CustomGame object on the <see cref="CustomGame.Settings"/> field.
    /// </remarks>
    public class Settings : CustomGameBase
    {
        internal Settings(CustomGame cg) : base(cg) { }

        private int numPresets = -1;

        /// <summary>
        /// Loads a preset saved in Overwatch.
        /// </summary>
        /// <param name="preset">Preset to load. 0 is the first preset</param>
        /// <param name="maxWaitTime">Maximum time to wait for the preset to show up.</param>
        /// <returns>Returns true if selecting the preset was successful.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throw if <paramref name="preset"/> is less than 0.</exception>
        public bool LoadPreset(int preset, int maxWaitTime = 5000)
        {
            lock (cg.CustomGameLock)
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

                        if (cg.CompareColor(presetLocation, Colors.SETTINGS_PRESETS_LOADABLE_PRESET, Fades.SETTINGS_PRESETS_LOADABLE_PRESET))
                        {
                            break;
                        }
                        else if (wait.ElapsedMilliseconds >= maxWaitTime)
                        {
                            cg.GoBack(2);
                            //cg.//ResetMouse();
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

                        if (cg.CompareColor(finalPresetLocation, Colors.SETTINGS_PRESETS_LOADABLE_PRESET, Fades.SETTINGS_PRESETS_LOADABLE_PRESET))
                        {
                            break;
                        }
                        else if (wait.ElapsedMilliseconds >= maxWaitTime)
                        {
                            cg.GoBack(2);
                            //cg.//ResetMouse();
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
                //cg.//ResetMouse();
                return true;
            }
        }

        /// <summary>
        /// Informs library of total number of saved presets. 
        /// May make preset loading faster or more accurate.
        /// </summary>
        /// <param name="num">Number of saved presets the host has.</param>
        public void SetNumPresets(int num)
        {
            lock (cg.CustomGameLock)
            {
                numPresets = num;
            }
        }

        private Point GetPresetLocation(int preset)
        {
            int x = 0;
            int y = 155;

            // Number of presets in a row is 4.
            while (preset >= 4)
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
            lock (cg.CustomGameLock)
            {
                cg.LeftClick(Points.LOBBY_JOIN_DROPDOWN);
                if (setting == Join.Everyone) cg.LeftClick(Points.LOBBY_JOIN_EVERYONE);
                if (setting == Join.FriendsOnly) cg.LeftClick(Points.LOBBY_JOIN_FRIENDS);
                if (setting == Join.InviteOnly) cg.LeftClick(Points.LOBBY_JOIN_INVITE);
                //cg.//ResetMouse();
            }
        }

        /// <summary>
        /// Changes the custom game's name.
        /// </summary>
        /// <param name="name">Name to change game name to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="name"/> is less than 3 characters or greater than 64 characters.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> has the text "admin" in it.</exception>
        public void SetGameName(string name)
        {
            lock (cg.CustomGameLock)
            {
                if (name.Length < 3)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too low, needs to be at least 3.");
                if (name.Length > 64)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too high, needs to be 64 or lower.");
                if (name.ToLower().Contains("admin"))
                    throw new ArgumentException("name can not have the text \"admin\" in it.", "name");
                cg.LeftClick(209, 165); // click on game's name
                cg.TextInput(name);
                cg.KeyPress(Keys.Return);
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Changes a team's name.
        /// </summary>
        /// <param name="team">Team to change name.</param>
        /// <param name="name">Name to change team's name to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="name"/> is less than 1 character or greater than 15 characters. Also thrown if <paramref name="team"/> is Spectator or Queue.</exception>
        /// <exception cref="ArgumentException">Thown if <paramref name="name"/> has the text "admin" in it.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public void SetTeamName(Team team, string name)
        {
            lock (cg.CustomGameLock)
            {
                if (name == null)
                    throw new ArgumentNullException("name", "name cannot be null.");
                if (name.Length < 1)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too low, needs to be at least 1.");
                if (name.Length > 15)
                    throw new ArgumentOutOfRangeException("name", name, "The length of name is too high, needs to be 15 or lower.");
                if (name.ToLower().Contains("admin"))
                    throw new ArgumentException("name can not have the text \"admin\" in it.", "name");
                if (team.HasFlag(Team.Spectator) || team.HasFlag(Team.Queue))
                    throw new ArgumentOutOfRangeException("team", team, "Team cannot be Spectator or Queue.");

                if (team.HasFlag(Team.Blue))
                {
                    cg.LeftClick(Points.LOBBY_BLUE_NAME);

                    cg.TextInput(name);
                    cg.KeyPress(Keys.Return);
                    Thread.Sleep(500);
                }

                if (team.HasFlag(Team.Red))
                {
                    cg.LeftClick(Points.LOBBY_RED_NAME);

                    cg.TextInput(name);
                    cg.KeyPress(Keys.Return);
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Sets the max player count for blue team, red team, free for all, or spectators.
        /// </summary>
        /// <param name="blueCount">Maximum number of blue players. Must be in the range of 1-6. Set to null to ignore.</param>
        /// <param name="redCount">Maximum number of red players. Must be in the range of 1-6. Set to null to ignore.</param>
        /// <param name="ffaCount">Maximum number of FFA players. Must be in the range of 1-12. Set to null to ignore.</param>
        /// <param name="spectatorCount">Maximum number of spectators. Must be in the range of 0-12. Set to null to ignore.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blueCount"/>, <paramref name="redCount"/>, <paramref name="ffaCount"/>, or <paramref name="spectatorCount"/> is less than 0 or greater than their max values.</exception>
        public void SetMaxPlayers(int? blueCount, int? redCount, int? ffaCount, int? spectatorCount)
        {
            lock (cg.CustomGameLock)
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
        }

        /// <summary>
        /// Changes settings in the Custom Game.
        /// </summary>
        /// <param name="settings">Settings to change.</param>
        public void SetSettings(GameSettings settings)
        {
            lock (cg.CustomGameLock)
            {
                settings.SetSettings(cg);
            }
        }
    }

#pragma warning disable CS1591
    public abstract class GameSettings
    {
        private GameSettings() { }

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

        public GameSettings SetSettingByName(string name, object value)
        {
            FieldInfo[] fis = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();

            bool found = false;

            for (int i = 0; i < fis.Length; i++)
                if (fis[i].Name.ToLower() == name.ToLower())
                {
                    fis[i].SetValue(this, value);

                    found = true;
                    break;
                }

            if (!found)
                throw new ArgumentException("Could not find a setting by the name of " + name + " in the " + this.GetType().Name + " settings.");

            return this;
        }

        internal abstract void Navigate(CustomGame cg);
        internal abstract void Return(CustomGame cg);

        public class Settings_Modes_All : GameSettings
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

            //public Setting<bool> EnemyHealth = new Setting<bool>("EnemyHealth", true);

            // Settings
            public bool? EnemyHealthBars;
            public Game_Mode_Start? GameModeStart;
            public int? HealthPackRespawnTimeScalar;
            public bool? KillCam;
            public bool? KillFeed;
            public bool? Skins;
            public Spawn_Health_Packs? SpawnHealthPacks;

            public bool? AllowHeroSwitching;
            public Hero_Limit? HeroLimit;
            public Limit_Roles? LimitRoles;
            public bool? RespawnAsRandomHero;
            public int? RespawnTimeScalar;
            // /Settings

            public enum Game_Mode_Start { All_Slots_Filled, Immediately, Manual }
            public enum Spawn_Health_Packs { Determined_By_Mode, Enabled, Disabled }
            public enum Hero_Limit { Off, One_Per_Team, Two_Per_Team, One_Per_Game, Two_Per_Game }
            public enum Limit_Roles { Off, Two_Of_Each_Role_Per_Team }

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                cg.LeftClick(98, 177);
                cg.KeyPress(Keys.Down, Keys.Up, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3);
            }
        }

        public class Settings_Modes_Assault : GameSettings
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

            public bool? Enabled;
            public int? CaptureSpeedModifier;
            public bool? CompetitiveRules;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Assault, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);

                cg.KeyPress(Keys.Up, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_AssaultEscort : GameSettings
        {
            public Settings_Modes_AssaultEscort(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = true;
                    CaptureSpeedModifier = 100;
                    CompetitiveRules = false;
                    PayloadSpeedModifier = 100;
                }
            }

            public bool? Enabled;
            public int? CaptureSpeedModifier;
            public bool? CompetitiveRules;
            public int? PayloadSpeedModifier;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.AssaultEscort, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Control : GameSettings
        {
            public Settings_Modes_Control(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = true;
                    CaptureSpeedModifier = 100;
                    CompetitiveRules = false;
                    LimitValidControlPoints = Limit_Valid_Control_Points.All;
                    ScoreToWin = 2;
                    ScoringSpeedModifier = 100;
                }
            }

            public bool? Enabled;
            public int? CaptureSpeedModifier;
            public bool? CompetitiveRules;
            public Limit_Valid_Control_Points? LimitValidControlPoints;
            public int? ScoreToWin;
            public int? ScoringSpeedModifier;

            public enum Limit_Valid_Control_Points
            {
                All,
                First,
                Second,
                Third
            }

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Control, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Escort : GameSettings
        {
            public Settings_Modes_Escort(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = true;
                    CompetitiveRules = false;
                    PayloadSpeedModifier = 100;
                }
            }

            public bool? Enabled;
            public bool? CompetitiveRules;
            public int? PayloadSpeedModifier;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Escort, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Left);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Deathmatch : GameSettings
        {
            public Settings_Modes_Deathmatch(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = false;
                    GameLengthInMinutes = 10;
                    ScoreToWin = 20;
                    SelfInitiatedRespawn = true;
                }
            }

            public bool? Enabled;
            public int? GameLengthInMinutes;
            public int? ScoreToWin;
            public bool? SelfInitiatedRespawn;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Deathmatch, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Elimination : GameSettings
        {
            public Settings_Modes_Elimination(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = false;
                    HeroSelectionTime = 20;
                    ScoreToWin = 3;
                    RestrictPreviouslyUsedHeroes = Restrict_Previously_Used_Heroes.Off;
                    HeroSelection = Hero_Selection.Any;
                    LimitedChoicePool = Limited_Choice_Pool.TeamSizePlus2;
                    CaptureObjectiveTiebreaker = true;
                    TiebreakerAfterMatchTimeElapsed = 105;
                    TimeToCapture = 3;
                    DrawAfterMatchTimeElapsedWithNoTiebreaker = 135;
                    RevealHeroes = false;
                    RevealHeroesAfterMatchTimeElapsed = 75;
                }
            }

            public bool? Enabled;
            public int? HeroSelectionTime;
            public int? ScoreToWin;
            public Restrict_Previously_Used_Heroes? RestrictPreviouslyUsedHeroes;
            public Hero_Selection? HeroSelection;
            public Limited_Choice_Pool? LimitedChoicePool;
            public bool? CaptureObjectiveTiebreaker;
            public int? TiebreakerAfterMatchTimeElapsed;
            public int? TimeToCapture;
            public int? DrawAfterMatchTimeElapsedWithNoTiebreaker;
            public bool? RevealHeroes;
            public int? RevealHeroesAfterMatchTimeElapsed;

            public enum Restrict_Previously_Used_Heroes { Off, AfterRoundWon, AfterRoundPlayed }
            public enum Hero_Selection { Any, Limited, Random, RandomMirrored }
            public enum Limited_Choice_Pool { TeamSize, TeamSizePlus1, TeamSizePlus2, TeamSizePlus3 }

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Elimination, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Lucioball : GameSettings
        {
            public Settings_Modes_Lucioball(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = false;
                    GameLength = 240;
                    SoccerBallKnockbackScalar = 100;
                    TeamWinsUponScoringEnoughGoals = false;
                    GoalsNeededToWin = 3;
                    ResetPlayersAfterGoalScored = true;
                }
            }

            public bool? Enabled;
            public int? GameLength;
            public int? SoccerBallKnockbackScalar;
            public bool? TeamWinsUponScoringEnoughGoals;
            public int? GoalsNeededToWin;
            public bool? ResetPlayersAfterGoalScored;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Lucioball, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_TeamDeathmatch : GameSettings
        {
            public Settings_Modes_TeamDeathmatch(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = false;
                    GameLengthInMinutes = 10;
                    MercyResurrectCounteractsKills = true;
                    ScoreToWin = 30;
                    SelfInitiatedRespawn = true;
                    ImbalancedTeamScoreToWin = false;
                    BlueScoreToWin = 30;
                    RedScoreToWin = 30;
                }
            }

            public bool? Enabled;
            public int? GameLengthInMinutes;
            public bool? MercyResurrectCounteractsKills;
            public int? ScoreToWin;
            public bool? SelfInitiatedRespawn;
            public bool? ImbalancedTeamScoreToWin;
            public int? BlueScoreToWin;
            public int? RedScoreToWin;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.TeamDeathmatch, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Down, Keys.Up);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Modes_Skirmish : GameSettings
        {
            public Settings_Modes_Skirmish(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    Enabled = true;
                }
            }

            public bool? Enabled;

            internal override void Navigate(CustomGame cg)
            {
                cg.NavigateToModesMenu();
                Point point = cg.GetModeLocation(Gamemode.Skirmish, cg.CurrentOverwatchEvent);
                cg.LeftClick(point.X, point.Y, 250);
                cg.KeyPress(Keys.Left);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(3, 0);
            }
        }

        public class Settings_Lobby : GameSettings
        {
            public Settings_Lobby(bool initializeWithDefaults = false)
            {
                if (initializeWithDefaults)
                {
                    MapRotation = Map_Rotation.AfterAMirrorMatch;
                    ReturnToLobby = Return_To_Lobby.AfterAMirrorMatch;
                    TeamBalancing = Team_Balancing.Off;
                    SwapTeamsAfterMatch = true;
                    BlueMaxPlayers = 6;
                    RedMaxPlayers = 6;
                    MaxFFAPlayers = 0;
                    MaxSpectators = 2;
                    MatchVoiceChat = false;
                    PauseGameOnPlayerDisconnect = false;
                }
            }

            public Map_Rotation? MapRotation;
            public Return_To_Lobby? ReturnToLobby;
            public Team_Balancing? TeamBalancing;
            public bool? SwapTeamsAfterMatch;
            public int? BlueMaxPlayers;
            public int? RedMaxPlayers;
            public int? MaxFFAPlayers;
            public int? MaxSpectators;
            public bool? MatchVoiceChat;
            public bool? PauseGameOnPlayerDisconnect;

            public enum Map_Rotation
            {
                AfterAMirrorMatch,
                AfterAGame,
                Paused,
            }

            public enum Return_To_Lobby
            {
                Never,
                AfterAGame,
                AfterAMirrorMatch
            }

            public enum Team_Balancing
            {
                Off,
                AfterAGame,
                AfterAMirrorMatch
            }

            internal override void Navigate(CustomGame cg)
            {
                cg.GoToSettings();
                cg.LeftClick(373, 182, 100);
                cg.KeyPress(Keys.Down);
                Thread.Sleep(100);
            }

            internal override void Return(CustomGame cg)
            {
                cg.GoBack(2);
            }
        }
    }
#pragma warning restore CS1591
}
