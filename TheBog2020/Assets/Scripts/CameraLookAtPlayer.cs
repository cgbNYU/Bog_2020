using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAtPlayer : MonoBehaviour
{
    
    //Public Tuneables
    public Transform TargetPlayer;
    public float RotateSpeed;
    
    //Private Variables
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion newRotation =
            Quaternion.LookRotation(TargetPlayer.position - transform.position, Vector3.up);

        newRotation = Quaternion.Euler(new Vector3(transform.localRotation.x, newRotation.y * Mathf.Rad2Deg, transform.localRotation.z));
        
        transform.localRotation = newRotation;
    }
}
