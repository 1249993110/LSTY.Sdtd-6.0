using LSTY.Sdtd.PatronsMod.Extensions;
using LSTY.Sdtd.Shared;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Platform.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.PatronsMod
{
    [HubName(nameof(IServerManageHub))]
    public class ServerManageHub : Hub, IServerManageHub
    {
        public async Task<IEnumerable<string>> ExecuteConsoleCommand(string command, bool inMainThread = false)
        {
            if (inMainThread == false)
            {
                return await Task.Factory.StartNew((state) =>
                {
                    return SdtdConsole.Instance.ExecuteSync((string)state, ModApi.GetCmdExecuteDelegate());
                }, command);
            }
            else
            {
                return await Task.Factory.StartNew((state) =>
                {
                    List<string> executeResult = null;
                    ModApi.MainThreadContext.Send((state1) =>
                    {
                        executeResult = SdtdConsole.Instance.ExecuteSync((string)state1, ModApi.GetCmdExecuteDelegate());
                    }, state);

                    return executeResult;
                }, command);
            }

        }

        public async Task<LivePlayer> GetPlayer(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                if (GameManager.Instance.World.Players.dict.TryGetValue((int)state, out var player))
                {
                    return player.ToLivePlayer();
                }

                return null;
            }, entityId);
        }

        public async Task<IEnumerable<LivePlayer>> GetPlayers()
        {
            return await Task.Run(() =>
            {
                var result = new List<LivePlayer>();
                foreach (var player in GameManager.Instance.World.Players.dict.Values)
                {
                    var livePlayer = player.ToLivePlayer();
                    if (livePlayer != null)
                    {
                        result.Add(livePlayer);
                    }
                }

                return result;
            });
        }

        public async Task<int> GetPlayerCount()
        {
            return await Task.FromResult(GameManager.Instance.World.Players.Count);
        }

        public async Task<Shared.Models.Inventory> GetPlayerInventory(int entityId)
        {
            return await Task.Factory.StartNew((state) =>
            {
                return ConnectionManager.Instance.Clients.ForEntityId((int)state)?.latestPlayerData.GetInventory();
            }, entityId);
        }

        public async Task<IEnumerable<PlayerInventory>> GetPlayerInventories()
        {
            return await Task.Run(() =>
            {
                var result = new List<PlayerInventory>();
                foreach (var player in GameManager.Instance.World.Players.dict.Values)
                {
                    var inventory = ConnectionManager.Instance.Clients.ForEntityId(player.entityId)?.latestPlayerData.GetInventory();

                    if (inventory != null)
                    {
                        result.Add(new PlayerInventory()
                        {
                            EntityId = player.entityId,
                            Inventory = inventory
                        });
                    }
                }

                return result;
            });
        }

        public async Task<byte[]> GetItemIcon(string iconName)
        {
            return await Task.Factory.StartNew((state) =>
            {
                string iconPath = FindIconPath((string)state);
                if (iconPath == null)
                {
                    return null;
                }
                else
                {
                    return File.ReadAllBytes(iconPath);
                }
            }, iconName);
        }

        private static string FindIconPath(string iconName)
        {
            string path = "Data/ItemIcons/" + iconName;
            if (File.Exists(path))
            {
                return path;
            }

            foreach (Mod mod in ModManager.GetLoadedMods())
            {
                path = Path.Combine(mod.Path, "ItemIcons", iconName);
                if (File.Exists(path))
                {
                    return path;
                }

                foreach (string dir in Directory.GetDirectories(mod.Path))
                {
                    path = Path.Combine(dir, iconName);
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    foreach (string subDir in Directory.GetDirectories(dir))
                    {
                        path = Path.Combine(subDir, iconName);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            return null;
        }

        private static async Task<IEnumerable<string>> ExecuteConsoleCommandBatch<TObject>(IEnumerable<TObject> objects, Func<TObject, string> getCommand)
        {
            if (objects?.Any() == false)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            return await Task.Factory.StartNew((state) =>
            {
                var executeResult = new List<string>();
                var executeDelegate = ModApi.GetCmdExecuteDelegate();
                foreach (TObject item in (IEnumerable<TObject>)state)
                {
                    string command = getCommand(item);
                    var resultEntry = SdtdConsole.Instance.ExecuteSync(command, executeDelegate);
                    executeResult.AddRange(resultEntry);
                }

                return executeResult;
            }, objects);
        }

        #region Admins
        public async Task<IEnumerable<string>> AddAdmins(IEnumerable<AdminEntry> admins)
        {
            return await ExecuteConsoleCommandBatch(admins, obj => $"admin add {obj.UserIdentifier} {obj.PermissionLevel} {obj.DisplayName}");
        }

        public async Task<IEnumerable<string>> RemoveAdmins(IEnumerable<string> userIdentifiers)
        {
            return await ExecuteConsoleCommandBatch(userIdentifiers, obj => $"admin remove {obj}");
        }

        public async Task<IEnumerable<AdminEntry>> GetAdmins()
        {
            return await Task.Run(() =>
            {
                var result = new List<AdminEntry>();

                foreach (var item in GameManager.Instance.adminTools.GetAdmins().Values)
                {
                    result.Add(new AdminEntry()
                    {
                        UserIdentifier = item.UserIdentifier.CombinedString,
                        PermissionLevel = item.PermissionLevel,
                        DisplayName = item.Name,

                    });
                }

                return result;
            });
        }
        #endregion

        #region Permissions
        public async Task<IEnumerable<string>> AddPermissions(IEnumerable<PermissionEntry> permissions)
        {
            return await ExecuteConsoleCommandBatch(permissions, obj => $"cp add {obj.Command} {obj.Level}");
        }

        public async Task<IEnumerable<string>> RemovePermissions(IEnumerable<string> userIdentifiers)
        {
            return await ExecuteConsoleCommandBatch(userIdentifiers, obj => $"cp remove {obj}");
        }

        public async Task<IEnumerable<PermissionEntry>> GetPermissions()
        {
            return await Task.Run(() =>
            {
                var result = new List<PermissionEntry>();

                foreach (var item in GameManager.Instance.adminTools.GetCommands().Values)
                {
                    result.Add(new PermissionEntry()
                    {
                        Command = item.Command,
                        Level = item.PermissionLevel
                    });
                }

                return result;
            });
        }
        #endregion

        #region Whitelist
        public async Task<IEnumerable<string>> AddWhitelist(IEnumerable<WhitelistEntry> whitelist)
        {
            return await ExecuteConsoleCommandBatch(whitelist, obj => $"whitelist add {obj.UserIdentifier} {obj.DisplayName}");
        }

        public async Task<IEnumerable<string>> RemoveWhitelist(IEnumerable<string> userIdentifiers)
        {
            return await ExecuteConsoleCommandBatch(userIdentifiers, obj => $"whitelist remove {obj}");
        }

        public async Task<IEnumerable<WhitelistEntry>> GetWhitelist()
        {
            return await Task.Run(() =>
            {
                var result = new List<WhitelistEntry>();

                foreach (var item in GameManager.Instance.adminTools.GetWhitelistedUsers().Values)
                {
                    result.Add(new WhitelistEntry()
                    {
                        UserIdentifier = item.UserIdentifier.CombinedString,
                        DisplayName = item.Name
                    });
                }

                return result;
            });
        }
        #endregion

        #region Blacklist
        public async Task<IEnumerable<string>> AddBlacklist(IEnumerable<BlacklistEntry> blacklist)
        {
            return await ExecuteConsoleCommandBatch(blacklist, obj =>
            {
                return $"ban add {obj.UserIdentifier} {(int)(DateTime.Now - obj.BannedUntil).TotalMinutes} minutes {obj.Reason} {obj.DisplayName}";
            });
        }

        public async Task<IEnumerable<string>> RemoveBlacklist(IEnumerable<string> userIdentifiers)
        {
            return await ExecuteConsoleCommandBatch(userIdentifiers, obj => $"ban remove {obj}");
        }

        public async Task<IEnumerable<BlacklistEntry>> GetBlacklist()
        {
            return await Task.Run(() =>
            {
                var result = new List<BlacklistEntry>();

                foreach (var item in GameManager.Instance.adminTools.GetBanned())
                {
                    result.Add(new BlacklistEntry()
                    {
                        UserIdentifier = item.UserIdentifier.CombinedString,
                        BannedUntil = item.BannedUntil,
                        Reason = item.BanReason,
                        DisplayName = item.Name
                    });
                }

                return result;
            });
        }
        #endregion

        public async Task<IEnumerable<string>> SendGlobalMessage(GlobalMessage globalMessage)
        {
            return await ExecuteConsoleCommand($"ty-say {globalMessage.Message} {globalMessage.SenderName ?? ExportedConstants.DefaultServerName}");
        }

        public async Task<IEnumerable<string>> SendPrivateMessage(PrivateMessage privateMessage)
        {
            return await ExecuteConsoleCommand($"ty-pm {privateMessage.TargetPlayerIdOrName} {privateMessage.Message} {privateMessage.SenderName ?? ExportedConstants.DefaultServerName}");
        }

        public async Task<IEnumerable<string>> TeleportPlayer(TeleportEntry teleportEntry)
        {
            return await ExecuteConsoleCommand($"tele {teleportEntry.OriginPlayerIdOrName} {teleportEntry.TargetPlayerIdOrNameOrPosition}");
        }

        public async Task<IEnumerable<string>> GiveItem(GiveItemEntry giveItemEntry)
        {
            return await ExecuteConsoleCommand($"ty-give {giveItemEntry.ItemName} {giveItemEntry.Count ?? 1} {giveItemEntry.Quality ?? 1} {giveItemEntry.Durability ?? 100}");
        }

        public async Task<IEnumerable<string>> SpawnEntity(SpawnEntityEntry spawnEntityEntry)
        {
            return await ExecuteConsoleCommand($"se {spawnEntityEntry.TargetPlayerIdOrName} {spawnEntityEntry.TargetPlayerIdOrName}");
        }

        public async Task<IEnumerable<AllowSpawnedEntity>> GetAllowSpawnedEntities()
        {
            return await Task.Run(() =>
            {
                var result = new List<AllowSpawnedEntity>();
                int num = 1;
                foreach (var item in EntityClass.list.Dict.Values)
                {
                    if (item.bAllowUserInstantiate)
                    {
                        result.Add(new AllowSpawnedEntity()
                        {
                            Id = num,
                            Name = item.entityClassName
                        });

                        ++num;
                    }
                }

                return result;
            });
        }

        public async Task<IEnumerable<AllowedCommandEntry>> GetAllowedCommands()
        {
            return await Task.Run(() =>
            {
                var result = new List<AllowedCommandEntry>();
                foreach (var consoleCommand in SdtdConsole.Instance.GetCommands())
                {
                    var commands = consoleCommand.GetCommands();
                    int commandPermissionLevel = GameManager.Instance.adminTools.GetCommandPermissionLevel(commands);

                    result.Add(new AllowedCommandEntry()
                    {
                        Commands = commands,
                        PermissionLevel = commandPermissionLevel,
                        Description = consoleCommand.GetDescription(),
                        Help = consoleCommand.GetHelp(),
                    });
                }

                return result;
            });
        }

        public async Task<Stats> GetStats()
        {
            return await Task.Run(() =>
            {
                var worldTime = GameManager.Instance.World.worldTime;
                var entityList = GameManager.Instance.World.Entities.list;

                int hostiles = 0;
                int animals = 0;
                foreach (var entity in entityList)
                {
                    if (entity.IsAlive())
                    {
                        if (entity is EntityEnemy)
                        {
                            ++hostiles;
                        }

                        if (entity is EntityAnimal)
                        {
                            ++animals;
                        }
                    }
                }

                return new Stats()
                {
                    GameTime = new GameTime()
                    {
                        Days = GameUtils.WorldTimeToDays(worldTime),
                        Hours = GameUtils.WorldTimeToHours(worldTime),
                        Minutes = GameUtils.WorldTimeToMinutes(worldTime),
                    },
                    Players = GameManager.Instance.World.Players.Count,
                    Hostiles = hostiles,
                    Animals = animals,
                };
            });
        }

        public async Task<IEnumerable<string>> RestartServer(bool force = false)
        {
            string cmd = "ty-rs";
            if (force)
            {
                cmd += " -f";
            }

            return await ExecuteConsoleCommand(cmd);
        }

        public async Task<IEnumerable<EntityLocation>> GetAnimalsLocation()
        {
            return await Task.Run(() =>
            {
                var entityLocations = new List<EntityLocation>();
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityAnimal entityAnimal && entity.IsAlive())
                    {
                        entityLocations.Add(new EntityLocation()
                        {
                            Id = entityAnimal.entityId,
                            Name = entityAnimal.EntityName ?? ("animal class #" + entityAnimal.entityClass),
                            Position = entityAnimal.GetPosition().ToPosition(),
                        });
                    }
                }

                return entityLocations;
            });
        }

        public async Task<IEnumerable<EntityLocation>> GetHostileLocation()
        {
            return await Task.Run(() =>
            {
                var entityLocations = new List<EntityLocation>();
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityEnemy entityEnemy && entity.IsAlive())
                    {
                        entityLocations.Add(new EntityLocation()
                        {
                            Id = entityEnemy.entityId,
                            Name = entityEnemy.EntityName ?? ("enemy class #" + entityEnemy.entityClass),
                            Position = entityEnemy.GetPosition().ToPosition(),
                        });
                    }
                }

                return entityLocations;
            });
        }

        public async Task<IEnumerable<PlayerLocation>> GetPlayersLocation()
        {
            return await Task.Run(() =>
            {
                var entityLocations = new List<PlayerLocation>();
                foreach (var players in GameManager.Instance.World.Players.dict.Values)
                {
                    entityLocations.Add(new PlayerLocation()
                    {
                        EntityId = players.entityId,
                        Name = players.EntityName,
                        Position = players.GetPosition().ToPosition(),
                    });
                }

                return entityLocations;
            });
        }

        public async Task<LandClaims> GetLandClaims()
        {
            return await Task.Run(() =>
            {
                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                var claimOwners = new Dictionary<int, ClaimOwner>();

                foreach (var item in persistentPlayerList.m_lpBlockMap)
                {
                    int entityId = item.Value.EntityId;
                    if (claimOwners.ContainsKey(entityId))
                    {
                        ((List<Position>)claimOwners[entityId].Claims).Add(item.Key.ToPosition());
                    }
                    else
                    {
                        bool claimActive = GameManager.Instance.World.IsLandProtectionValidForPlayer(persistentPlayerList.GetPlayerDataFromEntityID(entityId));
                        claimOwners.TryAdd(entityId, new ClaimOwner()
                        {
                            ClaimActive = claimActive,
                            EntityId = entityId,
                            Claims = new List<Position>() { item.Key.ToPosition() }
                        });
                    }
                }

                int claimsize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
                return new LandClaims()
                {
                    ClaimOwners = claimOwners.Values,
                    Claimsize = claimsize
                };
            });
        }

        public async Task<ClaimOwner> GetLandClaim(int entityId)
        {
            return await Task.Run(() =>
            {
                var claims = new List<Position>();
                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                var claimOwner = new ClaimOwner()
                {
                    ClaimActive = GameManager.Instance.World.IsLandProtectionValidForPlayer(persistentPlayerList.GetPlayerDataFromEntityID(entityId)),
                    EntityId = entityId,
                    Claims = claims
                };

                foreach (var item in persistentPlayerList.m_lpBlockMap)
                {
                    if(entityId == item.Value.EntityId)
                    {
                        claims.Add(item.Key.ToPosition());
                    }
                }

                return claimOwner;
            });
        }

        public async Task<ItemBlockPaged> GetItemBlocks(int pageIndex = 1, int pageSzie = 10)
        {
            return await Task.Run(() =>
            {
                var itemBlocks = new List<ItemBlock>();
                var result = new ItemBlockPaged() 
                {
                    Items = itemBlocks,
                    Total = (uint)ItemClass.list.Length
                };

                var items = pageSzie == -1 ? ItemClass.list : ItemClass.list.Skip((pageIndex - 1) * pageSzie).Take(pageSzie);
                foreach (var item in items)
                {
                    itemBlocks.Add(new ItemBlock()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        IsBlock = item.IsBlock()
                    });
                }

                return result;
            });
        }

        public async Task<IDictionary<string, string>> GetLocalization(string language)
        {
            return await Task.Factory.StartNew((state) =>
            {
                string language = (string)state;
                if (string.IsNullOrEmpty(language))
                {
                    language = "schinese";
                }

                var dict = Localization.dictionary;
                int languageIndex = Array.LastIndexOf(dict["KEY"], language);

                if (languageIndex < 0)
                {
                    throw new Exception($"The specified language: {language} does not exist");
                }

                return dict.ToDictionary(p => p.Key, p => p.Value[languageIndex]);
            }, language);
        }

        public async Task<string> GetLocalization(string itemName, string language)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(itemName))
                {
                    throw new ArgumentNullException(nameof(itemName));
                }

                if (string.IsNullOrEmpty(language))
                {
                    language = "schinese";
                }

                var dict = Localization.dictionary;
                int languageIndex = Array.LastIndexOf(dict["KEY"], language);

                if (languageIndex < 0)
                {
                    throw new Exception($"The specified language: {language} does not exist");
                }

                if (dict.ContainsKey(itemName) == false)
                {
                    throw new Exception($"The specified itemName: {itemName} does not exist");
                }

                return dict[itemName][languageIndex];
            });
        }

        public async Task<IEnumerable<string>> GetKnownLanguages()
        {
            return await Task.Run(() =>
            {
                return Localization.dictionary["KEY"];
            });
        }
    }
}
