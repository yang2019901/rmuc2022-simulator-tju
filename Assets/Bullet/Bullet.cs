using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public GameObject hitter;
    
    void Update()
    {
        if (!isServer)
            return ;
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
        if (!isServer)
            return ;
        ArmorController ac = collision.collider.GetComponent<ArmorController>();
        if (ac != null)
        {
            ac.TakeHit(collision, this.gameObject);
        }
        StartCoroutine(RemoveBullet());
    }
}
