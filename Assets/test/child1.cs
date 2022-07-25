using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class child1 : parent
{
    public float health;

    void Start()
    {
        health = 2;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void TakeDamage()
    {
        Debug.Log("this is in child1");
        health --;
        Debug.Log(health);
    }
}
