using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerModelIndex : MonoBehaviour
{
   
   public Transform bugTailRef;
   public Transform[] playerModelsByPlayerCam;
   
   [Header("Player meshes by camera model")]
   //These are the actual mesh renderer lists
   public List<MeshRenderer> Player1CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player2CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player3CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player4CamMeshRenderers = new List<MeshRenderer>();

   [Header("Spit Sacs")] 
   public List<MeshRenderer> spitSacs1 = new List<MeshRenderer>();
   public List<MeshRenderer> spitSacs2 = new List<MeshRenderer>();
   public List<MeshRenderer> spitSacs3 = new List<MeshRenderer>();
   public List<MeshRenderer> spitSacs4 = new List<MeshRenderer>();
   public List<MeshRenderer> spitSacs5 = new List<MeshRenderer>();

   [Header("DoTween Animations")] 
   public DOTweenAnimation ScaleAnimation;
}
