using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetModelMeshRenderers : MonoBehaviour
{

    public PlayerModelIndex ModelIndex;
    public int ModelPicker;
    
    // Start is called before the first frame update
    void Start()
    {
        if (ModelPicker == 0)
        {
            ModelIndex.Player1CamMeshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        }
        else if (ModelPicker == 1)
        {
            ModelIndex.Player2CamMeshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        }
        else if (ModelPicker == 2)
        {
            ModelIndex.Player3CamMeshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        }
        else if (ModelPicker == 4)
        {
            ModelIndex.Player4CamMeshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        }
    }
}
