using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace PathOfTheNinja
{
    public class LightningSpear : SpellCastCharge
    {
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {
                Debug.Log($"Fire called");
                Catalog.GetData<ItemData>("PotN.LightningSpear").SpawnAsync(spear =>
                {
                    spellCaster.Fire(false);
                    spellCaster.ragdollHand.Grab(spear.GetMainHandle(spellCaster.side), true);
                    spear.gameObject.AddComponent<LightningSpearMono>();
                }, spellCaster.transform.position, spellCaster.transform.rotation);
            }
        }
    }

    public class LightningSpearMono : MonoBehaviour
    {
        Item item;
        LineRenderer lineRenderer;
        Damager damager;

        public void Start()
        {
            item = GetComponent<Item>();
            lineRenderer = item.GetCustomReference<LineRenderer>("LineRenderer");
            damager = item.GetCustomReference<Damager>("Pierce");
        }

        void Update()
        {
            lineRenderer.SetPosition(1, damager.transform.position);
        }
    }
}
