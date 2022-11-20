using UnityEngine;

public enum RuneBuff { None, Junior, Senior };

/* Motion control and light control */
public class Rune : MonoBehaviour {
    /* external reference */
    public Transform rotator_rune;
    public RuneState rune_state_red;
    public RuneState rune_state_blue;
    public Transform[] mines;


    public bool activated { get; private set; }
    private const int jun_sta = 2;    // rune_junior starts
    private const int jun_end = 120;   // rune_junior ends
    private const int sen_sta = 240;   // rune_senior starts
    private const int sen_end = 420;   // rune_senior ends
    private const int mine_1_3 = 5;
    private const int mine_0_4 = 180;
    private const int mine_2 = mine_0_4 + 5;
    private RuneBuff rune_buff;
    private float a, w, t;
    private int sgn;


    /** use Init() instead of Start() to init instance of Rune, 
        so that instances of Rune, RuneState and RuneBlade are initialized in certain and correct order.
        
        Otherwise, maybe RuneBlade is initialized before RuneState, thereby causing null sharedMaterial of Renderer
     */
    public void Init() {
        rune_state_red.Init();
        rune_state_blue.Init();
        rune_state_red.SetActiveState(Activation.Idle);
        rune_state_blue.SetActiveState(Activation.Idle);
        sgn = Random.Range(0, 1) > 0.5 ? 1 : -1;
        Reset();

        for (int i = 0; i < mines.Length; i++)
            mines[i].GetComponent<Rigidbody>().useGravity = false;
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

        float t_next = t_bat + Time.deltaTime;
        if (t_bat < mine_1_3 && t_next > mine_1_3) {
            DropMine(1);
            DropMine(3);
        } else if (t_bat < mine_0_4 && t_next > mine_0_4) {
            DropMine(0);
            DropMine(4);
        } else if (t_bat < mine_2 && t_next > mine_2)
            DropMine(2);
    }

    
    void DropMine(int mineIdx) {
        mines[mineIdx].parent = null;
        mines[mineIdx].GetComponent<Rigidbody>().useGravity = true;
    }


    public void ActivateRune(ArmorColor armor_color) {
        activated = true;
        StartCoroutine(BattleField.singleton.ActivateRune(armor_color, rune_buff));
        if (armor_color == ArmorColor.Red) {
            /* rune_state_red's all blade has been turned on during hitting */
            rune_state_blue.SetActiveState(Activation.Idle);
        } else {
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
