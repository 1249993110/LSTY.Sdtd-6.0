using LSTY.Sdtd.Shared.Hubs;

namespace LSTY.Sdtd.Services.Extensions
{
    public static class IServerManageHubExtension
    {
        #region ChatHelper

        /// <summary>
        /// Sends a global message to all connected clients.
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="message"></param>
        /// <param name="senderName"></param>
        /// <returns></returns>
        public static async Task SendGlobalMessage(this IServerManageHub hub, string message, string senderName)
        {
            await hub.ExecuteConsoleCommand($"ty-say {message} {senderName}");
        }

        /// <summary>
        /// Sends a whisper message to single connected client.
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="playerIdOrName"></param>
        /// <param name="message"></param>
        /// <param name="senderName"></param>
        /// <returns></returns>
        public static async Task SendMessageToPlayer(this IServerManageHub hub, object playerIdOrName, string message, string senderName)
        {
            await hub.ExecuteConsoleCommand($"ty-pm {playerIdOrName} {message} {senderName}");
        }

        #endregion ChatHelper

        #region Teleport player

        public static async Task TelePlayer(this IServerManageHub hub, object playerIdOrName, string targetId)
        {
            await hub.ExecuteConsoleCommand($"tele {playerIdOrName} {targetId}");
        }

        #endregion Teleport player

        #region Give Item

        public static async Task GiveItem(this IServerManageHub hub, object playerIdOrName, string itemName, int count, int quality = 0, int durability = 0)
        {
            await hub.ExecuteConsoleCommand($"ty-gi {playerIdOrName} {itemName} {count} {quality} {durability}");
        }

        #endregion Give Item

        #region Spawn Entity

        public static async Task SpawnEntity(this IServerManageHub hub, int playerNameOrEntityId, string spawnEntityIdOrName)
        {
            await hub.ExecuteConsoleCommand($"se {playerNameOrEntityId} {spawnEntityIdOrName}");
        }

        #endregion Spawn Entity
    }
}