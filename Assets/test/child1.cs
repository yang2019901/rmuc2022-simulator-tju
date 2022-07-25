using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class child1 : parent
{
    public float t2;
    void Start()
    {
        t2 = 2f;
        Debug.Log("t1: " + t1);
        Debug.Log("t2: " + t2);
    }

}
