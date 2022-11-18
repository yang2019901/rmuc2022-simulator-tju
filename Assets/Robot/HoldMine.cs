using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldMine : MonoBehaviour {
    public const string mine_s = "mine";
    EngineerController ec;
    bool holding => ec.holding;
    void Start() {
        ec = GetComponentInParent<EngineerController>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!this.holding || other.name != mine_s) 
            return;
        other.transform.parent = this.transform;
        other.transform.localPosition = Vector3.zero;
        other.transform.localEulerAngles = Vector3.zero;
        Debug.Log("catch a mine");
    }
}
