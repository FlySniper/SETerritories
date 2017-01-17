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
using Sandbox.Common.ObjectBuilders;

namespace Territories
{
    public class GridCopier
    {

        public static List<MyObjectBuilder_CubeGrid> easyGrids = new List<MyObjectBuilder_CubeGrid>();
        public static List<MyObjectBuilder_CubeGrid> mediumGrids = new List<MyObjectBuilder_CubeGrid>();
        public static List<MyObjectBuilder_CubeGrid> hardGrids = new List<MyObjectBuilder_CubeGrid>();

        public static void FindAnalyzeAndAdd()
        {
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities,(ent) => ent is IMyCubeGrid);
            Random r = new Random();
            int indx = r.Next(0,entities.Count);
            if (entities.Count == 0)
                return;
            var grid = (IMyCubeGrid)entities.ElementAt(indx);
            AnalyzeAndAdd(grid);
        }
        

        public static void AnalyzeAndAdd(IMyCubeGrid grid)
        {
            if (grid == null)
                return;
            var mass = grid.Physics.Mass;
            bool Up = false;
            bool Down = false;
            bool Left = false;
            bool Right = false;
            bool Forward = false;
            bool Backward = false;
            float avgThrust = 0.0f;
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks,(block) =>block.FatBlock is IMyThrust);
            foreach (var block in blocks)
            {
                var thrustBlock = (IMyThrust)block.FatBlock;
                avgThrust += thrustBlock.MaxThrust / 6.0f;
                Up |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Up;
                Down |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Down;
                Left |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Left;
                Right |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Right;
                Forward |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Forward;
                Backward |= thrustBlock.Orientation.Forward == Base6Directions.Direction.Backward;
            }
            var mDt = mass / avgThrust;
            MyAPIGateway.Utilities.ShowNotification("AandA Before If "+mDt+" "+(Up && Down && Left && Right && Forward && Backward));
            if(mDt < 500f && Up && Down && Left && Right && Forward && Backward)
            {

                List<IMySlimBlock> turrets = new List<IMySlimBlock>();
                grid.GetBlocks(turrets, (block) => block.FatBlock is IMyLargeTurretBase);
                List<IMySlimBlock> reactors = new List<IMySlimBlock>();
                grid.GetBlocks(reactors, (block) => block.FatBlock is IMyReactor);
                List<IMySlimBlock> remotes = new List<IMySlimBlock>();
                grid.GetBlocks(remotes, (block) => block.FatBlock is IMyRemoteControl);
                if (turrets.Count == 0 || reactors.Count == 0 || remotes.Count == 0)
                    return;
                if(turrets.Count <= 3 && grid.GridSizeEnum == MyCubeSize.Small)
                {
                    var builder = grid.GetObjectBuilder(true);
                    if(builder is MyObjectBuilder_CubeGrid)
                    {
                        MyObjectBuilder_CubeGrid gridBuilder = (MyObjectBuilder_CubeGrid)builder;
                        var CubeBlocks = gridBuilder.CubeBlocks;
                        var storage = CubeBlocks.FindAll((b)=> b is MyObjectBuilder_CargoContainer);
                        var Turrets = CubeBlocks.FindAll((b) => b is MyObjectBuilder_LargeMissileTurret);
                        /*
                        foreach(var container in storage)
                        {
                            var tmp = (MyObjectBuilder_CargoContainer)container;
                            
                            tmp.Inventory.Clear();
                            MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 5, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "NATO_25x184mm" } };
                            MyObjectBuilder_InventoryItem missiles = new MyObjectBuilder_InventoryItem() { Amount = 1, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "Missile200mm" } };
                            tmp.Inventory.Items.Add(ammo);
                            tmp.Inventory.Items.Add(missiles);
                        }
                        foreach (var container in Turrets)
                        {
                            if (container is MyObjectBuilder_LargeGatlingTurret)
                            {
                                var tmp = (MyObjectBuilder_LargeGatlingTurret)container;
                                tmp.Inventory.Clear();
                                MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 1, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "NATO_25x184mm" } };
                                tmp.Inventory.Items.Add(ammo);
                            }
                            else if(container is MyObjectBuilder_LargeMissileTurret)
                            {
                                var tmp = (MyObjectBuilder_LargeMissileTurret)container;
                                tmp.Inventory.Clear();
                                MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 2, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "Missile200mm" } };
                                tmp.Inventory.Items.Add(ammo);
                            }
                        }*/
                        if (easyGrids.Count >= 10)
                            easyGrids.RemoveAt(0);
                        easyGrids.Add(gridBuilder);
                    }
                }
                else if (turrets.Count <= 6)
                {
                    var builder = grid.GetObjectBuilder(true);
                    if (builder is MyObjectBuilder_CubeGrid)
                    {
                        MyObjectBuilder_CubeGrid gridBuilder = (MyObjectBuilder_CubeGrid)builder;
                        var CubeBlocks = gridBuilder.CubeBlocks;
                        var storage = CubeBlocks.FindAll((b) => b is MyObjectBuilder_CargoContainer);
                        var Turrets = CubeBlocks.FindAll((b) => b is MyObjectBuilder_LargeMissileTurret);
                        
                        foreach (var container in storage)
                        {
                            var tmp = (MyObjectBuilder_CargoContainer)container;
                            tmp.Inventory = new MyObjectBuilder_Inventory();
                            tmp.Inventory.Clear();
                            MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 10, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "NATO_25x184mm" } };
                            MyObjectBuilder_InventoryItem missiles = new MyObjectBuilder_InventoryItem() { Amount = 4, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "Missile200mm" } };
                            tmp.Inventory.Items.Add(ammo);
                            tmp.Inventory.Items.Add(missiles);
                        }
                        foreach (var container in Turrets)
                        {
                            if (container is MyObjectBuilder_LargeGatlingTurret)
                            {
                                var tmp = (MyObjectBuilder_LargeGatlingTurret)container;
                                tmp.Inventory = new MyObjectBuilder_Inventory();
                                tmp.Inventory.Clear();
                                MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 2, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "NATO_25x184mm" } };
                                tmp.Inventory.Items.Add(ammo);
                            }
                            else if (container is MyObjectBuilder_LargeMissileTurret)
                            {
                                var tmp = (MyObjectBuilder_LargeMissileTurret)container;
                                tmp.Inventory = new MyObjectBuilder_Inventory();
                                tmp.Inventory.Clear();
                                MyObjectBuilder_InventoryItem ammo = new MyObjectBuilder_InventoryItem() { Amount = 2, Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = "Missile200mm" } };
                                tmp.Inventory.Items.Add(ammo);
                            }
                        }
                        if (mediumGrids.Count >= 10)
                            mediumGrids.RemoveAt(0);
                        mediumGrids.Add(gridBuilder);
                    }
                }
                else if (turrets.Count <= 10 && grid.GridSizeEnum == MyCubeSize.Large)
                {
                    var builder = ((MyCubeGrid)grid).GetObjectBuilder();
                    if (builder is MyObjectBuilder_CubeGrid)
                    {
                        MyObjectBuilder_CubeGrid gridBuilder = (MyObjectBuilder_CubeGrid)((MyObjectBuilder_CubeGrid)builder).Clone();
                        
                        if (hardGrids.Count >= 10)
                            hardGrids.RemoveAt(0);
                        hardGrids.Add(gridBuilder);
                    }
                }
            }
        }

        
    }
}
