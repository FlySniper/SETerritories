using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class TerritoryManager
    {
        public static Dictionary<long, ITerritory> Mappings = new Dictionary<long, ITerritory>();
        public static Dictionary<Vector3I, ITerritory> Locations = new Dictionary<Vector3I, ITerritory>(new Vector3I.EqualityComparer());
        public static int totalGrids = 0;
        public static int totalMaxGrids = 30;
        
       

        public static void checkPlayer(IMyPlayer player)
        {
            if(!Mappings.ContainsKey(player.IdentityId))
            {
                Mappings.Add(player.IdentityId, null);
            }
            ITerritory Iterritory = Mappings[player.IdentityId];
            if(Iterritory == null)
            {
                var pos = player.GetPosition();
                var adjusted = new Vector3D(Math.Round(pos.X/(Territory.radius*2000))*Territory.radius*2000, Math.Round(pos.Y / (Territory.radius * 2000)) * Territory.radius * 2000, Math.Round(pos.Z / (Territory.radius * 2000)) * Territory.radius * 2000);
                Iterritory = addTerritory(adjusted);
                if (Iterritory == null)
                    return;
                if (Locations.ContainsKey(new Vector3I(adjusted)))
                {
                    Iterritory.removePlayer(player.IdentityId);
                    Mappings[player.IdentityId] = Locations[new Vector3I(adjusted)];
                    Mappings[player.IdentityId].addPlayer(player.IdentityId);
                    return;
                }
                else
                {
                    Mappings[player.IdentityId] = Iterritory;
                    Mappings[player.IdentityId].addPlayer(player.IdentityId);
                }
                PlayerMoveTracker.SendMessageToClient("Territory: " + new Vector3I(Mappings[player.IdentityId].Center) + "\nOwner: " + player.DisplayName + "\nDifficulty: " + Mappings[player.IdentityId].difficulty, player.SteamUserId);
                Locations.Add(new Vector3I(adjusted),Iterritory);
                return;
            }
            string[] prefabs = {"ScarabMaw", "DuelEye","Eivogel" };
            Random r = new Random();
            Iterritory.SpawnGrid(prefabs[r.Next(prefabs.Length)]);
            double distance = (player.GetPosition() - Iterritory.Center).LengthSquared();
            if((Iterritory.Forward == null || Iterritory.Backward == null || Iterritory.Left == null || Iterritory.Right == null || Iterritory.Up == null || Iterritory.Down == null))
            {
                //MyAPIGateway.Utilities.ShowNotification("Adding Neighbors" );
                addNeighbors(Iterritory);
            }
            List<ITerritory> neighbors = new List<ITerritory>() {Iterritory.Forward, Iterritory.Backward, Iterritory.Left, Iterritory.Right, Iterritory.Up, Iterritory.Down};
            if (distance <= 40000000000d)
            {
                foreach (ITerritory n in neighbors)
                {
                    if (n == null)
                        continue;
                    var neighborDistance = (player.GetPosition() - n.Center).LengthSquared();
                    if (neighborDistance < distance)
                    {
                        Mappings[player.IdentityId].removePlayer(player.IdentityId);
                        Mappings[player.IdentityId] = n;
                        Mappings[player.IdentityId].addPlayer(player.IdentityId);
                        PlayerMoveTracker.SendMessageToClient("Territory: " + new Vector3I(Mappings[player.IdentityId].Center) + "\nOwner: " + player.DisplayName + "\nDifficulty: " + Mappings[player.IdentityId].difficulty, player.SteamUserId);
                        return;
                    }
                }
            }
            else
            {

                var pos2 = player.GetPosition();
                var adjusted2 = new Vector3D(Math.Round(pos2.X / (Territory.radius * 2000)) * Territory.radius * 2000, Math.Round(pos2.Y / (Territory.radius * 2000)) * Territory.radius * 2000, Math.Round(pos2.Z / (Territory.radius * 2000)) * Territory.radius * 2000);

                if (Locations.ContainsKey(new Vector3I(adjusted2)))
                {
                    if (Locations[new Vector3I(adjusted2)] != Mappings[player.IdentityId])
                    {
                        Mappings[player.IdentityId].removePlayer(player.IdentityId);
                        Mappings[player.IdentityId] = Locations[new Vector3I(adjusted2)];
                        Mappings[player.IdentityId].addPlayer(player.IdentityId);
                        PlayerMoveTracker.SendMessageToClient("Territory: " + new Vector3I(Mappings[player.IdentityId].Center) + "\nOwner: " + player.DisplayName + "\nDifficulty: " + Mappings[player.IdentityId].difficulty, player.SteamUserId);
                    }
                }
                else
                {
                    Iterritory = addTerritory(adjusted2);
                    if (Iterritory == null)
                        return;
                    Mappings[player.IdentityId].removePlayer(player.IdentityId);
                    Mappings[player.IdentityId] = Iterritory;
                    Mappings[player.IdentityId].addPlayer(player.IdentityId);
                    PlayerMoveTracker.SendMessageToClient("Territory: " + new Vector3I(Mappings[player.IdentityId].Center) + "\nOwner: " + player.DisplayName + "\nDifficulty: " + Mappings[player.IdentityId].difficulty, player.SteamUserId);
                }
            }
        }

        private static void addNeighbors(ITerritory curr)
        {
            //TODO: Check if Territory already exists before creating a new one
            if(curr.Right == null)
            {
                var point = curr.Center + new Vector3D(Territory.radius * 2000, 0, 0);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Right = Locations[new Vector3I(point)];
                    curr.Right.Left = curr;
                }
                else
                {
                    var terr = addTerritory(point);
                    curr.Right = terr;
                    Locations.Add(new Vector3I(point), terr);
                    curr.Right.Left = curr;
                }
            }

            if (curr.Left == null)
            {
                var point = curr.Center + new Vector3D(-Territory.radius * 2000, 0, 0);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Left = Locations[new Vector3I(point)];
                    curr.Left.Right = curr;
                }
                else
                {
                    curr.Left = addTerritory(point);
                    Locations.Add(new Vector3I(point), curr.Left);
                    curr.Left.Right = curr;
                }
            }

            if (curr.Forward == null)
            {
                var point = curr.Center + new Vector3D(0, Territory.radius * 2000, 0);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Forward = Locations[new Vector3I(point)];
                    curr.Forward.Backward = curr;
                }
                else
                {
                    curr.Forward = addTerritory(point);
                    Locations.Add(new Vector3I(point), curr.Forward);
                    curr.Forward.Backward = curr;
                }
            }
            if (curr.Backward == null)
            {
                var point = curr.Center + new Vector3D(0, -Territory.radius * 2000, 0);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Backward = Locations[new Vector3I(point)];
                    curr.Backward.Forward = curr;
                }
                else
                { 
                    curr.Backward = addTerritory(point);
                    Locations.Add(new Vector3I(point), curr.Backward);
                    curr.Backward.Forward = curr;
                }
            }

            if (curr.Up == null)
            {
                var point = curr.Center + new Vector3D(0, 0, Territory.radius * 2000);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Up = Locations[new Vector3I(point)];
                    curr.Up.Down = curr;
                }
                else
                {
                    curr.Up = addTerritory(point);
                    Locations.Add(new Vector3I(point), curr.Up);
                    curr.Up.Down = curr;
                }
            }
            if (curr.Down == null)
            {
                var point = curr.Center + new Vector3D(0, 0, -Territory.radius * 2000);
                if (Locations.ContainsKey(new Vector3I(point)))
                {
                    curr.Down = Locations[new Vector3I(point)];
                    curr.Down.Up = curr;
                }
                else
                {
                    curr.Down = addTerritory(point);
                    Locations.Add(new Vector3I(point), curr.Down);
                    curr.Down.Up = curr;
                }
            }
        }

        private static ITerritory addTerritory(Vector3D pos)
        {
            Random r = new Random();
            int rand = r.Next(100);
            HashSet<IMyEntity> Planets = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(Planets, e => e is MyPlanet);
            foreach (IMyEntity e in Planets)
            {
                MyPlanet p = (MyPlanet)e;
                if (p.AtmosphereRadius >= Vector3D.Distance(pos,p.WorldMatrix.Translation))
                {
                    return new PlanetTerritory();
                }
            }
            if(rand >= 80)
                return new Territory(pos,Territory.AREADIFFICULTY.HARD);
            if (rand >= 50)
                return new Territory(pos, Territory.AREADIFFICULTY.MEDIUM);
            if (rand >= 25)
                return new Territory(pos, Territory.AREADIFFICULTY.EASY);
            return new Territory(pos, Territory.AREADIFFICULTY.EASY);
        }


        private static double LVRemainder(double d, double m)
        {
            return d - m * Math.Floor(d/m);
        }
    }
}
