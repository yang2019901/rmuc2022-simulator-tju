using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XchgSpot : MonoBehaviour {
    public ArmorColor armor_color;
    void OnTriggerEnter(Collider other) {
        if (other.name.Contains(HoldMine.mine_s)) {
            other.transform.parent = null;
            other.gameObject.SetActive(false);
            BattleField.singleton.XchgMine(armor_color, other.name.Contains("gold"));
        }
    }
}