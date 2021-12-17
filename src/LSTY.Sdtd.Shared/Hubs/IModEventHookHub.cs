using LSTY.Sdtd.Shared.Models;

namespace LSTY.Sdtd.Shared.Hubs
{
    public interface IModEventHookHub
    {
        void OnGameAwake();

        void OnGameStartDone();

        void OnGameShutdown();

        void OnEntitySpawned(Entity entity);

        void OnChatMessage(ChatMessage chatMessage);

        void OnPlayerDisconnected(int entityId);

        void OnSavePlayerData(OnlinePlayer onlinePlayer);

        void OnPlayerSpawning(OnlinePlayer onlinePlayer);

        void OnPlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs);

        void OnEntityKilled(Entity entity, int entityIdThatKilledMe);
    }
}