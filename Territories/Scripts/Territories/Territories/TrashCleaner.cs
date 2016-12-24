using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;

namespace Territories
{
    class TrashCleaner
    {
        public static ulong itterations = 0;
        private static LinkedList<TrashList> trashes = new LinkedList<TrashList>();

        public static void Add(TrashList tl)
        {
            trashes.AddLast(tl);
        }

        public static void exec()
        {
            LinkedList<TrashList> newTrashes = new LinkedList<TrashList>();
            foreach(TrashList tl in trashes)
            {
                if(tl.TTL <= itterations)
                {
                    foreach(long ID in tl)
                    {
                        if (!MyAPIGateway.Entities.EntityExists(ID))
                        {
                            continue;
                        }
                        var grid = MyAPIGateway.Entities.GetEntityById(ID);
                        if(grid == null)
                        {
                            continue;
                        }
                        grid.Delete();
                    }
                    --tl.terr.grids;
                    --TerritoryManager.totalGrids;
                }
                else
                {
                    newTrashes.AddLast(tl);
                }
            }
            trashes = newTrashes;
            ++itterations;
        }

        public static void Load()
        {
            string EntityString = "";
            trashes = new LinkedList<TrashList>();

            if (!MyAPIGateway.Utilities.GetVariable<int>("Territories_totalGrids", out TerritoryManager.totalGrids))
            {
                MyAPIGateway.Utilities.SetVariable<int>("Territories_totalGrids", TerritoryManager.totalGrids);
            }
            if (!MyAPIGateway.Utilities.GetVariable<string>("Territories_Trash", out EntityString))
            {
                MyAPIGateway.Utilities.SetVariable<string>("Territories_Trash", EntityString);
                return;
            }

            var Entities = EntityString.Split(new string[] { "^@|" }, StringSplitOptions.RemoveEmptyEntries);
            if (Entities.Length == 0)
                return;
            for(int j = 0; j<Entities.Length; j+=2)
            {
                string TLstring = Entities[j];
                var terrData = Entities[j+1].Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                var tmp = TLstring.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i<tmp.Length; i+=2)
                {
                    string IDstring = tmp[i];
                    string TTLstring = tmp[i+1];
                    long ID;
                    ulong TTL;
                    if (long.TryParse(IDstring, out ID))
                    {
                        if (ulong.TryParse(TTLstring, out TTL))
                        {
                            var Center = Vector3D.Zero;
                            if (terrData.Length == 0)
                                continue;
                            Vector3D.TryParse(terrData[0], out Center);
                            Territory T;
                            if (TerritoryManager.Locations.ContainsKey(new Vector3I(Center)))
                            {
                                T = (Territory)TerritoryManager.Locations[new Vector3I(Center)];
                            }
                            else
                            {
                                T = new Territory(Center, (Territory.AREADIFFICULTY)int.Parse(terrData[1]), long.Parse(terrData[2]), int.Parse(terrData[3]), int.Parse(terrData[4]));
                                TerritoryManager.Locations.Add(new Vector3I(T.Center), T);
                            }
                            var tl = new TrashList(TTL, T);
                            tl.AddFirst(ID);
                            Add(tl);
                        }
                    }
                }
                
            }
        }

        public static void Save()
        {
            string EntityString = "";

            foreach(TrashList tl in trashes)
            {
                foreach(long ID in tl)
                {
                    ulong dTTL = tl.TTL - TrashCleaner.itterations;
                    EntityString += ID + "@@" + (tl.TTL > TrashCleaner.itterations ? dTTL:0)+"@@";
                }
                EntityString += "^@|"+tl.terr.Center+"@@"+((int)tl.terr.difficulty)+"@@"+tl.terr.owner+"@@"+tl.terr.health+"@@"+tl.terr.grids+"^@|";
            }
            MyAPIGateway.Utilities.SetVariable<string>("Territories_Trash", EntityString);
            MyAPIGateway.Utilities.SetVariable<int>("Territories_totalGrids", TerritoryManager.totalGrids);
        }
    }
}
