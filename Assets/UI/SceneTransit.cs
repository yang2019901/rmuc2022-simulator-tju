using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneTransit : MonoBehaviour {
    public static SceneTransit singleton;
    public Animator transition;
    public TextAsset f_tips;
    public TMP_Text txt_tip;

    
    string[] s_tips;
    void Awake() {
        if (singleton == null) {
            singleton = this;
        }
        else
            Destroy(this.gameObject);
    
        s_tips = f_tips.text.Split('\n');
    }
    

    public void StartTransit() {
        transition.SetTrigger("Start"); 
        StartCoroutine(DispTips());
    }


    IEnumerator DispTips() {
        while (true) {
            txt_tip.text = "Tips: " + s_tips[Random.Range(0, s_tips.Length)];
            yield return new WaitForSeconds(2f);
        }
    }
}
