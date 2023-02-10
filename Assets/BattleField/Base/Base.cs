/* This script is to manage motion of "Base" */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour {
    public void OpenShells(bool open) {
        Animator anim = GetComponent<Animator>();
        anim.SetBool("open", open);
        AssetManager.singleton.PlayClipAtPoint(
            AssetManager.singleton.base_opn, transform.position);
        if (this.GetComponent<BaseState>().armor_color == BattleField.singleton.robo_local.armor_color)
            AssetManager.singleton.PlayClipAround(AssetManager.singleton.base_warn);
    }
}
