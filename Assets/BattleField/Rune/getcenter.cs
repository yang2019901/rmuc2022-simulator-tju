using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class getcenter : MonoBehaviour {
    void Start() {
        // MeshRenderer mr = GetComponent<MeshRenderer>();
        // Debug.Log("center coord: " + mr.bounds.center);
        transform.position = new Vector3(5.3f, 0.7f, 0.27f);
        // transform.position = new Vector3(0f, 1.39f, 0f);
    }
}
