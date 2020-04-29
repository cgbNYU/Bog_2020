using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlendTreeControllerController : MonoBehaviour
{
    private Animator _myAnim; //1
 
    [Range(0.0f, 1.0f)] public float myFloatForControllingIdleAnimation; //to contorl with time //3
    
    void Start()
    {
        _myAnim = GetComponent<Animator>();//now have access to _myAnim //2
    }

    void Update()
    {
        _myAnim.SetFloat("IdleControlFloat", myFloatForControllingIdleAnimation); 
    }
}
