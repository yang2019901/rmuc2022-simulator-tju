using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string hitter;
    
    void Update()
    {
        if (!BattleField.singleton.OnField(this.gameObject))
        {
            BulletPool.singleton.RemoveBullet(this.gameObject);
        }
    }

    IEnumerator RemoveBullet()
    {
        yield return new WaitForSeconds(5);
        BulletPool.singleton.RemoveBullet(this.gameObject);
    }

    /* requirement: bullet need to be continous dynamic */
    void OnCollisionEnter(Collision collision)
    {
        ArmorController ac = collision.collider.GetComponent<ArmorController>();
        if (ac != null)
        {
            ac.TakeHit(collision, this.gameObject);
        }
        StartCoroutine("RemoveBullet");
    }
}
