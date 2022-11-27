using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HoldMine : MonoBehaviour {
    public const string mine_s = "mine";    // mark whether the collider is a mine
    public const string held_s = "held";    // mark whether the mine has been held (to prevent grab other team's mine)
    EngineerController ec;
    Rigidbody mine_holding;


    void Awake() {
        ec = GetComponentInParent<EngineerController>();
    }


    void Update() {
        if (mine_holding != null) {
            mine_holding.transform.position = transform.position;
            mine_holding.transform.eulerAngles = transform.eulerAngles;
            mine_holding.velocity = Vector3.zero;
        }
    }


    bool holding => ec.holding;
    void OnTriggerStay(Collider other) {
        if (!this.holding || !other.name.Contains(mine_s) || other.name.Contains(held_s))
            return;
        Hold(other.transform);
    }


    void Hold(Transform mine) {
        if (mine_holding != null)
            return;
        mine.name = mine.name + held_s;
        mine.GetComponent<Collider>().enabled = false;
        mine_holding = mine.GetComponent<Rigidbody>();
    }


    public void Release() {
        if (mine_holding == null)
            return;
        mine_holding.name = mine_holding.name.Replace(held_s, "");
        mine_holding.GetComponent<Collider>().enabled = true;
        mine_holding = null;
    }
}
