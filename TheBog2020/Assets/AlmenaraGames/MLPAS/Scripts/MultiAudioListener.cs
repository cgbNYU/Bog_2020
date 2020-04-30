using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames{
	[HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MultiAudioListener")]
	[AddComponentMenu("Almenara Games/MLPAS/Multi Audio Listener")]
public class MultiAudioListener : MonoBehaviour {

	[Tooltip("Sets a position offset to the listener")]
	[SerializeField] private Vector3 positionOffset = Vector3.zero;

		bool init=false;

		private Transform realListener;

	/// <summary>
	/// The listener with the offsets applied.
	/// </summary>
		[HideInInspector] public Transform RealListener{get{if (!init){Awake();} return realListener;}}

	/// <summary>
	/// Sets a position offset to the listener.
	/// </summary>
	public Vector3 PositionOffset{
		get
		{
			//Some other code
			return LocalOffset?transform.TransformVector(positionOffset):positionOffset;
		}
		set {
			positionOffset = value;
		}
	}
	
	/// <summary>
	/// Applied the offset in local space.
	/// </summary>
	public bool LocalOffset=false;

	[Tooltip("Overrides the listener direction, useful for fixed camera angle games like Top Downs or 2D Platformers. Use (0, 0, 0) to disable the forward override")]
	/// <summary>
	/// Overrides the listener direction, useful for fixed camera angle games like Top Downs or 2D Platformers. Use (0,0,0) to disable the forward override.
	/// </summary>
	public Vector3 OverrideForward = Vector3.zero;

	/// <summary>
	/// Applied the forward direction in local space.
	/// </summary>
	public bool LocalForward=false;

	private int globalIndex = -1;
	/// <summary>
	/// Gets the listener index, WARNING: Don't modify this value.
	/// </summary>
	/// <value>The index.</value>
	public int Index {get{ return globalIndex; }set{globalIndex = value;}}

	[Tooltip("This reverb preset will going to affect all nearest Multi Audio Sources, if \"useReverbZones\" is TRUE then this is going to be the default value when the listener is outside of a Multi Reverb Zone")]
	/// <summary>
		/// This reverb preset will going to affect all nearest Multi Audio Sources, if "useReverbZones" is TRUE then this is going to be the default value when the <see cref="MultiAudioListener"/>  is outside of a <see cref="MultiReverbZone"/> 
	/// </summary>
	public AudioReverbPreset ReverbPreset;
	[Tooltip("If set to TRUE the Multi Audio Listener will going to check whether or not is inside a Multi Reverb Zone")]
	/// <summary>
	/// If set to TRUE the <see cref="MultiAudioListener"/> will going to check whether or not is inside a <see cref="MultiReverbZone"/>.
	/// </summary>
	public bool useReverbZones = true;

	private AudioReverbPreset initialReverbPreset;

	float reverbZoneCheckTimer=0f;

	private bool isApplicationQuitting = false;

	void Awake()
	{
			if (init)
				return;
			
			init = true;
		GameObject realListenerGo = new GameObject ("realListener");
		realListener = realListenerGo.transform;
		RealListener.parent = this.transform;
		RealListener.localPosition = Vector3.zero;
		realListenerGo.hideFlags = HideFlags.HideInHierarchy;

	}

	void Start()
	{

		initialReverbPreset = ReverbPreset;

	}

	// Use this for initialization
	void OnEnable () {
		
			if (OverrideForward != Vector3.zero) {
				RealListener.forward = !LocalForward?OverrideForward:transform.TransformVector(OverrideForward);
			} else {
				RealListener.localEulerAngles = Vector3.zero;
			}

			RealListener.position = transform.position + PositionOffset;


		MultiAudioManager.Instance.AddAvailableListener (this);

		MultiAudioManager.noListeners = false;
		
		
	}
	
	// Update is called once per frame
	void OnDisable () {


			if (isApplicationQuitting)
				return;

		MultiPoolAudioSystem.audioManager.RemoveAudioListener (this);



	}


	void OnApplicationQuit () {
		isApplicationQuitting = true;
	}

	void CheckReverbZones()
	{

			if (ReverbPreset != initialReverbPreset) {
				ReverbPreset = initialReverbPreset;
			}

			int maxIndex = MultiPoolAudioSystem.audioManager.reverbZonesCount;

			if (maxIndex > 0) {
				
				for (int i = 0; i < maxIndex; i++) {

					var rz = MultiPoolAudioSystem.audioManager.reverbZones [i];

					if (rz.enabled && rz.col.enabled && rz.IsInside (RealListener.position)) {

						ReverbPreset = rz.ReverbPreset;

					}
				
				}

			}

	}

	void Update()
	{

			if (useReverbZones) {
				reverbZoneCheckTimer += Time.deltaTime;

				if (reverbZoneCheckTimer >= 0.1f) {
					reverbZoneCheckTimer = 0f;
					CheckReverbZones ();
				}
			}

	}

	void LateUpdate()
	{

		if (OverrideForward != Vector3.zero) {
			RealListener.forward = !LocalForward?OverrideForward:transform.TransformVector(OverrideForward);
		} else {
			RealListener.localEulerAngles = Vector3.zero;
		}

		RealListener.position = transform.position + PositionOffset;


		if (globalIndex != -1) {


			MultiPoolAudioSystem.audioManager.listenersPositions [globalIndex] = RealListener.position;
			MultiPoolAudioSystem.audioManager.listenersForwards [globalIndex] = RealListener.right;


		}

		if (ReverbPreset == AudioReverbPreset.User)
			ReverbPreset = AudioReverbPreset.Generic;

	}

	void OnDrawGizmosSelected()
	{
		Vector3 testForward = OverrideForward.normalized;

		Gizmos.color = Color.clear;

		if (Mathf.Abs(testForward.z)>0 && Mathf.Abs(testForward.z)>Mathf.Abs(testForward.x))
			Gizmos.color = Color.blue;
		else if (Mathf.Abs(testForward.x)>0 && Mathf.Abs(testForward.x)>Mathf.Abs(testForward.z))
			Gizmos.color = Color.red;
		if (Mathf.Abs(testForward.y)>0.5)
			Gizmos.color = Color.green;

		Gizmos.DrawWireSphere(transform.position + PositionOffset + (!LocalForward?OverrideForward.normalized:transform.TransformVector(OverrideForward.normalized)),0.25f);

			if (PositionOffset != Vector3.zero) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine (transform.position, transform.position + PositionOffset);
				Gizmos.DrawIcon (transform.position, "AlmenaraGames/MLPAS/AudioListenerNoCenterIco");
				Gizmos.DrawIcon (transform.position + PositionOffset, "AlmenaraGames/MLPAS/AudioListenerIco");
			} else {
				Gizmos.DrawIcon (transform.position, "AlmenaraGames/MLPAS/AudioListenerIco");
			}

	}

	#if UNITY_EDITOR

	[CustomEditor(typeof(MultiAudioListener)), CanEditMultipleObjects]
	public class MultiAudioListenerEditor : Editor
	{

		SerializedObject listenerObj;

		private Texture logoTex;
		private Texture logoTex2;

		private static readonly string[] _dontIncludeMe = new string[]{"m_Script"};

		void OnEnable()
		{
				listenerObj = new SerializedObject (targets);
		}

		public override void OnInspectorGUI()
		{
			listenerObj.Update();

				if ((target as MultiAudioListener).ReverbPreset == AudioReverbPreset.User) {
					(target as MultiAudioListener).ReverbPreset = AudioReverbPreset.Off;
				}

			DrawPropertiesExcluding(listenerObj, _dontIncludeMe);

			listenerObj.ApplyModifiedProperties();



		}

	}
	#endif

}
}