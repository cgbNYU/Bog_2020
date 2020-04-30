using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo{
    public class PlayAudioFromCode : MonoBehaviour
    {

        public AudioObject bgm;

        void Awake()
        {

            //Plays an occludable Audio Object and makes that follows the position of this GameObject
            MultiAudioManager.PlayAudioObject(bgm, transform, true);

        }


    }
}