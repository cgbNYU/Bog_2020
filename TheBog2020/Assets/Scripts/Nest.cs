using System;
using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
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
    private List<Egg> _eggList = new List<Egg>(); //holds reference to all of the eggs for this team
    
    //Audio
    public AudioObject RespawnSound;

    private void OnDrawGizmos()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        Gizmos.color = Color.green;
        if(sphereCollider != null)
        {
            float maxScale = Math.Max(gameObject.transform.localScale.x,
                Math.Max(gameObject.transform.localScale.y, gameObject.transform.localScale.z));
            Gizmos.DrawWireSphere(sphereCollider.bounds.center, sphereCollider.radius * maxScale);
        }
    }


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
            GameObject newEgg = Instantiate(Resources.Load<GameObject>("Prefabs/Players/Egg"));
            newEgg.transform.position = spawn.position;
            newEgg.GetComponent<Renderer>().material = EggMaterial;
            Egg eggScript = newEgg.GetComponent<Egg>();
            eggScript.TeamID = TeamID;
            eggScript.IsHeld = false;
            eggScript.OutOfNest = false;
            _eggList.Add(eggScript);
        }
    }
    
    //When a player dies, first see if any eggs are available for them to spawn into
    //If there is, pick the first egg in the list, and call RespawnPlayer using that egg
    //If there is not, send out info that the team of the player has lost
    public void CheckForSpawnableEgg(int playerID)
    {
        bool hasLost = true;
        foreach (var egg in _eggList)
        {
            if (!egg.OutOfNest && !egg.IsHeld && !egg.IsSpawning)
            {
                hasLost = false;
                egg.IsSpawning = true;
                egg.GetComponent<Collider>().isTrigger = true;
                RespawnPlayer(playerID, _eggList.IndexOf(egg));
                break;
            }
        }

        if (hasLost)
        {
            GameManager.GM.EndGame(TeamID);
        }
    }

    private void RespawnPlayer(int playerID, int eggID)
    {
        PlayerController pc = null;
        foreach (var playerController in GameManager.GM.PlayerControllers)
        {
            if (playerController.PlayerID == playerID)
            {
                pc = playerController;
            }
        }
        
        //Move player transform to egg and set physics to zero
        Rigidbody pc_rb = pc.GetComponent<Rigidbody>();
        pc_rb.velocity = Vector3.zero;
        pc_rb.transform.position = _eggList[eggID].transform.position;

        //Animate egg hatching

        //Respawn/Reactivate player model
        pc.GetComponent<PlayerModelSpawner>().SpawnModels();
        
        //Give the Egg Holder the ref to the Tail
        pc.GetComponent<PlayerEggHolder>().GetTailReference();
        
        //Give the Player Spit Sacs script reference to the spit sacs
        pc.GetComponent<PlayerSpitSacs>().GetSpitSacsReference();

        //Set player controller to active state
        pc.StateTransition(PlayerController.MoveState.Hatching, pc.HatchTime);

        //Pop the egg out of the egglist
        Egg removedEgg = _eggList[eggID];
        _eggList.RemoveAt(eggID);

        //Destroy the egg
        Destroy(removedEgg.gameObject);
        
        //Update the remaining eggs UI
        GameManager.GM.UpdateEggsRemainingUI(TeamID,_eggList.Count);
        
        //Play the respawn sound
        MultiAudioManager.PlayAudioObject(RespawnSound, pc.transform.position);
    }

    //Called from the GameManager, which is called from anything that destroys eggs that IS NOT RESPAWNING
    //Passes in the egg to be destroyed and removes that egg from the list
    public void DestroyEgg(Egg egg)
    {
        _eggList.Remove(egg);
        Destroy(egg.gameObject);
        
        //Update the remaining Eggs UI
        GameManager.GM.UpdateEggsRemainingUI(TeamID,_eggList.Count);
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
