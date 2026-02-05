using Alexandria.ItemAPI;
using LOLItems.active_items;
using LOLItems.passive_items;
using LOLItems.weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// maybe move synergies in here later (MANAMUNE)

namespace LOLItems
{
    public static class LOLItemsSynergies
    {
        private static int _NUM_SYNERGIES = Enum.GetNames(typeof(Synergy)).Length;                                              // number of synergies our mod adds
        public static List<CustomSynergyType> _Synergies = Enumerable.Repeat<CustomSynergyType>(0, _NUM_SYNERGIES).ToList();    // list of the actual new synergies
        public static List<string> _SynergyNames = Enumerable.Repeat<string>(null, _NUM_SYNERGIES).ToList();                    // list of friendly names of synergies
        public static List<string> _SynergyEnums = new(Enum.GetNames(typeof(Synergy)));                                         // list of enum names of synergies
        public static List<int> _SynergyIds = Enumerable.Repeat<int>(0, _NUM_SYNERGIES).ToList();                               // list of ids of synergies in the AdvancedSynergyEntry database
        
        //NOTE: needs to be done early so guns and items can reference synergy ids properly
        public static void InitEnums()
        {
            // Extend the base game's CustomSynergyType enum to make room for our new synergy
            for (int i = 0; i < _SynergyNames.Count; ++i)
                _Synergies[i] = _SynergyEnums[i].ExtendEnum<CustomSynergyType>();
        }
        public static T ExtendEnum<T>(this string s) where T : System.Enum
        {
            return ETGModCompatibility.ExtendEnum<T>("LOLItems".ToUpper(), s);
        }

        public static void Init()
        {
            /* NOTE:
                - Each synergy entry below must have a comment before the line and the name of the synergy as the first quoted string in the following line
                  This is to ensure that our automatic item tips generation script parses it correctly.
            */

            #region Synergies
            //example: NewSynergy(LOLItems.Synergy., "", new[] { IName(MainItem.ItemName) }, new[] { });

            
            //PASSIVE ITEMS ======================================================================================================================================================================================================================================
            
            
            //Blade of the Ruined King
            NewSynergy(LOLItems.Synergy.YOU_DARE_FACE_A_KING, "You dare face a king?!", new[] { IName(BladeOfTheRuinedKing.ItemName) }, new[] { "crown_of_guns", "gilded_bullets", "coin_crown"});
            NewSynergy(LOLItems.Synergy.FOR_ISOLDE, "For Isolde!", new[] { IName(BladeOfTheRuinedKing.ItemName) }, new[] { "excaliber", "blasphemy"});
            //NewSynergy(LOLItems.Synergy.BORK3, "3", new[] { IName(BladeOfTheRuinedKing.ItemName), IName(GuinsoosRageblade.ItemName) });

            //Cloak of Starry Night
            NewSynergy(LOLItems.Synergy.HEAVEN_AND_EARTH_COMBINED, "Heaven and Earth Combined", new[] { IName(CloakOfStarryNight.ItemName), IName(ShieldOfMoltenStone.ItemName)});

            //Collector
            NewSynergy(LOLItems.Synergy.RETURN_ON_INVESTMENT, "Return on Investment", new[] { IName(Collector.ItemName) }, new[] { "loot_bag", "briefcase_of_cash" });
            NewSynergy(LOLItems.Synergy.STROKE_OF_LUCK, "Stroke of Luck", new[] { IName(Collector.ItemName), "fortunes_favor" });
            NewSynergy(LOLItems.Synergy.AN_OFFERING, "An offering", new[] { IName(Collector.ItemName), "daruma" });
            NewSynergy(LOLItems.Synergy.BETTER_RNG, "Better RNG", new[] { IName(Collector.ItemName), "chance_bullets" });

            //Cull
            NewSynergy(LOLItems.Synergy.WEAK_EARLY_GAME, "weak early game...", new[] { IName(Cull.ItemName), "unfinished_gun" });
            NewSynergy(LOLItems.Synergy.BAUSEN_LAW, "Bausen Law", new[] { IName(Cull.ItemName), "huntsman" });

            //Detonation Orb
            NewSynergy(LOLItems.Synergy.LIFE_AND_DEATH, "Life & Death", new[] { IName(DetonationOrb.ItemName), "life_orb" });
            NewSynergy(LOLItems.Synergy.OVERCHARGED, "OVERCHARGED", new[] { IName(DetonationOrb.ItemName), "raiden_coil" });

            //Experimental Hexplate
            NewSynergy(LOLItems.Synergy.THAT_GOOD_SHIT, "that GOOD $#%&", new[] { IName(ExperimentalHexplate.ItemName), "cigarettes" });
            NewSynergy(LOLItems.Synergy.SPEED_BLITZ, "Speed Blitz", new[] { IName(ExperimentalHexplate.ItemName) }, new[] { "bionic_leg", "shotgun_coffee", "shotga_cola", "ballistic_boots", "magic_sweet" });
            NewSynergy(LOLItems.Synergy.FILLER_UP, "Fill'Er-Up", new[] { IName(ExperimentalHexplate.ItemName), "gungine" });

            //Frozen Heart
            NewSynergy(LOLItems.Synergy.ICE_TO_THE_CORE, "Ice to the core", new[] { IName(FrozenHeart.ItemName) }, new[] { "frost_bullets", "snowballets", "heart_of_ice" });
            NewSynergy(LOLItems.Synergy.FROZEN_BULLETS, "Frozen Bullets", new[] { IName(FrozenHeart.ItemName) }, new[] { "cold_45", "frost_giant"});

            //Guardian Angel
            NewSynergy(LOLItems.Synergy.DIVINE_JUDGEMENT, "Divine Judgement!", new[] { IName(GuardianAngel.ItemName), VirtueForm3.internalName });
            NewSynergy(LOLItems.Synergy.WHY_WONT_YOU_DIE, "Why won't you DIE!", new[] { IName(GuardianAngel.ItemName) }, new[] { "clone", "gun_soul", "pig" });

            //Guinsoo's Rageblade
            NewSynergy(LOLItems.Synergy.BLADES_OF_CHAOS, "Blades of Chaos", new[] { IName(GuinsoosRageblade.ItemName)}, new[] { "chaos_ammolet", "chaos_bullets" });
            NewSynergy(LOLItems.Synergy.POSEIGUNS_WRATH, "Poseigun's Wrath", new[] { IName(GuinsoosRageblade.ItemName), "trident" });
            NewSynergy(LOLItems.Synergy.TRIPLE_DELUXE, "TRIPLE DELUXE", new[] { IName(GuinsoosRageblade.ItemName), IName(BladeOfTheRuinedKing.ItemName), IName(KrakenSlayer.ItemName) });
            //NewSynergy(LOLItems.Synergy.ONHIT_SYNERGY_WITH_KRAKENSLAYER, "On-hit synergy", new[] { IName(GuinsoosRageblade.ItemName), IName(KrakenSlayer.ItemName) });

            //Horizon Focus
            NewSynergy(LOLItems.Synergy.AMPLIFIED_LENS, "Amplified Lens", new[] { IName(HorizonFocus.ItemName) }, new[] { "sniper_rifle", "m1", "awp" });
            NewSynergy(LOLItems.Synergy.FUTURISTIC_COMPATIBILITY, "Futuristic Compatibility", new[] { IName(HorizonFocus.ItemName) }, new[] { "railgun", "prototype_railgun", HextechRifle.internalName });
            NewSynergy(LOLItems.Synergy.GUARANTEED_HIT_IF_IT_HITS, "guaranteed hit \"IF\" it hits", new[] { IName(HorizonFocus.ItemName), "eyepatch" });

            //Hubris
            NewSynergy(LOLItems.Synergy.QUADRATIC_SCALING, "Quadratic Scaling", new[] { IName(Hubris.ItemName), "metronome", "platinum_bullets" });
            NewSynergy(LOLItems.Synergy.GLADITORIAL_CHALLENGE, "Gladitorial Challenge", new[] { IName(Hubris.ItemName), "lament_configurum" });
            NewSynergy(LOLItems.Synergy.PEACE_AND_WAR, "Peace & War", new[] { IName(Hubris.ItemName), "really_special_lute" });

            //Kraken Slayer
            NewSynergy(LOLItems.Synergy.TOP_TIER_FISHING_TOOL, "TOP TIER Fishing Tool", new[] { IName(KrakenSlayer.ItemName) }, new[] { "siren", "trident", "barrel" });
            NewSynergy(LOLItems.Synergy.ENTANGLEMENT, "Entanglement", new[] { IName(KrakenSlayer.ItemName), "abyssal_tentacle" });
            NewSynergy(LOLItems.Synergy.MEGALODON_SLAYER, "Megalodon Slayer", new[] { IName(KrakenSlayer.ItemName), "compressed_air_tank" });
            NewSynergy(LOLItems.Synergy.A_SAILORS_BEST_FRIEND, "A sailor's best friend.", new[] { IName(KrakenSlayer.ItemName), "double_vision" });

            //Fated Ashes
            NewSynergy(LOLItems.Synergy.BUILDS_INTO_LIANDRYS_TORMENT, "builds into Liandry's Torment", new[] { IName(FatedAshes.ItemName), IName(LiandrysTorment.ItemName) });

            //Liandry's Torment
            NewSynergy(LOLItems.Synergy.BURNING_VENGENCE, "Burning Vengence", new[] { IName(LiandrysTorment.ItemName) }, new[] { "phoenix", "napalm_strike" });
            NewSynergy(LOLItems.Synergy.BLAZING_UNIVERSE, "Blazing Universe!", new[] { IName(LiandrysTorment.ItemName) }, new[] { "hot_lead", "gungeon_pepper" });

            //Tear of the Goddess
            NewSynergy(LOLItems.Synergy.BUILDS_INTO_MANAMUNE, "builds into Manamune", new[] { IName(TearOfTheGoddess.ItemName), IName(Manamune.ItemName) });

            //Manamune
            NewSynergy(LOLItems.Synergy.BLADE_OF_THE_ONI_MANAMUNE, "Blade of the Oni", new[] { IName(Manamune.ItemName), "demon_head" });

            //Muramana
            NewSynergy(LOLItems.Synergy.BLADE_OF_THE_ONI_MURAMANA, "Blade of the Oni", new[] { IName(Muramana.ItemName), "demon_head" });
            NewSynergy(LOLItems.Synergy.IT_HAS_TO_BE_THIS_WAY, "It Has To Be This Way", new[] { IName(Muramana.ItemName), "raiden_coil" });
            NewSynergy(LOLItems.Synergy.JETSTREAM_SAM, "Jetstream Sam", new[] { IName(Muramana.ItemName), "bionic_leg" });

            //Navori Quickblades
            NewSynergy(LOLItems.Synergy.SPONSORED_BY_NAVORI, "*sponsored by Navori*", new[] { IName(NavoriQuickblades.ItemName) }, new[] { "knife_shield", "katana_bullets" });
            NewSynergy(LOLItems.Synergy.QUICKBLADES_AND_QUICKBULLETS, "Quickblades and Quickbullets", new[] { IName(NavoriQuickblades.ItemName), "rocket_powered_bullets" });
            NewSynergy(LOLItems.Synergy.LIGHTSLINGER, "Lightslinger", new[] { IName(NavoriQuickblades.ItemName) }, new[] { "big_iron", "the_judge" });

            //Puppeteer
            NewSynergy(LOLItems.Synergy.PLUS25_CHARM, "+25 Charm", new[] { IName(Puppeteer.ItemName) }, new[] { "shotgun_full_of_love", "charmed_bow", "really_special_lute", "charming_rounds" });
            NewSynergy(LOLItems.Synergy.CHARMING_REINVIGORATION, "Charming Reinvigoration", new[] { IName(Puppeteer.ItemName), "charm_horn" });

            //Rod of Ages
            NewSynergy(LOLItems.Synergy.SUPER_TRAINING, "Super Training", new[] { IName(RodOfAges.ItemName), "macho_brace" });
            NewSynergy(LOLItems.Synergy.AGE_OLD_WISDOM, "Age old wisdom", new[] { IName(RodOfAges.ItemName) }, new[] { "old_knights_shield", "old_knights_helm", "old_knights_flask" });
            NewSynergy(LOLItems.Synergy.ARCANE_MASTERY, "Arcane Mastery", new[] { IName(RodOfAges.ItemName), "bundle_of_wands", "staff_of_firepower" });

            //Rylai's Crystal Scepter
            NewSynergy(LOLItems.Synergy.ICE_II, "Ice II", new[] { IName(RylaisCrystalScepter.ItemName) }, new[] { "ice_breaker", "freeze_ray", "glacier", "snowballer" });
            NewSynergy(LOLItems.Synergy.WITCHCRAFT, "witchcraft...", new[] { IName(RylaisCrystalScepter.ItemName) }, new[] { "witch_pistol", "hexagun" });

            //Shadowflame
            //NewSynergy(LOLItems.Synergy.CONFLICTING_EXECUTIONERS, "Conflicting Executioners", new[] { IName(Shadowflame.ItemName), IName(Collector.ItemName) });
            NewSynergy(LOLItems.Synergy.HELLS_SHADOWS, "Hell's Shadows", new[] { IName(Shadowflame.ItemName) }, new[] { "pitchfork", "demon_head" });
            NewSynergy(LOLItems.Synergy.SOLAR_FLAME, "Solar Flame", new[] { IName(Shadowflame.ItemName), "sunlight_javelin" });

            //Silver Bolts
            NewSynergy(LOLItems.Synergy.EXTRA_SILVER, "Extra Silver", new[] { IName(SilverBolts.ItemName), "silver_bullets" });
            NewSynergy(LOLItems.Synergy.THE_NIGHT_HUNTER, "The Night Hunter", new[] { IName(SilverBolts.ItemName), "crossbow" });

            //Statikk Shiv
            NewSynergy(LOLItems.Synergy.STATIKK_ELECTRICITY, "statikk electricity", new[] { IName(StatikkShiv.ItemName) }, new[] { "thunderclap", "laser_lotus" });
            NewSynergy(LOLItems.Synergy.MO_LIGHTNING, "Mo' Lightning!", new[] { IName(StatikkShiv.ItemName), "shock_rounds" });
            NewSynergy(LOLItems.Synergy.EMPEROR_OF_LIGHTNING, "EMPEROR OF LIGHTNING", new[] { IName(StatikkShiv.ItemName), "the_emperor" });

            //Sunfire Aegis
            NewSynergy(LOLItems.Synergy.TRUE_SUN_GOD, "TRUE SUN GOD", new[] { IName(SunfireAegis.ItemName), "sunlight_javelin" });
            NewSynergy(LOLItems.Synergy.VOLTAGE_NOVA, "Voltage Nova", new[] { IName(SunfireAegis.ItemName), "gungeon_pepper" });

            //Thornmail
            NewSynergy(LOLItems.Synergy.ICE_COLD_THORNS, "Ice cold thorns", new[] { IName(Thornmail.ItemName), "heart_of_ice" });
            NewSynergy(LOLItems.Synergy.THORN_MAELSTROM, "Thorn Maelstrom", new[] { IName(Thornmail.ItemName), "armor_of_thorns" });
            NewSynergy(LOLItems.Synergy.OW_SPLINTER, "Ow! Splinter", new[] { IName(Thornmail.ItemName), "wood_beam" });

            //Zeke's Convergence
            NewSynergy(LOLItems.Synergy.ABSOLUTE_CONVERGENCE, "ABSOLUTE CONVERGENCE", new[] { IName(ZekesConvergence.ItemName), "dark_marker" });
            NewSynergy(LOLItems.Synergy.FLAME_OVER_ICE, "Flame > Ice", new[] { IName(ZekesConvergence.ItemName), "hot_lead", "phoenix" });
            NewSynergy(LOLItems.Synergy.ICE_OVER_FLAME, "Ice > Flame", new[] { IName(ZekesConvergence.ItemName), "frost_giant", "frost_bullets" });


            //ACTIVE ITEMS ==============================================================================================================================================================================================================================================================================


            //Galeforce
            NewSynergy(LOLItems.Synergy.GALEFORCE_FOUR, "FOUR!", new[] { IName(Galeforce.ItemName), Whisper.internalName });
            NewSynergy(LOLItems.Synergy.BOW_MASTERY, "Bow Mastery", new[] { IName(Galeforce.ItemName) }, new[] { "bow", "charmed_bow", "gunbow" });

            //Redemption
            NewSynergy(LOLItems.Synergy.ENLIGHTENED_BULLETS, "Enlightened Bullets", new[] { IName(Redemption.ItemName), "silver_bullets" });
            NewSynergy(LOLItems.Synergy.TERROR_TO_THE_GUNDEAD, "Terror to the Gundead", new[] { IName(Redemption.ItemName), "mourning_star" });

            //Refillable Potion
            NewSynergy(LOLItems.Synergy.BUNCH_O_POTIONS, "BunchO Potions", new[] { IName(RefillablePotion.ItemName), "old_knights_flask" });
            NewSynergy(LOLItems.Synergy.COCKTAIL_POTION, "Cocktail Potion", new[] { IName(RefillablePotion.ItemName) }, new[] { "potion_of_lead_skin", "potion_of_gun_friendship" });

            //Stridebreaker
            NewSynergy(LOLItems.Synergy.DEMACIAN_TRAITOR, "Demacian Traitor", new[] { IName(Stridebreaker.ItemName), "betrayers_shield" });

            //Perfectly Timed Stopwatch
            NewSynergy(LOLItems.Synergy.BUILDS_INTO_ZHONYAS_HOURGLASS, "builds into Zhonya's Hourglass", new[] { IName(PerfectlyTimedStopwatch.ItemName), IName(ZhonyasHourglass.ItemName) });

            //Zhonya's Hourglass
            NewSynergy(LOLItems.Synergy.CHAOS_CONTROL, "CHAOS CONTROL!", new[] { IName(ZhonyasHourglass.ItemName), "chaos_bullets", "chaos_bullets" });
            NewSynergy(LOLItems.Synergy.SEVEN_SECONDS_REMAIN, "Seven seconds remain...", new[] { IName(ZhonyasHourglass.ItemName), "super_hot_watch" });



            //WEAPONS ====================================================================================================================================================================================================================================================


            //Crossblade
            NewSynergy(LOLItems.Synergy.BOUNCEMAXXING, "Bouncemaxxing", new[] { Crossblade.internalName }, new[] { "bouncy_bullets", "boomerang" });
            
            //Electric Rifle
            // not sure how this synergy should work with this type of setup, shouldn't cause much issues if i keep it the old setup method
            //NewSynergy(LOLItems.Synergy.PASSIVE_CHARGE, "Passive charge", new[] { IName(ElectricRifle.internalName) }, new[] { "battery_bullets", "shock_rounds", "thunderclap", "shock_rifle", "laser_lotus" });

            //Hextech Rifle
            NewSynergy(LOLItems.Synergy.ME_MISS_NOT_BY_A_LONG_SHOT, "\"Me, miss? Not by a long shot.\"", new[] { HextechRifle.internalName }, new[] { "scope", "laser_sight" });

            //Pow-Pow/Fishbones
            NewSynergy(LOLItems.Synergy.RUNAANS_BULLETS, "Runaan's Bullets", new[] { PowPow.internalName, "scattershot" });

            //Soul Spear
            NewSynergy(LOLItems.Synergy.THREE_PRONGED_SPEARS, "Three-pronged spears", new[] { SoulSpear.internalName }, new[] { "trident", "pitchfork" });

            //Virtue
            NewSynergy(LOLItems.Synergy.EXP_SHARE_FORM_1, "EXP. Share", new[] { VirtueForm1.internalName }, new[] { "macho_brace", "scouter", "life_orb" });
            NewSynergy(LOLItems.Synergy.EXP_SHARE_FORM_2, "EXP. Share", new[] { VirtueForm2.internalName }, new[] { "macho_brace", "scouter", "life_orb" });
            NewSynergy(LOLItems.Synergy.EXP_SHARE_FORM_3, "EXP. Share", new[] { VirtueForm3.internalName }, new[] { "macho_brace", "scouter", "life_orb" });

            #endregion
        }

        private static AdvancedSynergyEntry NewSynergy(Synergy synergy, string name, string[] mandatory, string[] optional = null, bool ignoreLichEyeBullets = false)
        {
            // Get the enum index of our synergy
            int index = (int)synergy;
            // Register the AdvancedSynergyEntry so that the game knows about it
            AdvancedSynergyEntry ase = RegisterSynergy(_Synergies[index], name, mandatory.ToList(), (optional != null) ? optional.ToList() : null, ignoreLichEyeBullets);
            // Index the friendly name of our synergy
            _SynergyNames[index] = name;
            // Get the actual ID of our synergy entry in the AdvancedSynergyDatabase, which doesn't necessarily match the CustomSynergyType enum
            _SynergyIds[index] = GameManager.Instance.SynergyManager.synergies.Length - 1;
            // Return the AdvancedSynergyEntry
            return ase;
        }

        public static AdvancedSynergyEntry RegisterSynergy(CustomSynergyType synergy, string name, List<string> mandatoryConsoleIDs, List<string> optionalConsoleIDs = null, bool ignoreLichEyeBullets = false)
        {
            List<int> itemIDs = new();
            List<int> gunIDs = new();
            List<int> optItemIDs = new();
            List<int> optGunIDs = new();
            foreach (var id in mandatoryConsoleIDs)
            {
                PickupObject pickup = Gungeon.Game.Items[id];
                if (pickup && pickup.GetComponent<Gun>())
                    gunIDs.Add(pickup.PickupObjectId);
                else if (pickup && (pickup.GetComponent<PlayerItem>() || pickup.GetComponent<PassiveItem>()))
                    itemIDs.Add(pickup.PickupObjectId);
            }

            if (optionalConsoleIDs != null)
            {
                foreach (var id in optionalConsoleIDs)
                {
                    PickupObject pickup = Gungeon.Game.Items[id];
                    if (pickup && pickup.GetComponent<Gun>())
                        optGunIDs.Add(pickup.PickupObjectId);
                    else if (pickup && (pickup.GetComponent<PlayerItem>() || pickup.GetComponent<PassiveItem>()))
                        optItemIDs.Add(pickup.PickupObjectId);
                }
            }

            // Add our synergy's name to the string manager so it displays properly when activated
            string nameKey = $"#{name.ToID().ToUpperInvariant()}";
            ETGMod.Databases.Strings.Synergy.Set(nameKey, name);

            AdvancedSynergyEntry entry = new AdvancedSynergyEntry()
            {
                NameKey = nameKey,
                MandatoryItemIDs = itemIDs,
                MandatoryGunIDs = gunIDs,
                OptionalItemIDs = optItemIDs,
                OptionalGunIDs = optGunIDs,
                bonusSynergies = new() { synergy },
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = ignoreLichEyeBullets,
            };

            int oldLength = GameManager.Instance.SynergyManager.synergies.Length;
            Array.Resize(ref GameManager.Instance.SynergyManager.synergies, oldLength + 1);
            GameManager.Instance.SynergyManager.synergies[oldLength] = entry;
            return entry;
        }

        private static string IName(string itemName) => "LOLItems:" + itemName.ToID();
        public static CustomSynergyType Synergy(this Synergy synergy) => _Synergies[(int)synergy];
        public static string SynergyName(this Synergy synergy) => _SynergyNames[(int)synergy];
        public static bool HasSynergy(this PlayerController player, Synergy synergy) => player.ActiveExtraSynergies.Contains((int)_SynergyIds[(int)synergy]);

        
    }

    public enum Synergy
    {
        // Synergies
        RETURN_ON_INVESTMENT,
        STROKE_OF_LUCK,
        AN_OFFERING,
        BETTER_RNG,
        HEAVEN_AND_EARTH_COMBINED,
        ICE_TO_THE_CORE,
        FROZEN_BULLETS,
        GALEFORCE_FOUR,
        BOW_MASTERY,
        SUPER_TRAINING,
        AGE_OLD_WISDOM,
        //PASSIVE_CHARGE,
        YOU_DARE_FACE_A_KING,
        FOR_ISOLDE,
        //BORK3,
        BOUNCEMAXXING,
        ME_MISS_NOT_BY_A_LONG_SHOT,
        EXP_SHARE_FORM_1,
        EXP_SHARE_FORM_2,
        EXP_SHARE_FORM_3,
        WEAK_EARLY_GAME,
        BAUSEN_LAW,
        THAT_GOOD_SHIT,
        SPEED_BLITZ,
        FILLER_UP,
        DIVINE_JUDGEMENT,
        WHY_WONT_YOU_DIE,
        BUNCH_O_POTIONS,
        COCKTAIL_POTION,
        BLADES_OF_CHAOS,
        POSEIGUNS_WRATH,
        TRIPLE_DELUXE,
        //ONHIT_SYNERGY_WITH_KRAKENSLAYER,
        AMPLIFIED_LENS,
        FUTURISTIC_COMPATIBILITY,
        GUARANTEED_HIT_IF_IT_HITS,
        QUADRATIC_SCALING,
        GLADITORIAL_CHALLENGE,
        PEACE_AND_WAR,
        TOP_TIER_FISHING_TOOL,
        ENTANGLEMENT,
        MEGALODON_SLAYER,
        A_SAILORS_BEST_FRIEND,
        BUILDS_INTO_LIANDRYS_TORMENT,
        BUILDS_INTO_MANAMUNE,
        BUILDS_INTO_ZHONYAS_HOURGLASS,
        BLADE_OF_THE_ONI_MANAMUNE,
        BLADE_OF_THE_ONI_MURAMANA,
        IT_HAS_TO_BE_THIS_WAY,
        JETSTREAM_SAM,
        BURNING_VENGENCE,
        BLAZING_UNIVERSE,
        SPONSORED_BY_NAVORI,
        QUICKBLADES_AND_QUICKBULLETS,
        LIGHTSLINGER,
        PLUS25_CHARM,
        CHARMING_REINVIGORATION,
        ARCANE_MASTERY,
        ICE_II,
        WITCHCRAFT,
        //CONFLICTING_EXECUTIONERS,
        HELLS_SHADOWS,
        SOLAR_FLAME,
        STATIKK_ELECTRICITY,
        MO_LIGHTNING,
        EMPEROR_OF_LIGHTNING,
        TRUE_SUN_GOD,
        VOLTAGE_NOVA,
        ICE_COLD_THORNS,
        THORN_MAELSTROM,
        OW_SPLINTER,
        ABSOLUTE_CONVERGENCE,
        FLAME_OVER_ICE,
        ICE_OVER_FLAME,
        ENLIGHTENED_BULLETS,
        TERROR_TO_THE_GUNDEAD,
        DEMACIAN_TRAITOR,
        CHAOS_CONTROL,
        SEVEN_SECONDS_REMAIN,
        THREE_PRONGED_SPEARS,
        RUNAANS_BULLETS,
        EXTRA_SILVER,
        THE_NIGHT_HUNTER,
        LIFE_AND_DEATH,
        OVERCHARGED
    };
}
