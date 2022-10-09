using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum ArmorColor {Red=0, Blue=1}

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