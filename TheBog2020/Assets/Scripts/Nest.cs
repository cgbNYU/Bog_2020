using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn and track eggs for one team
/// Respawn players on death
/// Alert game to a loss by eggs
/// </summary>
public class Nest : MonoBehaviour
{
    //0 = neutral, 1 = red, 2 = blue
    public int TeamID;
    
    //Egg list
    public List<Transform> SpawnLocations; //where the eggs spawn on the nest. Set in inspector
    public List<Egg> EggList = new List<Egg>(); //holds reference to all of the eggs for this team
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //Go through the spawn locations and add an egg to each
    public void SpawnEggs()
    {
        foreach (var spawn in SpawnLocations)
        {
            GameObject newEgg = Instantiate(Resources.Load<GameObject>("Prefabs/Egg"));
            newEgg.transform.position = spawn.position;
            Egg eggScript = newEgg.GetComponent<Egg>();
            eggScript.TeamID = TeamID;
            eggScript.IsHeld = false;
            eggScript.InNest = true;
            EggList.Add(eggScript);
        }
    }
}
