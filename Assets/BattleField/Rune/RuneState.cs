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
    private bool center_light;
    private List<RuneBlade> blades = new List<RuneBlade>();
    /* All_off: not hit; Center_on: hitting; All_on: hit  */
    private List<RuneLight> blades_hit = new List<RuneLight>();
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
            blades_hit.Add(RuneLight.All_off);
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
                    blades_hit[i] = RuneLight.All_off;
                }
                rune_center.GetComponent<Renderer>().sharedMaterial = AssetManager.singleton.light_off;
                activate_state = new_state;
                center_light = false;
                break;

            case Activation.Ready:
                if (activate_state == Activation.Idle) {
                    rune_center.GetComponent<Renderer>().sharedMaterial = _light;
                    activate_state = new_state;
                    center_light = true;
                }
                break;

            case Activation.Hitting:
                if (activate_state == Activation.Ready) {
                    /* pick a blade as new target armor to hit */
                    idx_target = GetRandBladeIdx();
                    blades_hit[idx_target] = RuneLight.Center_on;
                    blades[idx_target].SetBladeLight(RuneLight.Center_on);
                    timer_hit = 0;
                    activate_state = new_state;
                    center_light = true;
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
            if (blades_hit[i] == RuneLight.All_off)
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
            blades_hit[idx_target] = RuneLight.All_on;
            blades[idx_target].SetBladeLight(RuneLight.All_on);

            /* pick another blade as new target */
            idx_target = GetRandBladeIdx();
            /* no idle blade left => all blades are hit => rune is activated */
            if (idx_target == -1) {
                this.activate_state = Activation.Activated;
                GetComponentInParent<Rune>().ActivateRune(armor_color);
            } else {
                blades_hit[idx_target] = RuneLight.Center_on;
                blades[idx_target].SetBladeLight(RuneLight.Center_on);
                /* reset timer_hit */
                timer_hit = 0;
            }
        } else {
            /* set all elements of "blades_hit" to not_hit */
            for (int i = 0; i < blades.Count; i++) {
                blades[i].SetBladeLight(RuneLight.All_off);
                blades_hit[i] = RuneLight.All_off;
            }
            /* repick a blade as new target armor to hit */
            idx_target = GetRandBladeIdx();
            blades_hit[idx_target] = RuneLight.Center_on;
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
                    blades_hit[i] = RuneLight.All_off;
                    blades[i].SetBladeLight(RuneLight.All_off);
                }
                /* repick a blade as new target armor to hit */
                idx_target = GetRandBladeIdx();
                blades_hit[idx_target] = RuneLight.Center_on;
                blades[idx_target].SetBladeLight(RuneLight.Center_on);
                timer_hit = 0;
            }
            timer_hit += Time.deltaTime;
        }
    }

    public RuneSync Pull() {
        RuneSync tmp = new RuneSync();
        tmp.center_light = this.center_light;
        tmp.blades_light = new RuneLight[blades.Count];
        for (int i = 0; i < blades.Count; i++)
            tmp.blades_light[i] = blades[i].blade_light;
        return tmp;
    }

    public void Push(RuneSync rune_sync) {
        rune_center.GetComponent<Renderer>().sharedMaterial = rune_sync.center_light ?
            _light : AssetManager.singleton.light_off;
        if (rune_sync.blades_light.Length != 5)
            Debug.LogWarning("rune_sync received by rune_state doesn't have five blades");
        for (int i = 0; i < blades.Count; i++) {
            // Debug.Log(string.Format("blade {0}'s center light: {1}", i, rune_sync.center_light));
            blades[i].SetBladeLight(rune_sync.blades_light[i]);
        }
    }
}
