using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using BepInEx;
using HarmonyLib;
using LOLItems.active_items;
using LOLItems.guon_stones;
using LOLItems.passive_items;
using LOLItems.weapons;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using SGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

//bother balancing these items later
// update bubbs with item list

namespace LOLItems
{
    [BepInDependency(Alexandria.Alexandria.GUID)] // this mod depends on the Alexandria API: https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "Ricky2148.etg.LOLItems";
        public const string NAME = "League of legends Items";
        public const string VERSION = "3.0.1";
        public const string TEXT_COLOR = "#F1C232";

        internal static Harmony _Harmony;

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            ETGMod.Assets.SetupSpritesFromAssembly(Assembly.GetExecutingAssembly(), "LOLItems/Resources/weapon_sprites");
            SoundManager.LoadSoundbanksFromAssembly();

            _Harmony = new Harmony(GUID);
            //_Harmony.PatchAll(Assembly.GetExecutingAssembly());

            /*
            if (_Harmony == null)
            {
                Log("harmony is null");
                _Harmony.PatchAll();
            }
            else
            {
                Log("harmony is not null");
                //_Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            */

            #region Items
            BladeOfTheRuinedKing.Init();
            ExperimentalHexplate.Init();
            GuardianAngel.Init();
            GuinsoosRageblade.Init();
            Hubris.Init();
            KrakenSlayer.Init();
            LiandrysTorment.Init();
            //MuramanaSynergyActivation.Init();
            Manamune.Init();
            Muramana.Init();
            StatikkShiv.Init();
            Stridebreaker.Init();
            SunfireAegis.Init();
            Thornmail.Init();
            ZhonyasHourglass.Init();

            //new update?
            Collector.Init();
            FrozenHeart.Init();
            RodOfAges.Init();
            HorizonFocus.Init();
            Puppeteer.Init();
            Galeforce.Init();
            RylaisCrystalScepter.Init();
            Shadowflame.Init();
            NavoriQuickblades.Init();

            //testing
            //CarefreeMelody.Init();
            //debugItem.Init();

            //guon stones
            BraumsShield.Init();

            //new items
            ShieldOfMoltenStone.Init();
            CloakOfStarryNight.Init();
            ZekesConvergence.Init();
            Redemption.Init();

            //low tier items
            Sheen.Init();
            FatedAshes.Init();
            Cull.Init();
            PerfectlyTimedStopwatch.Init();
            TearOfTheGoddess.Init();

            //next update items
            DetonationOrb.Init();
            RefillablePotion.Init();
            TalismanOfAscension.Init();
            SilverBolts.Init();
            #endregion

            //weapons
            //BasicGun.Add();
            //TemplateGun.Add();
            PowPow.Add();
            PowPowAltForm.Add();
            HextechRifle.Add();
            ElectricRifle.Add();
            PrayerBeads.Add();
            Whisper.Add();
            Crossblade.Add();
            VirtueForm1.Add();
            VirtueForm2.Add();
            VirtueForm3.Add();
            SoulSpear.Add();
            //GauntletOfNeZuk.Add(); UNFINISHED

            #region NPCs
            Bubbs.Init();
            #endregion

            #region Synergies
            LOLItemsSynergies.Init();
            #endregion

            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
            Log("Thirty seconds until minions spawn!", "#155DFC");

            /*var myOriginalMethods = _Harmony.GetPatchedMethods();
            foreach (var method in myOriginalMethods) 
            {
                Log("Patched Method: " + method.DeclaringType.FullName + "." + method.Name, TEXT_COLOR);
            }*/
        }

        public static void Log(string text, string color= "#FF007F")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
