using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerSpitSacs : MonoBehaviour
{
    //sac model refs 
    private List<MeshRenderer> spitSac1 = new List<MeshRenderer>();
    private List<MeshRenderer> spitSac2 = new List<MeshRenderer>();
    private List<MeshRenderer> spitSac3 = new List<MeshRenderer>();
    private List<MeshRenderer> spitSac4 = new List<MeshRenderer>();
    private List<MeshRenderer> spitSac5 = new List<MeshRenderer>();

    //tweening vars
    private float spitRechargeTime;
    public float emptyGlowValue = -2;
    public float fullGlowValue = 1;

    void Start()
    {
        //calculate spit timer 
        spitRechargeTime = GetComponent<PlayerController>().SpitTime;
    }

    public void EmptySacs()
    {
        TweenShaderGlowValue(spitSac5, emptyGlowValue,0);
        TweenShaderGlowValue(spitSac4, emptyGlowValue, 0);
        TweenShaderGlowValue(spitSac3, emptyGlowValue, 0);
        TweenShaderGlowValue(spitSac2, emptyGlowValue, 0);
        TweenShaderGlowValue(spitSac1, emptyGlowValue, 0);
    }
    
    public void RefillSacs()
    {
        TweenShaderGlowValue(spitSac1, fullGlowValue, spitRechargeTime * 0.3f);
        TweenShaderGlowValue(spitSac2, fullGlowValue, spitRechargeTime * 0.35f);
        TweenShaderGlowValue(spitSac3, fullGlowValue, spitRechargeTime * 0.4f);
        TweenShaderGlowValue(spitSac4, fullGlowValue, spitRechargeTime * 0.5f);
        TweenShaderGlowValue(spitSac5, fullGlowValue, spitRechargeTime * 0.7f);
    }
    
    //to set the glow power on the spit sacs on a tween
    private void TweenShaderGlowValue(List<MeshRenderer> meshRenderers, float glowPower, float tweenTime)
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.DOFloat(glowPower, "_MKGlowPower", tweenTime);
        }
    }

    public void GetSpitSacsReference()
    {
        //clear old references
        spitSac1.Clear();
        spitSac2.Clear();
        spitSac3.Clear();
        spitSac4.Clear();
        spitSac5.Clear();
        
        //get new refs
        PlayerModelIndex playerModelIndex = GetComponentInChildren<PlayerModelIndex>();
        spitSac1.AddRange(playerModelIndex.spitSacs1);
        spitSac2.AddRange(playerModelIndex.spitSacs2);
        spitSac3.AddRange(playerModelIndex.spitSacs3);
        spitSac4.AddRange(playerModelIndex.spitSacs4);
        spitSac5.AddRange(playerModelIndex.spitSacs5);
    }
    
}
