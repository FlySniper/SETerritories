using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Territories
{
    public enum DIFFICULTY
    {
        EASY,
        MEDIUM,
        HARD
    }
    public enum AREA
    {
        PLANET,
        MOON,
        SPACE
    }
    class PrefabStrings
    {
        private List<List<string>> EasyPrefabsPlanet = new List<List<string>>();
        private List<List<string>> EasyPrefabsMoon = new List<List<string>>();
        private List<List<string>> EasyPrefabsSpace = new List<List<string>>();

        public uint MAX_SCORE = 55;
        public Dictionary<uint, int> TierMap = new Dictionary<uint, int>()
        {
            {0,0},
            {1,0},
            {2,0},
            {3,0},

            {4,1},
            {5,1},
            {6,1},
            {7,1},

            {8,2},
            {9,2},
            {10,2},
            {11,2},

            {12,3},
            {13,3},
            {14,3},
            {15,3},
            {16,4},
            {17,4},
            {18,4},
            {19,4},
            {20,4},
            {21,4},
            {22,4},
            {23,4},
            {24,4},
            {25,4},
            {26,4},
            {27,4},
            {28,4},
            {29,4},
            {30,4},
            {31,4},
            {32,4},
            {33,4},
            {34,4},
            {35,5},
            {36,5},
            {37,5},
            {38,5},
            {39,5},
            {40,5},
            {41,5},
            {42,5},
            {43,5},
            {44,5},
            {45,5},
            {46,5},
            {47,5},
            {48,5},
            {49,5},
            {50,5},
            {51,5},
            {52,5},
            {53,5},
            {54,5},
            {55,6},
        };

        private static PrefabStrings Instance = null;

        private PrefabStrings()
        {
            
        }

        public static PrefabStrings GetInstance()
        {
            if (Instance == null)
            {
                Instance = new PrefabStrings();
                Instance.Init();
            }
            return Instance;
        }

        private void Init()
        {
            List<string> T1EasyPlanet = new List<string>() { 
            "Striker","Stinging_Adversary_mk.1","Broadhead"

            };
            EasyPrefabsPlanet.Add(T1EasyPlanet);

            List<string> T2EasyPlanet = new List<string>() { 
            "Stinging_Adversary_mk.1\\Striker","Broadhead\\Striker","Stinging_Adversary_mk.1\\Broadhead"

            };
            EasyPrefabsPlanet.Add(T2EasyPlanet);

            List<string> T3EasyPlanet = new List<string>() { 
            "Striker\\Stinging_Adversary_mk.1\\Broadhead","Spiteful_Aggressor_mk.1","Broadhead\\Striker\\Stinging_Adversary_mk.1"

            };
            EasyPrefabsPlanet.Add(T3EasyPlanet);

            List<string> T4EasyPlanet = new List<string>() { 
            "Striker\\Stinging_Adversary_mk.1\\Broadhead\\Stinging_Adversary_mk.1","Spiteful_Aggressor_mk.1\\Broadhead","Broadhead\\Striker\\Stinging_Adversary_mk.1\\Striker","Wyvern"

            };
            EasyPrefabsPlanet.Add(T4EasyPlanet);

            List<string> T5EasyPlanet = new List<string>() { 
            "Striker\\Stinging_Adversary_mk.1\\Broadhead\\Broadhead\\Striker\\Stinging_Adversary_mk.1\\Broadhead\\Striker",
            "Spiteful_Aggressor_mk.1\\Spiteful_Aggressor_mk.1",
            "Broadhead\\Striker\\Stinging_Adversary_mk.1\\Striker\\Broadhead\\Striker\\Stinging_Adversary_mk.1\\Striker",
            "Wyvern\\Wyvern"

            };
            EasyPrefabsPlanet.Add(T5EasyPlanet);

            List<string> T6EasyPlanet = new List<string>() { 
            "Wyvern\\Wyvern\\Broadhead\\Stinging_Adversary_mk.1\\Wyvern\\Wyvern",
            "Wyvern\\Wyvern\\Broadhead\\Striker\\Stinging_Adversary_mk.1\\Wyvern\\Broadhead\\Striker",
            "Wyvern\\Wyvern\\Broadhead\\Striker\\Stinging_Adversary_mk.1\\Striker\\Broadhead\\Striker\\Broadhead\\Striker",
            "Egide",
            "Egide",
            "Egide"

            };
            EasyPrefabsPlanet.Add(T6EasyPlanet);




            List<string> T1EasyMoon = new List<string>() { 
                "Bruiser"
            };
            EasyPrefabsMoon.Add(T1EasyMoon);

            List<string> T2EasyMoon = new List<string>()
            {
                "Bruiser\\Bruiser","Eivogel"
            };
            EasyPrefabsMoon.Add(T2EasyMoon);

            List<string> T3EasyMoon = new List<string>()
            {
                "Bruiser\\Bruiser\\Bruiser","Eivogel\\Bruiser"
            };
            EasyPrefabsMoon.Add(T3EasyMoon);

            List<string> T4EasyMoon = new List<string>()
            {
                "Bruiser\\Bruiser\\Bruiser\\Bruiser","Eivogel\\Eivogel"
            };
            EasyPrefabsMoon.Add(T4EasyMoon);

            List<string> T5EasyMoon = new List<string>()
            {
                "Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser",
                "Eivogel\\Eivogel\\Eivogel\\Eivogel","Eivogel\\Eivogel\\Bruiser\\Bruiser\\Bruiser\\Bruiser"
            };
            EasyPrefabsMoon.Add(T5EasyMoon);

            List<string> T6EasyMoon = new List<string>()
            {
                "Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Eivogel",
                "Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Bruiser\\Eivogel\\Eivogel",
                "Eivogel\\Eivogel\\Bruiser\\Bruiser\\Bruiser\\Eivogel\\Bruiser\\Eivogel\\Eivogel\\Eivogel\\Eivogel",
                "Eivogel\\Eivogel\\Bruiser\\Bruiser\\Eivogel\\Eivogel\\Eivogel\\Eivogel\\Eivogel\\Eivogel",
            };
            EasyPrefabsMoon.Add(T6EasyMoon);




            List<string> T1EasySpace = new List<string>()
            {
                "ScarabMaw",
                "VictorInterceptor",
                "GladiatorInterceptor"

            };
            EasyPrefabsSpace.Add(T1EasySpace);

            List<string> T2EasySpace = new List<string>()
            {
                "ScarabMaw\\VictorInterceptor",
                "VictorInterceptor\\GladiatorInterceptor",
                "ScarabMaw\\GladiatorInterceptor",
                "Eivogel"
            };
            EasyPrefabsSpace.Add(T2EasySpace);

            List<string> T3EasySpace = new List<string>()
            {
                "DuelEye",
                "ScarabMaw\\GladiatorInterceptor\\VictorInterceptor",
                "HawkWasp",
                "Eivogel\\ScarabMaw",
                "Eivogel\\GladiatorInterceptor",
                "Eivogel\\VictorInterceptor"
            };
            EasyPrefabsSpace.Add(T3EasySpace);

            List<string> T4EasySpace = new List<string>()
            {
                "DuelEye\\HawkWasp",
                "ScarabMaw\\GladiatorInterceptor\\VictorInterceptor\\VictorInterceptor",
                "HawkWasp\\GladiatorInterceptor",
                "DuelEye\\VictorInterceptor",
                "DefenderCorvette",
                "Eivogel\\Eivogel"
            };
            EasyPrefabsSpace.Add(T4EasySpace);

            List<string> T5EasySpace = new List<string>()
            {
                "DuelEye\\HawkWasp\\ScarabMaw\\VictorInterceptor",
                "HawkWasp\\HawkWasp\\HawkWasp",
                "HawkWasp\\GladiatorInterceptor\\VictorInterceptor\\DuelEye",
                "RedemptionDestroyer",
                "Eivogel\\Eivogel\\Eivogel\\GladiatorInterceptor\\VictorInterceptor",
                "Eivogel\\ScarabMaw\\GladiatorInterceptor\\VictorInterceptor\\DuelEye",
                "RedemptionDestroyer"
            };
            EasyPrefabsSpace.Add(T5EasySpace);

            List<string> T6EasySpace = new List<string>()
            {
                "RedemptionDestroyer\\Eivogel\\ScarabMaw\\GladiatorInterceptor\\VictorInterceptor\\DuelEye",
                "Egide",
                "Egide",
                "ScarabMaw\\ScarabMaw\\ScarabMaw\\ScarabMaw\\ScarabMaw\\ScarabMaw\\VictorInterceptor\\VictorInterceptor\\VictorInterceptor\\VictorInterceptor\\VictorInterceptor\\VictorInterceptor\\GladiatorInterceptor\\GladiatorInterceptor\\GladiatorInterceptor\\GladiatorInterceptor\\GladiatorInterceptor\\GladiatorInterceptor"
            };
            EasyPrefabsSpace.Add(T6EasySpace);



        }

        public List<string> SelectPrefabs(DIFFICULTY difficulty, AREA area,double score)
        {
            int index = TierMap[Math.Min((uint)Math.Floor(score),MAX_SCORE)] - 1;
            if(index == -1)
            {
                return null;
            }
            switch(difficulty)
            {
                case DIFFICULTY.EASY:
                    return SelectEasyPrefabs(area, index);
                case DIFFICULTY.MEDIUM:
                    return SelectEasyPrefabs(area, index);
                case DIFFICULTY.HARD:
                    return SelectEasyPrefabs(area, index);
            }
            return null;
        }

        private List<string> AllPrefabsFromString(string Prefabs)
        {
            var splt = Prefabs.Split('\\');

            return new List<string>(splt);
        }

        private List<string> SelectEasyPrefabs(AREA area, int index)
        {
            Random rand = new Random();
            switch (area)
            {
                case AREA.PLANET:
                    var tmp = EasyPrefabsPlanet[index];
                    return AllPrefabsFromString(tmp[rand.Next(0, tmp.Count)]);
                case AREA.MOON:
                    var temp = EasyPrefabsMoon[index];
                    return AllPrefabsFromString(temp[rand.Next(0, temp.Count)]);
                case AREA.SPACE:
                    var Tmp = EasyPrefabsSpace[index];
                    return AllPrefabsFromString(Tmp[rand.Next(0, Tmp.Count)]);
                    
            }
            return null;
        }
    }
}
