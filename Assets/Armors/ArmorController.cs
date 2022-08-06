using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorController : MonoBehaviour {
    public bool enable = true;
    public GameObject light_bar_left;
    public GameObject light_bar_right;

    /* cache */
    private Material _light;
    private ArmorColor armor_color;
    private BasicState bs;

    void Start() {
        bs = GetComponentInParent<BasicState>();
        armor_color = bs.armor_color;

        /* get material by color */
        if (armor_color == ArmorColor.Blue)
            _light = AssetManager.singleton.light_blue;
        else
            _light = AssetManager.singleton.light_red;
    }


    /** Since I'd not like add rigidbody to armors, OnCollisionEnter shouldn't be defined in 
        this class */
    public void TakeHit(Collision collision, GameObject bullet) {
        if (!enable)
            return;
        /* Decide whether this is a successful hit */
        Vector3 v_rel_local = this.transform.InverseTransformVector(collision.relativeVelocity);
        /* only bullet coms from outside can be detect, aka, velocity_vertical > 0 */
        float velocity_vertical = -v_rel_local.x;
        Debug.Log(v_rel_local);
        /* also, velocity_vertical shouldn't be too small. Otherwise, it's a bad hit */
        if ((bullet.name.Contains("17mm") && velocity_vertical < 12)
            || (bullet.name.Contains("42mm") && velocity_vertical < 8))
            return;

        /* find hitter */
        GameObject hitter = bullet.GetComponent<Bullet>().hitter;
        bs.TakeDamage(hitter, this.gameObject, bullet);
    }

    public void SetLight(bool turn_on) {
        light_bar_left.GetComponent<Renderer>().sharedMaterial = turn_on ? _light : AssetManager.singleton.light_off;
        light_bar_right.GetComponent<Renderer>().sharedMaterial = turn_on ? _light : AssetManager.singleton.light_off;
        return;
    }

    public void Enable() {
        this.enable = true;
        this.SetLight(true);
    }

    public void Disable() {
        this.enable = false;
        this.SetLight(false);
    }
}