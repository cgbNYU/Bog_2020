using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads the stick inputs and increases wing flap amp in response to input
/// Holds the wings in a public variable so we can set it in the prefab for the model
/// </summary>
public class WingFlap : MonoBehaviour
{
    //Wings
    public MeshRenderer[] LeftWingMeshRenderers;
    public MeshRenderer[] RightWingMeshRenderers;
    
    //Tunings
    public float MinAmp;
    public float MaxAmp;
    
    //Bool to prevent changing all the time
    private bool _hasInputLeft;
    private bool _hasInputRight;
    
    //PlayerController ref
    private PlayerController _pc;
    
    // Start is called before the first frame update
    void Start()
    {
        _hasInputLeft = false;
        _hasInputRight = false;

        _pc = GetComponentInParent<PlayerController>();

        foreach (MeshRenderer mr in LeftWingMeshRenderers)
        {
            mr.material.SetFloat("_Amplitude", MinAmp);
        }

        foreach (MeshRenderer mr in RightWingMeshRenderers)
        {
            mr.material.SetFloat("_Amplitude", MinAmp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //If the player has not pushed the left stick and pushes it, set the wings to max
        if (_pc.ReadLeftInput().magnitude > 0 && !_hasInputLeft)
        {
            Debug.Log("Input left");
            _hasInputLeft = true;
            foreach (MeshRenderer mr in LeftWingMeshRenderers)
            {
                mr.material.SetFloat("_Amplitude", MaxAmp);
            }
        }
        //otherwise if the player has pushed the left stick and lets it go, return the amp to min
        else if (_pc.ReadLeftInput().magnitude <= 0.01f && _hasInputLeft)
        {
            Debug.Log("Stop Input Left");
            _hasInputLeft = false;
            foreach (MeshRenderer mr in LeftWingMeshRenderers)
            {
                mr.material.SetFloat("_Amplitude", MinAmp);
            }
        }
        
        //Separate checks for the right
        if (_pc.ReadRightInput().magnitude > 0 & !_hasInputRight)
        {
            _hasInputRight = true;
            foreach (MeshRenderer mr in RightWingMeshRenderers)
            {
                mr.material.SetFloat("_Amplitude", MaxAmp);
            }
        }
        else if (_pc.ReadRightInput().magnitude <= 0.01f && _hasInputRight)
        {
            _hasInputRight = false;
            foreach (MeshRenderer mr in RightWingMeshRenderers)
            {
                mr.material.SetFloat("_Amplitude", MinAmp);
            }
        }
    }
}
