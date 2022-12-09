using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/* code will be executed in editor, therefore, component will be added to object persistantly */
[ExecuteInEditMode]
public class Traverse : MonoBehaviour {
    public Material mat;
    // Start is called before the first frame update
    void Start() {
        Transform[] allchildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allchildren) {

            // AddMC(child);
            // ResetConvex(child);
            // DeleteMC(child);
            ReplaceMaterial(child, AssetManager.singleton.light_blue);
            // DeleteNI(child);
        }
        // DestroyImmediate(this.GetComponent<Traverse>());
    }

    void AddMC(Transform child) {
        MeshFilter mf = child.gameObject.GetComponent<MeshFilter>();
        MeshCollider mc = child.gameObject.GetComponent<MeshCollider>();
        /* has mesh filter but have no mesh collider */
        if (mf != null && mc == null) {
            if (child.gameObject.name.Contains("BREP"))
                return;
            Debug.Log("Add Mesh Collider: " + child.name);
            mc = child.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            mc.convex = false;
        }
    }

    void DeleteMC(Transform child) {
        MeshCollider mc = child.gameObject.GetComponent<MeshCollider>();
        if (mc != null) {
            DestroyImmediate(mc);
            Debug.Log("Delete Mesh Collider: " + child.name);
        }
    }

    void SetConvex(Transform child) {
        MeshCollider mc = child.gameObject.GetComponent<MeshCollider>();
        if (mc != null) {
            mc.convex = true;
            Debug.Log("Set Convex: " + child.name);
        }
    }

    void ResetConvex(Transform child) {
        MeshCollider mc = child.gameObject.GetComponent<MeshCollider>();
        if (mc != null) {
            mc.convex = false;
            Debug.Log("Reset Convex: " + child.name);
        }
    }

    void ReplaceMaterial(Transform child, Material new_mat) {
        Renderer tmp = child.GetComponent<MeshRenderer>();
        if (tmp == null)
            return;
        if (tmp.sharedMaterial == AssetManager.singleton.light_red)
        // if (child.name.Contains("轨") && !child.name.Contains("板"))
        {
            Debug.Log("replace");
            tmp.sharedMaterial = new_mat;
        }
    }

    void DeleteNI(Transform child) {
        if (GetComponent<NetworkIdentity>() != null) {
            Debug.Log(child.name);
            DestroyImmediate(GetComponent<NetworkTransform>());
            DestroyImmediate(GetComponent<NetworkIdentity>());
        }

    }
}
