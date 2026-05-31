using Alexandria.DungeonAPI;
using Alexandria.ItemAPI;
using Alexandria.NPCAPI;
using Dungeonator;
using LOLItems.active_items;
using LOLItems.guon_stones;
using LOLItems.passive_items;
using LOLItems.weapons;
using LootTableAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

// check appearance weights
// update item list before release

namespace LOLItems
{
    public static class Bubbs
    {
        public static GenericLootTable ShopKeeperLootTable;
        public static GameObject HandyGameObject;
        public static void Init()
        {
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Everything I say is {ws}waterproof!{w} Or if not, {wq}water-absorbent!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Perhaps I could tell you about my impending odyssey!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Frugality is not a virtue, I assure you.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Purchase thoroughly! You'll never know when I shove off!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "You look like an aspiring patron of scientific research!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "{ws}The sea is the final frontier!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "I've been designing some new fins!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "{wq}Did you know?:{w} clams can smell colours!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Once, people could breathe in water! Then we forgot how.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Whale crabs. Octo-salmon. Barnacle sharks. Who knows what I'll find?");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "The helmet never comes off! Well, except when I get hungry.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "I theorize that beneath this ocean, there is another, wetter, ocean!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Undersea exploration isn't a job, it's a {wb}privilege!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "I've done several trial runs in local puddles. I'm ready for the big show!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Sea monsters are no more than misunderstood {wj}ambassadors of the deep.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Exploring the sea in a boat is like eating a melon by the rind!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Bilgewater is a lovely place—principally when facing out to sea.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "My research indicates that if you hold your breath long enough, you can kick the habit! You just need to stay awake.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "One day, the ocean will be my oyster—and the ocean's oysters will be my breakfast!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Oh, don't get me started on {ws}buoyancy.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Gadgetry in Piltover, extinct overlords in Freljord? Hmph! I'll show them!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "There's evidence to suggest that manatees once ruled the {wr}land and sea!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "I can't wait to school some fish.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "So many {wj}mysteries{w} to explore.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "I can't wait to see what lies beneath!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "The real sunken {wb}treasure{w} is aquatic knowledge.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_GENERIC_TALK", "Just as soon as I perfect my submersible, I'll be off!");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_STOPPER_TALK", "{wj}Soon, very soon...{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_STOPPER_TALK", "{wb}Every coin counts.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_STOPPER_TALK", "{wq}What mysteries await?{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_STOPPER_TALK", "{ws}To knowledge!{w}");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "{wb}Perfect!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "{wb}Delightful!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "{wb}Impeccable taste!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "My, that is a {wq}beauty!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Every little bit helps!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "You've just supported science!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "That is a singular find… oh, but I have more, if you need them!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "It complements you {wq}perfectly.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "That is guaranteed for, at a minimum, ballast.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "I've sold four of those today!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "That will spur—or deter—violence swimmingly!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Yeah, I have absolutely no idea what that does. {wr}Good luck!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Thank you for contributing to my expedition.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Would you like that wrapped?");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "{wj}Uh, sorry,{w} I haven't gotten around to drying that off!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "I prefer you only killed bad people with this—or nautical sceptics.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "That is, {wj}uh… most probably valuable!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "A bare minimum of rust.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "No price is too great for {wb}progress.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "That will serve you {wb}splendidly.{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Keep this up, and I'll be diving in no time.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "You'll receive a special thanks in my paper.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Now that you've got the hang of it, {wq}purchase something else!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Why not pick up a gift… {wr}for science?{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Your gold will be put to good use.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "Now you're part of the adventure!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "No need to explain what you plan to do with that.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "I can tell you're a collector.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_PURCHASE_TALK", "There's simply no better… {wj}for whatever that does.{w}");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Ah, yes, but science, {wj}tragically{w}, doesn’t operate on credit.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "{wq}I’d love to fund your dreams{w}, but I can barely fund my own!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "{wb}That’s the spirit!{w} The financial spirit, however, is lacking.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Sorry, but seawater doesn’t pay the bills.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "{wr}If only enthusiasm were a currency!{w} You’d be rich indeed.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "That price is, {wj}regrettably{w}, non-negotiable—even for pioneers.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Oh, don’t worry. Many great expeditions ended before they began.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Perhaps a smaller trinket? To, ah, {ws}stay afloat?{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "{wr}Close!{w} Now, just a few more clams in the purse, as they say.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "I accept gold, doubloons, and… well, mostly just gold.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Research may be priceless, but my wares are not.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "I could loan it to you, but then I’d need to repossess it underwater.");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "Come now, even barnacles save more diligently!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_NOSALE_TALK", "{wb}Ah, a true visionary!{w} Sadly, visions don’t spend well at market.");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_INTRO_TALK", "All profits go to {wq}cutting-edge{w} research!");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_INTRO_TALK", "Have I read you my thesis on {wj}sea slugs?{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_INTRO_TALK", "{wq}Welcome to the dive site!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_INTRO_TALK", "Would you like to join my {wr}expedition?{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_INTRO_TALK", "I brew my potions with salt water for that extra {wb}zing!{w}");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_ATTACKED_TALK", "{wr}Uncalled for!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_ATTACKED_TALK", "{wj}No discounts!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_ATTACKED_TALK", "{wr}Not the equipment!{w}");
            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_ATTACKED_TALK", "{wr}Science under siege!{w}");

            ETGMod.Databases.Strings.Core.AddComplex("#BUBBS_STOLEN_TALK", "{wr}Robbery{w} is hardly the scientific method!");

            List<int> LootTable = new List<int>()
            {
                BladeOfTheRuinedKing.ID,
                GuardianAngel.ID,
                GuinsoosRageblade.ID,
                Hubris.ID,
                KrakenSlayer.ID,

                FatedAshes.ID,
                LiandrysTorment.ID,

                TearOfTheGoddess.ID,
                Manamune.ID,

                StatikkShiv.ID,
                Stridebreaker.ID,
                SunfireAegis.ID,
                Thornmail.ID,

                PerfectlyTimedStopwatch.ID,
                ZhonyasHourglass.ID,

                Collector.ID,
                FrozenHeart.ID,
                RodOfAges.ID,
                HorizonFocus.ID,
                Puppeteer.ID,
                Galeforce.ID,
                RylaisCrystalScepter.ID,
                Shadowflame.ID,
                NavoriQuickblades.ID,
                BraumsShield.ID,

                ShieldOfMoltenStone.ID,
                CloakOfStarryNight.ID,

                ExperimentalHexplate.ID,
                ZekesConvergence.ID,
                Redemption.ID,

                Sheen.ID,
                TrinityForce.ID,
                DivineSunderer.ID,
                EssenceReaver.ID,
                LichBane.ID,

                Cull.ID,
                DetonationOrb.ID,
                RefillablePotion.ID,
                TalismanOfAscension.ID,
                SilverBolts.ID,

                PowPow.ID,
                HextechRifle.ID,
                ElectricRifle.ID,
                PrayerBeads.ID,
                Whisper.ID,
                Crossblade.ID,
                VirtueForm1.ID,
                SoulSpear.ID,
                Pinger.ID,
            };

            ShopKeeperLootTable = LootTableTools.CreateLootTable();
            foreach (int i in LootTable)
            {
                ShopKeeperLootTable.AddItemToPool(i);
            }
            
            GameObject BubbsObj = Alexandria.NPCAPI.ShopAPI.SetUpShop(
                "BUBBS", //name
                "LOLItems", //prefix
                new List<string>() //idle
                {
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_idle_01",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_idle_02",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_idle_03",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_idle_04"
                },
                6, //idle fps
                // talk animation moves sprite 1 pixel to the right, try to fix it idk how lmao
                new List<string>() //talk
                {
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_talk_01",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_talk_02",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_talk_03",
                    "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_talk_04"
                },
                6, //talk fps
                ShopKeeperLootTable, //table
                Alexandria.NPCAPI.CustomShopItemController.ShopCurrencyType.COINS, //cost

                "#BUBBS_GENERIC_TALK",
                "#BUBBS_STOPPER_TALK",
                "#BUBBS_PURCHASE_TALK",
                "#BUBBS_NOSALE_TALK",
                "#BUBBS_INTRO_TALK",
                "#BUBBS_ATTACKED_TALK",
                "#BUBBS_STOLEN_TALK",
                    
                //talk stuff
                new Vector3(44/16f, 46/16f, 0), //talk point
                new Vector3(3/16f, 48/16f, 0), //NPC position
                Alexandria.NPCAPI.ShopAPI.VoiceBoxes.DOUG,
                Alexandria.NPCAPI.ShopAPI.defaultItemPositions,

                0.8f, //cost mod
                false, //stats on purchase
                null, //no stats on purchase
                null, // custom can buy?
                null, // custom remove currency?
                null, // custom price
                null, // on buy
                null, // on rob
                null, // currency icon
                null, // currency name
                true, // can be robbed
                true, // has carpet
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_carpet",
                new Vector3(0, 0, 0), // carpet offset
                true, // minimap icon?
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_minimap", // minimap icon sprite
                true, // in main shop pool?
                0.1f // weight for main pool (default 0.1f)
            );
            
            List<string> purchaseAnimationSpritePathList = new List<string>
            {
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_01",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_02",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_03",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_04",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_05",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_06",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_07",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_08",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_09",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_10",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_11",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_12",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_13",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_14",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_15",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_16",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_17",
                "LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/bubbs_buy_18"
            };

            //ShopAPI.AddUnparentedAnimationToShop(BubbsObj, purchaseAnimationSpritePathList, 6, "purchase");
            ShopAPI.AddAdditionalAnimationsToShop(BubbsObj, purchaseAnimationSpritePathList, 6);

            PrototypeDungeonRoom Mod_Shop_Room = RoomFactory.BuildNewRoomFromResource("LOLItems/Resources/npc_sprites/shopkeeper/bubbs_sprites/test.newroom").room;
            RegisterShopRoom(BubbsObj, Mod_Shop_Room, new UnityEngine.Vector2(7.5f, 5f),1.2f); // 1.2

            GameManager.Instance.GlobalInjectionData.entries[2].injectionData.InjectionData.Add(new ProceduralFlowModifierData()
            {
                annotation = "bubbs",
                placementRules = new List<ProceduralFlowModifierData.FlowModifierPlacementType>()
                {
                    ProceduralFlowModifierData.FlowModifierPlacementType.END_OF_CHAIN
                },
                exactRoom = Mod_Shop_Room,
                prerequisites = new DungeonPrerequisite[0],
                CanBeForcedSecret = false,
            });

            HandyGameObject = BubbsObj;
        }

        public static void RegisterShopRoom(GameObject shop, PrototypeDungeonRoom protoroom, Vector2 vector, float m_weight = 1)
        {
            protoroom.category = PrototypeDungeonRoom.RoomCategory.NORMAL;
            DungeonPrerequisite[] array = shop.GetComponent<CustomShopController>()?.prerequisites != null ? shop.GetComponent<CustomShopController>().prerequisites : new DungeonPrerequisite[0];
            //Vector2 vector = new Vector2((float)(protoroom.Width / 2) + offset.x, (float)(protoroom.Height / 2) + offset.y);
            protoroom.placedObjectPositions.Add(vector);
            protoroom.placedObjects.Add(new PrototypePlacedObjectData
            {
                contentsBasePosition = vector,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = array,
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = new DungeonPlaceable
                {
                    width = 2,
                    height = 2,
                    respectsEncounterableDifferentiator = true,
                    variantTiers = new List<DungeonPlaceableVariant>
                    {
                        new DungeonPlaceableVariant
                        {
                            percentChance = 1f,
                            nonDatabasePlaceable = shop,
                            prerequisites = array,
                            materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    }
                }
            });
            RoomFactory.RoomData roomData = new RoomFactory.RoomData
            {
                room = protoroom,
                isSpecialRoom = true,
                category = "SPECIAL",
                specialSubCategory = "WEIRD_SHOP",

            };
            roomData.weight = 1f;
            RoomFactory.rooms.Add(shop.name, roomData);
            DungeonHandler.Register(roomData);
        }
    }
}