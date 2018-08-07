using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        internal bool OpenChatIsDefault = true;

        /// <summary>
        /// Chat for Overwatch.
        /// </summary>
        public Chat Chat;
    }

    /// <summary>
    /// Chat for Overwatch.
    /// </summary>
    /// <remarks>
    /// The Chat class is accessed in a CustomGame object on the <see cref="CustomGame.Chat"/> field.
    /// </remarks>
    public class Chat : CustomGameBase
    {
        internal Chat(CustomGame cg) : base(cg) { }

        /// <summary>
        /// Send message to chat.
        /// </summary>
        /// <param name="text">Text to send.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
        /// <example>
        /// The code below will send a message to the chat when a player joins.
        /// <code>
        /// using System.Threading;
        /// using Deltin.CustomGameAutomation;
        /// 
        /// class ChatExample
        /// {
        ///     public static void SendMessageWhenPlayerJoins(CustomGame cg)
        ///     {
        ///         int previousPlayerCount = cg.TotalPlayerCount;
        ///         while (true)
        ///         {
        ///             // If the last recorded player count does not equal the current player count, send a message to the chat.
        ///             int playerCount = cg.TotalPlayerCount;
        ///             if (playerCount != previousPlayerCount)
        ///             {
        ///                 cg.Chat.Chat("Welcome to my custom game!");
        ///                 previousPlayerCount = playerCount;
        ///             }
        ///             
        ///             Thread.Sleep(100);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void SendChatMessage(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text", "text was null.");

            OpenChat();
            cg.updateScreen();
            // To prevent abuse, make sure that the channel is not general.
            if (!cg.CompareColor(Points.LOBBY_CHAT_TYPE_INDICATOR, GeneralChatColor, 20) || !BlockGeneralChat)
            {
                cg.TextInput(text);
            }
            cg.KeyPress(Keys.Return);
            if (cg.OpenChatIsDefault)
            {
                Thread.Sleep(250);
                OpenChat();
            }
            cg.ResetMouse();
        }

        /// <summary>
        /// Prevents chat messages from being sent to the general channel.
        /// </summary>
        const bool BlockGeneralChat = true;

        /// <summary>
        /// Swaps to a chat channel.
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void SwapChannel(Channel channel)
        {
            OpenChat();
            cg.TextInput(GetChannelJoinCommand(channel));
            cg.KeyPress(Keys.Return);
            // Open chat if it is default to be opened
            if (cg.OpenChatIsDefault)
                OpenChat();
            else
                cg.KeyPress(Keys.Return);

            cg.ResetMouse();
        }

        /// <summary>
        /// Leaves a channel so no chat messages can be sent or recieved on that channel.
        /// </summary>
        /// <param name="channel">Channel to leave.</param>
        public void LeaveChannel(Channel channel)
        {
            if (!cg.OpenChatIsDefault)
                OpenChat();
            cg.TextInput(GetChannelJoinCommand(channel));
            cg.KeyPress(Keys.Return);
            Thread.Sleep(250);
            cg.updateScreen();
            if (cg.CompareColor(Points.LOBBY_CHAT_TYPE_INDICATOR, GetChannelColor(channel), ChatFade))
            {
                SendChatMessage("/leavechannel");
                if (cg.OpenChatIsDefault)
                {
                    cg.updateScreen();
                    if (cg.CompareColor(Points.LOBBY_CHAT_TYPE_INDICATOR, GetChannelColor(channel), ChatFade))
                        cg.KeyPress(Keys.Tab);
                }
            }
            else if (!cg.OpenChatIsDefault)
                cg.KeyPress(Keys.Return);
        }

        /// <summary>
        /// Joins a channel after leaving it with LeaveChannel.
        /// </summary>
        /// <param name="channel">Channel to rejoin.</param>
        public void JoinChannel(Channel channel)
        {
            SendChatMessage("/joinchannel " + channel.ToString());
        }

        internal void OpenChat()
        {
            cg.LeftClick(Points.LOBBY_CHATBOX, 100);
        }

        internal void CloseChat()
        {
            OpenChat();
            cg.KeyPress(Keys.Return);
            Thread.Sleep(250);
        }

        internal static int[] GetChannelColor(Channel channel)
        {
            return (ChatColors[(int)channel]);
        }

        internal static string GetChannelJoinCommand(Channel channel)
        {
            return ChannelJoinCommands[(int)channel];
        }

        // <image url="$(ProjectDir)\ImageComments\Chat.cs\ChatLocation.png" scale="2" />
        // The color of the pixel at 50, 505 changes depending on which channel the overwatch client is in.
        internal static int ChatFade = 20;
        internal static int[] TeamChatColor = new int[] { 65, 139, 162 };
        internal static int[] MatchChatColor = new int[] { 161, 122, 91 };
        internal static int[] GeneralChatColor = new int[] { 161, 161, 162 };
        internal static int[] GroupChatColor = new int[] { 0, 0, 0 }; // TODO: Get this color
        internal static int[] PrivateMessageChatColor = new int[] { 160, 118, 167 };
        // Must be the same order as the Channel enum below
        internal static int[][] ChatColors = new int[][]
        {
                TeamChatColor,
                MatchChatColor,
                GeneralChatColor,
                GroupChatColor,
                PrivateMessageChatColor
        };
        // These are commands when typed into the chat will join their respective channels.
        // Must be the same order as the Channel enum below
        internal static string[] ChannelJoinCommands = new string[] { "/t", "/m", "/all", "/g", "/r" };
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
}
