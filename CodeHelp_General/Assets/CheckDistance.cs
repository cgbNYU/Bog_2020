using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckDistance : MonoBehaviour
{
    //public Transform target; 
    public List<Transform> targets;
    
    public float distanceBetweenMarkerAndTarget;
    public bool isInsideRange;
    public float distanceToCompareAgainst;

    private SpriteRenderer _mySR;
    
    // Start is called before the first frame update
    void Start()
    {
        _mySR = GetComponent<SpriteRenderer>();

        targets.Clear();
        GameObject[] targetGameObjecs = GameObject.FindGameObjectsWithTag("target");
        for (int i = 0; i < targetGameObjecs.Length; i++)
        {
            targets.Add(targetGameObjecs[i].transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        isInsideRange = false;
        
        //false unless any one fo these are true 
        for (int i = 0; i < targets.Count; i++)
        {
            distanceBetweenMarkerAndTarget = 
                Vector3.Distance(transform.position, targets[i] .position);
            
            if (distanceBetweenMarkerAndTarget < distanceToCompareAgainst)
                isInsideRange = true; //like a tripwire, the first one to trip it flips the bool. 
        }

        if (isInsideRange)
            _mySR.enabled = true;
        else
            _mySR.enabled = false; 
    }
}

/*
RAM 






*/