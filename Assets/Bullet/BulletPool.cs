using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Singleton */
public class BulletPool : MonoBehaviour {
    public static BulletPool singleton { get; private set; }
    public GameObject smallbullet_t;
    public GameObject bigbullet_t;

    /* for small bullet */
    private HashSet<GameObject> smallbullet_idle;
    private HashSet<GameObject> smallbullet_busy;
    /* for big bullet */
    private HashSet<GameObject> bigbullet_idle;
    private HashSet<GameObject> bigbullet_busy;

    void Awake() {
        if (singleton == null) {
            singleton = this;
        } else {
            Destroy(this.gameObject);
        }
        smallbullet_busy = new HashSet<GameObject>();
        smallbullet_idle = new HashSet<GameObject>();
        bigbullet_busy = new HashSet<GameObject>();
        bigbullet_idle = new HashSet<GameObject>();
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
        GameObject tmp = smallbullet_idle.Count > 0 ? Pop(smallbullet_idle) : Instantiate(smallbullet_t);
        tmp.SetActive(true);
        smallbullet_busy.Add(tmp);
        return tmp;
    }

    public GameObject GetBigBullet() {
        GameObject tmp = bigbullet_idle.Count > 0 ? Pop(bigbullet_idle) : Instantiate(bigbullet_t);
        tmp.SetActive(true);
        bigbullet_busy.Add(tmp);
        return tmp;
    }

    /* Get any item from set randomly and remove it */
    T Pop<T>(HashSet<T> set) {
        foreach (T item in set) {
            set.Remove(item);
            return item;
        }
        return default(T);
    }

    /* Decide bullet type by its name */
    public void RemoveBullet(GameObject bullet) {
        bool is_small = bullet.name.Contains("17mm");

        ///  if bullet is still on field, then it will not be recycled
        if (BattleField.singleton.OnField(bullet)) {
            /// remove unnecessary components (especially Rigidbody) to cut down computation
            Destroy(bullet.GetComponent<Rigidbody>());
            Destroy(bullet.GetComponent<Bullet>());
            Destroy(bullet.GetComponent<Collider>());
            if (is_small)
                smallbullet_busy.Remove(bullet);
            else
                bigbullet_busy.Remove(bullet);
            return;
        }
        /// if bullet is not on field, then it will be recycled
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
