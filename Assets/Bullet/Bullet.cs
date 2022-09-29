using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : MonoBehaviour {
    [HideInInspector]
    public GameObject hitter;

    void Update() {
        // if (!isServer)
        //     return;
        if (!BattleField.singleton.OnField(this.gameObject)) {
            BulletPool.singleton.RemoveBullet(this.gameObject);
        }
    }

    IEnumerator RemoveBullet() {
        yield return new WaitForSeconds(10);
        BulletPool.singleton.RemoveBullet(this.gameObject);
    }

    /* requirement: bullet need to be continous dynamic */
    void OnCollisionEnter(Collision collision) {
        /* Note: if bullet hasn't been spawned, isServer returns false even if the code is executed in server */
        if (!NetworkServer.active)
            return;
        ArmorController ac = collision.collider.GetComponent<ArmorController>();
        if (ac != null) {
            ac.TakeHit(collision, this.gameObject);
        }
        StartCoroutine(RemoveBullet());
    }
}
