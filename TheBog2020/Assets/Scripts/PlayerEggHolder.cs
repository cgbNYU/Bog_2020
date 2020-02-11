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
    [HideInInspector] 
    public Egg EggHolder; //null if no egg is held by the player


    private void Start()
    {
        //Get the Team ID from the Player Controller at Start
        _teamID = GetComponent<PlayerController>().TeamID;
    }

    private void OnTriggerEnter(Collider other)
    {
        // The player moves into a Nest & is holding an egg
        if (other.gameObject.CompareTag("Nest") && EggHolder != null)
        {
            // If the Nest is the same team as the Egg
            if (other.gameObject.GetComponent<Nest>().TeamID == EggHolder.TeamID)
            {
                EggHolder.OutOfNest = false; // The Egg is in the Nest
                DropEgg(EggHolder); // Drop the Egg
            }
        }
        
        // The player moves into an Egg & is not holding an Egg
        if (other.gameObject.CompareTag("Egg") && EggHolder == null)
        {
            Egg eggToPickup = other.gameObject.GetComponent<Egg>();
            // If the Egg is outside the Nest, pick it up
            if (eggToPickup.OutOfNest)
            {
                PickupEgg(eggToPickup); //Pickup the Egg
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

    private void DropEgg(Egg eggToDrop)
    {
        //TODO
    }

    private void PickupEgg(Egg eggToPickup)
    {
        //TODO
    }
}
