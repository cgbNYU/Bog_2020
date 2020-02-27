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
    //0 = red, 1 = blue
    private int _teamID;
    //[HideInInspector] 
    public Egg EggHolder = null; //null if no egg is held by the player


    private void Start()
    {
        //Get the Team ID from the Player Controller at Start
        _teamID = GetComponent<PlayerController>().TeamID;
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
                if (EggHolder.TeamID == _teamID)
                {
                    EggHolder.OutOfNest = false; // The Egg is in the Nest
                    DropEgg(EggHolder); // Drop the Egg
                }
            }
            
            //Generic drop location
            if (other.gameObject.CompareTag("DropTrigger"))
            {
                DropEgg(EggHolder);
            }
        }
        
        
        // The player moves into an Egg & is not holding an Egg
        if (other.gameObject.CompareTag("Egg") && EggHolder == null)
        {
            Egg eggToPickup = other.gameObject.GetComponent<Egg>();
            
            // If the Egg is your team & outside the nest & is not being held by a player
            if (eggToPickup.TeamID == _teamID && eggToPickup.OutOfNest && !eggToPickup.IsHeld)
            {
                PickupEgg(eggToPickup);
            }

            // If the Egg is the other team
            if (eggToPickup.TeamID != _teamID)
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
    }
    
    private void PickupEgg(Egg eggToPickup)
    {
        EggHolder = eggToPickup;
        EggHolder.IsHeld = true;
        EggHolder.GetComponent<Rigidbody>().isKinematic = true;
        EggHolder.GetComponent<Collider>().isTrigger = true;
        EggHolder.transform.parent = transform;
    }

    private void DropEgg(Egg eggToDrop)
    {
        EggHolder.transform.parent = null;
        EggHolder.GetComponent<Rigidbody>().isKinematic = false;
        EggHolder.GetComponent<Collider>().isTrigger = false;
        EggHolder.IsHeld = false;
        EggHolder = null;
    }

}
