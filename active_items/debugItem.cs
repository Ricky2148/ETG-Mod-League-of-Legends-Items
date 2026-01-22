using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using Dungeonator;
using LOLItems.custom_class_data;
using LOLItems.guon_stones;
using LOLItems.passive_items;
using LOLItems.weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LOLItems.active_items
{
    internal class debugItem : PlayerItem
    {
        public static int ID;

        public static GameObject AscensionIcon;

        private static List<string> VFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/test_vfx/image (1)",
                "LOLItems/Resources/vfxs/test_vfx/image (2)",
                "LOLItems/Resources/vfxs/test_vfx/image (3)",
                "LOLItems/Resources/vfxs/test_vfx/image (4)",
                "LOLItems/Resources/vfxs/test_vfx/image (5)",
                "LOLItems/Resources/vfxs/test_vfx/image (6)",
                "LOLItems/Resources/vfxs/test_vfx/image (7)",
                "LOLItems/Resources/vfxs/test_vfx/image (8)",
                "LOLItems/Resources/vfxs/test_vfx/image (9)"
            };

        private static GameObject EffectVFX = VFXBuilder.CreateVFX
        (
            "test_vfx",
            VFXSpritePath,
            10,
            new IntVector2(0, 0),
            tk2dBaseSprite.Anchor.MiddleCenter,
            false,
            0,
            -1,
            Color.cyan,
            tk2dSpriteAnimationClip.WrapMode.Loop,
            true
        );

        private GameObject activeVFXObject;

        //doesn't work at all
        private static GameActorBleedEffect bleedEffect = new GameActorBleedEffect
        {
            duration = 5f,
            effectIdentifier = "debug bleed effect",
            resistanceType = EffectResistanceType.None,
            AppliesOutlineTint = true,
            OutlineTintColor = Color.gray,
        };

        public static void Init()
        {
            string itemName = "Debug Item";
            string resourceName = "LOLItems/Resources/black_dot";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<debugItem>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "idk";
            string longDesc = "idk";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            //ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.PerRoom, 20f);
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1f);
            item.consumable = false;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.EXCLUDED;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void DoEffect(PlayerController player)
        {
            //StartCoroutine(EffectCoroutine(player));

            //HelpfulMethods.CustomNotification("smth1", "smth2", this.sprite);

            //LootEngine.SpawnItem(PickupObjectDatabase.GetByName("Muramana").gameObject, player.CenterPosition, Vector2.down, 0);

            /*List<AIActor> enemyList = player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (enemyList != null)
            {
                foreach (AIActor enemy in enemyList)
                {
                    if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                    {
                        enemy.ApplyEffect(bleedEffect, 1f, null);
                    }
                }
            }*/

            //Plugin.Log("debug item finished");
        }

        private System.Collections.IEnumerator EffectCoroutine(PlayerController player)
        {
            //HelpfulMethods.DoRandomParticleBurst(num3, vector, vector2, 1f, 1f, 0.3f, 1, Color.cyan, GlobalSparksDoer.SparksType.FLOATY_CHAFF);

            //GlobalSparksDoer.DoSingleParticle(player.CenterPosition, Vector3.forward, 0.3f, 1, Color.white, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);

            ParticleSystem particleSystem = GlobalSparksDoer.InitializeParticles(GlobalSparksDoer.SparksType.FLOATY_CHAFF);

            Vector3 direction = BraveUtility.RandomAngle().DegreeToVector2();

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = player.CenterPosition + new Vector2(4,0),
                velocity = direction,
                startSize = 1f,
                startLifetime = 2.5f,
                startColor = Color.white,
                randomSeed = (uint)UnityEngine.Random.Range(1, 100)
            };

            particleSystem.Emit(emitParams, 1);

            yield return new WaitForSeconds(3f);

            //=================================================================================

            emitParams = new ParticleSystem.EmitParams
            {
                position = player.CenterPosition + new Vector2(4, 0),
                velocity = direction,
                startSize = 1f,
                startLifetime = 2.5f,
                startColor = Color.white,
                rotation = 10f,
                randomSeed = (uint)UnityEngine.Random.Range(1, 100)
            };

            particleSystem.Emit(emitParams, 1);

            yield return new WaitForSeconds(3f);

            //=================================================================================

            emitParams = new ParticleSystem.EmitParams
            {
                position = player.CenterPosition + new Vector2(4, 0),
                velocity = direction,
                startSize = 1f,
                startLifetime = 2.5f,
                startColor = Color.white,
                rotation = 0f,
                angularVelocity = 10f,
                randomSeed = (uint)UnityEngine.Random.Range(1, 100)
            };

            particleSystem.Emit(emitParams, 1);
        }

        /*private System.Collections.IEnumerator StasisCoroutine(PlayerController player)
        {
            player.healthHaver.TriggerInvulnerabilityPeriod(StasisDuration);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.PreventAllDamage = true;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;

            // find a better color later
            player.sprite.color = ExtendedColours.honeyYellow;
            player.CurrentGun.sprite.color = ExtendedColours.honeyYellow;

            AkSoundEngine.PostEvent("zhonyas_hourglass_activation_SFX", GameManager.Instance.gameObject);

            yield return new WaitForSeconds(StasisDuration);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;

            player.ForceBlank();
            player.CurrentInputState = PlayerInputState.AllInput;

            // wait an extra 0.25 seconds to prevent player from immediate collision damage
            yield return new WaitForSeconds(0.25f);
            player.healthHaver.PreventAllDamage = false;

            AkSoundEngine.PostEvent("zhonyas_hourglass_ending_SFX", GameManager.Instance.gameObject);
        }*/
    }
}
