using System;
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
    //0 = red, 1 = blue
    public int TeamID;
    public Material EggMaterial;
    
    //Egg list
    public List<Transform> SpawnLocations; //where the eggs spawn on the nest. Set in inspector
    public List<Egg> EggList = new List<Egg>(); //holds reference to all of the eggs for this team
    
    // Start is called before the first frame update
    void Start()
    {
        //Register Event Handlers
        EventManager.Instance.AddHandler<Events.PlayerDeath>(OnPlayerDeath);
    }

    private void OnDestroy()
    {
        //Deregister Event Handlers
        EventManager.Instance.RemoveHandler<Events.PlayerDeath>(OnPlayerDeath);
    }

    #region Spawning

    //Go through the spawn locations and add an egg to each
    public void SpawnEggs()
    {
        foreach (var spawn in SpawnLocations)
        {
            GameObject newEgg = Instantiate(Resources.Load<GameObject>("Prefabs/Egg"));
            newEgg.transform.position = spawn.position;
            //newEgg.GetComponent<Renderer>().material = EggMaterial;
            Egg eggScript = newEgg.GetComponent<Egg>();
            eggScript.TeamID = TeamID;
            eggScript.IsHeld = false;
            eggScript.OutOfNest = false;
            EggList.Add(eggScript);
        }
    }

    public void CheckForSpawnableEgg(int playerID)
    {
        bool hasLost = true;
        foreach (var egg in EggList)
        {
            if (!egg.OutOfNest && !egg.IsHeld && !egg.IsSpawning)
            {
                hasLost = false;
                egg.IsSpawning = true;
                egg.GetComponent<Collider>().isTrigger = true;
                RespawnPlayer(playerID, EggList.IndexOf(egg));
                break;
            }
        }

        if (hasLost)
        {
            Debug.Log("LOSE");
        }
    }

    private void RespawnPlayer(int playerID, int eggID)
    {
        //Move player transform to egg
        GameManager.GM.PlayerControllers[playerID].transform.position = EggList[eggID].transform.position;

        //Animate egg hatching

        //Respawn/Reactivate player model

        //Set player controller to active state

        //Pop the egg out of the egglist
        Egg removedEgg = EggList[eggID];
        EggList.RemoveAt(eggID);

        //Destroy the egg
        Destroy(removedEgg.gameObject);
    }
    
    #endregion

    #region Events

    private void OnPlayerDeath(Events.PlayerDeath evt)
    {
        if (evt.TeamID == TeamID)
        {
            CheckForSpawnableEgg(evt.PlayerID);
        }
    }

    #endregion
}
