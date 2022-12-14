using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldMine : MonoBehaviour {
    const int maxholding = 2;
    List<GameObject> mines_holding = new List<GameObject>();
    readonly Vector3[] mines_pos = {new Vector3(0, 0.25f, 0.08f),
        new Vector3(0, 0.45f, 0.08f)};  // local position


    void OnTriggerEnter(Collider other) {
        DetectMine(other.gameObject);
    }


    void OnTriggerExit(Collider other) {
        DetectMine(other.gameObject);
    }


    void Update() {
        KeepMinesPos();
    }


    void DetectMine(GameObject obj) {
        if (obj.name.Contains(CatchMine.mine_s) && !obj.name.Contains(CatchMine.held_s)
            && mines_holding.Count < maxholding && !mines_holding.Contains(obj)) {
            // obj.transform.parent = this.transform;
            mines_holding.Add(obj.gameObject);
        }
    }


    void KeepMinesPos() {
        if (mines_holding.Count > 0) {
            for (int i = 0; i < mines_holding.Count; i++) {
                if (mines_holding[i].name.Contains(CatchMine.held_s)) {
                    // mines_holding[i].transform.parent = null;
                    mines_holding.RemoveAt(i);
                    i--;
                    continue;
                }
                mines_holding[i].transform.position = transform.TransformPoint(mines_pos[i]);
                mines_holding[i].transform.rotation = transform.rotation;
            }
        }
        return;
    }
}
