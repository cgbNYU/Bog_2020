using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelSpawner : MonoBehaviour
{
    public GameObject ModelsPrefab;

    public void SpawnModels()
    {
        GameObject newModels = Instantiate(ModelsPrefab, transform.position, transform.rotation);
        newModels.transform.SetParent(gameObject.transform, true);
    }
}
