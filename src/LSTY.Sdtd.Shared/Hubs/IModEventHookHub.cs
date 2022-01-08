using LSTY.Sdtd.Shared.Models;

namespace LSTY.Sdtd.Shared.Hubs
{
    public interface IModEventHookHub
    {
        void OnLogCallback(LogEntry logEntry);

        void OnGameAwake();

        void OnGameStartDone();

        void OnGameShutdown();

        void OnEntitySpawned(Entity entity);

        void OnChatMessage(ChatMessage chatMessage);

        void OnPlayerDisconnected(int entityId);

        //void OnSavePlayerData(PlayerBase playerBase);

        void OnPlayerSpawning(PlayerBase playerBase);

        void OnPlayerSpawnedInWorld(PlayerSpawnedEventArgs playerSpawnedEventArgs);

        void OnEntityKilled(Entity entity, int entityIdThatKilledMe);
    }
}