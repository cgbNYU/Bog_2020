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
    private List<MeshRenderer> enemyMeshRenderers = new List<MeshRenderer>();
    private List<MeshRenderer> myMeshRenderers = new List<MeshRenderer>();
    private int _playerID;

    public float GlowPower;
    
    
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
            GetMeshRenderersFromIndex(enemyTransform);
            SetShaderGlowValue(enemyMeshRenderers, GlowPower);
        }
    }
    
    //Return the enemy model back to their default material
    public void UnHighlightEnemy(Transform enemyTransform)
    {
        if (enemyTransform != null && enemyTransform.GetComponentInChildren<PlayerModelIndex>() != null)
        {
            GetMeshRenderersFromIndex(enemyTransform);
            SetShaderGlowValue(enemyMeshRenderers, 0);
        }
    }

    public void UnhighlightPlayer()
    {
        //Get Player's meshes
        if (_playerID == 0)
        {
            myMeshRenderers = GetComponentInChildren<PlayerModelIndex>().Player1CamMeshRenderers;
        }
        else if (_playerID == 1)
        {
            myMeshRenderers = GetComponentInChildren<PlayerModelIndex>().Player2CamMeshRenderers;
        }
        else if (_playerID == 2)
        {
            myMeshRenderers = GetComponentInChildren<PlayerModelIndex>().Player3CamMeshRenderers;
        }
        else if (_playerID == 3)
        {
            myMeshRenderers = GetComponentInChildren<PlayerModelIndex>().Player4CamMeshRenderers;
        }
        
        SetShaderGlowValue(myMeshRenderers, 0);
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

    private void GetMeshRenderersFromIndex(Transform enemyTransform)
    {
        if (_playerID == 0)
        {
            enemyMeshRenderers = enemyTransform.GetComponentInChildren<PlayerModelIndex>().Player1CamMeshRenderers;
        }
        else if (_playerID == 1)
        {
            enemyMeshRenderers = enemyTransform.GetComponentInChildren<PlayerModelIndex>().Player2CamMeshRenderers;
        }
        else if (_playerID == 2)
        {
            enemyMeshRenderers = enemyTransform.GetComponentInChildren<PlayerModelIndex>().Player3CamMeshRenderers;
        }
        else if (_playerID == 3)
        {
            enemyMeshRenderers = enemyTransform.GetComponentInChildren<PlayerModelIndex>().Player4CamMeshRenderers;
        }
    }

    private void SetShaderGlowValue(List<MeshRenderer> meshRenderers, float glowPower)
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.SetFloat("_MKGlowPower", glowPower);
        }
    }
}
