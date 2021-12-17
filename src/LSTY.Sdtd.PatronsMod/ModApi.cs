using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using LSTY.Sdtd.Shared;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.AspNet.SignalR;
using LSTY.Sdtd.PatronsMod.Internal;
using IceCoffee.Common;
using Pathfinding;
using System.Collections.Generic;
using HarmonyLib;
using IceCoffee.Common.Extensions;
using Platform.Local;
using LSTY.Sdtd.PatronsMod.SignalR;

namespace LSTY.Sdtd.PatronsMod
{
    public class ModApi : IModApi
    {
        public const string ModIdentity = "LSTY.Sdtd.PatronsMod";

        private static SynchronizationContext _mainThreadContext;
        internal static SynchronizationContext MainThreadContext => _mainThreadContext;

        internal static readonly string ModDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static readonly AppSettings AppSettings = ConfigurationLoader.GetCallingAssemblySettings<AppSettings>();

        internal static ClientInfo GetCmdExecuteDelegate() => new ClientInfo() { PlatformId =  new UserIdentifierLocal(ModApi.ModIdentity) };

        public void InitMod(Mod modInstance)
        {
            try
            {
                _mainThreadContext = SynchronizationContext.Current;

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
                ModEvents.GameAwake.RegisterHandler(ModEventHook.GameAwake);
                ModEvents.GameStartDone.RegisterHandler(ModEventHook.GameStartDone);
                ModEvents.GameShutdown.RegisterHandler(ModEventHook.GameShutdown);
                ModEvents.PlayerSpawnedInWorld.RegisterHandler(ModEventHook.PlayerSpawnedInWorld);
                ModEvents.EntityKilled.RegisterHandler(ModEventHook.EntityKilled);
                ModEvents.PlayerDisconnected.RegisterHandler(ModEventHook.PlayerDisconnected);
                ModEvents.SavePlayerData.RegisterHandler(ModEventHook.SavePlayerData);
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
