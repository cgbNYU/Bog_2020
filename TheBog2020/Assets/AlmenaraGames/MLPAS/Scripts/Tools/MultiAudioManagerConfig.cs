using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames.Tools
{
	//OBSOLETE
	[AddComponentMenu("")]
	public class MultiAudioManagerConfig : MonoBehaviour {

	[Header("OBSOLETE")]
	
	public AudioMixerGroup sfxMixerGroup;
	[Tooltip("Default BGM Mixer Group Output")]
	
	public AudioMixerGroup bgmMixerGroup;
	[Tooltip("Layer Mask used for check whether or not a collider occludes the sound. Tip: Use a unique layer for the occludable colliders, then you can have more control putting invisible triggers on the objects that you want to occludes sound")]
	
	public LayerMask occludeCheck = ~0;
	[Tooltip("The higher the value, the less the audio is heard when occluded")]
	
	[Range(0,1)]
	public float occludeMultiplier=0.5f;
	[Tooltip("Max pooled Multi Audio Sources")]
	[Range(1,2048)]
	
	public uint maxAudioSources=512;

}

}
