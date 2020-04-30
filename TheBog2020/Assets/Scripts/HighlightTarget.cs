using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightTarget : MonoBehaviour
{
    public Material enemyMaterial;
    public Material enemyHighlightMaterial;
    public Material playerMaterial;

    private Transform enemyModel;
    private int _playerID;
    
    
    void Start()
    {
        //Get player ID
        _playerID = GetComponent<PlayerController>().PlayerID;
    }
    
    //Highlight the model of the target enemy only for the player 
    public void HighlightEnemy(Transform enemyTransform)
    {
        if (enemyTransform != null)
        {
            enemyModel = enemyTransform.GetComponentInChildren<PlayerModelIndex>().playerModelsByPlayerCam[_playerID];
            SetMaterial(enemyModel, enemyHighlightMaterial);
        }
    }
    
    //Return the enemy model back to their default material
    public void UnHighlightEnemy(Transform enemyTransform)
    {
        if (enemyTransform != null && enemyTransform.GetComponentInChildren<PlayerModelIndex>() != null)
        {
            enemyModel = enemyTransform.GetComponentInChildren<PlayerModelIndex>().playerModelsByPlayerCam[_playerID];
            SetMaterial(enemyModel, enemyMaterial);
        }
    }

    public void UnhighlightPlayer()
    {
        SetMaterial(transform, playerMaterial);
    }

    //Recursively set the material on all the children of a GameObject 
    private void SetMaterial(Transform model, Material material)
    {
        foreach (Transform child in model)
        {
            SetMaterial(child,material);
            if (child.GetComponent<Renderer>() != null)
                if (!child.CompareTag("Eyes") && !child.CompareTag("Wings")) 
                    child.GetComponent<Renderer>().material = material;
        }
    }
}
