using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using System.Collections;   
namespace PathOfTheNinja
{
    public class SixPathsSpell : SpellCastCharge
    {
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (spellCaster.ragdollHand.creature.gameObject.GetComponent<SixPathsSageMode>() == null && active) spellCaster.ragdollHand.creature.gameObject.AddComponent<SixPathsSageMode>();

            else
            {
                if (!active) return;
                GameObject.Destroy(spellCaster.ragdollHand.creature.GetComponent<SixPathsSageMode>());
            }
        }

    }


    public class SixPathsSageMode : MonoBehaviour
    {
        public static SixPathsSageMode local;
        public List<Item> activeOrbs;
        public ItemData orbData;
        public GameObject orbsParent;
        public int orbAmount;

        public Item leftOrb;
        public Item rightOrb;


        public Item GetOrbFromSide(Side side) => side == Side.Left ? leftOrb : rightOrb;

        void Start()
        {
            if (local == null) local = this;
            else
            {
                return;
            }
            if (PotNMaster.changeClothing)
            {
                Player.currentCreature.equipment.EquipWardrobe(new ContainerData.Content(Catalog.GetData<ItemData>("PotN.SixPaths.Clothing")));
            }
            Catalog.LoadAssetAsync<Texture>("PotN.SixPaths.Eyes", eyeText =>
            {
                Player.currentCreature.SetEyeTexture(eyeText);
            }, "PotN.SixPaths.Eyes");
            activeOrbs = new List<Item>();
            orbData = Catalog.GetData<ItemData>("PotN.TruthseekingOrb");
            orbAmount = PotNMaster.orbAmount;
            PlayerControl.handLeft.OnButtonPressEvent += HandLeft_OnButtonPressEvent;
            PlayerControl.handRight.OnButtonPressEvent += HandRight_OnButtonPressEvent;
            InitOrbs();
            foreach(var orb in activeOrbs)
            {
                orb.transform.parent = orbsParent.transform;
            }
        }

        private void HandRight_OnButtonPressEvent(PlayerControl.Hand.Button button, bool pressed)
        {
            ControlParser(button, pressed, Side.Right);
        }

        private void HandLeft_OnButtonPressEvent(PlayerControl.Hand.Button button, bool pressed)
        {
            ControlParser(button, pressed, Side.Left);
        }

        private void ControlParser(PlayerControl.Hand.Button button, bool pressed, Side side)
        {
            if(button == PlayerControl.Hand.Button.Grip && Player.currentCreature.GetHand(side).playerHand.controlHand.usePressed)
            {
                if (GetOrbFromSide(side) != null) return;
                OrbToHand(side);
            }
        }

        public void SetUpOrbParent()
        {
            orbsParent = new GameObject("OrbParent");
            orbsParent.transform.parent = Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Chest).transform;
            orbsParent.transform.localPosition = new Vector3(-0.453999999f, 0, -0.259000003f);
            orbsParent.transform.localEulerAngles = Vector3.zero;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = orbsParent.transform.up;
            cube.transform.rotation = Quaternion.identity;
        }
        public IEnumerator MoveOrb(Item orb, Vector3 Start, Vector3 End, float orbMoveIncrement)
        {
            float f = 0;
            Debug.Log(Start + " " + End);
            orb.transform.parent = orbsParent.transform;
            orb.transform.localPosition = Start;
            while (f <= 1)
            {
                if (orb == null) yield break;
                orb.transform.localPosition = Vector3.Lerp(Start, End, f);
                f += orbMoveIncrement;
                yield return new WaitForSeconds(orbMoveIncrement / 10);
            }
            orb.transform.localPosition = End;
        }

        public void OrbToHand(Side side)
        {
            var orb = LoseOrb();
            if (!orb) return;
            orb.transform.parent = Player.currentCreature.GetHand(side).transform;
            orb.transform.localPosition = new Vector3(0, 0, 0.1f);
            orb.physicBody.isKinematic = true;
            foreach(var c in orb.colliderGroups[0].colliders)
            {
                c.enabled = false;
            }
            if (side == Side.Right) rightOrb = orb;
            else leftOrb = orb;
        }

        public void ShootOrb(Side side)
        {
            var orb = GetOrbFromSide(side);
            if (orb == null) return;
            foreach (var c in orb.colliderGroups[0].colliders)
            {
                c.enabled = true;
            }
            orb.physicBody.isKinematic = false;
            orb.physicBody.AddForce(Player.currentCreature.GetHand(side).PointDir * 50, ForceMode.Impulse);
            orb.Throw();
            orb.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
        }
        public IEnumerator FlingOrb(Creature creature, Side side)
        {
            Debug.Log($"Fling Orb called");
            var orb = GetOrbFromSide(side);
            if(!orb)
            {
                Debug.Log($"Orb is null returning");

                yield break;
            }
            orb.physicBody.isKinematic = false;
            var targetPart = creature.ragdoll.targetPart.transform;
            orb.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            while(Vector3.Distance(orb.transform.position, targetPart.position) > 0.5f)
            {
                orb.physicBody.AddForce((targetPart.position - orb.transform.position).normalized * 100f, ForceMode.Force);
                yield return Yielders.EndOfFrame;
            }

        }

        public IEnumerator RecallOrbToHand(Side side)
        {
            var orb = GetOrbFromSide(side);
            while (Vector3.Distance(orb.transform.position, Player.currentCreature.GetHand(side).transform.position) > 0.5f)
            {
                orb.physicBody.AddForce((Player.currentCreature.GetHand(side).transform.position - orb.transform.position).normalized * 100f, ForceMode.Force);
                yield return Yielders.EndOfFrame;
            }
            orb.transform.parent = Player.currentCreature.GetHand(side).transform;
            orb.transform.localPosition = new Vector3(0, 0, 0.4f);
            orb.physicBody.isKinematic = true;
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if(collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart?.ragdoll?.creature?.isPlayer == false)
            {

            }
        }

        public Item LoseOrb(int numOfOrbs = 1)
        {
            if (orbAmount <= 0)
            {
                Debug.Log($"Orb num is less than or equal to 0!");
                return null;
            }
            orbAmount -= numOfOrbs;
            if (orbAmount < 0) orbAmount = 0;
            float angle = 0;
            if (orbAmount > 0) angle = 360 / orbAmount;
            int i = 0;
            if (activeOrbs == null)
            {
                Debug.Log($"No Orbs are active!");
                return null;
            }
            Item returnOrb = null;
            foreach (var orb in activeOrbs)
            {
                i++;
                if (orb.transform.parent != orbsParent.transform) Debug.LogWarning($"ORB HAS NO PARENT!");
                orb.transform.SetParent(orbsParent.transform);
                if (i > orbAmount)
                {
                   if (!returnOrb) returnOrb = orb;
                    else orb.Despawn();
                    continue;
                }
                var originalPos = orb.transform.localPosition;
                orb.transform.localPosition = Vector3.zero;
                orb.transform.localEulerAngles = new Vector3(0, 0, (90 + (angle * (1 + i))));
                orb.transform.position += orb.transform.up * 0.3f;
                StartCoroutine(MoveOrb(orb, originalPos, orb.transform.localPosition, 0.1f));
            }
            if(returnOrb)activeOrbs.Remove(returnOrb);
            return returnOrb;   
        }

            public void InitOrbs()
           {
            if (activeOrbs != null)
            {
                foreach (var item in activeOrbs)
                {
                    item?.Despawn();
                }
            }
            if (PotNMaster.orbAmount <= 0)
            {
                Debug.Log($"Orb Amount is 0!");
                return;
            }
            SetUpOrbParent();
            float angle = 360 / PotNMaster.orbAmount;

            if(PotNMaster.orbAmount == 0)
            {
                Debug.Log($"Orb Amount is 0");
                return;
            }
            for(int i = 0; i < PotNMaster.orbAmount; i++)
            {
                if(orbData == null || orbsParent == null || Player.currentCreature == null || activeOrbs == null)
                {
                    Debug.Log($"Something reasonable is null");
                    return;
                }
                Catalog.GetData<ItemData>("PotN.TruthseekingOrb").SpawnAsync(orb =>
                {
                    orb.transform.parent = orbsParent.transform;
                    orb.transform.localPosition = Vector3.zero;
                    orb.transform.localEulerAngles = new Vector3(0, 0, (90 + (angle * (1 + i))));
                    orb.transform.position += orb.transform.up * 0.3f;
                    orb.physicBody.isKinematic = true;
                    orb.physicBody.useGravity = false;
                    orb.disallowDespawn = true;
                    orb.Set("cullingDetectionEnabled", false);
                    if (activeOrbs == null) activeOrbs = new List<Item>();
                    activeOrbs.Add(orb);
                    Debug.Log($"Orb spawned and added");
                    Debug.Log($"Orb : {orb.transform.position}, Player: {Player.currentCreature?.transform.position}");
                    orb.transform.parent = orbsParent.transform;
                }, pooled: true) ;
            }
        }

        void OnDestroy()
        {
            foreach(var orb in activeOrbs)
            {
                orb.Despawn();
            }
            activeOrbs.Clear();
            activeOrbs = null;
            local = null;
            orbData = null;
            Player.currentCreature.RevertEyeTexture();
            Destroy(orbsParent);
        }
    }




    public class OrbBehaviour : MonoBehaviour
    {

    }
}
