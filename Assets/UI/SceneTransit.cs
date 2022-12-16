using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransit : MonoBehaviour {
    public static SceneTransit singleton;
    public Animator transition;

    void Awake() {
        if (singleton == null) {
            singleton = this;
        }
        else
            Destroy(this.gameObject);
    }
    
    public void StartTransit() {
        transition.SetTrigger("Start"); 
    }
}
