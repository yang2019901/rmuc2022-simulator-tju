using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleField : MonoBehaviour {
    public static BattleField singleton { get; private set; }

    public Outpost outpost_blue;
    public Outpost outpost_red;
    public Base base_blue;
    public Base base_red;
    public Rune rune;
    public RobotState[] robo_red;
    public RobotState[] robo_blue;

    [Header("boundary")]
    private int x_half_length = 16;
    private int y_half_length = 10;
    private int z_half_length = 10;

    private float t_start;


    void Awake() {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(this);
        } else
            Destroy(this);
    }


    void Start() {
        t_start = Time.time;
    }


    public bool OnField(GameObject obj) {
        Vector3 rel_pos = obj.transform.position - this.transform.position;
        return Mathf.Abs(rel_pos.x) < x_half_length && Mathf.Abs(rel_pos.y) < y_half_length
            && Mathf.Abs(rel_pos.z) < z_half_length;
    }


    public void Kill(GameObject hitter, GameObject hittee) {
        Debug.Log(hitter.name + " slays " + hittee.name);
        if (hittee == outpost_blue)
            base_blue.OpenShells(true);
        else if (hittee == outpost_red)
            base_red.OpenShells(true);
    }


    public IEnumerator ActivateRune(ArmorColor armor_color, RuneBuff rune_buff) {
        if (rune_buff == RuneBuff.None)
            Debug.LogError("Error: activate RuneBuff.None");
        float atk_up = rune_buff == RuneBuff.Junior ? 0.5f : 1f;
        if (armor_color == ArmorColor.Red) {
            Debug.Log("Team Red activates Rune");
            foreach (RobotState robot in robo_red)
                robot.B_atk += atk_up;
        } else {
            Debug.Log("Team Blue activates Rune");
            foreach (RobotState robot in robo_blue)
                robot.B_atk += atk_up;
        }
        yield return new WaitForSeconds(45);
        rune.rune_state_blue.SetActiveState(Activation.Idle);
        rune.rune_state_red.SetActiveState(Activation.Idle);
        yield return new WaitForSeconds(30);
        /* reset rune */
        rune.Reset();
    }


    public RuneBuff GetRuneBuff(ArmorColor armor_color) {
        return armor_color == ArmorColor.Red ? rune_buff_red : rune_buff_blue;
    }


    public float GetBattleTime() {
        return Time.time - t_start;
    }
}
