using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place events here that are called from various scripts
/// </summary>
public class Events : MonoBehaviour
{
    //Player Death Event
    public class PlayerDeath : GameEvent
    {
        public int TeamID { get; }
        public int PlayerID { get; }

        public PlayerDeath(int teamId, int playerId)
        {
            TeamID = teamId;
            PlayerID = playerId;
        }
    }
}
