using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodBar : MonoBehaviour {
    [Header("blood bar")]
    public GameObject mask;
    public GameObject bar_blood;
    public RectTransform bld_full;   // full blood's pixel position
    public RectTransform bld_empty;     // empty blood's pixel position
    
    public GameObject bar_golden;
    [Header("For Base and Guard")]
    public GameObject bar_shield;
    public RectTransform shd_full;   // full shield's pixel position
    public RectTransform shd_empty;     // empty shield's pixel position
    [Header("For Robot")]
    public GameObject bar_green;

    /* set blood bar's length */
    public void SetBlood(float ratio) {
        bar_blood.transform.position = ratio * (bld_full.position) + (1 - ratio) * (bld_empty.position);
    }


    public void SetShield(float ratio) {
        bar_shield.transform.position = ratio * (shd_full.position) + (1 - ratio) * (shd_empty.position);
    }

    /* Reset blood bar's visual effect to game-start state */
    public void Reset() {
        bar_blood.transform.position = bld_full.position;
        if (bar_shield != null)
            bar_shield.transform.position = shd_full.position;
        if (bar_golden != null)
            bar_golden.SetActive(false);
        if (bar_green != null)
            bar_green.SetActive(false);
    }

    public void SetMask(Sprite newmask) {
        this.mask.GetComponent<Image>().sprite = newmask;
    }

    // /* add a golden cover */
    // public void SetInvulState(bool is_on) {
    //     bar_golden.SetActive(is_on);
    // }

    // /* add a green cover */
    // public void SetBuffState(bool is_on) {
    //     bar_green.SetActive(is_on);
    // }
}
