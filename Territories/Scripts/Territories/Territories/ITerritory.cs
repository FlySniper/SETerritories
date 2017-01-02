using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRageMath;

namespace Territories
{
    public abstract class ITerritory
    {

        public Vector3D Center = Vector3D.Zero;
        public ITerritory Forward = null;
        public ITerritory Backward = null;
        public ITerritory Up = null;
        public ITerritory Down = null;
        public ITerritory Left = null;
        public ITerritory Right = null;
        public int health = 1;

        public Territory.AREADIFFICULTY difficulty;

        public abstract void addPlayer(long id);

        public abstract void removePlayer(long id);

        public abstract bool SpawnGrid(string prefabName);

        public abstract void OnHealthZero();

        public abstract string getOwnerName();
    }
}
