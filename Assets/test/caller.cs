using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caller : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        Debug.Log("collide");
        col.collider.GetComponent<parent>().TakeDamage();
    }
}
