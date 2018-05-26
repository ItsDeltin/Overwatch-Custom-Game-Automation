using System;
using System.Windows.Forms;
using System.Threading;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        // Does not work if the screenshot method is ScreenshotMethods.OverwatchScreenshotFunction.
        bool OpenChatIsDefault = true;

        public CG_Chat Chat;
        public class CG_Chat
        {
            private CustomGame cg;
            internal CG_Chat(CustomGame cg)
            { this.cg = cg; }

            /// <summary>
            /// Send message to chat.
            /// </summary>
            /// <param name="words">Text to send.</param>
            public void Chat(string words)
            {
                if (ChatPrefix != null) words = ChatPrefix + " " + words;
                //if (!cg.OpenChatIsDefault)
                    OpenChat();
                cg.updateScreen();
                // To prevent abuse, make sure that the channel is not general.
                if (!cg.CompareColor(50, 505, CALData.GeneralChatColor, 20) || !BlockGeneralChat)
                {
                    cg.TextInput(words);
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
            /// Prefix to CustomGame.Chat() messages.
            /// </summary>
            public string ChatPrefix = null;
            /// <summary>
            /// Prevents chat messages from being sent to the general channel.
            /// </summary>
            public bool BlockGeneralChat = false;

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
                if (cg.CompareColor(CALData.ChatLocation.X, CALData.ChatLocation.Y, GetChannelColor(channel), CALData.ChatFade))
                {
                    Chat("/leavechannel");
                    if (cg.OpenChatIsDefault)
                    {
                        cg.updateScreen();
                        if (cg.CompareColor(CALData.ChatLocation.X, CALData.ChatLocation.Y, GetChannelColor(channel), CALData.ChatFade))
                            cg.KeyPress(Keys.Tab);
                    }
                }
                else if (!cg.OpenChatIsDefault)
                    cg.KeyPress(Keys.Return);
            }

            public void JoinChannel(Channel channel)
            {
                Chat("/joinchannel " + channel.ToString());
            }

            internal void OpenChat()
            {
                cg.LeftClick(105, 504, 100); // 72
            }

            internal void CloseChat()
            {
                OpenChat();
                cg.KeyPress(Keys.Return);
                Thread.Sleep(250);
            }

            internal static int[] GetChannelColor(Channel channel)
            {
                if (channel == Channel.General)
                    return CALData.GeneralChatColor;
                else if (channel == Channel.Match)
                    return CALData.MatchChatColor;
                else if (channel == Channel.Team)
                    return CALData.TeamChatColor;
                else return null;
            }

            internal static string GetChannelJoinCommand(Channel channel)
            {
                if (channel == Channel.Team) return "/t"; // switch to team chat
                if (channel == Channel.Match) return "/m"; // switch to match chat
                if (channel == Channel.General) return "/all"; // switch to general chat
                if (channel == Channel.Group) return "/g"; // switch to group chat
                if (channel == Channel.PrivateMessage) return "/r"; // respond to last PM
                return String.Empty;
            }
        }
    }
    public enum Channel
    {
        Team,
        Match,
        General,
        Group,
        PrivateMessage
    }
}
