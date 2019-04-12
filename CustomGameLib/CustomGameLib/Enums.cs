using System;

namespace Deltin.CustomGameAutomation
{
    #region Public Enums

    /// <summary>
    /// The screenshot method used to capture the Overwatch window screen.
    /// BitBlt is faster and works even if another window is over the Overwatch window.
    /// If BitBlt does not work for you, use ScreenCopy.
    /// </summary>
    public enum ScreenshotMethod
    {
        /// <summary>
        /// The BitBlt method of screen capturing.
        /// </summary>
        BitBlt,

        /// <summary>
        /// The ScreenCopy method of screen capturing.
        /// </summary>
        ScreenCopy
    }

    /// <summary>
    /// AI heroes.
    /// </summary>
    public enum AIHero
    {
        /// <summary>
        /// Overwatch's reccommended AI hero.
        /// </summary>
        Recommended,
#pragma warning disable CS1591
        Ana,
        Bastion,
        Lucio,
        McCree,
        Mei,
        Reaper,
        Roadhog,
        Soldier76,
        Sombra,
        Torbjorn,
        Zarya,
        Zenyatta
#pragma warning restore CS1591
    }

    /// <summary>
    /// AI difficulties.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// Easy AI difficulty.
        /// </summary>
        Easy,
        /// <summary>
        /// Medium AI difficulty.
        /// </summary>
        Medium,
        /// <summary>
        /// Hard AI difficulty.
        /// </summary>
        Hard
    }

    /// <summary>
    /// Chat channels for Overwatch.
    /// </summary>
    public enum Channel
    {
        /// <summary>
        /// The team chat channel.
        /// </summary>
        Team,
        /// <summary>
        /// The match chat channel.
        /// </summary>
        Match,
        /// <summary>
        /// The general chat channel.
        /// </summary>
        General,
        /// <summary>
        /// The group chat channel.
        /// </summary>
        Group,
        /// <summary>
        /// The private message chat channel.
        /// </summary>
        PrivateMessage
    }

    /// <summary>
    /// Teams in Overwatch.
    /// </summary>
    [Flags]
    public enum Team
    {
        /// <summary>
        /// The blue team.
        /// </summary>
        Blue = 1 << 0,
        /// <summary>
        /// The red team.
        /// </summary>
        Red = 1 << 1,
        /// <summary>
        /// The blue and red team.
        /// </summary>
        BlueAndRed = Blue | Red,
        /// <summary>
        /// The spectators.
        /// </summary>
        Spectator = 1 << 2,
        /// <summary>
        /// The queue.
        /// </summary>
        Queue = 1 << 3
    }

    /// <summary>
    /// Teams in the queue.
    /// </summary>
    public enum QueueTeam
    {
        /// <summary>
        /// Queueing for both blue and red.
        /// </summary>
        Neutral,
        /// <summary>
        /// Queueing for blue.
        /// </summary>
        Blue,
        /// <summary>
        /// Queueing for red.
        /// </summary>
        Red
    }

    /// <summary>
    /// Options for who can join the game.
    /// </summary>
    public enum Join
    {
        /// <summary>
        /// Everyone can join the game.
        /// </summary>
        Everyone,
        /// <summary>
        /// Only friends of the moderator can join the game.
        /// </summary>
        FriendsOnly,
        /// <summary>
        /// Only players invited can join the game.
        /// </summary>
        InviteOnly
    }

    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The custom game is in the lobby.
        /// </summary>
        InLobby,
        /// <summary>
        /// The custom game is waiting for players.
        /// </summary>
        Waiting,
        /// <summary>
        /// The custom game is currently ingame.
        /// </summary>
        Ingame,
        /// <summary>
        /// The custom game is at player commendation.
        /// </summary>
        Ending_Commend,
        /// <summary>
        /// Cannot recognize what state the game is on.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Enables/disables settings before toggling them.
    /// </summary>
    public enum ToggleAction
    {
        /// <summary>
        /// Do not enable/disable.
        /// </summary>
        None,
        /// <summary>
        /// Disable all options before toggling.
        /// </summary>
        DisableAll,
        /// <summary>
        /// Enable all options before toggling.
        /// </summary>
        EnableAll
    }

    /// <summary>
    /// Result info of GetHero().
    /// </summary>
    /// <seealso cref="PlayerInfo.GetHero(int, out HeroResultInfo)"/>
    public enum HeroResultInfo
    {
        /// <summary>
        /// The hero the player was playing was successfully found.
        /// </summary>
        Success,
        /// <summary>
        /// Can't get the hero the player was playing because the player is dead. Try rescanning when the player is alive again.
        /// </summary>
        PlayerWasDead,
        /// <summary>
        /// The player did not choose a hero.
        /// </summary>
        NoHeroChosen,
        /// <summary>
        /// The slot was empty.
        /// </summary>
        SlotEmpty,
        /// <summary>
        /// Could not tell what hero the player is playing. Usually if you get this it is a bug with <see cref="PlayerInfo.GetHero(int, out HeroResultInfo)"/>.
        /// </summary>
        NoCompatibleHeroFound
    }

    /// <summary>
    /// Flags for obtaining slots.
    /// </summary>
    /// <seealso cref="CustomGame.GetSlots(SlotFlags, bool)"/>
    /// <seealso cref="CustomGame.GetCount(SlotFlags, bool)"/>
    [Flags]
    public enum SlotFlags
    {
        /// <summary>
        /// Get blue slots.
        /// </summary>
        Blue = 1 << 0,
        /// <summary>
        /// Get red slots.
        /// </summary>
        Red = 1 << 1,
        /// <summary>
        /// Get spectator slots.
        /// </summary>
        Spectators = 1 << 2,
        /// <summary>
        /// Get neutral queue slots.
        /// </summary>
        NeutralQueue = 1 << 3,
        /// <summary>
        /// Get red queue slots
        /// </summary>
        RedQueue = 1 << 4,
        /// <summary>
        /// Get blue queue slots
        /// </summary>
        BlueQueue = 1 << 5,
        /// <summary>
        /// Get queue slots
        /// </summary>
        Queue = NeutralQueue | RedQueue | BlueQueue,
        /// <summary>
        /// Get blue and red slots.
        /// </summary>
        BlueAndRed = Blue | Red,
        /// <summary>
        /// Gets blue, red, spectator, and queue slots.
        /// </summary>
        All = Blue | Red | Spectators | Queue,

        /// <summary>
        /// Players only, no AI.
        /// </summary>
        PlayersOnly = 1 << 6,
        /// <summary>
        /// AI only, no players.
        /// </summary>
        AIOnly = 1 << 7,

        /// <summary>
        /// Gets dead players only.
        /// </summary>
        DeadOnly = 1 << 8,
        /// <summary>
        /// Gets alive players only.
        /// </summary>
        AliveOnly = 1 << 9,

        /// <summary>
        /// Gets the invited players only.
        /// </summary>
        InvitedOnly = 1 << 10,
        /// <summary>
        /// Gets the ingame players only.
        /// </summary>
        IngameOnly = 1 << 11,
    }

    /// <summary>
    /// All heroes in Overwatch.
    /// </summary>
    public enum Hero
    {
#pragma warning disable CS1591
        Ana,
        Ashe,
        Baptiste,
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
#pragma warning restore CS1591
    }

    /// <summary>
    /// Flags for scanning an option menu in Overwatch.
    /// </summary>
    /// <seealso cref="Interact.MenuOptionScan(System.Drawing.Point, OptionScanFlags, string, DirectBitmap)"/>
    [Flags]
    public enum OptionScanFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// Open the menu before scanning starts.
        /// </summary>
        OpenMenu = 1 << 0,
        /// <summary>
        /// Close the menu after scanning finishes.
        /// </summary>
        CloseMenu = 1 << 1,
        /// <summary>
        /// Close the menu if the option being scanned for is not found.
        /// </summary>
        CloseIfNotFound = 1 << 2,
        /// <summary>
        /// Click the option if it is found.
        /// </summary>
        Click = 1 << 3,
        /// <summary>
        /// Return the location of the option.
        /// </summary>
        ReturnLocation = 1 << 4,
        /// <summary>
        /// Return whether or not the option is found.
        /// </summary>
        ReturnFound = 1 << 5
    }

    /// <summary>
    /// Overwatch's limited time events.
    /// </summary>
    public enum OWEvent
    {
#pragma warning disable CS1591
        None,
        SummerGames,
        HalloweenTerror,
        WinterWonderland,
        LunarNewYear,
        Archives,
        Aniversary
#pragma warning restore CS1591
    }

    /// <summary>
    /// Overwatch's gamemodes.
    /// </summary>
    [Flags]
    public enum Gamemode
    {
#pragma warning disable CS1591
        Assault = 1 << 0, // a
        AssaultEscort = 1 << 1, // ae
        CaptureTheFlag = 1 << 2, // ctf
        Control = 1 << 3, // c
        Deathmatch = 1 << 4, // dm
        Elimination = 1 << 5, // elim
        Escort = 1 << 6, // e
        JunkensteinsRevenge = 1 << 7, // jr
        Lucioball = 1 << 8, // lb
        MeisSnowballOffensive = 1 << 9, // mso
        Skirmish = 1 << 10, // skirm
        TeamDeathmatch = 1 << 11, // tdm
        YetiHunter = 1 << 12 // yh
#pragma warning restore CS1591
    }

    /// <summary>
    /// Default presets in Overwatch.
    /// </summary>
    /// <seealso cref="Settings.LoadPreset(DefaultPreset)"/>
    public enum DefaultPreset
    {
#pragma warning disable CS1591
        Standard_1v1LimitedDuel     = 0,  Standard_1v1MysteryDuel        = 1,  Standard_3v3Elimination       = 2,  Standard_3v3LockoutElimination = 3,
        Standard_4v4TeamDeatchmatch = 4,  Standard_6v6LockoutElimination = 5,  Standard_8PlayerFFADeathmatch = 6,  Standard_Assault               = 7,
        Standard_AssaultEscort      = 8,  Standard_CaptureTheFlag        = 9,  Standard_CaptureTheFlag2017   = 10, Standard_Competitive           = 11,
        Standard_Control            = 12, Standard_Escort                = 13, Standard_QuickPlay            = 14, /* 15 is empty */

        Brawl_MysteryHeroes = 16, Brawl_NoLimits = 17, Brawl_TotalMayhem = 18
#pragma warning restore CS1591
    }

    /// <summary>
    /// The state of Overwatch.
    /// </summary>
    public enum OverwatchState
    {
        /// <summary>
        /// Overwatch is ready to recieve commands from CustomGame functions.
        /// </summary>
        Ready,
        /// <summary>
        /// Overwatch disconnected.
        /// </summary>
        Disconnected,
        /// <summary>
        /// Overwatch is in the main menu. Call <see cref="CustomGame.CreateCustomGame"/> to create a custom game.
        /// </summary>
        MainMenu
    }

#pragma warning disable CS1591
    [Flags]
    public enum KeybindModifier
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4
    }
#pragma warning restore CS1591

    #endregion

    #region Internal Enums

    [Flags]
    internal enum DBCompareFlags
    {
        None = 0,
        Multithread = 1 << 1,
        IgnoreBlack = 1 << 2,
        IgnoreWhite = 1 << 3
    }
    internal enum SettingType
    {
        value,
        toggle,
        dropdown
    }
    internal enum PixelType
    {
        Any, // Filled or empty
        Filled, // Black pixel
        Empty, // Dark red pixel
        Required // Blue pixel
    }

    #endregion
}
