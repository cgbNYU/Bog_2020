using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;

namespace AlmenaraGames.Demo
{
    public class AnimatorFootstep : MonoBehaviour
    {

        Animator anim;
        int speedHash;

        [Range(0f,1f)]
        public float speed;

        public Transform rightFoot;
        public Transform leftFoot;

        //This is the important part, the Custom Play Method
        public void PlayFootstep(MLPASACustomPlayMethodParameters parameters)
        {

            MultiAudioSource source = MultiAudioManager.PlayAudioObject(parameters.AudioObject,parameters.CustomParameters.IntParameter==1?rightFoot.position:leftFoot.position);
            source.MasterVolume = speed;

        }

        public void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            speedHash = Animator.StringToHash("Speed");
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                speed += 1f * Time.deltaTime;
                speed = Mathf.Clamp01(speed);
            }

            else if (Input.GetKey(KeyCode.S))
            {
                speed -= 1f * Time.deltaTime;
                speed = Mathf.Clamp01(speed);
            }

            anim.SetFloat(speedHash, speed);
        }

        // UI Code - IGNORE
        public void OnGUI()
        {

            int xpos = 40;
            int ypos = 20;
            int spacing = 24;

            GUI.Label(new Rect(xpos, ypos, 200, 40), "Press W to Accelerate");
            GUI.Label(new Rect(xpos, ypos+spacing, 200, 40), "Press S to Decelerate");
            GUI.Label(new Rect(xpos, ypos + spacing*2, 200, 40), "Speed: "+(speed*100).ToString()+"%");

        }
    }
}