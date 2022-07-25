using UnityEngine;

public enum RuneBuff { None, Junior, Senior };

/* Motion control and light control */
public class Rune : MonoBehaviour {
    /* set in Unity */
    public Transform rotator_rune;
    public RuneState rune_state_red;
    public RuneState rune_state_blue;


    public bool activated { get; private set; }
    private const int jun_sta = 60;    // rune_junior starts
    private const int jun_end = 120;   // rune_junior ends
    private const int sen_sta = 240;   // rune_senior starts
    private const int sen_end = 420;   // rune_senior ends
    private RuneBuff rune_buff;
    private float a, w, t;
    private int sgn;

    void Start() {
        rune_state_red.SetActiveState(Activation.Idle);
        rune_state_blue.SetActiveState(Activation.Idle);
        sgn = Random.Range(0, 1) > 0.5 ? 1 : -1;
        Reset();
    }

    void Update() {
        float t_bat = BattleField.singleton.GetBattleTime();

        /* rune has been activated => no spinning */
        if (this.activated)
            return;

        /* time for rune_junior */
        if (t_bat >= jun_sta && t_bat <= jun_end) {
            this.rune_buff = RuneBuff.Junior;
            rune_state_blue.SetActiveState(Activation.Ready);
            rune_state_red.SetActiveState(Activation.Ready);
            RuneSpin();
        }
        /* time for rune_senior */
        else if (t_bat >= sen_sta && t_bat <= sen_end) {
            this.rune_buff = RuneBuff.Senior;
            rune_state_blue.SetActiveState(Activation.Ready);
            rune_state_red.SetActiveState(Activation.Ready);
            RuneSpin();
        }
        /* rune is not available => set light to idle, no spinning */
        else {
            rune_state_blue.SetActiveState(Activation.Idle);
            rune_state_red.SetActiveState(Activation.Idle);
        }
    }


    public void ActivateRune(ArmorColor armor_color) {
        activated = true;
        StartCoroutine(BattleField.singleton.ActivateRune(armor_color, rune_buff));
        if (armor_color == ArmorColor.Red) {
            rune_state_blue.SetActiveState(Activation.Idle);
            rune_state_red.SetActiveState(Activation.Activated);
        } else {
            rune_state_blue.SetActiveState(Activation.Activated);
            rune_state_red.SetActiveState(Activation.Idle);
        }
    }
           

    void RuneSpin() {       
        float spd = this.rune_buff == RuneBuff.Junior ? 60 : (this.a * Mathf.Sin(this.w * t) + 2.09f - this.a) * 180 / Mathf.PI;
        rotator_rune.localEulerAngles += sgn * new Vector3(0, 0, spd * Time.deltaTime);
        this.t += Time.deltaTime;
    }


    /* Reset Activation State to false and spin params to a new set of values */
    public void Reset() {
        this.activated = false;
        this.t = 0;
        this.a = Random.Range(0.78f, 1.045f);
        this.w = Random.Range(1.884f, 2f);
    }
}
