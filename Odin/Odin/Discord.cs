using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin
{
    public class Discord
    {
        #region Public

        public const string DISCORD_APPLICATION_ID = "496116687293120522";
        public static bool Enabled = false;

        public static string State
        {
            get => Presence.state;
            set
            {
                Presence.state = value;
                UpdatePresence();
            }
        }

        public static string Details
        {
            get => Presence.details;
            set
            {
                Presence.details = value;
                UpdatePresence();
            }
        }

        public static string LargeImageText
        {
            get => Presence.largeImageText;
            set
            {
                Presence.largeImageText = value;
                UpdatePresence();
            }
        }

        public static string LargeImageKey
        {
            get => Presence.largeImageKey;
            set
            {
                Presence.largeImageKey = value;
                UpdatePresence();
            }
        }

        public static string SmallImageText
        {
            get => Presence.smallImageText;
            set
            {
                Presence.smallImageText = value;
                UpdatePresence();
            }
        }

        public static string SmallImageKey
        {
            get => Presence.smallImageKey;
            set
            {
                Presence.smallImageKey = value;
                UpdatePresence();
            }
        }

        public static long StartTimestamp
        {
            get => Presence.startTimestamp;
            set
            {
                Presence.startTimestamp = value;
                UpdatePresence();
            }
        }

        public static long EndTimestamp
        {
            get => Presence.endTimestamp;
            set
            {
                Presence.endTimestamp = value;
                UpdatePresence();
            }
        }

        #endregion

        #region Private

        private static DiscordPresence.RichPresence Presence = new DiscordPresence.RichPresence();
        private static DiscordPresence.RichPresence EmptyPresence = new DiscordPresence.RichPresence();
        private static DiscordPresence.EventHandlers Handlers = new DiscordPresence.EventHandlers();

        #endregion

        #region Constructor

        static Discord()
        {
            DiscordPresence.Initialize(DISCORD_APPLICATION_ID, Handlers);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private static void UpdatePresence()
        {
            if (Enabled)
                DiscordPresence.UpdatePresence(Presence);
            else
                DiscordPresence.UpdatePresence(EmptyPresence);
        }

        #endregion
    }
}
