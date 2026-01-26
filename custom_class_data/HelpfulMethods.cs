using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static GlobalSparksDoer;

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

        public static void DoRandomParticleBurst(int num, Vector3 minPosition, Vector3 maxPosition, float angleVariance, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
        {
            for (int i = 0; i < num; i++)
            {
                Vector3 direction = BraveUtility.RandomAngle().DegreeToVector2();
                Vector3 position = new Vector3(UnityEngine.Random.Range(minPosition.x, maxPosition.x), UnityEngine.Random.Range(minPosition.y, maxPosition.y), UnityEngine.Random.Range(minPosition.z, maxPosition.z));
                Vector3 direction2 = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f - angleVariance, angleVariance)) * (direction.normalized * UnityEngine.Random.Range(direction.magnitude - magnitudeVariance, direction.magnitude + magnitudeVariance));
                GlobalSparksDoer.DoSingleParticle(position, direction2, startSize, startLifetime, startColor, systemType);
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

        /*
        public static void PlayRandomSFX(AIActor enemy, List<string> sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Count);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, enemy.gameObject);
        }

        public static void PlayRandomSFX(PlayerController player, List<string> sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Count);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, player.gameObject);
        }

        public static void PlayRandomSFX(Projectile proj, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, proj.gameObject);
        }

        public static void PlayRandomSFX(BeamController beam, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, beam.gameObject);
        }

        public static void PlayRandomSFX(Gun gun, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, gun.gameObject);
        }
        */

        public static void PlayRandomSFX(GameObject gameObject, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            //Plugin.Log($"Played {sfxName}");
            AkSoundEngine.PostEvent(sfxName, gameObject);
        }

        public static void PlayRandomSFX(GameObject gameObject, List<string> sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Count);
            string sfxName = sfxList[sfxIndex];
            //Plugin.Log($"Played {sfxName}");
            AkSoundEngine.PostEvent(sfxName, gameObject);
        }

        /*public static uint PlayAndReturnRandomSFX(GameObject gameObject, List<string> sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Count);
            string sfxName = sfxList[sfxIndex];
            //Plugin.Log($"Played {sfxName}");
            return AkSoundEngine.PostEvent(sfxName, gameObject);
        }*/

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

        public static void CustomNotification(string header, string text, tk2dBaseSprite sprite = null, UINotificationController.NotificationColor? color = null)
        {
            sprite ??= GameUIRoot.Instance.notificationController.notificationObjectSprite;
            GameUIRoot.Instance.notificationController.DoCustomNotification(
                header,
                text,
                sprite.Collection,
                sprite.spriteId,
                color ?? UINotificationController.NotificationColor.PURPLE,
                false,
                false);
        }

        public static GameObject Attach<T>(this GameObject go, Action<T> predicate = null, bool allowDuplicates = false) where T : MonoBehaviour
        {
            T component = allowDuplicates ? go.gameObject.AddComponent<T>() : go.gameObject.GetOrAddComponent<T>();
            if (predicate != null)
                predicate(component);
            return go;
        }

        public static List<string> GetResourceFrames(this string baseString, int length)
        {
            List<string> theList = new(length);
            for (int i = 1; i <= length; ++i)
                theList.Add($"{baseString}_{i:D3}");
            return theList;
        }

        public static void AddItemToSynergy(this PickupObject obj, CustomSynergyType type)
        {
            AddItemToSynergy(type, obj.PickupObjectId);
        }

        public static void AddItemToSynergy(CustomSynergyType type, int id)
        {
            foreach (AdvancedSynergyEntry entry in GameManager.Instance.SynergyManager.synergies)
            {
                if (entry.bonusSynergies.Contains(type))
                {
                    if (PickupObjectDatabase.GetById(id) != null)
                    {
                        PickupObject obj = PickupObjectDatabase.GetById(id);
                        if (obj is Gun)
                        {
                            if (entry.OptionalGunIDs != null)
                            {
                                entry.OptionalGunIDs.Add(id);
                            }
                            else
                            {
                                entry.OptionalGunIDs = new List<int> { id };
                            }
                        }
                        else
                        {
                            if (entry.OptionalItemIDs != null)
                            {
                                entry.OptionalItemIDs.Add(id);
                            }
                            else
                            {
                                entry.OptionalItemIDs = new List<int> { id };
                            }
                        }
                    }
                }
            }
        }
    }
}
