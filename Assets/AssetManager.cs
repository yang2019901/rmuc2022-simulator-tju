using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum ArmorColor { Red = 0, Blue = 1 }

public class AssetManager : MonoBehaviour {
    public static AssetManager singleton { get; private set; }

    public Material light_red;
    public Material light_blue;
    public Material light_purple;
    public Material light_off;

    public TextAsset f_infantry_chassis;
    public TextAsset f_hero_chassis;
    public TextAsset f_weapon;
    public TextAsset f_experience;

    public JObject infa_chs { get; private set; } // infantry_chassis config
    public JObject hero_chs { get; private set; } // hero_chassis config
    public JObject weapon { get; private set; } // weapon config
    public JObject exp { get; private set; } // experience config

    public GameObject Speaker;
    /* bullet-related */
    [Header("bullet-related")]
    public AudioClip _17mm;
    public AudioClip _42mm;
    public AudioClip hit_17mm;
    public AudioClip hit_42mm;

    /* background */
    [Header("background")]
    public AudioClip prepare;
    public AudioClip checking;
    public AudioClip cntdown;
    public AudioClip gamebg;
    public AudioClip gamefin;

    /* broadcast */
    [Header("broadcast")]
    public AudioClip ally_die;
    public AudioClip self_die;
    public AudioClip frst_bld;
    public AudioClip[] kill;
    public AudioClip ace;

    /* event */
    [Header("event")]
    public AudioClip base_warn;
    public AudioClip base_opn;
    public AudioClip base_unshd;
    public AudioClip base_lost;
    public AudioClip rune_activ;

    /* misc */
    [Header("misc")]
    public AudioClip victory;
    public AudioClip lv_up;
    public AudioClip robo_die;
    public AudioClip ilnd_taken;
    public AudioClip buff_taken;


    /// <summary>
    /// API 
    /// </summary>
    public void PlayClipAtPoint(AudioClip ac, Vector3 pos) => AudioSource.PlayClipAtPoint(ac, pos);


    public void BrdcstClip(AudioClip ac, bool loop=false, float volume=1f) {
        AudioSource[] src = Speaker.GetComponents<AudioSource>();
        AudioSource tmp = null;
        bool aud_idle = false;
        foreach (AudioSource aud in src)
            if (!aud.isPlaying) {
                tmp = aud;
                aud_idle = true;
                break;
            }
        if (!aud_idle)
            tmp = Speaker.AddComponent<AudioSource>();

        tmp.clip = ac;
        tmp.loop = loop;
        tmp.minDistance = 100;      // within minDistance, volume won't decay
        tmp.volume = volume;
        tmp.Play();
    }



    /// <summary>
    /// non-API 
    /// </summary>
    void Awake() {
        /* declare singleton */
        if (singleton == null) {
            singleton = this;
        } else
            Destroy(this);

        infa_chs = JObject.Parse(f_infantry_chassis.text);
        hero_chs = JObject.Parse(f_hero_chassis.text);
        weapon = JObject.Parse(f_weapon.text);
        exp = JObject.Parse(f_experience.text);
    }

}