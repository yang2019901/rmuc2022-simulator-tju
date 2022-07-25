using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuneLight {All_off, All_on, Center_on}

public class RuneBlade : MonoBehaviour
{
    public GameObject[] flank_lightbars;
    public GameObject[] central_lightbars;

    private ArmorColor armor_color;
    private Material light_on;
    private Material light_off;

    void Start()
    {
        armor_color = GetComponentInParent<RuneState>().armor_color;
        light_on = (armor_color == ArmorColor.Red) ? AssetManager.singleton.light_red : AssetManager.singleton.light_blue;
        light_off = AssetManager.singleton.light_off;
    }

    public void SetBladeLight(RuneLight state)
    {
        switch (state)
        {
            case RuneLight.All_off:
                foreach (GameObject bar in flank_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_off;
                foreach (GameObject bar in central_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_off;
            break;
            case RuneLight.Center_on:
                foreach (GameObject bar in flank_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_off;
                foreach (GameObject bar in central_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_on;
            break;
            case RuneLight.All_on:
                foreach (GameObject bar in flank_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_on;
                foreach (GameObject bar in central_lightbars)
                    bar.GetComponent<Renderer>().sharedMaterial = light_on;
            break;
        }
        return ;
    }
}
