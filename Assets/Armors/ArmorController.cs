using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorController : MonoBehaviour {
    public static List<ArmorController> vis_armors_red = new List<ArmorController>();
    public static List<ArmorController> vis_armors_blue = new List<ArmorController>();
    public bool en = true;          // whether this armor is enabled (to be target of auto-aim, to take hit)
    public GameObject[] light_bars;
    [Header("inward normal vector(local)")]
    public Vector3 norm_in;

    /* cache */
    private Material _light;
    private ArmorColor armor_color;
    private BasicState bs;

    void Awake() {
        bs = GetComponentInParent<BasicState>();
    }

    void Start() {
        armor_color = bs.armor_color;
        /* get material by color */
        if (armor_color == ArmorColor.Blue) {
            _light = AssetManager.singleton.light_blue;
            vis_armors_blue.Add(this);
        }
        else {
            _light = AssetManager.singleton.light_red;
            vis_armors_red.Add(this);
        }
    }


    /** Since I'd not like add rigidbody to armors, OnCollisionEnter shouldn't be defined in 
        this class */
    public void TakeHit(Collision collision, GameObject bullet) {
        if (!en)
            return;
        /* to judge whether this is a successful hit */
        float vel_vert = Vector3.Dot(-collision.relativeVelocity, transform.TransformVector(norm_in.normalized));
        Debug.Log(vel_vert);
        /* only bullet coms from outside can be detect, aka, velocity_vertical > 0 */
        /* also, velocity_vertical shouldn't be too small. Otherwise, it's a bad hit */
        if (this.gameObject.name.ToLower().Contains("triangle")) {
            if (bullet.name.Contains("17mm") || (bullet.name.Contains("42mm") && vel_vert < 6))
                return;
        } else {
            if ((bullet.name.Contains("17mm") && vel_vert < 12)
                || (bullet.name.Contains("42mm") && vel_vert < 8))
                return;
        }

        /* find hitter */
        GameObject hitter = bullet.GetComponent<Bullet>().hitter;
        bs.TakeDamage(hitter, this.gameObject, bullet);
    }

    public void SetLight(bool turn_on) {
        foreach (GameObject tmp in light_bars)
            tmp.GetComponent<Renderer>().sharedMaterial = turn_on ? _light : AssetManager.singleton.light_off;
        return;
    }

    public void SetLight(Material mat) {
        foreach (GameObject tmp in light_bars)
            tmp.GetComponent<Renderer>().sharedMaterial = mat;
        return;
    }

    public void Enable() {
        this.en = true;
        this.SetLight(true);
    }

    public void Disable() {
        this.en = false;
        this.SetLight(false);
    }

    void OnDestroy() {
        if (armor_color == ArmorColor.Red)
            vis_armors_red.Remove(this);
        else
            vis_armors_blue.Remove(this);
    }

    // void OnBecameVisible() {
    //     if (armor_color == ArmorColor.Red)
    //         vis_armors_red.Add(this);
    //     else
    //         vis_armors_blue.Add(this);
    //     // Debug.Log("add armor: " + this);
    //     // Debug.Log("vis_armors_red.length: " + vis_armors_red.Count + "\nvis_armors_blue.length: " + vis_armors_blue.Count);
    // }

    // void OnBecameInvisible() {
    //     if (armor_color == ArmorColor.Red)
    //         vis_armors_red.Remove(this);
    //     else
    //         vis_armors_blue.Remove(this);
    //     // Debug.Log("Remove armor: " + this);
    //     // Debug.Log("vis_armors_red.length: " + vis_armors_red.Count + "\nvis_armors_blue.length: " + vis_armors_blue.Count);
    // }
}