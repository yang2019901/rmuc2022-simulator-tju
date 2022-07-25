using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerState : BasicState
{
    /* set in Unity */
    public GameObject[] blood_bars;

    public int blood;
    public bool active = true;
    /* Cache */
    private int blood_left;
    private ArmorController[] acs;

    void Start()
    {
        blood_left = blood;
        acs = GetComponentsInChildren<ArmorController>();
    }


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet)
    {
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage = bullet.name.Contains("17mm") ? 5 : 200;
        damage = Mathf.RoundToInt( damage * (hitter.GetComponent<RobotState>().B_atk + 1) );
        blood_left -= damage;

        Debug.Log("current blood: " + blood_left);
        
        if (blood_left <= 0)
        {
            blood_left = 0;
            foreach (ArmorController ac in acs)
                ac.Disable();
            this.active = false;
            BattleField.singleton.Kill(hitter, this.gameObject);
        }
        else
            StartCoroutine("ArmorsBlink", 0.1f);
        
        SetBloodBars();
    }


    private IEnumerator ArmorsBlink(float interval)
    {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            ac.SetLight(true);
    }


    private void SetBloodBars()
    {
        Vector3 scale = new Vector3(1, 1, (float)blood_left / blood);
        foreach (GameObject bb in blood_bars)
        {
            bb.transform.localScale = scale;
        }
    }
}
