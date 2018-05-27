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
        public void SetHeroRoster(ToggleAction ta, BotTeam team, params Hero[] heroes)
        {
            GoToSettings();
            LeftClick(351, 311); // click heroes
            LeftClick(287, 158); // click hero roster
            // If team doesn't equal both, click a team to change hero roster for.
            if (team == BotTeam.Blue)
            {
                LeftClick(492, 127, 250);
                LeftClick(484, 173, 250);
            }
            if (team == BotTeam.Red)
            {
                LeftClick(492, 127, 250);
                LeftClick(484, 193, 250);
            }
            // If the toggle action is disable all, disable all heroes before toggling.
            if (ta == ToggleAction.DisableAll)
                LeftClick(635, 130, 250);
            // If the toggle action is enable all, enable all heroes before toggling.
            else if (ta == ToggleAction.EnableAll)
                LeftClick(597, 130, 250);

            if (OpenChatIsDefault)
            {
                Chat.CloseChat();
            }

            KeyPress(Keys.Down);
            Thread.Sleep(1);
            if (team == BotTeam.Both)
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

        private enum SettingType
        {
            value,
            toggle,
            dropdown
        }
        static Setting[][] Settings = Setting.GetSettings();
        private class Setting
        {
            public string setting;
            public SettingType type;
            private Setting(string setting, SettingType type)
            {
                this.setting = setting;
                this.type = type;
            }
            public static Setting[][] GetSettings()
            {
                // Read hero_settings resource. Each value in array is a line in hero_settings.txt.
                string[] settings = Properties.Resources.hero_settings.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                // List of settings for each hero.                                                 V +1 due to general settings.
                List<Setting>[] settinglist = new List<Setting>[Enum.GetNames(typeof(Hero)).Length + 1];
                for (int i = 0, heroindex = -1; i < settings.Length; i++)
                {
                    if (settings[i].Length >= 1) // Make sure line is not empty
                    {
                        if (settings[i][0] == '-')
                        {
                            heroindex++; // Index of hero to add settings to. 0 = General, 1 = Ana, 2 = Bastion...27 = Zenyatta.
                            settinglist[heroindex] = new List<Setting>();
                        }
                        if (heroindex != -1 && settings[i][0] != '-')
                        {
                            // Add setting to list
                            string[] settingsData = settings[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            Setting add = new Setting(
                                    settingsData[0],
                                    (SettingType)Enum.Parse(typeof(SettingType), settingsData[1])
                                    );
                            settinglist[heroindex].Add(add);
                        }
                    }
                }
                // Convert from List<Setting>[] to Setting[][]
                List<Setting[]> ret = new List<Setting[]>();
                for (int i = 0; i < settinglist.Length; i++)
                {
                    ret.Add(settinglist[i].ToArray());
                }
                return ret.ToArray();
            }
        }

        private SettingType? GetSettingType(Hero? hero, string setting)
        {
            // Get the setting type for a setting for a hero. Return null if the setting does not exist.
            int heroid = 0;
            if (hero != null)
                heroid = (int)hero + 1;
            for (int i = 0; i < Settings[heroid].Length; i++)
                if (Settings[heroid][i].setting == setting)
                    return Settings[heroid][i].type;
            return null;
        }

        /// <summary>
        /// Change individual hero settings.
        /// </summary>
        /// <param name="herodata">Settings of the heroes you want to change.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a herodata's set and setto length are not equal length.</exception>
        /// <exception cref="InvalidSetheroException">Thrown when any of the settings do not exist for their respective heros or if a set's respective setto is not the correct type.</exception>
        public void SetHeroSettings(params SetHero[] herodata)
        {
            // Throw exception if any of the set or setto values in herodata are not the same length.
            for (int i = 0; i < herodata.Length; i++)
                if (herodata[i].Set.Length != herodata[i].SetTo.Length)
                    throw new ArgumentOutOfRangeException("herodata", "The values \"set\" and \"setto\" must be equal length.");
            // Throw exception if any of the settings do not exist for their respective heros or if a set's respective setto is not the correct type.
            foreach(SetHero hero in herodata)
                for(int i = 0; i < hero.Set.Length; i++)
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
            LeftClick(351, 311); // click heroes (SETTINGS/HEROES/)
            // For each hero to change settings for
            foreach (SetHero hero in herodata)
            {
                int heroid = 0;
                // Click the hero selected's settings.
                if (hero.Hero == null)
                {
                    // *General
                    LeftClick(80, 146); // Open general settings
                }
                else
                {
                    // *Everyone else
                    // Open the hero in hero.Hero's settings.
                    heroid = (int)hero.Hero; // Get int value of hero from the enum.
                    int select = heroid;
                    int y = 208; // Y coordinate of first hero row (Ana, Bastion, Dva, Doomfist). Increments by 33 for each row.
                    while (true)
                    {
                        if (select > 3)
                        {
                            y += 33; // add y by length of hero selection row
                            select -= 4;
                        }
                        else
                        {
                            // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\Column.png" scale="0.7" />
                            // Select the first column
                            if (select == 0)
                                LeftClick(80, y);
                            // Select the second column
                            else if (select == 1)
                                LeftClick(224, y);
                            // Select the third column
                            else if (select == 2)
                                LeftClick(368, y);
                            // Select the fourth column
                            else if (select == 3)
                                LeftClick(511, y);
                            break;
                        }
                    }
                    heroid++;
                }
                // heroid is now 0 = general, 1 = ana, 2 = bastion and so on.

                if (hero.Team != BotTeam.Both)
                {
                    // Click team to change settings for.
                    if (hero.Team == BotTeam.Blue)
                    {
                        LeftClick(572, 126); // click team menu
                        LeftClick(572, 173); // click blue
                    }
                    if (hero.Team == BotTeam.Red)
                    {
                        LeftClick(572, 126); // click team menu
                        LeftClick(572, 192); // click red
                    }
                    // Make topmost option highlighted
                    KeyPress(Keys.Down);
                    Thread.Sleep(25);
                }
                else
                {
                    // Make topmost option highlighted
                    KeyPress(Keys.Down);
                    Thread.Sleep(KeyPressWait);
                    // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\TopOption.png" scale="0.55" />
                    updateScreen();
                    // Check if the second option is highlighted.
                    if (CompareColor(422, 200, CALData.WhiteColor, 10))
                    {
                        // If it is, press the up arrow.
                        KeyPress(Keys.Up);
                        Thread.Sleep(KeyPressWait);
                    }
                }

                // Get the last setting to change.
                int max = 0;
                for (int si = 0; si < Settings[heroid].Length; si++)
                    if (hero.Set.Contains(Settings[heroid][si].setting))
                        max = si;
                max += 1;

                // si stands for setting index.
                for (int si = 0; si < max; si++)
                {
                    // Test if current setting selected is a setting that needs to be changed.
                    for (int setSettingIndex = 0; setSettingIndex < hero.Set.Length; setSettingIndex++)
                        if (hero.Set[setSettingIndex] == Settings[heroid][si].setting)
                        {
                            // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\SettingType.png" scale="0.5" />
                            // If the setting selected a toggle setting...
                            if (Settings[heroid][si].type == SettingType.toggle)
                            {
                                // Check what the toggle setting selected is set to.
                                Thread.Sleep(100); // Sleep to allow the scrolling animation to catch up
                                updateScreen();
                                for (int y = 110; y < 436; y++)
                                    if (CompareColor(653, y, new int[] { 127, 127, 127 }, 10)
                                        && CompareColor(649, y, CALData.WhiteColor, 10))
                                    {
                                        bool option = (bool)hero.SetTo[setSettingIndex];

                                        bool selectedOption = !CompareColor(600, y, CALData.WhiteColor, 10); // Will equal true if selected setting is ENABLED,
                                                                                                                           // and equal false if DISABLED, OFF, or ON.
                                        // if selected option is OFF or ON...
                                        if (!selectedOption && CompareColor(599, y, CALData.WhiteColor, 10))
                                        {
                                            if (CompareColor(582, y, CALData.WhiteColor, 20) != option)
                                            {
                                                KeyPress(Keys.Space);
                                                Thread.Sleep(KeyPressWait);
                                            }
                                        }
                                        // else, the setting is enabled or disabled.
                                        else if (selectedOption == option)
                                        {
                                            KeyPress(Keys.Space);
                                            Thread.Sleep(KeyPressWait);
                                        }
                                        break;
                                    }
                            }
                            // If the selected setting is a dropdown menu...
                            else if (Settings[heroid][si].type == SettingType.dropdown)
                            {
                                KeyPress(Keys.Space);
                                Thread.Sleep(KeyPressWait);
                                KeyPress(Keys.Up); Thread.Sleep(KeyPressWait); KeyPress(Keys.Up);
                                int option = (int)hero.SetTo[setSettingIndex];
                                /* If the option variable equals 2, only Torbjorn's hammer can be used.
                                 * 0 = ALL
                                 * 1 = RIVET GUN ONLY
                                 * 2 = FORGE HAMMER ONLY */
                                for (int oi = 0; oi < option; oi++)
                                {
                                    KeyPress(Keys.Down);
                                    Thread.Sleep(KeyPressWait);
                                }
                                KeyPress(Keys.Space);
                                Thread.Sleep(KeyPressWait);
                            }
                            // If the selected setting is a numeric value...
                            else // the last possible thing Settings[heroid][si].type could be is SettingType.value
                            {
                                string setvalue = hero.SetTo[setSettingIndex].ToString(); // The numeric value to set the setting to as a string.
                                for (int stringset = 0; stringset < setvalue.Length; stringset++)
                                {
                                    int simulate = Int32.Parse(setvalue[stringset].ToString());
                                    KeyDown(NumberKeys[simulate]);
                                    Thread.Sleep(KeyPressWait);
                                }
                                KeyPress(Keys.Return);
                                Thread.Sleep(KeyPressWait);
                            }
                        }

                    // Go to next setting
                    KeyPress(Keys.Down);
                    Thread.Sleep(KeyPressWait);
                }
                GoBack(1);
            }
            // Go back to custom game menu.
            GoBack(2);
            if (OpenChatIsDefault)
                Chat.OpenChat();
        }
    }

    /// <summary>
    /// Data to change hero settings.
    /// </summary>
    public class SetHero
    {
        /// <summary>
        /// Hero to change settings for. Set to null for general settings.
        /// </summary>
        public Hero? Hero;
        /// <summary>
        /// Team to change hero settings for.
        /// </summary>
        public BotTeam Team;
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
        public SetHero(Hero? hero, BotTeam team, string[] set, object[] setto)
        {
            Hero = hero;
            Team = team;
            Set = set;
            SetTo = setto;
        }
    }

    /// <summary>
    /// All heroes in Overwatch.
    /// </summary>
    public enum Hero
    {
        Ana,
        Bastion,
        Brigitte,
        DVA,
        Doomfist,
        Genji,
        Hanzo,
        Junkrat,
        Lucio,
        McCree,
        Mei,
        Mercy,
        Moira,
        Orisa,
        Pharah,
        Reaper,
        Reinhardt,
        Roadhog,
        Soldier76,
        Sombra,
        Symmetra,
        Torbjorn,
        Tracer,
        Widowmaker,
        Winston,
        Zarya,
        Zenyatta
    }
}
