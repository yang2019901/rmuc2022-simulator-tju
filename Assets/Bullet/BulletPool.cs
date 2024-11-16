using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Mirror;

/* Singleton */
public class BulletPool : MonoBehaviour {
    /* singleton, used to get */
    public static BulletPool singleton { get; private set; }
    public GameObject smallbullet_t;
    public GameObject bigbullet_t;

    /* for small bullet */
    private List<GameObject> smallbullet_idle;
    private List<GameObject> smallbullet_busy;
    /* for big bullet */
    private List<GameObject> bigbullet_idle;
    private List<GameObject> bigbullet_busy;

    // public override void OnStartServer() {
    //     base.OnStartServer();
    void Awake() {
        if (singleton == null) {
            singleton = this;
        } else {
            Destroy(this.gameObject);
        }
        smallbullet_busy = new List<GameObject>();
        smallbullet_idle = new List<GameObject>();
        bigbullet_busy = new List<GameObject>();
        bigbullet_idle = new List<GameObject>();
        for (int i = 0; i < 400; i++) {
            GameObject tmp = (GameObject)Instantiate(smallbullet_t);
            tmp.transform.parent = this.transform;
            tmp.SetActive(false);
            smallbullet_idle.Add(tmp);
        }

        for (int i = 0; i < 100; i++) {
            GameObject tmp = (GameObject)Instantiate(bigbullet_t);
            tmp.transform.parent = this.transform;
            tmp.SetActive(false);
            bigbullet_idle.Add(tmp);
        }
    }

    public GameObject GetSmallBullet() {
        GameObject tmp;
        if (smallbullet_idle.Count > 0) {
            tmp = smallbullet_idle[0];
            smallbullet_idle.RemoveAt(0);
        } else
            tmp = Instantiate(smallbullet_t);
        tmp.SetActive(true);
        smallbullet_busy.Add(tmp);
        // NetworkServer.Spawn(tmp);
        return tmp;
    }

    public GameObject GetBigBullet() {
        GameObject tmp;
        if (bigbullet_idle.Count > 0) {
            tmp = bigbullet_idle[0];
            bigbullet_idle.RemoveAt(0);
        } else
            tmp = (GameObject)Instantiate(bigbullet_t);
        tmp.SetActive(true);
        bigbullet_busy.Add(tmp);
        // NetworkServer.Spawn(tmp);
        return tmp;
    }

    /* Decide bullet type by its name */
    public void RemoveBullet(GameObject bullet) {
        bool is_small = bullet.name.Contains("17mm");

        if (BattleField.singleton.OnField(bullet)) {
            /* Due to the need of visual effect, bullet will not be recycled
               however, component<rigidbody> will be removed to avoid unnecessary calculation */
            Destroy(bullet.GetComponent<Rigidbody>());
            Destroy(bullet.GetComponent<Bullet>());
            Destroy(bullet.GetComponent<Collider>());
            if (is_small)
                smallbullet_busy.Remove(bullet);
            else
                bigbullet_busy.Remove(bullet);
            return;
        }
        bullet.SetActive(false);
        if (is_small) {
            smallbullet_busy.Remove(bullet);
            smallbullet_idle.Add(bullet);
        } else {
            bigbullet_busy.Remove(bullet);
            bigbullet_idle.Add(bullet);
        }
        return;
    }
}
