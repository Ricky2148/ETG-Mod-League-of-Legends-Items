using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria;
using Alexandria.ItemAPI;

namespace LOLItems
{
    public static class HelpfulMethods
    {
        public static float GetFloorDamageScale()
        {
            string currentFloor = GameManager.Instance.GetLastLoadedLevelDefinition().dungeonSceneName;
            
            switch (currentFloor)
            {
                case "tt_castle": return 1.0f;
                case "tt_sewer": return 1.25f;
                case "tt5": return 1.25f;
                case "tt_cathedral": return 1.5f;
                case "tt_mines": return 1.5f;
                case "ss_resourcefulrat": return 1.75f;
                case "tt_catacombs": return 1.75f;
                case "tt_nakatomi": return 2.0f;
                case "tt_forge": return 2.0f;
                case "tt_bullethell": return 2.25f;
                default: return 0f; // safety fallback
            }
        }

        public static float GetFloorPriceMod()
        {
            float floorPriceMod = GameManager.Instance.GetLastLoadedLevelDefinition().priceMultiplier;

            return floorPriceMod;
        }

        public static string[,] FloorNames = {
            {"tt_castle", "Keep of the Lead Lord / Floor 1"},
            {"tt_sewer", "Oubliette / Floor 1.5"},
            {"tt5", "Gungeon Proper / Floor 2"},
            {"tt_cathedral", "Abbey of the True Gun / Floor 2.5"},
            {"tt_mines", "Black Powder Mine / Floor 3"},
            {"ss_resourcefulrat", "Resourceful Rat's Lair / Floor 3.5"},
            {"tt_catacombs", "Hollow / Floor 4"},
            {"tt_nakatomi", "R&G Dept / Floor 4.5"},
            {"tt_forge", "Forge / Floor 5"},
            {"tt_bullethell", "Bullet Hell / Floor 6"}
        };

        public static void PlayRandomSFX(AIActor enemy, List<string> sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Count);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, enemy.gameObject);
        }

        public static void PlayRandomSFX(Projectile proj, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, proj.gameObject);
        }

        public static float GetFloorValue()
        {
            string currentFloor = GameManager.Instance.GetLastLoadedLevelDefinition().dungeonSceneName;

            // Loop through the array
            for (int i = 0; i < FloorNames.GetLength(0); i++)
            {
                string floorKey = FloorNames[i, 0];
                if (currentFloor == floorKey)
                {
                    // Set your custom float values here
                    switch (floorKey)
                    {
                        case "tt_castle": return 1.0f;
                        case "tt_sewer": return 1.5f;
                        case "tt5": return 2.0f;
                        case "tt_cathedral": return 2.5f;
                        case "tt_mines": return 3.0f;
                        case "ss_resourcefulrat": return 3.5f;
                        case "tt_catacombs": return 4.0f;
                        case "tt_nakatomi": return 4.5f;
                        case "tt_forge": return 5.0f;
                        case "tt_bullethell": return 6.0f;
                        default: return 0f; // safety fallback
                    }
                }
            }
            return 0f;
        }
    }
}
