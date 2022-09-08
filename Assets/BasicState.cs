using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicState : MonoBehaviour {
    public abstract void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet);
    public void SetArmorColor(ArmorColor armor_color) {
        this.armor_color = armor_color;
        string other_color = armor_color == ArmorColor.Red ? "blue" : "red";
        Material new_mat = armor_color == ArmorColor.Red ? AssetManager.singleton.light_red 
            : AssetManager.singleton.light_blue;
        Renderer[] rens = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in rens) {
            if (ren.sharedMaterial.name.ToLower().Contains(other_color)) {
                ren.sharedMaterial = new_mat;
                Debug.Log("replace");
            }
        }
    }
    public ArmorColor armor_color;
}
