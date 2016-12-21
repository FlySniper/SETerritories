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

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class PlayerMoveTracker : MySessionComponentBase
    {
        private bool firstcall = true;
        public static ulong CleanTimer = 72000;
        static ulong itteration = 0;
        public override void UpdateBeforeSimulation()
        {
            
            base.UpdateBeforeSimulation();
            if (MyAPIGateway.Session == null)
            {
                return;
            }
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                if (firstcall)
                {
                    AddMessageHandler();
                }
                return;
            }
            if(firstcall)
            {
                TrashCleaner.Load();
                firstcall = false;
                return;
            }
            
            List<IMyPlayer> Players = new List<IMyPlayer>();
            List<IMyIdentity> Ids = new List<IMyIdentity>();
            MyAPIGateway.Players.GetPlayers(Players);
            MyAPIGateway.Players.GetAllIdentites(Ids);
            foreach (IMyPlayer p in Players)
            {
                var ID = Ids.Find((id)=>id.IdentityId == p.IdentityId);
                if(ID != null && !ID.IsDead)
                    TerritoryManager.checkPlayer(p);
            }
            TrashCleaner.exec();
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

        protected override void UnloadData()
        {
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
        public void HandleMessageData(byte[] data)
        {
            char[] chars = new char[data.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                chars[i] = (char)data[i];
            }
            string message = new string(chars);
            MyAPIGateway.Utilities.ShowNotification(message, 6000);

        }
        public static void SendMessageToClient(string message, ulong steamid)
        {
            if (steamid == MyAPIGateway.Session.LocalHumanPlayer.SteamUserId)
            {
                MyAPIGateway.Utilities.ShowNotification(message, 6000);
                return;
            }
            char[] chars = message.ToCharArray();
            byte[] data = new byte[chars.Length];
            for (int i = 0; i < chars.Length; ++i)
            {
                data[i] = (byte)chars[i];
            }

            MyAPIGateway.Multiplayer.SendMessageTo(5000, data, steamid);
        }
    }
}
