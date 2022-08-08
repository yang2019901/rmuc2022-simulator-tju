using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Activation { Idle, Ready, Hitting, Activated };

/**********************************************************************************
    RuneBlade shift, turn on rune center's light, take hit and report hitting state
***********************************************************************************/
public class RuneState : BasicState {
    /* set in Unity */
    public GameObject rune_center;

    /* Cache */
    private List<RuneBlade> blades = new List<RuneBlade>();
    /* -1: not hit; 0: hitting; 1: hit  */
    private List<int> blades_hit = new List<int>();
    private Material _light;
    /* when timer_hit > max_hit_time, blade will shift to another one */
    private const float max_hit_time = 2.5f;
    private float timer_hit = 0f;
    private int idx_target;
    private Activation activate_state;

    public void Init() {
        _light = (armor_color == ArmorColor.Red) ? AssetManager.singleton.light_red : AssetManager.singleton.light_blue;
        foreach (RuneBlade blade in GetComponentsInChildren<RuneBlade>()) {
            blades.Add(blade);
            blade.Init();
            blade.SetBladeLight(RuneLight.All_off);
            blades_hit.Add(-1);
        }
        rune_center.GetComponent<Renderer>().sharedMaterial = AssetManager.singleton.light_off;
        activate_state = Activation.Idle;
    }


    public void SetActiveState(Activation new_state) {
        switch (new_state) {
            case Activation.Idle:
                if (activate_state == Activation.Idle)
                    break;
                for (int i = 0; i < blades.Count; i++) {
                    blades[i].SetBladeLight(RuneLight.All_off);
                    blades_hit[i] = -1;
                }
                rune_center.GetComponent<Renderer>().sharedMaterial = AssetManager.singleton.light_off;
                activate_state = new_state;
                break;

            case Activation.Ready:
                if (activate_state == Activation.Idle) {
                    rune_center.GetComponent<Renderer>().sharedMaterial = _light;
                    activate_state = new_state;
                }
                break;

            case Activation.Hitting:
                if (activate_state == Activation.Ready) {
                    /* pick a blade as new target armor to hit */
                    idx_target = GetRandBladeIdx();
                    blades_hit[idx_target] = 0;
                    blades[idx_target].SetBladeLight(RuneLight.Center_on);
                    timer_hit = 0;
                    activate_state = new_state;
                }
                break;

            default:
                Debug.LogError("wrong state received by RuneState.cs");
                break;
        }

    }


    int GetRandBladeIdx() {
        List<int> indexes_not_hit = new List<int>();
        for (int i = 0; i < blades_hit.Count; i++) {
            if (blades_hit[i] == -1)
                indexes_not_hit.Add(i);
        }
        if (indexes_not_hit.Count == 0)
            return -1;
        int target = Random.Range(0, indexes_not_hit.Count);
        return indexes_not_hit[target];
    }


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        if (activate_state != Activation.Hitting)
            return;
        RuneBlade blade_hit = armor_hit.GetComponentInParent<RuneBlade>();
        if (blade_hit == blades[idx_target]) {
            blades_hit[idx_target] = 1;
            blades[idx_target].SetBladeLight(RuneLight.All_on);

            /* pick another blade as new target */
            idx_target = GetRandBladeIdx();
            /* no idle blade left => all blades are hit => rune is activated */
            if (idx_target == -1) {
                this.activate_state = Activation.Activated;
                GetComponentInParent<Rune>().ActivateRune(armor_color);
            } else {
                blades_hit[idx_target] = 0;
                blades[idx_target].SetBladeLight(RuneLight.Center_on);
                /* reset timer_hit */
                timer_hit = 0;
            }
        } else {
            /* set all elements of "blades_hit" to not_hit */
            for (int i = 0; i < blades.Count; i++) {
                blades[i].SetBladeLight(RuneLight.All_off);
                blades_hit[i] = -1;
            }
            /* repick a blade as new target armor to hit */
            idx_target = GetRandBladeIdx();
            blades_hit[idx_target] = 0;
            blades[idx_target].SetBladeLight(RuneLight.Center_on);
            timer_hit = 0;
        }
    }


    void Update() {
        /* set blade shifting */
        if (activate_state == Activation.Hitting) {
            if (timer_hit >= max_hit_time) {
                /* set all elements of "blades_hit" to not_hit */
                for (int i = 0; i < blades.Count; i++) {
                    blades[i].SetBladeLight(RuneLight.All_off);
                    blades_hit[i] = -1;
                }
                /* repick a blade as new target armor to hit */
                idx_target = GetRandBladeIdx();
                blades_hit[idx_target] = 0;
                blades[idx_target].SetBladeLight(RuneLight.Center_on);
                timer_hit = 0;
            }
            timer_hit += Time.deltaTime;
        }
    }
}
