﻿using LSTY.Sdtd.PatronsMod.Internal;
using LSTY.Sdtd.Shared;
using System.Collections.Generic;

namespace LSTY.Sdtd.PatronsMod.Commands
{
    public class SayToPlayer : ConsoleCmdBase
    {
        public override string GetDescription()
        {
            return "Send a message to a single player";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ty-pm <entity id / platform user id / player name> <message>\n" +
                   "1. Send a PM to the player given by the entity id or platform user id or player name (as given by e.g. \"lpi\").";
        }

        public override string[] GetCommands()
        {
            return new[] { "ty-SayToPlayer", "ty-pm" };
        }

        private void SendMessage(ClientInfo receiver, ClientInfo sender, string message, string senderName)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string senderId;

            if (sender == null)
            {
                senderId = ExportedConstants.NonPlayer;
            }
            else
            {
                senderId = sender.PlatformId.CombinedString;
                senderName = sender.playerName;
            }

            receiver.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, message, senderName, false, null));

            CustomLogger.Info("Message to player {0} sent with sender {1}.", receiver.PlatformId.CombinedString, senderId);
        }

        private void InternalExecute(ClientInfo sender, List<string> args)
        {
            if (args.Count < 2)
            {
                Log("Usage: sayplayer <entityId|platformUserId|playerName> <message>");
                return;
            }

            string message = args[1];

            ClientInfo receiver = ConsoleHelper.ParseParamIdOrName(args[0]);
            if (receiver == null)
            {
                Log("EntityId or platformUserId or playerName not found.");
            }
            else
            {
                string senderName = (args.Count < 3 || string.IsNullOrEmpty(args[2])) ? DefaultServerName : args[2];
                SendMessage(receiver, sender, message, senderName);
            }
        }

        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            // From game client.
            if (senderInfo.RemoteClientInfo != null)
            {
                InternalExecute(senderInfo.RemoteClientInfo, args);
            }
            // From console.
            else
            {
                InternalExecute(null, args);
            }
        }
    }
}