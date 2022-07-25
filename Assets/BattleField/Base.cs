/* This script is to manage motion of "Base" */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public void OpenShells(bool open)
    {
        Animator anim = GetComponent<Animator>();
        anim.SetBool("open", open);    
    }
}
