using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on the Player GameObject along with Player Controller
/// Handles player interactions with the eggs
/// Tracks and changes the bools on the eggs held by the player
/// </summary>
public class PlayerEggHolder : MonoBehaviour
{
    public Egg EggHolder = null; //null if no egg is held by the player
    public Transform BugTail;
    
    //Reference to player controller
    private PlayerController _pc;


    private void Start()
    {
        //Get the Team ID from the Player Controller at Start
        _pc = GetComponent<PlayerController>();
    }

    //Get ref to tail transform from the player model index 
    public void GetTailReference()
    {
        BugTail = GetComponentInChildren<PlayerModelIndex>().bugTailRef;
    }

    private void OnTriggerEnter(Collider other)
    {
        // The player is holding an egg
        if (EggHolder != null)
        {
            //Nest
            if (other.gameObject.CompareTag("Nest"))
            {
                // If the Egg is your team
                if (EggHolder.TeamID == _pc.TeamID)
                {
                    EggHolder.OutOfNest = false; // The Egg is in the Nest
                    DropEgg(); // Drop the Egg
                }
            }
            
            //Generic drop location
            if (other.gameObject.CompareTag("DropTrigger"))
            {
                EggHolder.inWhirlpool = true; // The Egg was dropped into the whirlpool
                DropEgg();
            }
        }
        
        
        // The player moves into an Egg & is not holding an Egg
        if (other.gameObject.CompareTag("Egg") && EggHolder == null)
        {
            Egg eggToPickup = other.gameObject.GetComponent<Egg>();
            
            // If the Egg is your team & outside the nest & is not being held by a player 
            if (eggToPickup.TeamID == _pc.TeamID && eggToPickup.OutOfNest && !eggToPickup.IsHeld )
            {
                PickupEgg(eggToPickup);
            }

            // If the Egg is the other team && not in the whirlpool
            if (eggToPickup.TeamID != _pc.TeamID && !eggToPickup.inWhirlpool)
            {
                PickupEgg(eggToPickup);
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        // The player moves out of the Nest & is holding an egg
        if (other.gameObject.CompareTag("Nest") && EggHolder != null)
        {
            EggHolder.OutOfNest = true; // The Egg is outside the Nest
        }
        
        // The player moves out of the whirpool area & is holding an egg
        if (other.gameObject.CompareTag("DropTrigger") && EggHolder != null)
        {
            EggHolder.inWhirlpool = false; // The Egg is outside the Whirlpool
        }
    }

    private void PickupEgg(Egg eggToPickup)
    {
        if (_pc.CheckState() != PlayerController.MoveState.Dead && !eggToPickup.IsHeld)
        {
            EggHolder = eggToPickup;
            EggHolder.IsHeld = true;
            EggHolder.GetComponent<Rigidbody>().isKinematic = true;
            EggHolder.GetComponent<Collider>().isTrigger = true;
            
            //EggHolder.transform.SetParent(BugTail.transform);
            EggHolder.transform.parent = BugTail.transform;
            EggHolder.transform.localPosition = new Vector3(0,0,-0.4f);
        }
    }

    public void DropEgg()
    {
        if (EggHolder != null)
        {
            EggHolder.transform.parent = null;
            EggHolder.GetComponent<Rigidbody>().isKinematic = false;
            EggHolder.GetComponent<Collider>().isTrigger = false;
            EggHolder.IsHeld = false;
            EggHolder = null;
        }
    }

}
