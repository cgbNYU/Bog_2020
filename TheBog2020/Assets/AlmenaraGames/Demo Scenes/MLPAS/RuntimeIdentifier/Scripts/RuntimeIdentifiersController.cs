using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo
{
    public class RuntimeIdentifiersController : MonoBehaviour
    {

        int surface = 0;

        public AudioObject waterSfx;
        public AudioObject metalSfx;
        public AudioObject concreteSfx;

        string surfaceSoundIdentifier = "surfaceSound";

        private void Awake()
        {

            //Defines the Runtime Identifier "surfaceSound" with a default value
            MultiAudioManager.DefineRuntimeIdentifier(surfaceSoundIdentifier, concreteSfx);

        }

        private void Update()
        {

            bool surfaceChange = false;

            if (Input.GetKeyDown(KeyCode.D))
            {
                surface += 1;
                if (surface > 2)
                    surface = 0;

                surfaceChange = true;
            } 

            else if (Input.GetKeyDown(KeyCode.A))
            {
                surface -= 1;
                if (surface < 0)
                    surface = 2;

                surfaceChange = true;
            }

            //Assigns a different Audio Object to the Runtime Identifier "surfaceSound" when surface changes
            if (surfaceChange)
            {
                switch (surface)
                {
                    case 1:
                        MultiAudioManager.AssignRuntimeIdentifierAudioObject(surfaceSoundIdentifier, metalSfx);
                        break;
                    case 2:
                        MultiAudioManager.AssignRuntimeIdentifierAudioObject(surfaceSoundIdentifier, waterSfx);
                        break;
                    default:
                        MultiAudioManager.AssignRuntimeIdentifierAudioObject(surfaceSoundIdentifier, concreteSfx);
                        break;
                }

                surfaceChange = false;
            }

        }

        // UI Code - IGNORE
        public void OnGUI()
        {

            int xpos = 40;
            int ypos = 20;

            GUI.Label(new Rect(xpos, ypos, 200, 40), "A | D = Change Surface");

            string surfaceName = "Concrete";

            switch (surface)
            {
                case 1:
                    surfaceName = "Metal";
                    break;

                case 2:
                    surfaceName = "Water";
                    break;
                default:
                    surfaceName = "Concrete";
                    break;
            }

            GUI.Label(new Rect(xpos, ypos+24, 200, 40), "Surface: "+ surfaceName);



        }

    }
}