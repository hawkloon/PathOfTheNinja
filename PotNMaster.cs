using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using System.Collections;
using UnityEngine;
using IngameDebugConsole;

namespace PathOfTheNinja
{
    public class PotNMaster : ThunderScript
    {
        public static ModOptionInt[] oneToTen()
        {
            ModOptionInt[] options = new ModOptionInt[10];
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionInt(i.ToString("0"), i);
            }
            return options;
        }


        [ModOption("Pick Up Kunai On Teleport", category = "Teleportation Jutsu", categoryOrder = 0, defaultValueIndex = 0, tooltip = "Whether or not the Kunai is teleported to your hand after teleported", interactionType = ModOption.InteractionType.ButtonList)]
        public static bool grabOnTele;


        [ModOption("Keep Velocity on Teleport", category = "Teleportation Jutsu", defaultValueIndex = 1, tooltip = "if active, you will keep all velocity after teleporting", interactionType = ModOption.InteractionType.ButtonList)]
        public static bool keepVeloTele;

        [ModOption("Change Clothing", category = "Six Paths Sage Mode", defaultValueIndex = 1, interactionType = ModOption.InteractionType.ButtonList)]
        public static bool changeClothing;

        [ModOptionSlider]
        [ModOptionCategory("SixPathsSageMode", 1)]
        [ModOptionTooltip("Amount of orbs you six paths sage mode begins with")]
        [ModOption("Amount of Truthseeking Orbs", valueSourceName = nameof(oneToTen), defaultValueIndex = 7)]
        
        public static int orbAmount;


        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            DebugLogConsole.AddCommand("ToggleSixPaths", "Toggles Six Paths Sage Mode", TryAddSixPaths);
            DebugLogConsole.AddCommand("LoseOrb", "Removes an orb from the existing circle", TryRemoveOrb);
            DebugLogConsole.AddCommand<Side>("FlingOrb", "Throws orb at creature!", FlingOrb,"side");
            DebugLogConsole.AddCommand<Side>("OrbToHand", "Moves an orb to the Hand", OrbToHand, "side");
            DebugLogConsole.AddCommand<Side>("ShootOrb", "Shoots orb!", ShootOrb, "side");
            EventManager.onPossess += EventManager_onPossess;
        }

        public IEnumerator EyeSetDelay(Creature creature)
        {
            yield return Yielders.ForRealSeconds(5f);
            Utils.originalEyeMaterial = creature.GetEyeMaterial();
         }
        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            { 
                GameManager.local.StartCoroutine(EyeSetDelay(creature));
                DebugLogConsole.ExecuteCommand("setspectatorshooter DummyProjectile 10");
            }
        }

        void ShootOrb(Side side)
        {
            SixPathsSageMode.local.ShootOrb(side);
        }
        void OrbToHand(Side side)
        {
            SixPathsSageMode.local.OrbToHand(side);
        }
        void FlingOrb(Side side)
        {
            foreach(Creature creature in Creature.allActive)
            {
                if (creature.isPlayer || creature.isKilled) continue;

                SixPathsSageMode.local.StartCoroutine(SixPathsSageMode.local.FlingOrb(creature, side));
            }
        }
        void TryRemoveOrb()
        {
            if (SixPathsSageMode.local == null || SixPathsSageMode.local.activeOrbs == null) return;
            SixPathsSageMode.local.LoseOrb();
        }
        void TryAddSixPaths()
        {
            Player.currentCreature.gameObject.TryGetOrAddComponent<SixPathsSageMode>(out SixPathsSageMode s);
        }
    }
}
