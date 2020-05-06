using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelIndex : MonoBehaviour
{
   public Transform bugTailRef;
   public Transform[] playerModelsByPlayerCam;
   
   //These are the actual mesh renderer lists
   public List<MeshRenderer> Player1CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player2CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player3CamMeshRenderers = new List<MeshRenderer>();
   public List<MeshRenderer> Player4CamMeshRenderers = new List<MeshRenderer>();
}
