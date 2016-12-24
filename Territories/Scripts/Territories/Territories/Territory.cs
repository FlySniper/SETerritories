using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.IO;

namespace Territories
{
    public class Territory : ITerritory
    {
        

        HashSet<long> Players = new HashSet<long>();

        public static int gridCalls = 0;

        public int maxGrids = 10;
        public int maxBlocks = 5000;
        public int grids = 0;
        public int health = 5;
        public long owner = MyVisualScriptLogicProvider.GetPirateId();
        public static int radius = 100; // This is in Km
        static long droneCount = 0;

        public Territory(Vector3D center, AREADIFFICULTY difficulty) 
        {
            Center = center;
            this.difficulty = difficulty;
            switch(difficulty)
            {
                case (AREADIFFICULTY.NONE):
                    maxGrids = 0;
                    maxBlocks = 0;
                    health = 3;
                    break;
                case (AREADIFFICULTY.EASY):
                    maxGrids = 4;
                    maxBlocks = 300;
                    health = 3;
                    break;
                case (AREADIFFICULTY.MEDIUM):
                    maxGrids = 7;
                    maxBlocks = 600;
                    health = 5;
                    break;
                case (AREADIFFICULTY.HARD):
                    maxGrids = 10;
                    maxBlocks = 1200;
                    health = 10;
                    break;

            }
        }

        public Territory(Vector3D center, AREADIFFICULTY difficulty, long owner, int health, int grids)
        {
            Center = center;
            this.difficulty = difficulty;
            this.owner = owner;
            this.health = health;
            this.grids = grids;
            switch (difficulty)
            {
                case (AREADIFFICULTY.NONE):
                    maxGrids = 0;
                    maxBlocks = 0;
                    break;
                case (AREADIFFICULTY.EASY):
                    maxGrids = 4;
                    maxBlocks = 500;
                    break;
                case (AREADIFFICULTY.MEDIUM):
                    maxGrids = 7;
                    maxBlocks = 2000;
                    break;
                case (AREADIFFICULTY.HARD):
                    maxGrids = 10;
                    maxBlocks = 5000;
                    break;

            }
        }

        public override void addPlayer(long id)
        {
            Players.Add(id);
        }

        public override void removePlayer(long id)
        {
            Players.Remove(id);
        }

        public override bool SpawnGrid(string prefabName)
        {
            if(gridCalls < 100)
            {
                ++gridCalls;
                return false;
            }
            gridCalls = 0;
            if (grids >= maxGrids || TerritoryManager.totalGrids >= TerritoryManager.totalMaxGrids)
            {
                return false;
            }

            if(!hasEnemies())
            {
                return false;
            }
            if(health <= 0)
            {
                OnHealthZero();
                return false;
            }

            //HashSet<IMyEntity> Planets = new HashSet<IMyEntity>();
            //MyAPIGateway.Entities.GetEntities(Planets, e => e is MyPlanet);
            var prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
            if (prefab.CubeGrids == null)
            {
                MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
            }

            //Prefab setup
            var tempList = new List<MyObjectBuilder_EntityBase>();
            // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
            foreach (var g in prefab.CubeGrids)
            {
                var spawnPos = GenerateWaypoint();

                HashSet<IMyEntity> Planets = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(Planets, e => e is MyPlanet);
                foreach (IMyEntity e in Planets)
                {
                    MyPlanet p = (MyPlanet)e;
                    if (p.AtmosphereRadius >= Vector3D.Distance(spawnPos, p.WorldMatrix.Translation))
                    {
                        return false;
                    }
                }
                var gridBuilder = (MyObjectBuilder_CubeGrid)g.Clone();
                /*foreach(IMyEntity e in Planets)
                {
                    MyPlanet p = (MyPlanet)e;
                    if(p.MaximumRadius >= (spawnPos - e.GetPosition()).Length())
                    {
                        return false;
                    }
                }*/
                var Forward = (spawnPos - Center);
                Forward.Normalize();
                var Up = Vector3D.Zero;
                Forward.CalculatePerpendicularVector(out Up);
                gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(spawnPos, Forward, Up);

                tempList.Add(gridBuilder);
            } // you want to iterate here through all the rotor/piston connected parts, or fleet of ships.
            var entities = new List<IMyEntity>();
            MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
            TrashList tl = new TrashList(TrashCleaner.itterations + PlayerMoveTracker.CleanTimer,this);
            foreach (var item in tempList)
            {
                var entity = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item);
                if (entity is MyCubeGrid)
                {
                    var localGrid = (MyCubeGrid)entity;
                    localGrid.ChangeGridOwnership(owner, MyOwnershipShareModeEnum.Faction);
                    if (localGrid.BlocksCount > maxBlocks)
                    {
                        MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);
                        return false;
                    }
                    localGrid.AddToGamePruningStructure();
                    tl.AddLast(localGrid.EntityId);
                    localGrid.OnGridSplit += (a1, a2) => OnGridSplit(a1, a2, tl);
                    localGrid.OnGridSplit += LocalGrid_OnBlockIntegrityChanged;
                    Random r = new Random();
                    var rand = r.Next(int.MinValue, int.MaxValue);
                    localGrid.Name = "Drone #" + rand;
                    MyAPIGateway.Entities.SetEntityName(localGrid);
                    MyVisualScriptLogicProvider.SetDroneBehaviourFull("Drone #" + rand,maxPlayerDistance:75000,assignToPirates:false,presetName: "C_LongRangeGatlingDrone");

                    ++droneCount;
                    entities.Add(localGrid);
                }
            }
            TrashCleaner.Add(tl);
            MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);
            ++grids;
            ++TerritoryManager.totalGrids;
            return true;
        }

        private void LocalGrid_OnBlockIntegrityChanged(MyCubeGrid arg1, MyCubeGrid arg2)
        {
            if (health > 0)
                --health;
            if (health == 0)
                OnHealthZero();
            arg1.OnGridSplit -= LocalGrid_OnBlockIntegrityChanged;
        }

        private void OnGridSplit(MyCubeGrid arg1, MyCubeGrid arg2, TrashList tl)
        {
            tl.AddLast(arg2.EntityId);
            arg2.OnGridSplit += (a1, a2) => OnGridSplit(a1, a2, tl);
        }


        public Vector3D GenerateWaypoint()
        {
            var rad = radius - 1;
            Random r = new Random();
            var diff1 = r.Next(-rad,rad);
            var diff2 = r.Next(-rad, rad);
            var diff3 = r.Next(-rad, rad);

            Vector3D toAdd = new Vector3D(diff1*1000,diff2 * 1000, diff3 * 1000);
            return Center + toAdd;
        }

        public override void OnHealthZero()
        {
            switch (difficulty)
            {
                case (AREADIFFICULTY.NONE):
                    health = 3;
                    break;
                case (AREADIFFICULTY.EASY):
                    health = 3;
                    break;
                case (AREADIFFICULTY.MEDIUM):
                    health = 5;
                    break;
                case (AREADIFFICULTY.HARD):
                    health = 10;
                    break;

            }
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players,(p) =>Players.Contains(p.IdentityId));
            var tmp = new HashSet<long>();
            foreach(IMyPlayer p in players)
            {
                if(Players.Contains(p.IdentityId))
                {
                    tmp.Add(p.IdentityId);
                }
            }
            Players = tmp;
            Random r = new Random();
            var rand = r.Next(0, Players.Count);
            if (players.Count == 0)
            {
                return;
            }
            var player = players.Find((p) => p.IdentityId == Players.ElementAt(rand));
            if (player == null)
            {
                Players.Remove(owner);
                owner = MyVisualScriptLogicProvider.GetPirateId();
                return;
            }
            foreach (long id in Players)
            {
                var steamid = MyAPIGateway.Players.TryGetSteamId(id);
                PlayerMoveTracker.SendMessageToClient("Territory: "+new Vector3I(Center)+"\nNew Owner: "+ player.DisplayName+"\nDifficulty: "+difficulty,steamid);
            }
        }

        public bool hasEnemies()
        {
            
            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
            if(faction == null)
            {
                return Players.Count > 1;
            }
            foreach(long id in Players)
            {
                var faction2 = MyAPIGateway.Session.Factions.TryGetPlayerFaction(id);
                if(faction2 == null)
                {
                    return true;
                }
                if(MyAPIGateway.Session.Factions.AreFactionsEnemies(faction.FactionId, faction2.FactionId))
                {
                    return true;
                }
            }
            return false;
        }

        public enum AREADIFFICULTY
        {
            NONE,
            EASY,
            MEDIUM,
            HARD
        }
    }

    public class PlanetTerritory : ITerritory
    {
        public override void addPlayer(long id)
        {
            
        }

        public override void OnHealthZero()
        {

        }

        public override void removePlayer(long id)
        {
            
        }

        public override bool SpawnGrid(string prefabName)
        {
            return false;
        }
    }
}
