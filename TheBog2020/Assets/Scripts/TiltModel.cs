using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use the input vectors generated in Player controller to tilt
/// Forward backward input rotates the x axis, left and right rotates z
/// </summary>
public class TiltModel : MonoBehaviour
{
    //PlayerController
    private PlayerController _pc;
    
    //Public tuneables
    public float ForwardBackwardTilt;
    public float LeftRightTilt;
    
    // Start is called before the first frame update
    void Start()
    {
        //Grab the playercontroller
        _pc = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Create vector3 to perform rotation
        Vector3 newRot = Vector3.zero;
        
        //Place the whole input vector into newRot
        newRot = _pc.ReadHeading();
        
        //Convert the side to side input into z rotation, and the forward backward into x
        //Leave the y rotation as it is
        //Use the tuning values to modify the x and z tilt
        newRot = new Vector3(newRot.z * ForwardBackwardTilt, 0, newRot.x * -LeftRightTilt);
        
        //Convert to Quaternion and apply the rotation
        transform.localRotation = Quaternion.Euler(newRot.x, 0, newRot.z);
    }
}
