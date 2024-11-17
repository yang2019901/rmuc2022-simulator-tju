using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            DeleteMC(child);
            // ReplaceMaterial(child, mat);
            // DeleteNI(child);
            // ReplaceMesh(child);
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
        if (mc != null && child.name.ToLower().Contains("socket")) {
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
        if (tmp.sharedMaterial == AssetManager.singleton.light_blue)
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


    // Dictionary<string, string> dict = new Dictionary<string, string>() {{"垫片", ""}, {"", ""}, {"", ""}}
    void ReplaceMesh(Transform child) {
        Mesh[] meshs = Resources.LoadAll<Mesh>("");
        MeshFilter mf = child.GetComponent<MeshFilter>();
        if (mf != null) {
            // string name = child.name.Replace(' ', '_').Replace('.', '_').Replace('-', '_');
            string name = child.name.Replace(' ', '_');
            name = Regex.Replace(name, @"([\p{P}*])", "_");
            name = Regex.Replace(name, @"([\u4e00-\u9fa5])", "___");        // replace all chinese character with ___ (fit simplygon)
            Debug.Log("target obj: " + name);
            foreach (Mesh mesh in meshs) {
                // Debug.Log(mesh.name);
                if (mesh.name.Contains(name)) {
                    mf.mesh = mesh;
                }
            }
        }
    }
}
