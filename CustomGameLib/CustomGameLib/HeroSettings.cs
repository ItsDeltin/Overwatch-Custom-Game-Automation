using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Toggles what heroes can be selected.
        /// </summary>
        /// <param name="ta">Determines if all heroes should be enabled, disabled or neither before toggling</param>
        /// <param name="team">Team to change roster for.</param>
        /// <param name="heroes">Heroes to toggle.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="team"/> is Spectator or Queue.</exception>
        /// <include file='docs.xml' path='doc/setHeroRoster/example'></include>
        public void SetHeroRoster(ToggleAction ta, Team team, params Hero[] heroes)
        {
            using (LockHandler.Interactive)
            {
                if (team.HasFlag(Team.Spectator) || team.HasFlag(Team.Queue))
                    throw new ArgumentOutOfRangeException("team", team, "Team cannot be Spectator or Queue.");

                GoToSettings();
                LeftClick(Points.SETTINGS_HEROES); // click heroes
                LeftClick(Points.SETTINGS_HEROES_ROSTER); // click hero roster
                                                          // If team doesn't equal both, click a team to change hero roster for.
                if (team == Team.Blue)
                {
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_DROPDOWN, 250);
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_BLUE, 250);
                }
                if (team == Team.Red)
                {
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_DROPDOWN, 250);
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_RED, 250);
                }
                // If the toggle action is disable all, disable all heroes before toggling.
                if (ta == ToggleAction.DisableAll)
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_DISABLE_ALL, 250);
                // If the toggle action is enable all, enable all heroes before toggling.
                else if (ta == ToggleAction.EnableAll)
                    LeftClick(Points.SETTINGS_HEROES_ROSTER_ENABLE_ALL, 250);

                if (OpenChatIsDefault)
                {
                    Chat.CloseChat();
                }

                KeyPress(Keys.Down);
                Thread.Sleep(1);
                if (team == Team.BlueAndRed)
                {
                    KeyPress(Keys.Down);
                    Thread.Sleep(1);
                }

                // For each hero
                for (int i = 0; i < Enum.GetNames(typeof(Hero)).Length; i++)
                {
                    // Toggle hero if current hero selected is set in the heroes array.
                    for (int hi = 0; hi < heroes.Length; hi++)
                        if ((int)heroes[hi] == i)
                        {
                            KeyPress(Keys.Space);
                            Thread.Sleep(1);
                        }
                    KeyPress(Keys.Down);
                    Thread.Sleep(1);
                }

                GoBack(3, 0);

                if (OpenChatIsDefault)
                    Chat.OpenChat();
            }
        }

        private enum SettingType
        {
            value,
            toggle,
            dropdown
        }
        static List<HeroSettingData>[] HeroSettings = HeroSettingData.GetSettings(); // HeroSettings[hero][settingindex]
        private class HeroSettingData
        {
            public string setting;
            public SettingType type;
            private HeroSettingData(string setting, SettingType type)
            {
                this.setting = setting;
                this.type = type;
            }
            public static List<HeroSettingData>[] GetSettings()
            {
                // Read hero_settings resource. Each value in array is a line in hero_settings.txt.
                string[] settings = Properties.Resources.hero_settings.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                // List of settings for each hero.                                                 V +1 due to general settings.
                List<HeroSettingData>[] settinglist = new List<HeroSettingData>[Enum.GetNames(typeof(Hero)).Length + 1];
                for (int i = 0, heroindex = -1; i < settings.Length; i++)
                {
                    if (settings[i].Length >= 1) // Make sure line is not empty
                    {
                        if (settings[i][0] == '-')
                        {
                            heroindex++; // Index of hero to add settings to. 0 = General, 1 = Ana, 2 = Bastion...27 = Zenyatta.
                            settinglist[heroindex] = new List<HeroSettingData>();
                        }
                        if (heroindex != -1 && settings[i][0] != '-')
                        {
                            // Add setting to list
                            string[] settingsData = settings[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            HeroSettingData add = new HeroSettingData(
                                    settingsData[0],
                                    (SettingType)Enum.Parse(typeof(SettingType), settingsData[1])
                                    );
                            settinglist[heroindex].Add(add);
                        }
                    }
                }
                return settinglist;
            }
        }

        private SettingType? GetSettingType(Hero? hero, string setting)
        {
            // Get the setting type for a setting for a hero. Return null if the setting does not exist.
            int heroid = 0;
            if (hero != null)
                heroid = (int)hero + 1;
            for (int i = 0; i < HeroSettings[heroid].Count; i++)
                if (HeroSettings[heroid][i].setting == setting)
                    return HeroSettings[heroid][i].type;
            return null;
        }

        /// <summary>
        /// Change individual hero settings.
        /// </summary>
        /// <param name="herodata">Settings of the heroes you want to change.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a <paramref name="herodata"/>'s set and setto length are not equal length.</exception>
        /// <exception cref="InvalidSetheroException">Thrown if a setting does not exist for their respective heros or if a set's respective setto is not the correct type.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="herodata"/> is null.</exception>
        /// <remarks>
        /// The complete list of settings can be found <a href="https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation/blob/master/CustomGameLib/CustomGameLib/Resources/hero_settings.txt" content="here."/>
        /// Toggle settings require a boolean. Value and dropdown settings require an integer.
        /// </remarks>
        /// <include file='docs.xml' path='doc/setHeroSettings/example'></include>
        /// <seealso cref="SetHero"/>
        public void SetHeroSettings(params SetHero[] herodata)
        {
            int keyPressWait = 50;
            using (LockHandler.Interactive)
            {
                if (herodata == null)
                    throw new ArgumentNullException("herodata", "herodata was null.");

                // Throw exception if any of the set or setto values in herodata are not the same length.
                for (int i = 0; i < herodata.Length; i++)
                    if (herodata[i].Set.Length != herodata[i].SetTo.Length)
                        throw new ArgumentOutOfRangeException("herodata", "The values \"set\" and \"setto\" must be equal length.");
                // Throw exception if any of the settings do not exist for their respective heros or if a set's respective setto is not the correct type.
                foreach (SetHero hero in herodata)
                    for (int i = 0; i < hero.Set.Length; i++)
                    {
                        SettingType? st = GetSettingType(hero.Hero, hero.Set[i]);
                        // If the setting does not exist.
                        if (st == null)
                        {
                            if (hero.Hero != null)
                                throw new InvalidSetheroException(string.Format("The setting \"{0}\" does not exist in {1}'s settings.", hero.Set[i], hero.Hero.ToString()));
                            else
                                throw new InvalidSetheroException(string.Format("The setting \"{0}\" does not exist in the general settings.", hero.Set[i]));
                        }
                        // For setting types that require a boolean.
                        if (st == SettingType.toggle)
                        {
                            if (hero.SetTo[i] is bool == false)
                                throw new InvalidSetheroException(string.Format("The setting \"{0}\" requires a boolean on index '{1}' of setto.", hero.Set[i], i));
                        }
                        // For setting types that require an integer.
                        else if (st == SettingType.value || st == SettingType.dropdown)
                        {
                            if (hero.SetTo[i] is int == false)
                                throw new InvalidSetheroException(string.Format("The setting \"{0}\" requires a integer on index '{1}' of setto.", hero.Set[i], i));
                        }
                    }

                if (OpenChatIsDefault)
                    Chat.CloseChat();
                GoToSettings(); // Open settings (SETTINGS/)
                LeftClick(Points.SETTINGS_HEROES); // click heroes (SETTINGS/HEROES/)
                                                   // For each hero to change settings for
                foreach (SetHero hero in herodata)
                {
                    int heroid = 0;
                    // Click the hero selected's settings.
                    if (hero.Hero == null)
                    {
                        // *General
                        LeftClick(Points.SETTINGS_HEROES_GENERAL); // Open general settings
                    }
                    else
                    {
                        // *Everyone else
                        // Open the hero's settings.
                        heroid = (int)hero.Hero;

                        KeyPress(Keys.Down, Keys.Down);

                        int column = heroid % 4;
                        int row = heroid / 4;

                        for (int rowindex = 0; rowindex < row; rowindex++)
                            KeyPress(keyPressWait, Keys.Down);
                        for (int columnindex = 0; columnindex < column; columnindex++)
                            KeyPress(keyPressWait, Keys.Right);
                        KeyPress(500, Keys.Space);

                        heroid += 1;
                    }
                    // heroid is now 0 = General, 1 = Ana, 2 = Bastion, etc.

                    if (hero.Team != Team.BlueAndRed)
                    {
                        // Click team to change settings for.
                        if (hero.Team == Team.Blue)
                        {
                            LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_DROPDOWN); // click team menu
                            LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_BLUE); // click blue
                        }
                        if (hero.Team == Team.Red)
                        {
                            LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_DROPDOWN); // click team menu
                            LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_RED); // click red
                        }
                        // Make topmost option highlighted
                        KeyPress(Keys.Down);
                        Thread.Sleep(25);
                    }
                    else
                    {
                        // Make topmost option highlighted
                        KeyPress(Keys.Down);
                        Thread.Sleep(keyPressWait);
                        // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\TopOption.png" scale="0.55" />
                        UpdateScreen();
                        // Check if the second option is highlighted.
                        if (Capture.CompareColor(422, 200, Colors.WHITE, 10))
                        {
                            // If it is, press the up arrow.
                            KeyPress(Keys.Up);
                            Thread.Sleep(keyPressWait);
                        }
                    }

                    // Get the last setting to change.
                    int max = 0;
                    for (int si = 0; si < HeroSettings[heroid].Count; si++)
                        if (hero.Set.Contains(HeroSettings[heroid][si].setting))
                            max = si;
                    max += 1;

                    // si stands for setting index.
                    for (int si = 0; si < max; si++)
                    {
                        // Test if current setting selected is a setting that needs to be changed.
                        for (int setSettingIndex = 0; setSettingIndex < hero.Set.Length; setSettingIndex++)
                            if (hero.Set[setSettingIndex] == HeroSettings[heroid][si].setting)
                            {
                                // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\SettingType.png" scale="0.5" />
                                // If the setting selected a toggle setting...
                                if (HeroSettings[heroid][si].type == SettingType.toggle)
                                {
                                    // Check what the toggle setting selected is set to.
                                    bool value = (bool)GetHighlightedSettingValue(true);
                                    bool option = (bool)hero.SetTo[setSettingIndex];

                                    if (value != option)
                                    {
                                        KeyPress(Keys.Space);
                                        Thread.Sleep(keyPressWait);
                                    }
                                }
                                // If the selected setting is a dropdown menu...
                                else if (HeroSettings[heroid][si].type == SettingType.dropdown)
                                {
                                    KeyPress(Keys.Space);
                                    Thread.Sleep(keyPressWait);
                                    KeyPress(Keys.Up); Thread.Sleep(keyPressWait); KeyPress(Keys.Up);
                                    int option = (int)hero.SetTo[setSettingIndex];
                                    /* If the option variable equals 2, only Torbjorn's hammer can be used.
                                     * 0 = ALL
                                     * 1 = RIVET GUN ONLY
                                     * 2 = FORGE HAMMER ONLY */
                                    for (int oi = 0; oi < option; oi++)
                                    {
                                        KeyPress(Keys.Down);
                                        Thread.Sleep(keyPressWait);
                                    }
                                    KeyPress(Keys.Space);
                                    Thread.Sleep(keyPressWait);
                                }
                                // If the selected setting is a numeric value...
                                else // the last possible thing Settings[heroid][si].type could be is SettingType.value
                                {
                                    var keys = GetNumberKeys((int)hero.SetTo[setSettingIndex]); // The numeric value to set the setting to as a string.
                                    for (int sk = 0; sk < keys.Length; sk++)
                                    {
                                        KeyDown(keys[sk]);
                                        Thread.Sleep(keyPressWait);
                                    }
                                    KeyPress(Keys.Return);
                                    Thread.Sleep(keyPressWait);
                                }
                            }

                        // Go to next setting
                        KeyPress(Keys.Down);
                        Thread.Sleep(keyPressWait);
                    }
                    GoBack(1);
                }
                // Go back to custom game menu.
                GoBack(2);
                if (OpenChatIsDefault)
                    Chat.OpenChat();
            }
        }
    }

    /// <summary>
    /// Data to change hero settings.
    /// </summary>
    /// <seealso cref="CustomGame.SetHeroSettings(SetHero[])"/>
    public class SetHero
    {
        /// <summary>
        /// Hero to change settings for. Set to null for general settings.
        /// </summary>
        public Hero? Hero;
        /// <summary>
        /// Team to change hero settings for.
        /// </summary>
        public Team Team;
        /// <summary>
        /// Array of settings to change. Must be the same size as setto.
        /// </summary>
        public string[] Set;
        /// <summary>
        /// Array to change settings to. Must be the same size as set.
        /// </summary>
        public object[] SetTo;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hero">Hero to change settings for. Set to null for general settings.</param>
        /// <param name="team">Team to change hero settings for.</param>
        /// <param name="set">Array of settings to change. Must be the same size as setto.</param>
        /// <param name="setto">Array to change settings to. Must be the same size as set.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="team"/> is Spectator or Queue.</exception>
        public SetHero(Hero? hero, Team team, string[] set, object[] setto)
        {
            if (Team.HasFlag(Team.Spectator) || Team.HasFlag(Team.Queue))
                throw new ArgumentOutOfRangeException("team", team, "Team cannot be Spectator or Queue.");

            Hero = hero;
            Team = team;
            Set = set;
            SetTo = setto;
        }
    }
}
