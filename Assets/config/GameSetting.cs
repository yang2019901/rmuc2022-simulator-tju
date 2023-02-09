using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* used to set volume, visiblility, etc */
public class GameSetting : MonoBehaviour {
    public static GameSetting singleton;

    public float volume = 1;    // voice volume: range from 0 to 1
    public bool show_enemy = false;

    void Awake() {
        if (singleton == null) {
            singleton = this;
            this.transform.parent = null;
            DontDestroyOnLoad(this);
        } else
            Destroy(this.gameObject);
    }

    void Update() {
        AudioListener.volume = volume;
    }
}
