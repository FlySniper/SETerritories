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
        //static ulong itteration = 0;
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
                    MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
                    MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
                    AddMessageHandler();
                }
                return;
            }
            if(firstcall)
            {
                MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
                MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
                MyAPIGateway.Multiplayer.RegisterMessageHandler(6060, HandleServerData);
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

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            var text = messageText.ToLower();
            if(text.StartsWith("/t ") || text.StartsWith("/terr ") || text.StartsWith("/territory "))
            {
                sendToOthers = false;
                var splt = text.Split(new char[] { ' '},StringSplitOptions.RemoveEmptyEntries);
                var len = splt.Length;
                if(len == 1)
                {
                    var lbytes = BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId);
                    var bytes = new byte[] {1,lbytes[0], lbytes[1], lbytes[2], lbytes[3], lbytes[4], lbytes[5], lbytes[6], lbytes[7] };
                    MyAPIGateway.Multiplayer.SendMessageToServer(6060,bytes);
                }
                if(len ==2 && splt[1].Equals("show"))
                {
                    var lbytes = BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId);
                    var bytes = new byte[] { 2, lbytes[0], lbytes[1], lbytes[2], lbytes[3], lbytes[4], lbytes[5], lbytes[6], lbytes[7] };
                    MyAPIGateway.Multiplayer.SendMessageToServer(6060, bytes);
                }
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

        public void HandleServerData(byte[] data)
        {

            if(data[0]==1)
            {
                ulong id = BitConverter.ToUInt64(data, 1);
                SendMessageToClient("Territories Help\n/t show - Shows Territory Information",id);
            }
            if(data[0] == 2)
            {
                ulong id = BitConverter.ToUInt64(data, 1);
                long IdId = MyAPIGateway.Players.TryGetIdentityId(id);
                if(TerritoryManager.Mappings.ContainsKey(IdId))
                {
                    var terr = TerritoryManager.Mappings[IdId];
                    SendMessageToClient("Territory: " + new Vector3I(terr.Center) + "\nOwner: " + terr.getOwnerName() + "\nDifficulty: " + terr.difficulty, id);
                }
            }

        }
        public static void SendMessageToClient(string message, ulong steamid)
        {
            if (MyAPIGateway.Multiplayer.IsServer)
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
