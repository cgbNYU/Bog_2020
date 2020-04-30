using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo
{
    public class SurfaceSound : MonoBehaviour
    {

        private void Update()
        {

            //Plays the Audio Object assigned to the Runtime Identifier "surfaceSound"
            if (Input.GetKeyDown(KeyCode.Space))
            {

                MultiAudioManager.PlayAudioObjectByIdentifier("[rt]surfaceSound",transform.position);

            }

        }

        // UI Code - IGNORE
        public void OnGUI()
        {

            int xpos = 40;
            int ypos = 20;

            GUI.Label(new Rect(xpos, ypos + 64, 200, 40), "Press Spacebar to Play Surface Sound");
        }

    }
}