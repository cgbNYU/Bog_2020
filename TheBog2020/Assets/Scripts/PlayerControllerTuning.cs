using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that lets us create and store different sets of player controller tuning variables.
/// Create a new tuning from the Inspector by right-clicking Create > Player Controller Tuning .
/// </summary>

[CreateAssetMenu(fileName = "New PC Tuning",menuName = "Player Controller Tuning")]
public class PlayerControllerTuning : ScriptableObject
{
    [Tooltip("Dev description and details of this tuning.")]
    [TextArea(3,10)]
    public string TuningNotes;
    [Header("Movement Variables")] 
    public float MaxForce;
    public float WingOffset;
    public float WingDrag; //MUST BE NEGATIVE
    public float MaxTorque;
    public float QuadAngularDrag;
    public int BufferFrames;
    public AnimationCurve WingForceCurve = new AnimationCurve();
    
    [Header("Attack Variables")]
    public float LungeTargetRadius;
    public float LungeRange;
    public float LungeForce;
    public float LungeTime;
    public float ClashForce;

    public float SpitForce;
    public float SpitTime;
    public float DeathTime;
    
    [Header("Lock On Variables")] 
    public float LockOnRange;
    [Range(0.01f, 5f)]
    public float LockTorque;
    [Range(-0.001f, -2f)]
    public float LockDrag;

    //Section for phased out/archived tuning vars so it doesnt break when using old tuning sets.
    [Tooltip("These variables are unused in later versions of the player controller.")]
    [Header("Archived Tuning Variables")]
    public int ExampleArchivedTuningVar;
    
}
