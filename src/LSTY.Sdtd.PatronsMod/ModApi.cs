using HarmonyLib;
using LSTY.Sdtd.PatronsMod.Internal;
using LSTY.Sdtd.PatronsMod.SignalR;
using LSTY.Sdtd.Shared;
using Microsoft.Owin.Hosting;
using Platform.Local;
using System.Reflection;

namespace LSTY.Sdtd.PatronsMod
{
    public class ModApi : IModApi
    {
        public const string ModIdentity = "LSTY.Sdtd.PatronsMod";

        private static Mod _modInstance;

        private static SynchronizationContext _mainThreadContext;
        internal static SynchronizationContext MainThreadContext => _mainThreadContext;

        internal static string ModDirectory => _modInstance.Path;

        private static AppSettings _appSettings;
        internal static AppSettings AppSettings => _appSettings;

        internal static ClientInfo GetCmdExecuteDelegate() => new ClientInfo() { PlatformId =  new UserIdentifierLocal(ModApi.ModIdentity) };

        public void InitMod(Mod modInstance)
        {
            try
            {
                _modInstance = modInstance;

                _mainThreadContext = SynchronizationContext.Current;

                _appSettings = ConfigurationLoader.GetCallingAssemblySettings<AppSettings>();

                StartupSignalR();

                PatchByHarmony();

                RegisterModEventHandlers();
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Initialize mod " + ModIdentity + " failed");
            }
        }

        private static void StartupSignalR()
        {
            try
            {
                string url = AppSettings.SignalRUrl;
                WebApp.Start<SignalRStartup>(url);
                CustomLogger.Info("SignalR Server running on " + url);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Startup signalR server failed");
                throw;
            }
        }

        private static void PatchByHarmony()
        {
            try
            {
                var harmony = new Harmony(ModIdentity);
                harmony.PatchAll();

                CustomLogger.Info("Successfully patch all by harmony");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Patch by harmony failed");
                throw;
            }
        }

        private static void RegisterModEventHandlers()
        {
            try
            {
                Log.LogCallbacks += ModEventHook.LogCallback;
                ModEvents.GameAwake.RegisterHandler(ModEventHook.GameAwake);
                ModEvents.GameStartDone.RegisterHandler(ModEventHook.GameStartDone);
                ModEvents.GameShutdown.RegisterHandler(ModEventHook.GameShutdown);
                ModEvents.PlayerSpawnedInWorld.RegisterHandler(ModEventHook.PlayerSpawnedInWorld);
                ModEvents.EntityKilled.RegisterHandler(ModEventHook.EntityKilled);
                ModEvents.PlayerDisconnected.RegisterHandler(ModEventHook.PlayerDisconnected);
                // ModEvents.SavePlayerData.RegisterHandler(ModEventHook.SavePlayerData);
                ModEvents.ChatMessage.RegisterHandler(ModEventHook.ChatMessage);
                ModEvents.PlayerSpawning.RegisterHandler(ModEventHook.PlayerSpawning);

                CustomLogger.Info("Successfully registered mod event handlers");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Register mod event handlers failed");
                throw;
            }
        }
    }
}