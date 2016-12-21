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

namespace Territories
{
    class TrashList : LinkedList<long>
    {
        private ulong ttl = 0;
        public Territory terr;
        public ulong TTL
        {
            get
            {
                return ttl;
            }
        }
        public TrashList(ulong TTL, Territory territory)
        {
            ttl = TTL;
            terr = territory;
        }
    }
}
