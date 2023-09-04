using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using System.Reflection;

namespace PathOfTheNinja
{
    public static class Utils 
    {

        public static Material originalEyeMaterial;

        public static void ReleaseAll(this Item item)
        {
            if (item.handlers == null) return;
            for(int i = 0; i < item.handlers.Count; i++)
            {
                item.handlers[i].UnGrab(false);
            }
        }

        public static void SetUpDebugLineRenderer(Transform parent, Vector3 start, Vector3 end, Side side)
        {
            var game = new GameObject("DebugLineRenderer");
            game.transform.parent = parent;
            var line = game.gameObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
        public static Vector3 InverseVelocity(this Rigidbody rigidbody, Rigidbody child)
        {
            var velo = (child.velocity - rigidbody.velocity);
            return velo;
        }

        public static void Set<T>(this object source, string fieldName, T val)
        {
            source.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(source, val);
        }

        public static void RevertEyeTexture(this Creature creature)
        {
            if (!originalEyeMaterial) return;
            foreach(var renderer in creature.renderers)
            {
                if(renderer.renderer.name == "Eyes_LOD0")
                {
                    renderer.renderer.material = originalEyeMaterial;
                    return;
                }
            }
        }
        public static void SetEyeTexture(this Creature creature, Texture texture)
        {
            if (creature.renderers == null || creature.renderers.Count == 0) Debug.Log($"No renderers on creature");
            foreach (var renderer in creature.renderers)
            {
                Debug.Log("RENDERER " + renderer.renderer.name);
                if (renderer.renderer.name == "Eyes_LOD0")
                {
                    if(renderer.renderer.material.name != "PotN.EyeMaterial")
                    {
                        Catalog.LoadAssetAsync<Material>("PotN.EyeMaterial", mat =>
                        {
                            renderer.renderer.material = mat;
                            renderer.renderer.material.SetTexture("_BaseMap", texture);
                            renderer.renderer.material.SetTexture("_EmissionMap", texture);
                            renderer.renderer.material.SetColor("_EmissionColor", Color.white * 3);
                        }, "PotN.EyeMaterial");
                    }
                    else
                    {
                        renderer.renderer.material.SetTexture("_BaseMap", texture);
                        renderer.renderer.material.SetTexture("_EmissionMap", texture);
                        renderer.renderer.material.SetColor("_EmissionColor", Color.white * 3);
                    }
                }
            }
        }
        public static Material GetEyeMaterial(this Creature creature)
        {
            Debug.Log("cock");
            if (creature.renderers == null || creature.renderers.Count == 0) Debug.Log($"No renderers on creature");
             foreach(var renderer in creature.renderers)
            {
                Debug.Log("RENDERER " + renderer.renderer.name);
                if (renderer.renderer.name.ToUpper().Contains("EYE"))
                {
                    Debug.Log($"Shader: {renderer.renderer.material.shader.name}");
                    for(int i = 0; i < renderer.renderer.material.shader.GetPropertyCount(); i++)
                    {
                        Debug.Log($"Property: {renderer.renderer.material.shader.GetPropertyName(i)} {renderer.renderer.material.shader.GetPropertyType(i)}");
                    }
                    foreach(var p in renderer.renderer.material.GetTexturePropertyNames())
                    {
                        Debug.Log($"Property: {p}");
                    }
                    return renderer.renderer.material;
                }
            }
            return null;
        }
    }
}
