using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Sandbox.Common;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRageMath;
using VRage.ObjectBuilders;
using Sandbox.Definitions;
using VRage;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game.Components;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Territories
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class PirateSpawner : MySessionComponentBase
    {
        bool FirstCall = true;
        //long MaxCount = 800;
        long MaxCount = 288000;

        //ulong CleanTimer = 400;
        ulong CleanTimer = 72000;
        Dictionary<long, long> Counts = new Dictionary<long, long>();
        public static bool EnableTrash = false;
        long droneCount = 0;
        /*public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return this.GetObjectBuilder();
        }*/

        public override void UpdatingStopped()
        {
            base.UpdatingStopped();
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                return;
            }
            TrashCleaner.Save();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if(MyAPIGateway.Session == null)
            {
                return;
            }
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                if (FirstCall)
                {
                    AddMessageHandler();
                }
                return;
            }
            if(FirstCall)
            {
                MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
                MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
                LoadVariables();
            }
            if(EnableTrash)
                TrashCleaner.exec();
            var Factions = MyAPIGateway.Session.Factions.GetObjectBuilder();
            foreach(MyObjectBuilder_Faction faction in Factions.Factions)
            {
                if(!faction.AcceptHumans)
                {
                    continue;
                }
                if(!Counts.ContainsKey(faction.FactionId))
                {
                    Counts.Add(faction.FactionId, 0);
                }
                var newCount = calcPirateSpawn(faction, Counts[faction.FactionId], MaxCount, FirstCall);
                Counts[faction.FactionId] = newCount;
            }
            FirstCall = false;
        }

        void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            if(!MyAPIGateway.Session.Player.IsAdmin)
            {
                return;
            }
            if(messageText.Equals("/random_pirates trash on"))
            {
                sendToOthers = false;
                EnableTrash = true;
                MyAPIGateway.Utilities.SetVariable<bool>("Random_Pirates_Trash_Enable", true);
                MyAPIGateway.Utilities.ShowNotification("Random Pirates: Trash Enabled");
            }
            if (messageText.Equals("/random_pirates trash off"))
            {
                sendToOthers = false;
                EnableTrash = false;
                MyAPIGateway.Utilities.SetVariable<bool>("Random_Pirates_Trash_Enable", false);
                MyAPIGateway.Utilities.ShowNotification("Random Pirates: Trash Disabled");
            }
            if(messageText.StartsWith("/random_pirates time "))
            {
                sendToOthers = false;
                string numText = messageText.Replace("/random_pirates time ", "");
                long timer = 3600;
                if(long.TryParse(numText, out timer))
                {
                    MyAPIGateway.Utilities.ShowNotification("Random Pirates: Spawn Interval set to: "+timer+" seconds");
                    timer = timer * 1000;//Convert to ms
                    timer = (long)(timer / 12.5);//Convert to Ticks
                    MaxCount = timer;
                    if (MaxCount < 400)
                    {
                        MaxCount = 400;
                    }
                    MyAPIGateway.Utilities.SetVariable<long>("Random_Pirates_Timer", MaxCount);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("Random Pirates: Invalid Spawn Interval. Specify the spawn interval in seconds.");
                }
            }
            if (messageText.StartsWith("/random_pirates cleaner "))
            {
                sendToOthers = false;
                string numText = messageText.Replace("/random_pirates cleaner ", "");
                ulong timer = 900;
                if (ulong.TryParse(numText, out timer))
                {
                    MyAPIGateway.Utilities.ShowNotification("Random Pirates: Cleaner Interval set to: "+timer+" seconds");
                    timer = timer * 1000;//Convert to ms
                    timer = (ulong)(timer / 12.5);//Convert to Ticks
                    CleanTimer = timer;
                    if (CleanTimer < 400)
                    {
                        CleanTimer = 400;
                    }
                    MyAPIGateway.Utilities.SetVariable<ulong>("Random_Pirates_Cleaner", CleanTimer);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("Random Pirates: Invalid Cleaner Interval");
                }
            }
            if (messageText.StartsWith("/random_pirates help"))
            {
                sendToOthers = false;
                MyAPIGateway.Utilities.ShowNotification("Random Pirates: Here's the list of possible commands:", 12000);
                MyAPIGateway.Utilities.ShowNotification("/random_pirates trash <on/off> (Turn automatic deletion on or off)", 12000);
                MyAPIGateway.Utilities.ShowNotification("/random_pirates time <seconds> (Specify the ammount of seconds in between spawns)", 12000);
                MyAPIGateway.Utilities.ShowNotification("/random_pirates cleaner <seconds> (Specify how long pirates will last before being deleted in seconds)", 12000);
            }
        }

        public override void SaveData()
        {
            base.SaveData();
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                return;
            }
            TrashCleaner.Save();
        }


        public void LoadVariables()
        {
            if (!MyAPIGateway.Utilities.GetVariable<bool>("Random_Pirates_Trash_Enable", out EnableTrash))
            {
                EnableTrash = false;
                MyAPIGateway.Utilities.SetVariable<bool>("Random_Pirates_Trash_Enable", false);
            }
            if (!MyAPIGateway.Utilities.GetVariable<long>("Random_Pirates_Timer", out MaxCount))
            {
                MaxCount = 288000;
                MyAPIGateway.Utilities.SetVariable<long>("Random_Pirates_Timer", 288000);
            }
            if(MaxCount<400)
            {
                MaxCount = 400;
            }
            if (!MyAPIGateway.Utilities.GetVariable<ulong>("Random_Pirates_Cleaner", out CleanTimer))
            {
                CleanTimer = 72000;
                MyAPIGateway.Utilities.SetVariable<ulong>("Random_Pirates_Cleaner", 72000);
            }
            if (CleanTimer < 400)
            {
                CleanTimer = 400;
            }
            TrashCleaner.Load();
        }
        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
            RemoveMessageHandler();
        }
        public void AddMessageHandler()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(5000, HandleMessageData);
        }

        public void RemoveMessageHandler()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(5000, HandleMessageData);
        }
        public void HandleMessageData(byte [] data)
        {
            char [] chars = new char[data.Length];
            for (int i = 0; i < data.Length; ++i )
            {
                chars[i] = (char)data[i];
            }
            string message = new string(chars);
            MyAPIGateway.Utilities.ShowNotification(message, 6000);
            
        }
        public void SendMessageToClient(string message,ulong steamid)
        {
            if(steamid == MyAPIGateway.Session.LocalHumanPlayer.SteamUserId)
            {
                MyAPIGateway.Utilities.ShowNotification(message, 6000);
                return;
            }
            char [] chars = message.ToCharArray();
            byte [] data = new byte[chars.Length];
            for (int i = 0; i < chars.Length; ++i )
            {
                data[i] = (byte)chars[i];
            }
            
            MyAPIGateway.Multiplayer.SendMessageTo(5000,data,steamid);
        }
        public long calcPirateSpawn(MyObjectBuilder_Faction Faction, long count, long maxCount, bool firstcall)
        {
            if (firstcall)
            {
                if (!MyAPIGateway.Utilities.GetVariable<long>("Random_Pirates_" + Faction.FactionId, out count))
                {
                    count = 0;
                    MyAPIGateway.Utilities.SetVariable<long>("Random_Pirates_" + Faction.FactionId, count);
                }
            }
            if (count < maxCount)
            {
                ++count;
                MyAPIGateway.Utilities.SetVariable<long>("Random_Pirates_" + Faction.FactionId, count);
                return count;
            }
            Random r = new Random();
            var Members = Faction.Members;
            if (Members.Count == 0)
                return 0;
            var indx = r.Next(0,Members.Count);
            var Member = Members[indx];
            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);
            IMyIdentity LivingIdentity = null;
            var iter = 0;
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            IMyPlayer Player = null;
            
            while(iter < Members.Count && Player == null)
            {
                Member = Members[(indx+iter)%Members.Count];
                ++iter;
                LivingIdentity = identities.FirstOrDefault(e => !e.IsDead && e.PlayerId == Member.PlayerId);
                
                if (LivingIdentity == null)
                    continue;
                Player = (IMyPlayer)players.FirstOrDefault(e => e.PlayerID == LivingIdentity.PlayerId);
            }
            if (Player == null)
            {
                return 0;
            }
            var Playerpos = Player.GetPosition();
            var Planet = GetClosestPlanet(Playerpos);

            if(Planet == null || Vector3D.Distance(Planet.PositionComp.GetPosition(),Playerpos)>Planet.AtmosphereRadius)
            {
                SpawnSpace(Playerpos, Members, players);
            }
            else if(Planet.HasAtmosphere)
            {
                SpawnAtmosphere(Playerpos, Planet.PositionComp.GetPosition(), Members, players);
            }
            else
            {
                //Moon spawning was always busted
                //SpawnMoon(Playerpos, Planet.PositionComp.GetPosition(), Members, players);
            }
            return 0;
        }


        public void SpawnAtmosphere(Vector3D Playerpos, Vector3D Planetpos, List<MyObjectBuilder_FactionMember> Members, List<IMyPlayer> players)
        {
            double level = calcPirateLevel(Playerpos);
            List<string> prefabs = PrefabStrings.GetInstance().SelectPrefabs(DIFFICULTY.EASY, AREA.PLANET, level);
            if (prefabs == null)
                return;
            foreach (string prefabName in prefabs)
            {
                Random r = new Random();
                var difference = Playerpos - Planetpos;
                //Vector3D Rand = new Vector3D(r.NextDouble()/4.0+(r.NextDouble()/2-.25),r.NextDouble()/4.0+(r.NextDouble()/2-.25),r.NextDouble()/4.0+(r.NextDouble()/2-.25));
                //difference = difference + Rand;
                difference.Normalize();
                var normDifference = difference;
                difference = difference * r.Next(4000, 5000);
                var spawnPos = Playerpos + difference;
                List<IMyCubeGrid> prefabsGrid = new List<IMyCubeGrid>();
                var Up = (Playerpos - Planetpos);
                Up.Normalize();
                Vector3D Forward = Vector3D.Zero;
                Up.CalculatePerpendicularVector(out Forward);
                spawnPos = spawnPos + Forward.Normalize() * r.Next(1500, 2500) * (r.Next(0, 2) == 1 ? 1 : -1);

                Vector3 lastOutsidePos = Vector3.Zero;

                if (MyAPIGateway.Entities.IsInsideVoxel(spawnPos, spawnPos, out lastOutsidePos))
                {
                    var freeplace = MyAPIGateway.Entities.FindFreePlace(spawnPos, 1000);
                    if (freeplace.HasValue)
                    {
                        spawnPos = freeplace.Value;
                    }
                    else
                        return;
                }

                //Loading our prefab
                var prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
                if (prefab.CubeGrids == null)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                    prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
                }

                //Prefab setup
                var tempList = new List<MyObjectBuilder_EntityBase>();
                // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
                foreach (var grid in prefab.CubeGrids)
                {
                    var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                    gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(spawnPos, Forward, Up);

                    tempList.Add(gridBuilder);
                } // you want to iterate here through all the rotor/piston connected parts, or fleet of ships.

                //Get the Pirate NPC
                var identities = new List<IMyIdentity>();
                MyAPIGateway.Players.GetAllIdentites(identities);
                var identity = identities.FirstOrDefault(e => e.DisplayName == "Space Pirates");

                if (identity == null)
                    return;

                //Spawning the prefab
                var entities = new List<IMyEntity>();
                MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
                TrashList tl = new TrashList(TrashCleaner.itterations + CleanTimer);
                foreach (var item in tempList)
                {
                    var entity = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item);
                    if (entity is MyCubeGrid)
                    {
                        var localGrid = (MyCubeGrid)entity;
                        localGrid.ChangeGridOwnership(identity.PlayerId, MyOwnershipShareModeEnum.Faction);
                        localGrid.AddToGamePruningStructure();
                        tl.AddLast(localGrid.EntityId);
                        localGrid.OnGridSplit += (a1, a2) => localGrid_OnGridSplit(a1, a2, tl);
                        entities.Add(localGrid);
                    }
                }
                if (EnableTrash)
                    TrashCleaner.Add(tl);
                MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);
                
                
            }
            for (int i = 0; i < Members.Count; ++i)
            {
                var member = Members[i];
                var player = players.FirstOrDefault(e => e.PlayerID == member.PlayerId);
                if (player == null)
                {
                    continue;
                }
                SendMessageToClient("Enemy Pirates detected in your area! Score: " + ((long)level) + " Tier: " + PrefabStrings.GetInstance().TierMap[Math.Min((uint)Math.Floor(level), PrefabStrings.GetInstance().MAX_SCORE)], player.SteamUserId);
            }
        }

        /*public void SpawnMoon(Vector3D Playerpos, Vector3D Planetpos, List<MyObjectBuilder_FactionMember> Members, List<IMyPlayer> players)
        {
            double level = calcPirateLevel(Playerpos);
            List<string> prefabs = PrefabStrings.GetInstance().SelectPrefabs(DIFFICULTY.EASY, AREA.MOON, level);
            if (prefabs == null)
                return;
            foreach (string prefabName in prefabs)
            {
                Random r = new Random();
                var difference = Playerpos - Planetpos;
                //Vector3D Rand = new Vector3D(r.NextDouble()/4.0+(r.NextDouble()/2-.25),r.NextDouble()/4.0+(r.NextDouble()/2-.25),r.NextDouble()/4.0+(r.NextDouble()/2-.25));
                //difference = difference + Rand;
                difference.Normalize();
                var normDifference = difference;
                difference = difference * r.Next(4000, 5000);
                var spawnPos = Playerpos + difference;
                List<IMyCubeGrid> prefabsGrid = new List<IMyCubeGrid>();
                var Up = (Playerpos - Planetpos);
                Up.Normalize();
                Vector3D Forward = Vector3D.Zero;
                Up.CalculatePerpendicularVector(out Forward);
                spawnPos = spawnPos + Forward.Normalize() * r.Next(1500, 2500) * (r.Next(0, 2) == 1 ? 1 : -1);

                Vector3 lastOutsidePos = Vector3.Zero;

                if (MyAPIGateway.Entities.IsInsideVoxel(spawnPos, spawnPos, out lastOutsidePos))
                {
                    var freeplace = MyAPIGateway.Entities.FindFreePlace(spawnPos, 1000);
                    if (freeplace.HasValue)
                    {
                        spawnPos = freeplace.Value;
                    }
                    else
                        return;
                }

                //Loading our prefab
                var prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
                if (prefab.CubeGrids == null)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                    prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
                }

                //Prefab setup
                var tempList = new List<MyObjectBuilder_EntityBase>();
                // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
                foreach (var grid in prefab.CubeGrids)
                {
                    var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                    gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(spawnPos, Forward, Up);

                    tempList.Add(gridBuilder);
                } // you want to iterate here through all the rotor/piston connected parts, or fleet of ships.

                //Get the Pirate NPC
                var identities = new List<IMyIdentity>();
                MyAPIGateway.Players.GetAllIdentites(identities);
                var identity = identities.FirstOrDefault(e => e.DisplayName == "Space Pirates");

                if (identity == null)
                    return;

                //Spawning the prefab
                var entities = new List<IMyEntity>();
                MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
                TrashList tl = new TrashList(TrashCleaner.itterations + CleanTimer);
                foreach (var item in tempList)
                {
                    var entity = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item);
                    if (entity is MyCubeGrid)
                    {
                        var localGrid = (MyCubeGrid)entity;
                        localGrid.ChangeGridOwnership(identity.PlayerId, MyOwnershipShareModeEnum.Faction);
                        localGrid.AddToGamePruningStructure();
                        tl.AddLast(localGrid.EntityId);
                        localGrid.OnGridSplit += (a1, a2) => localGrid_OnGridSplit(a1, a2, tl);
                        entities.Add(localGrid);
                    }
                }
                if(EnableTrash)
                    TrashCleaner.Add(tl);
                MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);

            }
            for (int i = 0; i < Members.Count; ++i)
            {
                var member = Members[i];
                var player = players.FirstOrDefault(e => e.PlayerID == member.PlayerId);
                if (player == null)
                {
                    continue;
                }
                SendMessageToClient("Enemy Pirates detected in your area! Score: " + ((long)level) + " Tier: " + PrefabStrings.GetInstance().TierMap[Math.Min((uint)Math.Floor(level), PrefabStrings.GetInstance().MAX_SCORE)], player.SteamUserId);
            }
        }*/

        public void SpawnSpace(Vector3D Playerpos, List<MyObjectBuilder_FactionMember> Members, List<IMyPlayer> players)
        {
            double level = calcPirateLevel(Playerpos);
            List<string> prefabs = PrefabStrings.GetInstance().SelectPrefabs(DIFFICULTY.EASY, AREA.SPACE, level);
            if (prefabs == null)
                return;
            foreach (string prefabName in prefabs)
            {
                Random r = new Random();

                Vector3D spawnModifier = new Vector3D(r.Next(3000, 4000) * (r.Next(0, 2) == 1 ? 1.0 : -1.0), r.Next(3000, 4000) * (r.Next(0, 2)) == 1 ? 1.0 : -1.0, r.Next(3000, 4000) * (r.Next(0, 2) == 1 ? 1.0 : -1.0));
                var spawnPos = Playerpos + spawnModifier;
                var Forward = -spawnModifier;
                Vector3D Up = Vector3D.Zero;

                Forward.CalculatePerpendicularVector(out Up);


                Vector3 lastOutsidePos = Vector3.Zero;

                if (MyAPIGateway.Entities.IsInsideVoxel(spawnPos, spawnPos, out lastOutsidePos))
                {
                    var freeplace = MyAPIGateway.Entities.FindFreePlace(spawnPos, 1000);
                    if (freeplace.HasValue)
                    {
                        spawnPos = freeplace.Value;
                    }
                    else
                        return;
                }

                //Loading our prefab
                var prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
                if (prefab.CubeGrids == null)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                    prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
                }

                //Prefab setup
                var tempList = new List<MyObjectBuilder_EntityBase>();
                // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
                foreach (var grid in prefab.CubeGrids)
                {
                    var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                    gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(spawnPos, Forward, Up);

                    tempList.Add(gridBuilder);
                } // you want to iterate here through all the rotor/piston connected parts, or fleet of ships.

                //Get the Pirate NPC
                var identities = new List<IMyIdentity>();
                MyAPIGateway.Players.GetAllIdentites(identities);
                var identity = identities.FirstOrDefault(e => e.DisplayName == "Space Pirates");

                if (identity == null)
                    return;

                //Spawning the prefab
                var entities = new List<IMyEntity>();
                MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
                TrashList tl = new TrashList(TrashCleaner.itterations + CleanTimer);
                foreach (var item in tempList)
                {
                    var entity = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item);
                    if (entity is MyCubeGrid)
                    {
                        var localGrid = (MyCubeGrid)entity;
                        localGrid.ChangeGridOwnership(identity.PlayerId, MyOwnershipShareModeEnum.Faction);
                        localGrid.AddToGamePruningStructure();
                        tl.AddLast(localGrid.EntityId);
                        localGrid.OnGridSplit += (a1,a2) => localGrid_OnGridSplit(a1,a2,tl);
                        localGrid.Name = "Drone #"+droneCount;
                        MyAPIGateway.Entities.SetEntityName(localGrid);
                        MyVisualScriptLogicProvider.SetDroneBehaviourFull("Drone #" + droneCount, maxPlayerDistance:15000f);
                        ++droneCount;
                        entities.Add(localGrid);
                    }
                }
                if (EnableTrash)
                    TrashCleaner.Add(tl);
                MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);
            }
            for (int i = 0; i < Members.Count; ++i)
            {
                var member = Members[i];
                var player = players.FirstOrDefault(e => e.PlayerID == member.PlayerId);
                if (player == null)
                {
                    continue;
                }
                SendMessageToClient("Enemy Pirates detected in your area! Score: " + ((long)level) + " Tier: " + PrefabStrings.GetInstance().TierMap[Math.Min((uint)Math.Floor(level), PrefabStrings.GetInstance().MAX_SCORE)], player.SteamUserId);
            }
        }

        void localGrid_OnGridSplit(MyCubeGrid arg1, MyCubeGrid arg2, TrashList tl)
        {
            tl.AddLast(arg2.EntityId);
            arg2.OnGridSplit += (a1, a2) => localGrid_OnGridSplit(a1, a2,tl);
        }

        public MyPlanet GetClosestPlanet(Vector3D Playerpos)
        {
            HashSet<IMyEntity> Planets = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(Planets, e => e is MyPlanet);
            double minDist = double.MaxValue;
            IMyEntity minPlanet = null;
            foreach(IMyEntity Planet in Planets)
            {
                var Dist = Vector3D.Distance(Planet.GetPosition(),Playerpos);
                if(Dist < minDist)
                {
                    minDist = Dist;
                    minPlanet = Planet;
                }
            }

            if(minPlanet == null|| !(minPlanet is MyPlanet))
            {
                return null;
            }
            return (MyPlanet)minPlanet;
        }

        public double calcPirateLevel(Vector3D PlayerPos)
        {
            BoundingSphereD area = new BoundingSphereD(PlayerPos, 5000);

            List<IMyEntity> NearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref area);

            var grids = NearbyEntities.FindAll(e => e is IMyCubeGrid);
            double AreaScore = 0;
            //double AssemMulti = MyAPIGateway.Session.AssemblerEfficiencyMultiplier + MyAPIGateway.Session.AssemblerSpeedMultiplier;
            //AssemMulti /= 2.0;
            //double RefMulti = MyAPIGateway.Session.RefinerySpeedMultiplier;
            foreach(IMyEntity e in grids)
            {
                var grid = (IMyCubeGrid)e;
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks, (b) => b.FatBlock is Sandbox.ModAPI.IMyRefinery || b.FatBlock is Sandbox.ModAPI.IMyAssembler || b.FatBlock is Sandbox.ModAPI.IMyUserControllableGun);
                foreach(IMySlimBlock slimBlock in blocks)
                {
                    if(slimBlock.FatBlock is Sandbox.ModAPI.IMyRefinery)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 0.3  : 0.15 ; ;
                    }
                    else if(slimBlock.FatBlock is Sandbox.ModAPI.IMyAssembler)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 0.3  : 0.15;
                    }
                    else if (slimBlock.FatBlock is IMyLargeMissileTurret)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 2.2: 1.8;
                    }
                    else if (slimBlock.FatBlock is IMyLargeGatlingTurret)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 2.0 : 1.6; ;
                    }
                    else if (slimBlock.FatBlock is IMyLargeInteriorTurret)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 0.3 : 0.1; ;
                    }
                    else if (slimBlock.FatBlock is IMyLargeTurretBase)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 2.5 : 2;
                    }
                    else if (slimBlock.FatBlock is IMyUserControllableGun)
                    {
                        AreaScore += grid.GridSizeEnum == MyCubeSize.Large ? 0.7 : 0.3;
                    }
                }
            }
            return AreaScore;
        }
    }

    
}
