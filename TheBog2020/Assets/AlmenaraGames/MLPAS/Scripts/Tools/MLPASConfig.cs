using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


namespace AlmenaraGames.Tools
{
    [HelpURL("https://drive.google.com/uc?&id=1Ob8smsWzHfTC4JiCiRonlJPVCcislqx4#page=2")]
    //[CreateAssetMenu(fileName = "New Config File", menuName = "Multi Listener Pooling Audio System/Config")]
    public class MLPASConfig : ScriptableObject
    {

        [Tooltip("Default SFX Mixer Group Output")]
        public AudioMixerGroup sfxMixerGroup;
        [Tooltip("Default BGM Mixer Group Output")]
        public AudioMixerGroup bgmMixerGroup;
        [Tooltip("Layer Mask used for check whether or not a collider occludes the sound. Tip: Use a unique layer for the occludable colliders, then you can have more control putting invisible triggers on the objects that you want to occludes sound")]
        public LayerMask occludeCheck = ~0;
        [Tooltip("The higher the value, the less the audio is heard when occluded")]
        [Range(0, 1)]
        public float occludeMultiplier = 0.5f;
        public string runtimeIdentifierPrefix = "[rt]";
        [Tooltip("Max pooled Multi Audio Sources")]
        [Range(1, 2048)]
        public uint maxAudioSources = 256;

        [Space]
        [Tooltip("Set how audible the Doppler effect is. Use 0 to disable it. Use 1-5 to make it audible for fast moving objects.")]
        [Range(0, 10)]
        public float dopplerFactor = 1;

        [HideInInspector]
        public MLPASAnimatorSFX.StateSFX[] stateClipboardBuffer = new MLPASAnimatorSFX.StateSFX[0];

        [Space]
        public bool updated = false;

    }
}