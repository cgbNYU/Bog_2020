using System.Collections;
using System.Collections.Generic;
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
   public List<MeshRenderer> spitSacs1;
   public List<MeshRenderer> spitSacs2;
   public List<MeshRenderer> spitSacs3;
   public List<MeshRenderer> spitSacs4;
   public List<MeshRenderer> spitSacs5;
}
