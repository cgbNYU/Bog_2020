using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo
{
    public class JumpController : MonoBehaviour
    {

        Animator anim;
        int jumpTriggerHash;
        string jumpTag = "Jump";

        // Start is called before the first frame update
        void Awake()
        {
            anim = GetComponent<Animator>();
            jumpTriggerHash = Animator.StringToHash("Jump");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsTag(jumpTag))
                    anim.SetTrigger(jumpTriggerHash);
            }
        }

        // UI Code - IGNORE
        public void OnGUI()
        {

            int xpos = 40;
            int ypos = 20;

            GUI.Label(new Rect(xpos, ypos, 200, 40), "Press Spacebar to Jump");


        }
    }
}