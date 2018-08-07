﻿using System;
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
        /// <example>
        /// The example below will enable random heroes for a team.
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using System.Linq;
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class SetHeroRosterExample
        /// {
        ///     public static void ChooseRandomHeroes(CustomGame cg, BotTeam team, int randomHeroCount)
        ///     {
        ///         Random rnd = new Random();
        ///         int heroCount = Enum.GetNames(typeof(MyEnum)).Length - 1;
        ///         
        ///         List&lt;Hero&gt; chooseHeroes = new List&lt;Hero&gt;();
        ///         while (chooseHeroes.Count &lt; randomHeroCount)
        ///         {
        ///             int heroID = rnd.Next(heroCount);
        ///             if (chooseHeroes.Select(v => v as int).Contains(heroID))
        ///                 continue;
        ///             chooseHeroes.Add(heroID as Hero);
        ///         }
        ///         
        ///         cg.SetHeroRoster(ToggleAction.DisableAll, team, chooseHeroes.ToArray());
        ///     }
        /// }
        /// </code>
        /// </example>
        public void SetHeroRoster(ToggleAction ta, BotTeam team, params Hero[] heroes)
        {
            GoToSettings();
            LeftClick(Points.SETTINGS_HEROES); // click heroes
            LeftClick(Points.SETTINGS_HEROES_ROSTER); // click hero roster
            // If team doesn't equal both, click a team to change hero roster for.
            if (team == BotTeam.Blue)
            {
                LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_DROPDOWN, 250);
                LeftClick(Points.SETTINGS_HEROES_ROSTER_TEAM_BLUE, 250);
            }
            if (team == BotTeam.Red)
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
        static HeroSettingData[][] HeroSettings = HeroSettingData.GetSettings();
        private class HeroSettingData
        {
            public string setting;
            public SettingType type;
            private HeroSettingData(string setting, SettingType type)
            {
                this.setting = setting;
                this.type = type;
            }
            public static HeroSettingData[][] GetSettings()
            {
                // Read hero_settings resource. Each value in array is a line in hero_settings.txt.
                string[] settings = Properties.Resources.hero_settings.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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
                // Convert from List<Setting>[] to Setting[][]
                List<HeroSettingData[]> ret = new List<HeroSettingData[]>();
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
            for (int i = 0; i < HeroSettings[heroid].Length; i++)
                if (HeroSettings[heroid][i].setting == setting)
                    return HeroSettings[heroid][i].type;
            return null;
        }

        /// <summary>
        /// Change individual hero settings.
        /// </summary>
        /// <param name="herodata">Settings of the heroes you want to change.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a <paramref name="herodata"/>'s set and setto length are not equal length.</exception>
        /// <exception cref="InvalidSetheroException">Thrown when any of the settings do not exist for their respective heros or if a set's respective setto is not the correct type.</exception>
        /// <remarks>
        /// The complete list of settings can be found <a href="https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation/blob/master/CustomGameLib/CustomGameLib/Resources/hero_settings.txt" content="here."/>
        /// Toggle settings require a boolean. Value and dropdown settings require an integer.
        /// </remarks>
        /// <example>
        /// The code below will make blue spawn with ultimates and have infinite ultimate duration, and give red 200% movement speed and 50% movement gravity.
        /// <code>
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class SetHeroSettingsExample
        /// {
        ///     public void SetHeroValues(CustomGame cg)
        ///     {
        ///         cg.SetHeroSettings
        ///         (
        ///             // Make blue spawn with ultimates and have infinite ultimate duration.
        ///             new SetHero(
        ///                 null, // General is null
        ///                 BotTeam.Blue,
        ///                 new string[] { "spawn_with_ultimate_ready", "infinite_ultimate_duration" },
        ///                 new object[] { true,                         true }
        ///             )
        ///             // Give red 200% movement speed and 50% movement gravity.
        ///             , new SetHero(
        ///                 null,
        ///                 BotTeam.Red,
        ///                 new string[] { "movement_speed", "movement_gravity" },
        ///                 new object[] { 200,              50 }
        ///             )
        ///         );
        ///     }
        /// }
        /// </code>
        /// The code below will make torbjorn on either team use hammer only and mercy on red use her staff.
        /// <code>
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class SetHeroSettingsExample
        /// {
        ///     public void SetHeroValues(CustomGame cg)
        ///     {
        ///         cg.SetHeroSettings
        ///         (
        ///             // Make torbjorn only allowed to use his hammer.
        ///             new SetHero(
        ///                 Hero.Torbjorn, // Torbjorn
        ///                 BotTeam.Both, // On both teams
        ///                 new string[] { "weapons_enabled" },
        ///                 new object[] { 2 } // 0 = All, 1 = Rivet gun only, 2 = Forge hammer only.
        ///             )
        ///             // Make mercy on red only allowed to use her staff.
        ///             , new SetHero(
        ///                 Hero.Mercy, // Mercy
        ///                 BotTeam.Red, // On red
        ///                 new string[] { "weapons_enabled" },
        ///                 new object[] { 1 } // 0 = All, 1 = Caduceus staff only, 2 = Caduceus blaster only
        ///             )
        ///         );
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="SetHero"/>
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
                        LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_DROPDOWN); // click team menu
                        LeftClick(Points.SETTINGS_HEROES_SETTINGS_TEAM_BLUE); // click blue
                    }
                    if (hero.Team == BotTeam.Red)
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
                    Thread.Sleep(KeyPressWait);
                    // <image url="$(ProjectDir)\ImageComments\SelectHero.cs\TopOption.png" scale="0.55" />
                    updateScreen();
                    // Check if the second option is highlighted.
                    if (CompareColor(422, 200, Colors.WHITE, 10))
                    {
                        // If it is, press the up arrow.
                        KeyPress(Keys.Up);
                        Thread.Sleep(KeyPressWait);
                    }
                }

                // Get the last setting to change.
                int max = 0;
                for (int si = 0; si < HeroSettings[heroid].Length; si++)
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
                                    Thread.Sleep(KeyPressWait);
                                }
                            }
                            // If the selected setting is a dropdown menu...
                            else if (HeroSettings[heroid][si].type == SettingType.dropdown)
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
                                var keys = GetNumberKeys((int)hero.SetTo[setSettingIndex]); // The numeric value to set the setting to as a string.
                                for (int sk = 0; sk < keys.Length; sk++)
                                {
                                    KeyDown(keys[sk]);
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
        WreckingBall,
        Zarya,
        Zenyatta
    }
}
