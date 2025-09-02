using BepInEx;
using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.SoundAPI;
using LOLItems.passive_items;
using LOLItems.active_items;

//bother balancing these items later

namespace LOLItems
{
    [BepInDependency(Alexandria.Alexandria.GUID)] // this mod depends on the Alexandria API: https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "Ricky2148.etg.LOLItems";
        public const string NAME = "League of legends Items";
        public const string VERSION = "1.1.0";
        public const string TEXT_COLOR = "#FF007F";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            BladeOfTheRuinedKing.Init();
            ExamplePassive.Register();
            ExperimentalHexplate.Init();
            GuardianAngel.Init();
            GuinsoosRageblade.Init();
            Hubris.Init();
            KrakenSlayer.Init();
            LiandrysTorment.Init();
            Manamune.Init();
            Muramana.Init();
            StatikkShiv.Init();
            Stridebreaker.Init();
            SunfireAegis.Init();
            Thornmail.Init();
            ZhonyasHourglass.Init();
            //Redemption.Init();

            // new update?
            Collector.Init();
            FrozenHeart.Init();
            RodOfAges.Init();
            HorizonFocus.Init();
            Puppeteer.Init();
            Galeforce.Init();
            RylaisCrystalScepter.Init();
            Shadowflame.Init();
            NavoriQuickblades.Init();
            SoundManager.LoadSoundbanksFromAssembly();
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }

        public static void Log(string text, string color= "#FF007F")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
