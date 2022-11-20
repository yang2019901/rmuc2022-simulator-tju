using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldMine : MonoBehaviour {
    public const string mine_s = "mine";
    public bool holding = false;
    Transform mine_holding;


    void Update() {
        if (mine_holding != null) {
            mine_holding.localPosition = Vector3.zero;
            mine_holding.localEulerAngles = Vector3.zero;
        }
    }


    void OnTriggerStay(Collider other) {
        if (!this.holding || !other.name.Contains(mine_s) || other.transform.parent != null)
            return;
        Hold(other.transform);
    }


    // void OnTriggerExit(Collider other) {
    // if (other.transform == mine_holding)
    // Release();
    // }


    void Hold(Transform mine) {
        mine.parent = this.transform;
        mine_holding = mine;
    }


    public void Release() {
        if (mine_holding == null)
            return;
        mine_holding.parent = null;
        mine_holding = null;
    }
}
