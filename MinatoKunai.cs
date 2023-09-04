using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace PathOfTheNinja
{
    public class MinatoKunai : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MinatoKunaiMono>();
        }
    }

    public class MinatoKunaiMono : MonoBehaviour
    {
        Item item;
        RagdollHand listeningHand;

        private void Start()
        {
            item = GetComponent<Item>();
            item.OnUngrabEvent += Item_OnUngrabEvent;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            if (!throwing) return;
            listeningHand = ragdollHand;
            item.OnGrabEvent += Item_OnGrabEvent;
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            if (!listeningHand) return;
            listeningHand = null;
        }

        public bool CanTeleport()
        {
            if (listeningHand == null) return false;
            if (listeningHand.grabbedHandle != null) return false;
            if(listeningHand.caster.isFiring || listeningHand.caster.isMerging || listeningHand.caster.isSpraying) return false;



            return true;
        }

        public void Teleport()
        {

            Player.local.Teleport(item.transform, PotNMaster.keepVeloTele);


            if (PotNMaster.grabOnTele) listeningHand.Grab(item.GetMainHandle(listeningHand.side), true);


            listeningHand = null;
        }
        void Update()
        {
            if (!listeningHand) return;

            if (listeningHand.playerHand.controlHand.alternateUsePressed)
            {
                if (!CanTeleport()) return;
                Teleport();
            }
        }
    }
}
