using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/* store and indicate game settings; however, some variables control the game, changing which will cause changing of game as well */
public class GameSetting : MonoBehaviour {
    public static GameSetting singleton;

    [Header("indicate the game")]
    public float volume;    // voice volume: range from 0 to 1
    public int fps;
    

    [Header("control the game")]
    [Tooltip("if minimap will show enemy's position")]
    public bool show_enemy = false;
    [Tooltip("how long will game start after scene switched")]
    public int prepare_sec = 10;
    
    void Awake() {
        if (singleton == null) {
            singleton = this;
            this.transform.parent = null;
            DontDestroyOnLoad(this);
        } else
            Destroy(this.gameObject);
    }


    public void SetGenVol(float volume) {
        this.volume = volume;
        AudioListener.volume = this.volume;
    }
}
