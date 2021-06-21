using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon;
using Eleon.Modding;
using UnityEngine;

namespace MissionMod
{
    public class MyEmpyrionMod : ModInterface, IMod
    {
        public static string ModVersion = "MissionControl v0.0.1";
        public static string ModPath = "..\\Content\\Mods\\MissionControl\\";
        public static string ModShortName = "MissionControl";
        internal static bool debug = true;
        internal static Dictionary<int, Storage.StorableData> SeqNrStorage = new Dictionary<int, Storage.StorableData> { };
        public int thisSeqNr = 2000;
        internal static SetupYaml.Root SetupYamlData = new SetupYaml.Root { };
        public ItemStack[] blankItemStack = new ItemStack[] { };

        internal static IModApi modApi;
        public static string ProcessName = "DefaultName";


        //########################################################################################################################################################

        //########################################################################################################################################################
        //################################################ This is where the actual Empyrion Modding API stuff Begins ############################################
        //########################################################################################################################################################
        public void Game_Start(ModGameAPI gameAPI)
        {
            Storage.DediAPI = gameAPI;
            //if (debug) { File.WriteAllText(ModPath + "ERROR.txt", ""); }
            //if (debug) { File.WriteAllText(ModPath + "debug.txt", ""); }
            //SetupYamlData = SetupYaml.Setup();
            //CommonFunctions.Log("--------------------" + " Server Start " + CommonFunctions.TimeStamp() + "----------------------------");
        }

        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_ChatMessage:
                        //Triggered when player says something in-game
                        ChatInfo Received_ChatInfo = (ChatInfo)data;
                        string msg = Received_ChatInfo.msg.ToLower();
                        if (msg == SetupYamlData.General.ReinitializeCommand) //Reinitialize
                        {
                            SetupYaml.Setup();
                        }
                        else if (msg == "/mods")
                        {
                            API.Chat("Player", Received_ChatInfo.playerId, ModVersion);
                        }
                        else if (msg == "!mods")
                        {
                            API.Chat("Player", Received_ChatInfo.playerId, ModVersion);
                        }
                        else if (msg == SetupYamlData.General.ListMissionsCommand.ToLower())
                        {
                            try
                            {
                                Storage.StorableData function = new Storage.StorableData
                                {
                                    function = "MarketPlace",
                                    Match = Convert.ToString(Received_ChatInfo.playerId),
                                    Requested = "PlayerInfo",
                                    ChatInfo = Received_ChatInfo
                                };
                                API.PlayerInfo(Received_ChatInfo.playerId, function);
                            }
                            catch
                            {
                                CommonFunctions.DebugAPI2(ProcessName, "Fail: Marketplace at Chat");
                            }
                        }
                        break;


                    case CmdId.Event_Player_Connected:
                        //Triggered when a player logs on
                        Id Received_PlayerConnected = (Id)data;
                        break;


                    case CmdId.Event_Player_Disconnected:
                        //Triggered when a player logs off
                        Id Received_PlayerDisconnected = (Id)data;
                        break;


                    case CmdId.Event_Player_ChangedPlayfield:
                        //Triggered when a player changes playfield
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ChangePlayfield, (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [PlayerID], [Playfield Name], [PVector3 position], [PVector3 Rotation] ));
                        IdPlayfield Received_PlayerChangedPlayfield = (IdPlayfield)data;
                        break;


                    case CmdId.Event_Playfield_Loaded:
                        //Triggered when a player goes to a playfield that isnt currently loaded in memory
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Load_Playfield, (ushort)CurrentSeqNr, new PlayfieldLoad( [float nSecs], [string nPlayfield], [int nProcessId] ));
                        PlayfieldLoad Received_PlayfieldLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Playfield_Unloaded:
                        //Triggered when there are no players left in a playfield
                        PlayfieldLoad Received_PlayfieldUnLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Faction_Changed:
                        //Triggered when an Entity (player too?) changes faction
                        FactionChangeInfo Received_FactionChange = (FactionChangeInfo)data;
                        break;


                    case CmdId.Event_Statistics:
                        //Triggered on various game events like: Player Death, Entity Power on/off, Remove/Add Core
                        StatisticsParam Received_EventStatistics = (StatisticsParam)data;
                        break;


                    case CmdId.Event_Player_DisconnectedWaiting:
                        //Triggered When a player is having trouble logging into the server
                        Id Received_PlayerDisconnectedWaiting = (Id)data;
                        break;


                    case CmdId.Event_TraderNPCItemSold:
                        //Triggered when a player buys an item from a trader
                        TraderNPCItemSoldInfo Received_TraderNPCItemSold = (TraderNPCItemSoldInfo)data;
                        break;


                    case CmdId.Event_Player_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CurrentSeqNr, null));
                        IdList Received_PlayerList = (IdList)data;
                        break;


                    case CmdId.Event_Player_Info:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        PlayerInfo Received_PlayerInfo = (PlayerInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            Storage.StorableData RetrievedData = SeqNrStorage[seqNr];
                            if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "MarketPlace" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                RetrievedData.function = "MarketPlace";
                                RetrievedData.Match = Convert.ToString(Received_PlayerInfo.entityId);
                                RetrievedData.Requested = "ItemExchange";
                                RetrievedData.TriggerPlayer = Received_PlayerInfo;
                                //thisSeqNr = API.OpenItemExchange(Received_PlayerInfo.entityId, SetupYamlData.Name, "Insert Items to sell. Note: The items we buy are subject to change without notice.", "Close", blankItemStack);
                                SeqNrStorage[thisSeqNr] = RetrievedData;
                            }
                        }
                        break;


                    case CmdId.Event_Player_Inventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerInventory = (Inventory)data;
                        break;


                    case CmdId.Event_Player_ItemExchange:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CurrentSeqNr, new ItemExchangeInfo( [id], [title], [description], [buttontext], [ItemStack[]] ));
                        ItemExchangeInfo Received_ItemExchangeInfo = (ItemExchangeInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            Storage.StorableData RetrievedData = SeqNrStorage[seqNr];
                            if (RetrievedData.Requested == "ItemExchange" && RetrievedData.function == "MarketPlace" && Convert.ToString(Received_ItemExchangeInfo.id) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                            }
                            else if (RetrievedData.Requested == "ItemExchange" && RetrievedData.function == "ReturnItems" && Convert.ToString(Received_ItemExchangeInfo.id) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                //thisSeqNr = API.OpenItemExchange(RetrievedData.TriggerPlayer.entityId, "Marketplace", "Take them back or they will be deleted.", "Close", Received_ItemExchangeInfo.items);
                            }
                        }

                        break;


                    case CmdId.Event_DialogButtonIndex:
                        //All of This is a Guess
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Save/Pos = 0, Close/Cancel/Neg = 1
                        IdAndIntValue Received_DialogButtonIndex = (IdAndIntValue)data;
                        break;


                    case CmdId.Event_Player_Credits:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Credits, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        IdCredits Received_PlayerCredits = (IdCredits)data;
                        break;


                    case CmdId.Event_Player_GetAndRemoveInventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetAndRemoveInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerGetRemoveInventory = (Inventory)data;
                        break;


                    case CmdId.Event_Playfield_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_List, (ushort)CurrentSeqNr, null));
                        PlayfieldList Received_PlayfieldList = (PlayfieldList)data;
                        break;


                    case CmdId.Event_Playfield_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Stats, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldStats Received_PlayfieldStats = (PlayfieldStats)data;
                        break;


                    case CmdId.Event_Playfield_Entity_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldEntityList Received_PlayfieldEntityList = (PlayfieldEntityList)data;
                        break;


                    case CmdId.Event_Dedi_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Dedi_Stats, (ushort)CurrentSeqNr, null));
                        DediStats Received_DediStats = (DediStats)data;
                        //Received_DediStats.players
                        break;


                    case CmdId.Event_GlobalStructure_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, (ushort)CurrentSeqNr, null));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        GlobalStructureList Received_GlobalStructureList = (GlobalStructureList)data;
                        break;


                    case CmdId.Event_Entity_PosAndRot:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdPositionRotation Received_EntityPosRot = (IdPositionRotation)data;
                        break;


                    case CmdId.Event_Get_Factions:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CurrentSeqNr, new Id( [int] )); //Requests all factions from a certain Id onwards. If you want all factions use Id 1.
                        FactionInfoList Received_FactionInfoList = (FactionInfoList)data; // List<FactionInfo>
                        break;


                    case CmdId.Event_NewEntityId:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_NewEntityId, (ushort)CurrentSeqNr, null));
                        Id Request_NewEntityId = (Id)data;
                        break;


                    case CmdId.Event_Structure_BlockStatistics:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdStructureBlockInfo Received_StructureBlockStatistics = (IdStructureBlockInfo)data;
                        break;


                    case CmdId.Event_AlliancesAll:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesAll, (ushort)CurrentSeqNr, null));
                        AlliancesTable Received_AlliancesAll = (AlliancesTable)data;
                        break;


                    case CmdId.Event_AlliancesFaction:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesFaction, (ushort)CurrentSeqNr, new AlliancesFaction( [int nFaction1Id], [int nFaction2Id], [bool nIsAllied] ));
                        AlliancesFaction Received_AlliancesFaction = (AlliancesFaction)data;
                        break;


                    case CmdId.Event_BannedPlayers:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GetBannedPlayers, (ushort)CurrentSeqNr, null ));
                        BannedPlayerData Received_BannedPlayers = (BannedPlayerData)data;
                        break;


                    case CmdId.Event_GameEvent:
                        //Triggered by PDA Events
                        GameEventData Received_GameEvent = (GameEventData)data;
                        break;


                    case CmdId.Event_Ok:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetInventory, (ushort)CurrentSeqNr, new Inventory(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddItem, (ushort)CurrentSeqNr, new IdItemStack(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [+/- Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Finish, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Resources, (ushort)CurrentSeqNr, new BlueprintResources( [PlayerID], [List<ItemStack>], [bool ReplaceExisting?] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Teleport, (ushort)CurrentSeqNr, new IdPositionRotation( [EntityId OR PlayerID], [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_ChangePlayfield , (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [EntityId OR PlayerID], [Playfield],  [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy2, (ushort)CurrentSeqNr, new IdPlayfield( [EntityID], [Playfield] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_SetName, (ushort)CurrentSeqNr, new Id( [EntityID] )); Wait, what? This one doesn't make sense. This is what the Wiki says though.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Spawn, (ushort)CurrentSeqNr, new EntitySpawnInfo()); Doesn't make sense to me.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_Touch, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_Faction, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CurrentSeqNr, new PString( [Telnet Command] ));

                        //uh? Not Listed in Wiki... Received_ = ()data;
                        break;


                    case CmdId.Event_Error:
                        //Triggered when there is an error coming from the API
                        ErrorInfo Received_ErrorInfo = (ErrorInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            CommonFunctions.LogFile("Debug.txt", "API Error:");
                            CommonFunctions.LogFile("Debug.txt", "ErrorType: " + Received_ErrorInfo.errorType);
                            CommonFunctions.LogFile("Debug.txt", "");
                        }
                        break;


                    case CmdId.Event_PdaStateChange:
                        //Triggered by PDA: chapter activated/deactivated/completed
                        PdaStateInfo Received_PdaStateChange = (PdaStateInfo)data;
                        break;


                    case CmdId.Event_ConsoleCommand:
                        //Triggered when a player uses a Console Command in-game
                        ConsoleCommandInfo Received_ConsoleCommandInfo = (ConsoleCommandInfo)data;
                        break;


                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CommonFunctions.LogFile("ERROR.txt", "Message: " + ex.Message);
                CommonFunctions.LogFile("ERROR.txt", "Data: " + ex.Data);
                CommonFunctions.LogFile("ERROR.txt", "HelpLink: " + ex.HelpLink);
                CommonFunctions.LogFile("ERROR.txt", "InnerException: " + ex.InnerException);
                CommonFunctions.LogFile("ERROR.txt", "Source: " + ex.Source);
                CommonFunctions.LogFile("ERROR.txt", "StackTrace: " + ex.StackTrace);
                CommonFunctions.LogFile("ERROR.txt", "TargetSite: " + ex.TargetSite);
                CommonFunctions.LogFile("ERROR.txt", "");
            }
        }
        public void Game_Update()
        {
            //Triggered whenever Empyrion experiences "Downtime", roughly 75-100 times per second
        }
        public void Game_Exit()
        {
            //Triggered when the server is Shutting down. Does NOT pause the shutdown.
        }

        public void Init(IModApi modAPI)
        {
            modApi = modAPI;
            try { modAPI.Application.OnPlayfieldLoaded += Application_OnPlayfieldLoaded; } catch { }
            try { modAPI.GameEvent += ModAPI_GameEvent; } catch { }

            //throw new NotImplementedException();
        }

        private void ModAPI_GameEvent(GameEventType type, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            try
            {
                CommonFunctions.LogFile(ProcessName + ".txt", "Type = " + type);
                if (arg1 != null) CommonFunctions.LogFile(ProcessName + ".txt", "arg1 = " + arg1);
                if (arg2 != null) CommonFunctions.LogFile(ProcessName + ".txt", "arg2 = " + arg2);
                if (arg3 != null) CommonFunctions.LogFile(ProcessName + ".txt", "arg3 = " + arg3);
                if (arg4 != null) CommonFunctions.LogFile(ProcessName + ".txt", "arg4 = " + arg4);
                if (arg5 != null) CommonFunctions.LogFile(ProcessName + ".txt", "arg5 = " + arg5);
                CommonFunctions.LogFile(ProcessName + ".txt", "");
            }
            catch { }
        }

        private void Application_OnPlayfieldLoaded(IPlayfield playfield)
        {
            
            if (playfield.Name != null)
            {
                try
                {
                    if (modApi.Application.Mode == ApplicationMode.PlayfieldServer)
                    {
                        try
                        {
                            if (playfield.Name != null)
                            {
                                ProcessName = playfield.Name;
                            }
                            else
                            {
                                ProcessName = "NoName";
                            }
                        }
                        catch
                        {
                            ProcessName = "Debug";
                            CommonFunctions.LogFile(ProcessName + ".txt", "Error, Unable to retrieve Playfield name. Name not set");
                        }
                    }
                    else if (modApi.Application.Mode == ApplicationMode.DedicatedServer)
                    {
                        ProcessName = "Dedicated";
                    }
                    else if (modApi.Application.Mode == ApplicationMode.Client)
                    {
                        ProcessName = "Client";
                    }
                    else if (modApi.Application.Mode == ApplicationMode.SinglePlayer)
                    {
                        ProcessName = "SinglePlayer";
                    }
                    else
                    {
                        ProcessName = "Other";
                    }
                }
                catch
                {
                    CommonFunctions.LogFile(ProcessName + ".txt", "Failed Getting Playfield name on playfield start");
                }

                try
                {
                    foreach (IEntity entity in playfield.Entities.Values)
                    {
                        Playfield_OnEntityLoaded(entity);
                        //CommonFunctions.LogFile(Name + ".txt", "Entity Reading " + entity.Name);
                    }
                }
                catch
                {
                    CommonFunctions.LogFile(ProcessName + ".txt", "ERROR: Entity Reading ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
                }

                try
                {
                    if (modApi.Application.Mode == ApplicationMode.PlayfieldServer)
                    {
                        playfield.OnEntityLoaded += Playfield_OnEntityLoaded;
                    }
                }
                catch
                {
                }
            }
        }

        private void Playfield_OnEntityLoaded(IEntity entity)
        {
            try
            {
                if (entity.Type == EntityType.CV || entity.Type == EntityType.HV || entity.Type == EntityType.SV || entity.Type == EntityType.BA)
                {
                    //CommonFunctions.LogFile(Name + ".txt", "EntityLoading = " + entity.Name);
                    //IDevicePosList DevPos = entity.Structure.GetDevices(DeviceTypeName.Container);
                    //CommonFunctions.DebugAPI2(Name, "DeviceCount Containers = " + DevPos.Count);
                    //entity.Structure.GetBlock();

                    string[] Devices = entity.Structure.GetAllCustomDeviceNames();
                    foreach (string Device in Devices)
                    {
                        List<VectorInt3> DevicePositions = entity.Structure.GetDevicePositions(Device);
                        foreach (VectorInt3 pos in DevicePositions)
                        {
                            try
                            {
                                IBlock Block = entity.Structure.GetBlock(pos);
                                
                                CommonFunctions.DebugAPI2(ProcessName, "Block=" + Block.CustomName + "   Hitpoints =" + Block.GetHitPoints() + "   Damage=" + Block.GetDamage());
                                
                                //IBlock ThisDevice = entity.Structure.GetDevice<IBlock>(pos.x, pos.y, pos.z);
                                //ThisDevice.TargetData .SetText("Ticker = " + Convert.ToString(modApi.Application.GameTicks));
                                //ThisDevice.SetBackgroundColor(Color.green);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
                CommonFunctions.LogFile(ProcessName + ".txt", "Failed in Playfield_OnEntityLoaded trying to Do things to devices");
            }
            entity.DamageEntity(25, 1);
            if (entity.IsPoi && !entity.IsProxy)
            {
                CommonFunctions.DebugAPI2(ProcessName, "EntityName = " + entity.Name);
                /*
                for (int x = 0; x < 200; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        for (int z = 0; z < 200; z++)
                        {
                            try
                            {
                                //IBlock block = entity.Structure.GetBlock(x, y, z);
                                //CommonFunctions.DebugAPI2(ProcessName, "hp=" + block.GetHitPoints() + "   Dam=" + block.GetDamage());
                                //block.SetDamage(25);
                                //CommonFunctions.DebugAPI2(ProcessName, "Did blocks get damaged?");
                            }
                            catch { }
                        }
                    }
                }*/
            }
        }

        public void Shutdown()
        {
            //throw new NotImplementedException();
        }
    }
}
