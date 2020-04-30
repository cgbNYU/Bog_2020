using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AlmenaraGames;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames{
	[HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MultiAudioSource")]
	[AddComponentMenu("Almenara Games/MLPAS/Multi Audio Source",-1)]

	public class MultiAudioSource : MonoBehaviour {

	[SerializeField] private AudioObject audioObject;

	/// <summary>
	/// Gets or sets the <see cref="AudioObject"/>.
	/// </summary>
	public AudioObject AudioObject{get{ return audioObject; } 
		set{
			
				if (!playing && !play) {
					audioObject = value;
                    sequenceIndex = 0;
				} else {
					Debug.LogWarning ("Can't change the Audio Object if the <b>Multi Audio Source</b> is playing");
				}
		
		}
	}

	Transform audioSourceTransform;
	Transform audioSourceReverbTransform;

	MultiAudioListener nearestListener;

	Vector3 nearestListenerPosition;
	Vector3 secondNearestListenerPosition;
	Vector3 thirdNearestListenerPosition;
	Vector3 fourthNearestListenerPosition;

	Vector3 nearestListenerForward;
	Vector3 secondNearestListenerForward;
	Vector3 thirdNearestListenerForward;
	Vector3 fourthNearestListenerForward;

	bool nearestListenerNULL=true;
	bool secondNearestListenerNULL=true;
	bool thirdNearestListenerNULL=true;
	bool fourthNearestListenerNULL=true;

	float nearestListenerDistance;
	float secondNearestListenerDistance;
	float thirdNearestListenerDistance;
	float fourthNearestListenerDistance;

	float nearestListenerBlend;
	/// <summary>
	/// The blend amount of the nearest listener.
	/// </summary>
	public float NearestListenerBlend { get { return nearestListenerBlend; } }
	float secondNearestListenerBlend;
	float thirdNearestListenerBlend;
	float fourthNearestListenerBlend;

	float nearestListenerBlendDistance;
	float secondNearestListenerBlendDistance;
	float thirdNearestListenerBlendDistance;
	float fourthNearestListenerBlendDistance;

	Transform thisTransform;
	Vector3 thisPosition;
	Vector3 averagePosition;
    Vector3 listenersAveragePosition;
	/// <summary>
	/// The average position of all the listeners.
	/// </summary>
	public Vector3 AveragePosition { get { return listenersAveragePosition; } }
	Vector3 smoothedAveragePosition;
	float distanceBetweenListeners;

        [SerializeField] private int defaultChannel = -1;

    [Range(0f,1f)]
	[SerializeField] private float masterVolume=1f;

    [SerializeField] private bool overrideMasterVolume = false;
    [Range(0f, 1f)]
    [SerializeField] private float masterVolumeOverride = 1f;

    /// <summary>
    /// Gets or sets the master volume of the <see cref="MultiAudioSource"/>.
    /// </summary>
    /// <value>The master volume.</value>

    public float MasterVolume{get { return !overrideMasterVolume?masterVolume:masterVolumeOverride; } set { overrideMasterVolume=insidePool; if (!insidePool) { masterVolume = value; } else { masterVolumeOverride = value; } ChangingParameters = true; } }

	[Space(10)]
	[SerializeField] private MultiAudioManager.UpdateModes playUpdateMode=MultiAudioManager.UpdateModes.UnscaledTime;
        [SerializeField] private bool overridePlayUpdateMode = false;
        [SerializeField] private MultiAudioManager.UpdateModes playUpdateModeOverride = MultiAudioManager.UpdateModes.UnscaledTime;
        /// <summary>
        /// If set to UnscaledTime the audio will always be played at normal speed even with the TimeScale set to 0. Otherwise the audio will multiply its speed by the current TimeScale.
        /// </summary>
        public MultiAudioManager.UpdateModes PlayUpdateMode {get{ return !overridePlayUpdateMode?playUpdateMode : playUpdateModeOverride; }set { overridePlayUpdateMode = insidePool; if (!insidePool) { playUpdateMode = value; } else { playUpdateModeOverride = value; } ChangingParameters = true; } }

        public enum ValueOverride
        {

            AudioClip,
            RandomStartVolume,
            Volume,
            RandomStartPitch,
            Pitch,
            Spread,
            SpatialMode,
            StereoPan2D,
            Distance,
            ReverbZoneMix,
            DopplerLevel,
            VolumeRolloff,
            MixerGroup,
            Spatialize

        }

	[Space(10)]

	private int channel=-1;
	/// <summary>
	/// Gets the channel where the <see cref="MultiAudioSource"/> is playing.
	/// </summary>
	/// <value>The channel.</value>
	public int Channel { get { return !insidePool && !IsPlaying?defaultChannel:channel; } }

	[Space(10)]
	/// <summary>
	/// The target to follow.
	/// </summary>
	public Transform TargetToFollow;


    [Space(10)]
	/// <summary>
	/// Mute the <see cref="MultiAudioSource"/>.
	/// </summary>
	public bool Mute;

	[Space(10)]
	/// <summary>
	/// Use the override values.
	/// </summary>
	public bool OverrideValues = false;

	[Space(10)]
	[SerializeField] private bool overrideRandomVolumeMultiplier=false;
	[SerializeField] private bool randomVolumeMultiplier = true;
	/// <summary>
	/// Use random volume multiplier.
	/// </summary>
	public bool RandomVolumeMultiplierOverride {get{return randomVolumeMultiplier;} set{OverrideValues=true;overrideRandomVolumeMultiplier=true;randomVolumeMultiplier=value; ChangingParameters = true; } }
	
	[SerializeField] private bool overrideRandomPitchMultiplier=false;
	[SerializeField] private bool randomPitchMultiplier = true;
	/// <summary>
	/// Use random pitch multiplier.
	/// </summary>
	public bool RandomPitchMultiplierOverride {get{return randomPitchMultiplier;} set{OverrideValues=true;overrideRandomPitchMultiplier=true;randomPitchMultiplier=value; ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideAudioClip=false;
	[SerializeField] private AudioClip audioClipOverride;
	/// <summary>
	/// Gets or sets the audio clip for override the <see cref="AudioObject"/> default clips.
	/// </summary>
	public AudioClip AudioClipOverride {get{ return audioClipOverride; }set {OverrideValues=true;overrideAudioClip=true;audioClipOverride = value; ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideVolume=false;
	[Range(0f,2f)]
	[SerializeField] private float volumeOverride=1f;
	/// <summary>
	/// Gets or sets the volume for override the <see cref="AudioObject"/> default volume.
	/// </summary>
		public float VolumeOverride {get{ return volumeOverride; }set {OverrideValues=true;overrideVolume = true;volumeOverride =  Mathf.Clamp(value,0,1); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overridePitch=false;
	[Range(-3f,3f)]
	[SerializeField] private float pitchOverride=1f;
	/// <summary>
	/// Gets or sets the pitch for override the <see cref="AudioObject"/> default pitch.
	/// </summary>
		public float PitchOverride {get{ return pitchOverride; }set { OverrideValues=true;overridePitch = true;pitchOverride = Mathf.Clamp(value,-3,3); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideSpatialMode=false;
	[SerializeField] private bool spatialMode2DOverride=false;
	/// <summary>
	/// Gets or sets the spatial mode for override the <see cref="AudioObject"/> default spatial mode.
	/// </summary>
		public bool SpatialMode2DOverride {get{ return spatialMode2DOverride; }set {OverrideValues=true;overrideSpatialMode = true;spatialMode2DOverride =  value; ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideStereoPan=false;
	[Range(-1f,1f)]
	[SerializeField] private float stereoPanOverride=0f;
	/// <summary>
	/// Gets or sets the 2D stereo pan for override the <see cref="AudioObject"/> default 2D stereo pan.
	/// </summary>
		public float StereoPanOverride {get{ return stereoPanOverride; }set {OverrideValues=true;overrideStereoPan = true;stereoPanOverride =  Mathf.Clamp(value,-1,1); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideSpread=false;
	[Range(0f,1f)]
	[SerializeField] private float spreadOverride=0f;
	/// <summary>
	/// Gets or sets the spread for override the <see cref="AudioObject"/> default spread.
	/// </summary>
		public float SpreadOverride {get{ return spreadOverride; }set {OverrideValues=true;overrideSpread = true;spreadOverride =  Mathf.Clamp(value,0,360); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideDopplerLevel=false;
	[Range(0f,5f)]
	[SerializeField] private float dopplerLevelOverride=0.25f;
	/// <summary>
	/// Gets or sets the doppler level for override the <see cref="AudioObject"/> default doppler level.
	/// </summary>
		public float DopplerLevelOverride {get{ return dopplerLevelOverride; }set {OverrideValues=true;overrideDopplerLevel = true;dopplerLevelOverride =  Mathf.Clamp(value,0,1); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideReverbZone=false;
	[Range(0f,1.1f)]
	[SerializeField] private float reverbZoneMixOverride = 1;
	/// <summary>
	/// Gets or sets the reverb zone mix for override the <see cref="AudioObject"/> default reverb zone mix.
	/// </summary>
		public float ReverbZoneMixOverride {get{ return reverbZoneMixOverride; }set {OverrideValues=true;overrideReverbZone = true;reverbZoneMixOverride =  Mathf.Clamp(value,0,1); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideDistance=false;
        [SerializeField] internal bool OverrideDistance { get { return overrideDistance; } }
        [SerializeField] private float minDistanceOverride=1f;
	/// <summary>
	/// Gets or sets the min hearable distance for override the <see cref="AudioObject"/> default min hearable distance.
	/// </summary>
		public float MinDistanceOverride {get{ return minDistanceOverride; }set {OverrideValues=true;overrideDistance = true;minDistanceOverride =  Mathf.Clamp(value,0,Mathf.Infinity); ChangingParameters = true; } }
	[SerializeField] private float maxDistanceOverride=20f;
	/// <summary>
	/// Gets or sets the max hearable distance for override the <see cref="AudioObject"/> default max hearable distance.
	/// </summary>
		public float MaxDistanceOverride {get{ return maxDistanceOverride; }set {OverrideValues=true;overrideDistance = true;maxDistanceOverride =  Mathf.Clamp(value,0,Mathf.Infinity); ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool overrideMixerGroup=false;
	[SerializeField] private AudioMixerGroup mixerGroupOverride;
	/// <summary>
	/// Gets or sets the mixer group for override the <see cref="AudioObject"/> default mixer group.
	/// </summary>
		public AudioMixerGroup MixerGroupOverride {get{ return mixerGroupOverride; }set {OverrideValues=true;overrideMixerGroup=true;mixerGroupOverride = value; ChangingParameters = true; } }

	[Space(10)]

		[Tooltip("Occludes the sound if there is an Collider between the source and the listener with one of the <b>Multi Listener Pooling Audio System Config</b> occludeCheck layers")]
	[SerializeField] private bool occludeSound=false;
        [SerializeField] private bool overrideOccludeSound = false;
        [SerializeField] private bool occludeSoundOverride = false;
	/// <summary>
	/// Occludes the sound if there is an Collider between the source and the listener.
	/// </summary>
		public bool OccludeSound {get{ return !overrideOccludeSound?occludeSound:occludeSoundOverride; }set { overrideOccludeSound=insidePool; OverrideValues=true;if (!insidePool) { occludeSound = value; } else { occludeSoundOverride = value; } ChangingParameters = true; } }

	[Space(10)]
	[SerializeField] private float delay=0f;
	/// <summary>
	/// Gets a value indicating whether this <see cref="AlmenaraGames.MultiAudioSource"/>  is delayed.
	/// </summary>
	/// <value><c>true</c> if delayed; otherwise, <c>false</c>.</value>
	public bool Delayed{ get { return delayed; } }
	/// <summary>
	/// Gets or sets the delay before the <see cref="MultiAudioSource"/> starts playing the <see cref="AudioObject"/> or Audio Clip.
	/// </summary>
	public float Delay {get{ return delay; }set { if (!insidePool) { delayed = value > 0; delay = value; } } }
	
	[Space(10)]
	[SerializeField] private MultiAudioManager.UpdateModes delayUpdateMode=MultiAudioManager.UpdateModes.ScaledTime;
        [SerializeField] private bool overrideDelayUpdateMode = false;
        [SerializeField] private MultiAudioManager.UpdateModes delayUpdateModeOverride = MultiAudioManager.UpdateModes.ScaledTime;
        /// <summary>
        /// Gets or sets the delay update mode. If set to ScaledTime, the delay counter will take the TimeScale into account, otherwise it will ignore it.
        /// </summary>
        /// <value>The delay mode.</value>
        public MultiAudioManager.UpdateModes DelayUpdateMode {get{ return !overrideDelayUpdateMode?delayUpdateMode: delayUpdateModeOverride; }set { overrideDelayUpdateMode=insidePool;  if (!insidePool) { delayUpdateMode = value; } else { delayUpdateModeOverride = value; } ChangingParameters = true; } }
	
	[Space(10)]
	[SerializeField] private bool overrideVolumeRolloff=false;
	[SerializeField] private bool volumeRolloffOverride=false;
	[SerializeField] private AnimationCurve volumeRolloffCurveOverride=AnimationCurve.EaseInOut(0,0,1,1);


	/// <summary>
	/// Gets or sets the <see cref="MultiAudioSource"/> volume rolloff for override the <see cref="AudioObject"/> default volume rolloff.
	/// </summary>
		public bool VolumeRolloffOverride {get{ return volumeRolloffOverride; }set {OverrideValues=true;overrideVolumeRolloff=true;volumeRolloffOverride = value; ChangingParameters = true;

                if (value)
                {
                    if (OverrideValues && overrideVolumeRolloff && volumeRolloffOverride)
                    {
                        usingLogRolloff = false;
                    }
                    else
                    {
                        if (CompareCurves(logCurve, audioObject.volumeRolloffCurve))
                        {

                            LogCurveDist();

                        }
                        else
                        {
                            usingLogRolloff = false;
                        }
                    }
                }
                else
                {
                    if (CompareCurves(logCurve, audioObject.volumeRolloffCurve))
                    {
                        
                        LogCurveDist();

                    }
                    else
                    {
                        usingLogRolloff = false;
                    }
                }
                

            } }
	
	/// <summary>
	/// Gets or sets the volume rolloff curve for override the <see cref="AudioObject"/> default volume rolloff curve.
	/// </summary>
		public AnimationCurve VolumeRolloffCurveOverride {get{ return volumeRolloffCurveOverride; }set {OverrideValues=true;overrideVolumeRolloff=true;volumeRolloffCurveOverride = value; ChangingParameters = true; } }

	[Space(10)]
	[SerializeField] private bool overrideSpatialize=false;
	[SerializeField] private bool spatializeOverride;

	/// <summary>
	/// Gets or sets the spatialize value for override the <see cref="AudioObject"/> default spatialize value.
	/// </summary>
		public bool SpatializeOverride {get{ return spatializeOverride; }set {OverrideValues=true;overrideSpatialize=true;spatializeOverride = value; ChangingParameters = true; } }


	[Space(10)]

	[SerializeField] private bool playOnStart = true;
        bool playedFromStart = false;
    [SerializeField] private bool fadeInOnStart = false;
       [SerializeField] float fadeInOnStartTime = 1f;

    [Space(10)]

	[SerializeField] private bool ignoreListenerPause=false;
        [SerializeField] private bool overrideIgnoreListenerPause = false;
        [SerializeField] private bool ignoreListenerPauseOverride = false;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MultiAudioSource"/> ignores the listener pause.
        /// </summary>
        public bool IgnoreListenerPause { get { return !overrideIgnoreListenerPause ? ignoreListenerPause : ignoreListenerPauseOverride; } set { overrideIgnoreListenerPause = insidePool;  if (!insidePool) { ignoreListenerPause = value; } else { ignoreListenerPauseOverride = value; } ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool persistsBetweenScenes=false;
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="MultiAudioSource"/> persists between scenes. (Only for Pooled Audios)
	/// </summary>
		public bool PersistsBetweenScenes{ get { return persistsBetweenScenes; } set{persistsBetweenScenes = value; if (!insidePool) { Debug.LogWarning("Audio Source: " + "<b>" + gameObject.name + "</b>" + " can't be set to <i>PERSISTENT</i> because is not a pooled source", gameObject); } ChangingParameters = true; } }

	[Space(10)]

	[SerializeField] private bool paused=false;
        [SerializeField] private bool overridePaused = false;
        [SerializeField] private bool pausedOverride = false;
		bool prevPaused=false;
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="MultiAudioSource"/> is paused.
	/// </summary>
	public bool LocalPause{ get { return !overridePaused ? paused:pausedOverride; } set{ overridePaused = insidePool; if (!insidePool) { paused = value; } else { pausedOverride = value; } } }

	[Space(10)]

	private ulong sessionIdx;
	public ulong SessionIndex{get{return sessionIdx;} set{sessionIdx = value;}}
	private float volumeMultiplier=0f;
	private float volumeRandomStartMultiplier=1f;
	private float pitchRandomStartMultiplier=1f;
	private float volume;
	private float pitch;
	private float dopplerLevel;
	private float spread;
	private float smoothOcclude;
	private bool spatialMode2D;
	private float stereoPan;
	private float maxDistance;
	private float minDistance;
    public float StartDistance=0f;
	/// <summary>
	/// Gets the current max hearable distance of this <see cref="MultiAudioSource"/>.
	/// </summary>
	public float MaxDistance{get{return maxDistance;}}
	/// <summary>
	/// Gets the current min hearable distance of this <see cref="MultiAudioSource"/>.
	/// </summary>
	public float MinDistance{get{return minDistance;}}
	private bool sameClip = false;
	private bool changingParameters=false;
	private bool delayed=false;
	/// <summary>
	/// Is This <see cref="MultiAudioSource"/> out of range? 
	/// </summary>
	[HideInInspector] public bool outOfRange=false;
	private bool spatialize;
	private bool isBGM;
	private float fadeOutMultiplier=0f;
	private float fadeOutTime;
	private bool fadeOutDisableObject=false;
	private bool fadeOut=false;
	private float fadeInMultiplier=0f;
	private float fadeInTime;
	private bool fadeIn=false;
	private float stopDelayTime;
	private bool stopDelay;
	private bool stopDelayDisableObject;
	private float stopDelayFadeOutTime;
	private bool stopDelayFadeOut;
	private int loopIndex=0;
    private int sequenceIndex = 0;
    private AudioClip audioClipForOverride;

	private float occludeMultiplier = 0;
	private float occludeFilterValue = 0;

	private float waitForReverb=0f;
	private float updateTime = 0f;
	private bool onFocus = true;
	private bool play=false;
	private bool playing=false;
	private bool firstTimePlaying=false;
	[HideInInspector] bool clamped=false;

		/// <summary>
		/// Gets a value indicating whether this <see cref="MultiAudioSource"/> is playing an audio clip.
		/// </summary>
		public bool IsPlaying{get{return play && delay==0 && !LocalPause || playing && !LocalPause; }}


	private bool globalPaused = false;
	private bool loop=false;
	private float customUnscaledDeltaTime;
	private float pitchTimeScale=1f;
	/// <summary>
	/// Gets a value indicating whether this <see cref="MultiAudioSource"/> is looping.
	/// </summary>
	public bool Loop{get{return loop;}}

		private AudioSource audioSource;
		private AudioLowPassFilter occludeFilter;
		private AudioReverbZone listenerReverb;

        /// <summary>
        /// Gets the current playback position of this <see cref="MultiAudioSource"/>  in seconds.
        /// </summary>
        public float PlaybackTime { get { return audioSource.time; } }

		/// <summary>
		/// Scene that the <see cref="MultiAudioSource"/> is part of (Only for Pooled Audios).
		/// </summary>
		public UnityEngine.SceneManagement.Scene scene;

	private bool insidePool = false;
		internal bool InsidePool{get{return insidePool;} set{ insidePool = !cantChangePooleable ? value : insidePool; }}

        public bool ChangingParameters
        {
            get
            {
                return changingParameters;
            }

            set
            {
                changingParameters = value;

            }
        }

        //  public Collider Shape { get { return shape; } set { shape = value; usingShape = value; } }

        private bool cantChangePooleable=false;
		bool noListener=false;
		[HideInInspector] [SerializeField] private bool init=false;
        bool usingShape = false;
       // [SerializeField] private Collider shape;
        GameObject emitter;
        Transform emitterTransform;
       AnimationCurve logCurve;

        public UnityEngine.Events.UnityEvent OnPlay;
        public UnityEngine.Events.UnityEvent OnStop;
        public UnityEngine.Events.UnityEvent OnRange;
        public UnityEngine.Events.UnityEvent OnOutOfRange;
        public UnityEngine.Events.UnityEvent OnLoop;

        [HideInInspector] [SerializeField] bool callbacksFold;

        // Use this for initialization
        void Awake () {

			if (init)
				return;

			init = true;

          //  usingShape = shape;

            //Generate Log Curve

            logCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(0.2f, 0.015f, 0.09f, 0.09f), new Keyframe(0.6f, 0.1f, 0.3916f, 0.3916f), new Keyframe(0.8f, 0.25f, 1.33f, 1.33f), new Keyframe(0.9f, 0.5f, 5f, 5f), new Keyframe(0.95f, 1f, 14.26f, 14.26f) });

            // // // // //

            thisTransform = transform;

		//Initialize the real audio source
		GameObject audioSourceGo = new GameObject ("Audio Source", typeof(AudioSource));
		audioSourceGo.hideFlags = HideFlags.HideInHierarchy;
		audioSource = audioSourceGo.GetComponent<AudioSource> ();
		audioSourceTransform = audioSourceGo.transform;
		audioSourceTransform.parent = MultiPoolAudioSystem.audioManager.transform;
        audioSourceTransform.position = MultiPoolAudioSystem.audioManager.AudioListenerPosition;
        audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
       
            /*    GameObject emitterGo = new GameObject("Real Emitter");
                // emitterGo.hideFlags = HideFlags.HideInHierarchy;
                emitterTransform = emitterGo.transform;
                emitterTransform.parent = thisTransform;
                emitterTransform.localPosition = Vector3.zero;*/

            emitterTransform = thisTransform;

            if (occludeFilter == null)
            {
                occludeFilter = audioSourceGo.AddComponent<AudioLowPassFilter>();
            }

            occludeFilter.enabled = false;


		audioSource.playOnAwake = false;
		audioSource.mute = false;
		audioSource.bypassEffects = false;
		audioSource.bypassListenerEffects = false;
		audioSource.bypassReverbZones = false;
		audioSource.reverbZoneMix = 1;
		audioSource.dopplerLevel = 0f;
		audioSource.spread = 0;
		audioSource.spatialBlend = 1;
		audioSource.loop = false;
		audioSource.volume = 0;

            audioSource.enabled = false;
        listenerReverb = audioSourceGo.AddComponent<AudioReverbZone> ();

		listenerReverb.enabled = false;

		listenerReverb.hideFlags = HideFlags.HideInInspector;

		pitchTimeScale = 1;


	}
			

	void SceneChanged()
	{

			if ((play && delay==0 || playing || Delayed) && insidePool && !scene.isLoaded && !persistsBetweenScenes) {

				Stop ();

			}

	}

	void Start()
	{

		if (InsidePool && transform.parent.gameObject == MultiPoolAudioSystem.audioManager.gameObject) {

			if (!play) {
				gameObject.SetActive (false);

				return;
			}

		}

		if (!insidePool) {
			MultiAudioManager.Instance.AudioSources.Add (this);
		}

		cantChangePooleable = true;

		if (delay > 0)
			delayed = true;

			if (!InsidePool && playOnStart && !playedFromStart && !delayed && audioObject!=null) {

                if (!fadeInOnStart)
                    Play(defaultChannel);
                else
                    PlayFadeIn(fadeInOnStartTime, defaultChannel);

                playedFromStart = true;

            }

	}

	// Update is called once per frame
	void Update () {

		if (Object.ReferenceEquals(audioObject, null)) {

			if (delayed || (play || playing)) {

				if (Object.ReferenceEquals(audioObject, null)) {

					if (InsidePool)
						Debug.LogWarning ("<i>Audio Object</i> to play is missing or invalid", MultiPoolAudioSystem.audioManager.gameObject);
					else {
						Debug.LogWarning ("Audio Source: " + "<b>"+gameObject.name+"</b>" + " doesn't have a valid <i>Audio Object</i> to play",gameObject);
					}
							
					Stop ();

					return;

				}

				if (Object.ReferenceEquals(audioObject.RandomClip, null)) {

					Debug.LogWarning ("Audio Object: " + "<b>"+audioObject.name+"</b>" + " doesn't have a valid <i>Audio Clip</i> to play",audioObject);

					Stop ();

					return;

				}

			}

			return;

		}


            usingShape = false;//(play || playing) && shape;

            // if (!usingShape)
            //   {
            if ((play || playing) && TargetToFollow /*&& TargetToFollow.gameObject.activeInHierarchy*/)
                {

                    transform.position = TargetToFollow.position;

                }
            // }

           /* if ((play || playing))
            {
                if (usingShape && !nearestListenerNULL)
                {
                    Vector3 centerPoint = nearestListener.RealListener.position;

                    if (!secondNearestListenerNULL && thirdNearestListenerNULL && fourthNearestListenerNULL)
                    {
                        Vector3 lerp1 = Vector3.Lerp(Vector3.zero, nearestListenerPosition, nearestListenerBlend);
                        Vector3 lerp2 = Vector3.Lerp(Vector3.zero, secondNearestListenerPosition, secondNearestListenerBlend);

                        centerPoint = lerp1 + lerp2;
                    }

                    if (!secondNearestListenerNULL && !thirdNearestListenerNULL && fourthNearestListenerNULL)
                    {
                        Vector3 lerp1 = Vector3.Lerp(Vector3.zero, nearestListenerPosition, nearestListenerBlend);
                        Vector3 lerp2 = Vector3.Lerp(Vector3.zero, secondNearestListenerPosition, secondNearestListenerBlend);
                        Vector3 lerp3 = Vector3.Lerp(Vector3.zero, thirdNearestListenerPosition, thirdNearestListenerBlend);

                        centerPoint = lerp1 + lerp2 + lerp3;
                    }

                    if (!secondNearestListenerNULL && !thirdNearestListenerNULL && !fourthNearestListenerNULL)
                    {
                        Vector3 lerp1 = Vector3.Lerp(Vector3.zero, nearestListenerPosition, nearestListenerBlend);
                        Vector3 lerp2 = Vector3.Lerp(Vector3.zero, secondNearestListenerPosition, secondNearestListenerBlend);
                        Vector3 lerp3 = Vector3.Lerp(Vector3.zero, thirdNearestListenerPosition, thirdNearestListenerBlend);
                        Vector3 lerp4 = Vector3.Lerp(Vector3.zero, fourthNearestListenerPosition, fourthNearestListenerBlend);

                        centerPoint = lerp1 + lerp2 + lerp3 + lerp4;
                    }

                    Vector3 pos = shape.ClosestPoint(centerPoint);

                    emitterTransform.position = pos;
 
                }
                else
                {
                    emitterTransform.localPosition = Vector3.zero;
                }
            }*/
           

			if (Time.timeScale > 0) {
				customUnscaledDeltaTime = Time.deltaTime / Time.timeScale;
			}
			else {
				customUnscaledDeltaTime = Time.unscaledDeltaTime;
			}

			pitchTimeScale = PlayUpdateMode == MultiAudioManager.UpdateModes.ScaledTime?Time.timeScale:1f;

			if (!InsidePool && playOnStart && !playedFromStart && delayed && delay==0 && audioObject!=null) {

                if (!fadeInOnStart)
                    Play(channel);
                else
                    PlayFadeIn(fadeInOnStartTime, channel);

                playedFromStart = true;

            }


			if (!IgnoreListenerPause && MultiAudioManager.Paused && onFocus || !onFocus)
			globalPaused = true;


			if (Application.runInBackground || Time.realtimeSinceStartup!=updateTime)
				onFocus = true;


			if (onFocus && (!MultiAudioManager.Paused || IgnoreListenerPause) && !globalPaused && !play && playing && (true && !loop && !audioSource.isPlaying || pitch<0 && loop && audioSource.time <= 0.01f || pitch>=0 && loop && audioSource.time >= audioSource.clip.length - 0.01f || true && loop && !audioSource.isPlaying)) {

				if (loop) {
                    if (OnLoop != null)
                    {
                        OnLoop.Invoke();
                    }
                    if (audioClipForOverride == null) {
						PlayLoop (channel, TargetToFollow);
					} else {
						PlayOverride (audioClipForOverride, channel, TargetToFollow);
					}
				} else {
					if (waitForReverb <= 0f) {

						Stop ();
					}
				}

			}


			if (fadeOut && fadeOutMultiplier<=0) {

				fadeOut = false;

				Stop (fadeOutDisableObject);

			}

			if (fadeIn && fadeInMultiplier>=1) {

				fadeInMultiplier = 1;
				fadeIn = false;

			}

			if (stopDelay && stopDelayTime <= 0) {

				stopDelay = false;
				stopDelayTime = 0f;

				if (stopDelayFadeOut) {
					FadeOut (stopDelayFadeOutTime, stopDelayDisableObject);
				} else {

					Stop (stopDelayDisableObject);
				}
                 
			}
				
			if (play || !InsidePool && playOnStart) {
              //  if (!MultiAudioManager.Paused || MultiAudioManager.Paused && IgnoreListenerPause)
				delay = Mathf.Clamp (delay - (DelayUpdateMode == MultiAudioManager.UpdateModes.UnscaledTime?customUnscaledDeltaTime:Time.deltaTime), 0, Mathf.Infinity);
			}


			if (!globalPaused && fadeOut && playing && !LocalPause)
			{
				fadeOutMultiplier = Mathf.Clamp (fadeOutMultiplier - (customUnscaledDeltaTime/fadeOutTime), 0, Mathf.Infinity);
			}

			if (!globalPaused && fadeIn && playing && !LocalPause)
			{
				fadeInMultiplier = Mathf.Clamp (fadeInMultiplier + (customUnscaledDeltaTime/fadeInTime), 0, Mathf.Infinity);
			}

			if (!globalPaused && !LocalPause && playing)
				waitForReverb= Mathf.Clamp (waitForReverb - customUnscaledDeltaTime, 0, Mathf.Infinity);

			if (stopDelay && playing) {

				stopDelayTime = stopDelayTime - (DelayUpdateMode == MultiAudioManager.UpdateModes.UnscaledTime?customUnscaledDeltaTime:Time.deltaTime);

			}

		audioSource.ignoreListenerPause = IgnoreListenerPause;

		

			if (secondNearestListenerNULL) {
				secondNearestListenerBlend = 0;
				secondNearestListenerBlendDistance = 0;
			}
			if (thirdNearestListenerNULL) {
				thirdNearestListenerBlend = 0;
				thirdNearestListenerBlendDistance = 0;
			}
			if (fourthNearestListenerNULL) {
				fourthNearestListenerBlend = 0;
				fourthNearestListenerBlendDistance = 0;
			}

			SceneChanged ();

        
		SetParameters (true);

	}

	void LatePlay()
	{

			if (MultiAudioManager.noListeners)
				Debug.LogWarning ("There are no <b>Multi Audio Listeners</b> in the scene. Please ensure there is always at least one audio listener in the scene.", InsidePool?MultiAudioManager.Instance.gameObject:gameObject);

			if (channel > -1) {

				int audioSourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < audioSourcesCount; i++) {

					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audioSourcePlaying = source.play && source.delay==0 || source.playing;

					if (audioSourcePlaying && source!=this && source.Channel==channel) {

						if (fadeIn) {
							source.FadeOut (fadeInTime);
						} else {
							source.Stop ();
						}

					}

				}

			}

			if (!MultiAudioManager.ClampedAudioCanBePlayed (audioObject)) {

				Stop ();

				return;

			}


			AddClampedSource (audioObject);

			if (!nearestListenerNULL && nearestListener.ReverbPreset != AudioReverbPreset.Off) {
				waitForReverb = audioSource.clip.length + listenerReverb.decayTime + 0.25f;
				audioSource.bypassReverbZones = false;
			} else {
				audioSource.bypassReverbZones = true;
				listenerReverb.maxDistance = 0;
				listenerReverb.minDistance = 0;
			}

		delay = 0;
		delayed = false;
		play = false;
			firstTimePlaying = false;

            audioSource.enabled = true;

            Vector3 dopplerCenter = /*usingShape ? shape.bounds.center : */emitterTransform.position;
            lastSourcePosition = dopplerCenter;
            dopplerPitch = 1f;

            lastListenerPosition = nearestListenerPosition;

            if (OnPlay != null)
            {
                OnPlay.Invoke();
            }

            if (!sameClip) {
				audioSource.time = 0;
			if (pitch < 0) {
				audioSource.timeSamples = audioSource.clip.samples - 1;
			}
			audioSource.Play ();
		} else {

			if (pitch >= 0)
				audioSource.time = 0;
			else {
				audioSource.timeSamples = audioSource.clip.samples - 1;
			}

			if (!audioSource.isPlaying) {
                  
                    audioSource.Play ();
			}

		}
				
		playing = true;
	}

        float SpeedOfSound = 343.3f;
        float DopplerFactor = 2f;

        Vector3 lastSourcePosition = Vector3.zero;
        Vector3 lastListenerPosition = Vector3.zero;

        float dopplerPitch=1f;

        void Doppler()
        {

            //Custom Doppler Implementation

           if (dopplerLevel > 0 && (play || playing))
            {
                DopplerFactor = dopplerLevel / 2.5f;
                DopplerFactor *= Mathf.Clamp(MultiPoolAudioSystem.audioManager.config.dopplerFactor, 0.0001f, 10);

                Vector3 dopplerCenter = /*usingShape ? shape.bounds.center : */thisPosition;

                Vector3 sourceSpeed = (lastSourcePosition - dopplerCenter) / Time.deltaTime;
                lastSourcePosition = dopplerCenter;

                Vector3 listenerSpeed = (lastListenerPosition - nearestListenerPosition) / Time.deltaTime;
                lastListenerPosition = nearestListenerPosition;

               
                var distance = (nearestListenerPosition - dopplerCenter);
                var listenerRelativeSpeed = Vector3.Dot(distance, listenerSpeed) / distance.magnitude;
                var emitterRelativeSpeed = Vector3.Dot(distance, sourceSpeed) / distance.magnitude;
                listenerRelativeSpeed = Mathf.Min(listenerRelativeSpeed, (SpeedOfSound / DopplerFactor));
                emitterRelativeSpeed = Mathf.Min(emitterRelativeSpeed, (SpeedOfSound / DopplerFactor));
                dopplerPitch = (SpeedOfSound + (listenerRelativeSpeed * DopplerFactor)) / (SpeedOfSound + (emitterRelativeSpeed * DopplerFactor));
                float multiplier = 1 - (dopplerLevel/5);
                bool testSpeed = listenerSpeed.magnitude > 20 * multiplier || sourceSpeed.magnitude > 20 * multiplier;

                if (!testSpeed)
                {
                    dopplerPitch = 1f;
                }
            }
            else
            {

                Vector3 dopplerCenter = /*usingShape ? shape.bounds.center :*/ thisPosition;

                lastSourcePosition = dopplerCenter;
                lastListenerPosition = nearestListenerPosition;
                dopplerPitch = 1f;
            }

        }

    void LateUpdate()
	{

            thisPosition = emitterTransform.position;

            GetNearestListeners ();

            if (nearestListenerBlend > 0 || spatialMode2D)
            {

                if (play || playing || waitForReverb > 0)
                {
                    if (!spatialMode2D)
                    {

                        if (!nearestListenerNULL && secondNearestListenerNULL && thirdNearestListenerNULL && fourthNearestListenerNULL)
                        {
                            if (nearestListenerForward != Vector3.zero)
                                audioSourceTransform.forward = (nearestListenerForward);
                            else
                                nearestListenerForward = Vector3.forward;
                        }
                        else if (!nearestListenerNULL && !secondNearestListenerNULL && thirdNearestListenerNULL && fourthNearestListenerNULL)
                        {
                            Vector3 forwardToTest = (Vector3.Lerp(Vector3.zero, nearestListenerForward, nearestListenerBlendDistance * nearestListenerBlend) + Vector3.Lerp(Vector3.zero, secondNearestListenerForward, secondNearestListenerBlendDistance * secondNearestListenerBlend)) / 2;

                            if (forwardToTest != Vector3.zero)
                                audioSourceTransform.forward = forwardToTest;
                            else
                                nearestListenerForward = Vector3.forward;
                        }
                        else if (!nearestListenerNULL && !secondNearestListenerNULL && !thirdNearestListenerNULL && fourthNearestListenerNULL)
                        {
                            Vector3 forwardToTest = (Vector3.Lerp(Vector3.zero, nearestListenerForward, nearestListenerBlendDistance * nearestListenerBlend) + Vector3.Lerp(Vector3.zero, secondNearestListenerForward, secondNearestListenerBlendDistance * secondNearestListenerBlend) + Vector3.Lerp(Vector3.zero, thirdNearestListenerForward, thirdNearestListenerBlendDistance * thirdNearestListenerBlend)) / 3;
                            if (forwardToTest != Vector3.zero)
                                audioSourceTransform.forward = forwardToTest;
                            else
                                nearestListenerForward = Vector3.forward;
                        }
                        else if (!nearestListenerNULL && !secondNearestListenerNULL && !thirdNearestListenerNULL && !fourthNearestListenerNULL)
                        {
                            Vector3 forwardToTest = (Vector3.Lerp(Vector3.zero, nearestListenerForward, nearestListenerBlendDistance * nearestListenerBlend) + Vector3.Lerp(Vector3.zero, secondNearestListenerForward, secondNearestListenerBlendDistance * secondNearestListenerBlend) + Vector3.Lerp(Vector3.zero, thirdNearestListenerForward, thirdNearestListenerBlendDistance * thirdNearestListenerBlend) + Vector3.Lerp(Vector3.zero, fourthNearestListenerForward, fourthNearestListenerBlendDistance * fourthNearestListenerBlend)) / 4;
                            if (forwardToTest != Vector3.zero)
                                audioSourceTransform.forward = forwardToTest;
                            else
                                nearestListenerForward = Vector3.forward;
                        }
                    }
                }
            }


                    if (!nearestListenerNULL && secondNearestListenerNULL && thirdNearestListenerNULL && fourthNearestListenerNULL) {

                    averagePosition = audioSourceTransform.InverseTransformDirection (new Vector3 (nearestListenerPosition.x - thisPosition.x, nearestListenerPosition.y - thisPosition.y, nearestListenerPosition.z - thisPosition.z));

                    listenersAveragePosition = nearestListenerPosition;

                    averagePosition = Quaternion.Euler(0, 90, 0) * averagePosition;

                    averagePosition += new Vector3(0.0001f, 0.0001f, 0.0001f);

               

            } else if (!nearestListenerNULL && !secondNearestListenerNULL && thirdNearestListenerNULL && fourthNearestListenerNULL) {


                Vector3 firstLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (nearestListenerPosition - thisPosition), nearestListenerBlendDistance * nearestListenerBlend));
					Vector3 secondLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (secondNearestListenerPosition - thisPosition), secondNearestListenerBlendDistance * secondNearestListenerBlend));

                secondLerp += new Vector3(firstLerp.x==secondLerp.x ? 0.01f:0f, firstLerp.y == secondLerp.y ? 0.01f : 0f, firstLerp.z == secondLerp.z ? 0.01f : 0f);

                averagePosition = (firstLerp + secondLerp);

                listenersAveragePosition = (nearestListenerPosition + secondNearestListenerPosition) / 2;

                averagePosition = Quaternion.Euler(0, 90, 0) * averagePosition;

                averagePosition += new Vector3(0.0001f, 0.0001f, 0.0001f);
            } else if (!nearestListenerNULL && !secondNearestListenerNULL && !thirdNearestListenerNULL && fourthNearestListenerNULL) {

                Vector3 firstLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (nearestListenerPosition - thisPosition), nearestListenerBlendDistance * nearestListenerBlend));
				Vector3 secondLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (secondNearestListenerPosition - thisPosition), secondNearestListenerBlendDistance * secondNearestListenerBlend));
				Vector3 thirdLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (thirdNearestListenerPosition - thisPosition), thirdNearestListenerBlendDistance * thirdNearestListenerBlend));

                secondLerp += new Vector3(firstLerp.x == secondLerp.x ? 0.01f : 0f, firstLerp.y == secondLerp.y ? 0.01f : 0f, firstLerp.z == secondLerp.z ? 0.01f : 0f);

                thirdLerp += new Vector3(secondLerp.x == thirdLerp.x ? 0.01f : 0f, secondLerp.y == thirdLerp.y ? 0.01f : 0f, secondLerp.z == thirdLerp.z ? 0.01f : 0f);

                averagePosition = (firstLerp + secondLerp + thirdLerp);

                listenersAveragePosition = (nearestListenerPosition + secondNearestListenerPosition + thirdNearestListenerPosition) / 3;

                averagePosition = Quaternion.Euler(0, 90, 0) * averagePosition;

                averagePosition += new Vector3(0.0001f, 0.0001f, 0.0001f);
            } else if (!nearestListenerNULL && !secondNearestListenerNULL && !thirdNearestListenerNULL && !fourthNearestListenerNULL) {

                Vector3 firstLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (nearestListenerPosition - thisPosition), nearestListenerBlendDistance * nearestListenerBlend));
					Vector3 secondLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (secondNearestListenerPosition - thisPosition), secondNearestListenerBlendDistance * secondNearestListenerBlend));
					Vector3 thirdLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (thirdNearestListenerPosition - thisPosition), thirdNearestListenerBlendDistance * thirdNearestListenerBlend));
					Vector3 fourthLerp = (Vector3.Lerp (Vector3.zero, audioSourceTransform.InverseTransformDirection (fourthNearestListenerPosition - thisPosition), fourthNearestListenerBlendDistance * fourthNearestListenerBlend));

                secondLerp += new Vector3(firstLerp.x == secondLerp.x ? 0.01f : 0f, firstLerp.y == secondLerp.y ? 0.01f : 0f, firstLerp.z == secondLerp.z ? 0.01f : 0f);

                thirdLerp += new Vector3(secondLerp.x == thirdLerp.x ? 0.01f : 0f, secondLerp.y == thirdLerp.y ? 0.01f : 0f, secondLerp.z == thirdLerp.z ? 0.01f : 0f);

                fourthLerp += new Vector3(fourthLerp.x == thirdLerp.x ? 0.01f : 0f, fourthLerp.y == thirdLerp.y ? 0.01f : 0f, fourthLerp.z == thirdLerp.z ? 0.01f : 0f);

                averagePosition = (firstLerp + secondLerp + thirdLerp + fourthLerp);

                listenersAveragePosition = (nearestListenerPosition + secondNearestListenerPosition + thirdNearestListenerPosition + fourthNearestListenerPosition) / 4;

                averagePosition = Quaternion.Euler(0, 90, 0) * averagePosition;

                averagePosition += new Vector3(0.0001f, 0.0001f, 0.0001f);
            }
			

		if (OccludeSound && !spatialMode2D) {

				if (play || playing || waitForReverb > 0) {
					if (Physics.Linecast (thisPosition, nearestListenerPosition, MultiPoolAudioSystem.audioManager.OccludeCheck, QueryTriggerInteraction.Ignore)) {

						occludeMultiplier = MultiPoolAudioSystem.audioManager.OccludeMultiplier * (spatialMode2D ? 0 : 1) * (6 / 5f);

						occludeFilterValue = Mathf.Lerp (22000, 2000, occludeMultiplier);

						smoothOcclude = Mathf.Lerp (smoothOcclude, occludeMultiplier, customUnscaledDeltaTime * 10f);

						if (!occludeFilter.isActiveAndEnabled)
							occludeFilter.enabled = true;

						occludeFilter.cutoffFrequency = Mathf.Lerp (occludeFilter.cutoffFrequency, occludeFilterValue, customUnscaledDeltaTime * 10f);

						if (play) {
							smoothOcclude = occludeMultiplier;
						}

					} else {

						smoothOcclude = Mathf.Lerp (smoothOcclude, 0, customUnscaledDeltaTime * 10f);

						occludeFilter.cutoffFrequency = Mathf.Lerp (occludeFilter.cutoffFrequency, 22000, customUnscaledDeltaTime * 10f);

						if (occludeFilter.cutoffFrequency > 21999) {
							occludeFilter.cutoffFrequency = 22000;
							occludeFilter.enabled = false;
						}

						if (play) {
							smoothOcclude = 0;
						}

					}
				}

			} else {
				smoothOcclude = 0;

				if (occludeFilter.isActiveAndEnabled)
					occludeFilter.enabled = false;
			}

			if (OverrideValues && overrideVolumeRolloff || (!OverrideValues || !overrideVolumeRolloff) && (playing||play) && audioObject.volumeRolloff) {
				volumeMultiplier = Mathf.Lerp (0, 1, ( OverrideValues && overrideVolumeRolloff && volumeRolloffOverride?volumeRolloffCurveOverride.Evaluate (nearestListenerBlend):audioObject.volumeRolloffCurve.Evaluate (nearestListenerBlend) ) );

                //  if (usingLogRolloff)
                //    volumeMultiplier = Mathf.Clamp01(1.0f / (1.0f + MultiPoolAudioSystem.audioManager.config.volumeRolloffScale * (nearestListenerDistance - 1.0f)));
              
                float startDistance = Mathf.Clamp01(nearestListenerDistance - StartDistance);
                if (StartDistance == 0)
                    startDistance = 1f;

                audioSource.volume = volume * (spatialMode2D?1:volumeMultiplier) * (fadeOut?Mathf.Clamp01(fadeOutMultiplier):1) * (fadeIn?Mathf.Clamp01(fadeInMultiplier):1) * startDistance;

				if (outOfRange || noListener) {
                    audioSource.volume = 0;
				}

			} else {

				audioSource.volume = volume * (fadeOut?Mathf.Clamp01(fadeOutMultiplier):1) * (fadeIn?Mathf.Clamp01(fadeInMultiplier):1);
				if (outOfRange || noListener) {
					audioSource.volume = 0;
				}

			}

			if (OccludeSound && !spatialMode2D) {

			audioSource.volume *= 1 - (smoothOcclude);

			}
           

			if (play || playing || waitForReverb > 0) {
				smoothedAveragePosition = Vector3.ClampMagnitude(averagePosition,1f);
			}

            bool usingLog = usingLogRolloff && audioSource.volume > 0.01f;

            if (usingLog)
            {
                //Log Implementation
            }

            if (nearestListenerBlend > 0 || spatialMode2D) {

				if (play || playing || waitForReverb > 0) {
					if (!spatialMode2D) {
						if (!float.IsNaN (MultiPoolAudioSystem.audioManager.AudioListenerPosition.x) && !float.IsNaN (MultiPoolAudioSystem.audioManager.AudioListenerPosition.y) && !float.IsNaN (MultiPoolAudioSystem.audioManager.AudioListenerPosition.z) &&
						   !float.IsNaN (smoothedAveragePosition.x) && !float.IsNaN (smoothedAveragePosition.y) && !float.IsNaN (smoothedAveragePosition.z)) {
                            audioSourceTransform.position = new Vector3 (MultiPoolAudioSystem.audioManager.AudioListenerPosition.x - smoothedAveragePosition.x, MultiPoolAudioSystem.audioManager.AudioListenerPosition.y - smoothedAveragePosition.y, MultiPoolAudioSystem.audioManager.AudioListenerPosition.z - smoothedAveragePosition.z);
						}
					} else {

						audioSourceTransform.position = MultiPoolAudioSystem.audioManager.AudioListenerPosition;

					}

					audioSource.maxDistance = 1000000;
					audioSource.minDistance = 1000000 - 1;
               
					bool enableReverb = (waitForReverb > 0 || loop) && nearestListener.ReverbPreset != AudioReverbPreset.Off && audioSource.reverbZoneMix > 0;

					if (enableReverb) {

						audioSource.bypassReverbZones = false;
						listenerReverb.enabled = true;
						listenerReverb.reverbPreset = nearestListener.ReverbPreset;
						listenerReverb.maxDistance = Mathf.Lerp (listenerReverb.maxDistance, audioSource.maxDistance, customUnscaledDeltaTime * 8f);
						listenerReverb.minDistance = Mathf.Lerp (listenerReverb.minDistance, audioSource.maxDistance - 10, customUnscaledDeltaTime * 8f);

					} else {
					
						listenerReverb.reverbPreset = AudioReverbPreset.Off;

						listenerReverb.maxDistance = Mathf.Lerp (listenerReverb.maxDistance, 0, customUnscaledDeltaTime * 8f);
						listenerReverb.minDistance = Mathf.Lerp (listenerReverb.minDistance, 0, customUnscaledDeltaTime * 8f);

					}
				}
                else
                {
                    audioSourceTransform.position = MultiPoolAudioSystem.audioManager.AudioListenerPosition;
                }

				if (outOfRange) {

                    if (OnRange != null)
                    {
                        OnRange.Invoke();
                    }
                    outOfRange = false;


				}
			} else {

				if (!outOfRange) {
					audioSourceTransform.localPosition = Vector3.zero;
                    if (OnOutOfRange != null)
                    {
                        OnOutOfRange.Invoke();
                    }
                    outOfRange = true;

				}

				audioSource.maxDistance = maxDistance;
				audioSource.minDistance = minDistance;

                audioSourceTransform.position = MultiPoolAudioSystem.audioManager.AudioListenerPosition;

            }

			if (prevPaused != LocalPause) {
				prevPaused = LocalPause;
				Pause ();
			}

            bool playSfx = play && !ChangingParameters && (!delayed && true || delayed && delay == 0);


            if (!outOfRange || playSfx) {
				if (!LocalPause) {

                    float pitchValue = Mathf.Clamp((!nearestListenerNULL ? pitch : 1) * pitchTimeScale * dopplerPitch, -3, 3);
                    if (audioSource.pitch != pitchValue)
                    {
                        if (pitchValue>3 || pitchValue<-3)
                        {
                            Debug.LogError("DAFUQQ");
                        }
                        audioSource.pitch = pitchValue;
                    }

                } else {
                    float pitchValue = 0;
                    if (audioSource.pitch != pitchValue)
                        audioSource.pitch = pitchValue;
				}
                if (audioSource.panStereo != stereoPan)
                {
                    float stereoPanValue = stereoPan;
                    if (audioSource.panStereo != stereoPanValue)
                        audioSource.panStereo = stereoPanValue;
                }

               
                float nearest2DBlend = !nearestListenerNULL ? Mathf.Clamp01((nearestListenerDistance-0.05f)/0.1f) : 1f;
                float spatialBlendValue = spatialMode2D /*|| nearestListenerDistance < 0.05f*/ ? 0 : nearest2DBlend;

                if (audioSource.spatialBlend != spatialBlendValue)
                    audioSource.spatialBlend = spatialBlendValue;

                float dopplerLevelValue = 0;

                if (audioSource.dopplerLevel != dopplerLevelValue)
                    audioSource.dopplerLevel = dopplerLevelValue;//dopplerLevel;

                float spreadValue = spread;

                if (audioSource.spread != spreadValue)
                    audioSource.spread = spreadValue;

				if (OccludeSound) {
					audioSource.spread += 50 * smoothOcclude;
				}

                bool spatializeValue = spatialize;

                if (audioSource.spatialize != spatializeValue)
                    audioSource.spatialize = spatializeValue;

                #if UNITY_5_5_OR_NEWER
                bool spatializePostEffectsValue = false;

                if (audioSource.spatializePostEffects != spatializePostEffectsValue)
                    audioSource.spatializePostEffects = spatializePostEffectsValue;
                #endif

                bool muteValue = Mute || clamped;

                if (audioSource.mute != muteValue)
                    audioSource.mute = muteValue;
			}

            if (outOfRange)
            {

                if (LocalPause)
                {
                    float pitchValue = 0;
                    if (audioSource.pitch != pitchValue)
                        audioSource.pitch = pitchValue;
                }

                bool muteValue = Mute || clamped;

                if (audioSource.mute != muteValue)
                    audioSource.mute = muteValue;

			}

			if (playing && ChangingParameters || !firstTimePlaying && play && ChangingParameters) {

				ChangingParameters = false;
			
			}

			if (play && !ChangingParameters && (!delayed && true || delayed && delay == 0)) {

				LatePlay ();

			}

			if (globalPaused && !MultiAudioManager.Paused && onFocus || globalPaused && IgnoreListenerPause && onFocus) {
				globalPaused = false;
			}
				
			updateTime = Time.realtimeSinceStartup;

            Doppler();


            if (ChangingParameters) {

				firstTimePlaying = false;

				SetParameters ();

				ChangingParameters = false;
				return;

			}

	}

        /// <summary>
        /// Determines if the <see cref="MultiAudioSource"/> is using a specific Override Value.
        /// </summary>
        /// <param name="value">The Override Value to Check.</param>
        /// <returns><c>true</c> if the specified Override Value is currently in use; otherwise, <c>false</c>.</returns>
        public bool IsValueOverridden(ValueOverride value)
        {
            bool overridden = false;

            if (OverrideValues)
            {
                switch (value)
                {
                    case ValueOverride.AudioClip:
                        overridden = overrideAudioClip;
                        break;
                    case ValueOverride.RandomStartVolume:
                        overridden = overrideRandomVolumeMultiplier;
                        break;
                    case ValueOverride.Volume:
                        overridden = overrideVolume;
                        break;
                    case ValueOverride.RandomStartPitch:
                        overridden = overrideRandomPitchMultiplier;
                        break;
                    case ValueOverride.Pitch:
                        overridden = overridePitch;
                        break;
                    case ValueOverride.Spread:
                        overridden = overrideSpread;
                        break;
                    case ValueOverride.SpatialMode:
                        overridden = overrideSpatialMode;
                        break;
                    case ValueOverride.StereoPan2D:
                        overridden = overrideStereoPan;
                        break;
                    case ValueOverride.Distance:
                        overridden = overrideDistance;
                        break;
                    case ValueOverride.ReverbZoneMix:
                        overridden = overrideReverbZone;
                        break;
                    case ValueOverride.DopplerLevel:
                        overridden = overrideDopplerLevel;
                        break;
                    case ValueOverride.VolumeRolloff:
                        overridden = overrideVolumeRolloff;
                        break;
                    case ValueOverride.MixerGroup:
                        overridden = overrideMixerGroup;
                        break;
                    case ValueOverride.Spatialize:
                        overridden = overrideSpatialize;
                        break;
                }
            }

            return overridden;

        }

        [HideInInspector] public bool usingLogRolloff = false;

        void LogCurveDist()
        {

            usingLogRolloff = true;


        }

	void SetParameters(bool onUpdate=false)
	{
		if (!onUpdate) {
			volumeRandomStartMultiplier = Random.Range (audioObject.minVolumeMultiplier, audioObject.maxVolumeMultiplier);
			pitchRandomStartMultiplier = Random.Range (audioObject.minPitchMultiplier, audioObject.maxPitchMultiplier);
		}

			if (OverrideValues && overrideReverbZone) {
				audioSource.reverbZoneMix = reverbZoneMixOverride;
			} else {
				audioSource.reverbZoneMix = audioObject.reverbZoneMix;
			}

		if (!OverrideValues || !overrideSpread) {
			spread = audioObject.spread;
		} else {
			spread = spreadOverride;
		}
		if (!OverrideValues || !overrideVolume) {
				volume = Mathf.Clamp01 ( (audioObject.volume * (OverrideValues && overrideRandomVolumeMultiplier && !randomVolumeMultiplier?1:volumeRandomStartMultiplier)) * Mathf.Clamp01(MasterVolume) );
		} else {
				volume = Mathf.Clamp01 ( (volumeOverride * (OverrideValues && overrideRandomVolumeMultiplier && !randomVolumeMultiplier?1:volumeRandomStartMultiplier) ) * Mathf.Clamp01(MasterVolume) );
		}
		if (!OverrideValues || !overridePitch) {
                pitch = Mathf.Clamp (audioObject.pitch * (OverrideValues && overrideRandomPitchMultiplier && !randomPitchMultiplier?1:pitchRandomStartMultiplier) , -3f, 3f);
		} else {
                pitch = Mathf.Clamp (pitchOverride * (OverrideValues && overrideRandomPitchMultiplier && !randomPitchMultiplier?1:pitchRandomStartMultiplier), -3f, 3f);
		}

		if (!OverrideValues || !overrideDopplerLevel) {
			dopplerLevel = audioObject.dopplerLevel;
		} else {
			dopplerLevel = dopplerLevelOverride;
		}

		if (!OverrideValues || !overrideSpatialMode) {
			spatialMode2D = audioObject.spatialMode2D;
		} else {
			spatialMode2D = spatialMode2DOverride;
		}

		if (!OverrideValues || !overrideSpatialize) {
			spatialize = audioObject.spatialize;
		} else {
			spatialize = spatializeOverride;
		}

		if (!OverrideValues || !overrideStereoPan) {
			stereoPan = Mathf.Clamp (audioObject.stereoPan, -1, 1);
		} else {
			stereoPan = Mathf.Clamp (stereoPanOverride, -1, 1);
		}

		if (OverrideValues && overrideMixerGroup && mixerGroupOverride!=null)
			audioSource.outputAudioMixerGroup = mixerGroupOverride;
		else {

			if (audioObject.mixerGroup!=null) {
				audioSource.outputAudioMixerGroup = audioObject.mixerGroup;
			} else {
				if (audioObject.isBGM && audioSource.outputAudioMixerGroup!=MultiPoolAudioSystem.audioManager.BgmMixerGroup || !audioObject.isBGM && audioSource.outputAudioMixerGroup!=MultiPoolAudioSystem.audioManager.SfxMixerGroup)
					audioSource.outputAudioMixerGroup = audioObject.isBGM?MultiPoolAudioSystem.audioManager.BgmMixerGroup:MultiPoolAudioSystem.audioManager.SfxMixerGroup;
			}

		}

            if (!OverrideValues || !overrideDistance)
            {
                maxDistance = audioObject.maxDistance;
                minDistance = audioObject.minDistance;
            }
            else
            {
                maxDistance = maxDistanceOverride;
                minDistance = minDistanceOverride;
            }

            if (!onUpdate)
            {
                if (OverrideValues && overrideVolumeRolloff && volumeRolloffOverride)
                {
                    usingLogRolloff = false;
                }
                else
                {
                  

                    if (CompareCurves(logCurve,audioObject.volumeRolloffCurve))
                    {
                
                        LogCurveDist();

                    }
                    else
                    {
                        usingLogRolloff = false;
                    }
                }
            }

    }

         bool CompareCurves(AnimationCurve a, AnimationCurve b)
        {
            bool same = true;

            if (a.length != b.length)
            {
                same = false;
            }

            if (b.keys.Length == a.keys.Length)
            {
                for (int i = 0; i < a.keys.Length; i++)
                {

                    if (a.keys[i].value != b.keys[i].value || a.keys[i].time != b.keys[i].time)
                    {
                        same = false;
                        break;
                    }
                 }
            }
            else
            {
                same = false;
            }


            return same;
        }

    void RealPlay(float _delay = 0, int _channel = -1, Transform _targetToFollow = null, float _fadeInTime = 0f, bool looping = false)
	{
           
            // // // //
 
            thisPosition = emitterTransform.position;
       
            GetNearestListeners();

            // // // //

            if (!init) {
				Awake ();
			}
        
		if (audioObject == null) {

			if (InsidePool)
				Debug.LogWarning ("<i>Audio Object</i> to play is missing or invalid", MultiPoolAudioSystem.audioManager.gameObject);
			else {
				Debug.LogWarning ("Audio Source: " + "<b>"+gameObject.name+"</b>" + " doesn't have a valid <i>Audio Object</i> to play",gameObject);
			}
					
			Stop ();

			return;

		}

			if (InsidePool) {

                if (!looping)
                {
                    persistsBetweenScenes = false;
                    ignoreListenerPause = false;
                    Mute = false;
                    clamped = false;
                    TargetToFollow = null;
                    overrideAudioClip = false;
                    overrideMixerGroup = false;
                    overrideDistance = false;
                    overrideDopplerLevel = false;
                    overridePitch = false;
                    overrideRandomPitchMultiplier = false;
                    overrideRandomVolumeMultiplier = false;
                    overrideReverbZone = false;
                    overrideSpatialize = false;
                    overrideSpatialMode = false;
                    overrideSpread = false;
                    overrideStereoPan = false;
                    OverrideValues = false;
                    overrideVolume = false;
                    overrideVolumeRolloff = false;
                    overrideMasterVolume = false;
                    overrideOccludeSound = false;
                    overridePlayUpdateMode = false;
                    overridePaused = false;
                    overrideDelayUpdateMode = false;
                    overrideIgnoreListenerPause = false;
                }
                else
                {
                    RemoveClampedSource(audioObject);
                }

            } else {

                if (!looping)
                {
                    Stop();
                }
                else
                {
                    RemoveClampedSource(audioObject);
                }

			}
				

		delay = _delay;
		delayed = delay > 0;

		AudioClip _clip = audioObject.RandomClip;

		if (!Object.ReferenceEquals(_clip, null) && audioObject.loop && audioObject.playClipsSequentially) {

			if (loopIndex >= audioObject.clips.Length)
				loopIndex = 0;

			_clip = audioObject.clips [loopIndex];
			loopIndex += 1;

		}


            if (!Object.ReferenceEquals(_clip, null) && !audioObject.loop && audioObject.playClipsSequentially)
            {

                if (sequenceIndex >= audioObject.clips.Length)
                    sequenceIndex = 0;

                _clip = audioObject.clips[sequenceIndex];
                sequenceIndex += 1;

            }

            if (overrideAudioClip && !Object.ReferenceEquals(audioClipOverride, null))
			_clip = audioClipOverride;

		if (_clip == null) {

			Debug.LogWarning ("Audio Object: " + "<b>"+audioObject.name+"</b>" + " doesn't have a valid <i>Audio Clip</i> to play",audioObject);

			Stop ();

			return;

		}

		sameClip = audioSource.clip == _clip;

		channel = _channel;

			if (!playOnStart || InsidePool) {
			TargetToFollow = _targetToFollow;
		}

		audioSource.clip = _clip;

		SetParameters ();

			clamped = false;

          //  if (!usingShape)
           // {
                if (TargetToFollow)
                {

                    transform.position = TargetToFollow.position;

                }
          //  }

		loop = audioObject.loop;

			if (_fadeInTime > 0) {

				fadeOut = false;
				fadeIn = true;
				fadeInMultiplier = 0f;
				fadeInTime = Mathf.Clamp (_fadeInTime, 0.1f, Mathf.Infinity);

			}

		play = true;
		firstTimePlaying = true;



	}

		void AddClampedSource(AudioObject _audioObject)
		{

			if (_audioObject == null)
				return;

			if (_audioObject.maxSources < 1)
				return;

			if (!MultiAudioManager.clampedSources.ContainsKey (_audioObject)) {
				MultiAudioManager.clampedSources.Add (_audioObject, 1);
				MultiAudioManager.clampedSourcesCount++;
			} else {
				MultiAudioManager.clampedSources[_audioObject]++;
			}

		}

		public void RemoveClampedSource(AudioObject _audioObject)
		{

			if (_audioObject == null)
				return;

			if (_audioObject.maxSources < 1 || !playing)
				return;

			if (MultiAudioManager.clampedSources.ContainsKey (_audioObject)) {
				if (MultiAudioManager.clampedSources [_audioObject] > 0) {
					MultiAudioManager.clampedSources [_audioObject]--;
				} else {
					if (MultiAudioManager.clampedSourcesCount > 15) {
						MultiAudioManager.clampedSources.Remove (_audioObject);
						MultiAudioManager.clampedSourcesCount--;
					}
				}
			}

		}


        void RealPlayOverride(AudioClip _audioClipOverride, float _delay = 0, int _channel = -1, Transform _targetToFollow = null, float _fadeInTime = 0f, bool looping = false)
        {

            // // // //


            thisPosition = emitterTransform.position;

            GetNearestListeners();

            // // // //

            if (!init)
            {
                Awake();
            }

            if (audioObject == null)
            {

                if (InsidePool)
                    Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid", MultiPoolAudioSystem.audioManager.gameObject);
                else
                {
                    Debug.LogWarning("Audio Source: " + "<b>" + gameObject.name + "</b>" + " doesn't have a valid <i>Audio Object</i> to play", gameObject);
                }

                Stop();

                return;

            }


            if (InsidePool)
            {

                if (!looping)
                {
                    persistsBetweenScenes = false;
                    ignoreListenerPause = false;
                    Mute = false;
                    clamped = false;
                    TargetToFollow = null;
                    overrideAudioClip = false;
                    overrideMixerGroup = false;
                    overrideDistance = false;
                    overrideDopplerLevel = false;
                    overridePitch = false;
                    overrideRandomPitchMultiplier = false;
                    overrideRandomVolumeMultiplier = false;
                    overrideReverbZone = false;
                    overrideSpatialize = false;
                    overrideSpatialMode = false;
                    overrideSpread = false;
                    overrideStereoPan = false;
                    OverrideValues = false;
                    overrideVolume = false;
                    overrideVolumeRolloff = false;
                    overrideMasterVolume = false;
                    overrideOccludeSound = false;
                    overridePlayUpdateMode = false;
                    overridePaused = false;
                    overrideDelayUpdateMode = false;
                    overrideIgnoreListenerPause = false;
                }
                else
                {
                    RemoveClampedSource(audioObject);
                }
            }
            else
            {

                if (!looping)
                {
                    Stop();
                }
                else
                {
                    RemoveClampedSource(audioObject);
                }

            }

            clamped = false;

            delay = _delay;
            delayed = delay > 0;

            AudioClip _clip = _audioClipOverride;
            audioClipForOverride = _clip;

            if (_clip == null)
            {

                Debug.LogWarning("Audio Object: " + "<b>" + audioObject.name + "</b>" + " doesn't have a valid <i>Audio Clip</i> to play", audioObject);

                Stop();

                return;

            }

            sameClip = audioSource.clip == _clip;

            channel = _channel;

            if (!playOnStart || InsidePool)
            {
                TargetToFollow = _targetToFollow;
            }

            audioSource.clip = _clip;


            SetParameters();

           // if (!usingShape)
           // {
                if (TargetToFollow)
                {

                    transform.position = TargetToFollow.position;

                }
            //}

            loop = audioObject.loop;

            if (_fadeInTime > 0)
            {

                fadeOut = false;
                fadeIn = true;
                fadeInMultiplier = 0f;
                fadeInTime = Mathf.Clamp(_fadeInTime, 0.1f, Mathf.Infinity);

            }

            play = true;
            firstTimePlaying = true;
           
        }
/*
        /// <summary>
        /// Sets the Playback position in seconds of the <see cref="MultiAudioSource"/>.
        /// </summary>
        /// <param name="time">The time in seconds</param>
        public void SetPlaybackTime(float time)
        {
            if (IsPlaying)
            {
                audioSource.time = time;
            }
        }*/

	#region No Delay Methods
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/>.
	/// </summary>
	public void Play()
	{

		RealPlay (0f, defaultChannel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> at the specified Channel.
	/// </summary>
	/// <param name="_channel">Channel.</param>
	public void Play(int _channel)
	{

		RealPlay (0f,_channel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> and makes that follow a target.
	/// </summary>
	/// <param name="_targetToFollow">Target to follow.</param>
	public void Play(Transform _targetToFollow)
	{

		RealPlay (0f, defaultChannel, _targetToFollow);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> at the specified Channel and makes that follow a target.
	/// </summary>
	/// <param name="_channel">Channel.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	public void Play(int _channel,Transform _targetToFollow)
	{

		RealPlay (0f,_channel, _targetToFollow);

	}


        void PlayLoop(int _channel, Transform _targetToFollow)
        {

            RealPlay(0f, _channel, _targetToFollow,0,true);

        }


        void PlayOverrideLoop(AudioClip audioClipOverride, int _channel, Transform _targetToFollow)
        {

            RealPlayOverride(audioClipOverride, 0f, _channel, _targetToFollow,0,true);

        }
        #endregion

        #region Delay Methods
        /// <summary>
        /// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds.
        /// </summary>
        /// <param name="_delay">Delay.</param>
        public void Play(float _delay)
	{

		RealPlay (_delay, defaultChannel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds at the specified Channel.
	/// </summary>
	/// <param name="_channel">Channel.</param>
	/// <param name="_delay">Delay.</param>
	public void Play(int _channel,float _delay)
	{

		RealPlay (_delay,_channel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds and makes that follow a target.
	/// </summary>
	/// <param name="_targetToFollow">Target to follow.</param>
	/// <param name="_delay">Delay.</param>
		public void Play(Transform _targetToFollow,float _delay)
	{

		RealPlay (_delay, defaultChannel, _targetToFollow);

	}
	/// <summary>
	/// Play the <see cref="MultiAudioSource"/> with a delay specified in seconds at the specified Channel and makes that follow a target.
	/// </summary>
	/// <param name="_channel">Channel.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	/// <param name="_delay">Delay.</param>
		public void Play(int _channel,Transform _targetToFollow,float _delay)
	{

		RealPlay (_delay, _channel, _targetToFollow);

	}

	// No Delay Override Methods
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	public void PlayOverride(AudioClip audioClipOverride)
	{

		RealPlayOverride (audioClipOverride,0f, defaultChannel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip at the specified Channel.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_channel">Channel.</param>
	public void PlayOverride(AudioClip audioClipOverride, int _channel)
	{

		RealPlayOverride (audioClipOverride,0f,_channel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip and makes that follow a target.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	public void PlayOverride(AudioClip audioClipOverride, Transform _targetToFollow)
	{

		RealPlayOverride (audioClipOverride,0f, defaultChannel, _targetToFollow);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip at the specified Channel and makes that follow a target.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_channel">Channel.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	public void PlayOverride(AudioClip audioClipOverride, int _channel,Transform _targetToFollow)
	{

		RealPlayOverride (audioClipOverride,0f,_channel, _targetToFollow);

	}

	//Delay Override Methods
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_delay">Delay.</param>
	public void PlayOverride(AudioClip audioClipOverride, float _delay)
	{

		RealPlayOverride (audioClipOverride,_delay, defaultChannel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds at the specified Channel.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_channel">Channel.</param>
	/// <param name="_delay">Delay.</param>
	public void PlayOverride(AudioClip audioClipOverride, int _channel,float _delay)
	{

		RealPlayOverride (audioClipOverride,_delay,_channel, null);

	}
	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds and makes that follow a target.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	/// <param name="_delay">Delay.</param>
	public void PlayOverride(AudioClip audioClipOverride, Transform _targetToFollow,float _delay)
	{

		RealPlayOverride (audioClipOverride,_delay, defaultChannel, _targetToFollow);

	}

	/// <summary>
	/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds at the specified Channel and makes that follow a target.
	/// </summary>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="_channel">Channel.</param>
	/// <param name="_targetToFollow">Target to follow.</param>
	/// <param name="_delay">Delay.</param>
	public void PlayOverride(AudioClip audioClipOverride, int _channel,Transform _targetToFollow,float _delay)
	{

		RealPlayOverride (audioClipOverride,_delay, _channel, _targetToFollow);

	}
	#endregion

	#region No Delay Fade In Methods
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/>.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		public void PlayFadeIn(float fadeInTime)
		{

			RealPlay (0f, defaultChannel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> at the specified Channel.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_channel">Channel.</param>
		public void PlayFadeIn(float fadeInTime,int _channel)
		{

			RealPlay (0f,_channel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		public void PlayFadeIn(float fadeInTime,Transform _targetToFollow)
		{

			RealPlay (0f, defaultChannel, _targetToFollow,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> at the specified Channel and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		public void PlayFadeIn(float fadeInTime,int _channel,Transform _targetToFollow)
		{

			RealPlay (0f,_channel, _targetToFollow,fadeInTime);

		}

		//Delay Fade In Methods
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeIn(float fadeInTime,float _delay)
		{

			RealPlay (_delay, defaultChannel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds at the specified Channel.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeIn(float fadeInTime,int _channel,float _delay)
		{

			RealPlay (_delay,_channel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> with a delay specified in seconds and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeIn(float fadeInTime,Transform _targetToFollow,float _delay)
		{

			RealPlay (_delay, defaultChannel, _targetToFollow,fadeInTime);

		}
		/// <summary>
		/// Play the <see cref="MultiAudioSource"/> with a delay specified in seconds at the specified Channel and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeIn(float fadeInTime,int _channel,Transform _targetToFollow,float _delay)
		{

			RealPlay (_delay,_channel, _targetToFollow,fadeInTime);

		}

	#endregion

	#region No Delay Override Methods
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride)
		{

			RealPlayOverride (audioClipOverride,0f, defaultChannel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip at the specified Channel.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_channel">Channel.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, int _channel)
		{

			RealPlayOverride (audioClipOverride,0f,_channel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, Transform _targetToFollow)
		{

			RealPlayOverride (audioClipOverride,0f, defaultChannel, _targetToFollow,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip at the specified Channel and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, int _channel,Transform _targetToFollow)
		{

			RealPlayOverride (audioClipOverride,0f,_channel, _targetToFollow,fadeInTime);

		}

		//Delay Override Methods
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, float _delay)
		{

			RealPlayOverride (audioClipOverride,_delay, defaultChannel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds at the specified Channel.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, int _channel,float _delay)
		{

			RealPlayOverride (audioClipOverride,_delay,_channel, null,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, Transform _targetToFollow,float _delay)
		{

			RealPlayOverride (audioClipOverride,_delay, defaultChannel, _targetToFollow,fadeInTime);

		}
		/// <summary>
		/// Plays the <see cref="MultiAudioSource"/> using its <see cref="AudioObject"/> but with another Audio Clip with a delay specified in seconds at the specified Channel and makes that follow a target.
		/// </summary>
		/// <param name="fadeInTime">Fade In Time.</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="_channel">Channel.</param>
		/// <param name="_targetToFollow">Target to follow.</param>
		/// <param name="_delay">Delay.</param>
		public void PlayFadeInOverride(float fadeInTime,AudioClip audioClipOverride, int _channel,Transform _targetToFollow,float _delay)
		{

			RealPlayOverride (audioClipOverride,_delay, _channel, _targetToFollow,fadeInTime);

		}

	#endregion

		void Pause()
		{

			if (!isActiveAndEnabled)
				return;
			
			if (LocalPause && !Object.ReferenceEquals(audioObject, null)) {
				RemoveClampedSource (audioObject);
			}

			if (!LocalPause && !Object.ReferenceEquals(audioObject, null)) {

				if (!MultiAudioManager.ClampedAudioCanBePlayed (audioObject)) {

					Stop ();

					return;

				}

				AddClampedSource (audioObject);
			}

		}
	
	/// <summary>
	/// Stops playing the <see cref="MultiAudioSource"/>.
	/// </summary>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
	public void Stop(bool disableObject=false)
	{
			if (!init) {
				Awake ();
			}

            audioSource.enabled = false;
            audioSource.Stop ();

			if (InsidePool) {

				persistsBetweenScenes = false;
				ignoreListenerPause = false;
				Mute = false;
				clamped = false;
				TargetToFollow = null;
				overrideAudioClip = false;
				overrideMixerGroup = false;
				overrideDistance = false;
				overrideDopplerLevel = false;
				overridePitch = false;
				overrideRandomPitchMultiplier = false;
				overrideRandomVolumeMultiplier = false;
				overrideReverbZone = false;
				overrideSpatialize = false;
				overrideSpatialMode = false;
				overrideSpread = false;
				overrideStereoPan = false;
				OverrideValues = false;
				overrideVolume = false;
				overrideVolumeRolloff = false;
                overrideMasterVolume = false;
                overrideOccludeSound = false;
                overridePlayUpdateMode = false;
                overridePaused = false;
                overrideDelayUpdateMode = false;
                overrideIgnoreListenerPause = false;

            }
		loop = false;

			RemoveClampedSource (audioObject);

            if (play || playing) {
                if (OnStop != null)
                {
                    OnStop.Invoke();
                }
            }

            play = false;
		firstTimePlaying = false;
		playing = false;
		delay = 0;
		delayed = false;
		stopDelay = false;
		fadeIn = false;
		fadeOut = false;
		onFocus = true;
			if (!Object.ReferenceEquals(audioClipForOverride, null)) {
				audioClipForOverride = null;
			}

		loopIndex = 0;

          

            if (InsidePool) {
			overrideVolume = false;
			overridePitch = false;
			overrideSpread = false;
			overrideReverbZone = false;
                overrideMasterVolume = false;
                mixerGroupOverride = null;
			overrideDistance = false;
			overrideOccludeSound = false;

			gameObject.SetActive (false);

                OnPlay = new UnityEngine.Events.UnityEvent();
                OnStop = new UnityEngine.Events.UnityEvent();
                OnLoop = new UnityEngine.Events.UnityEvent();
                OnOutOfRange = new UnityEngine.Events.UnityEvent();
                OnRange = new UnityEngine.Events.UnityEvent();

            } else {

			if (disableObject) {
				gameObject.SetActive (false);
			}

		}

	}

	/// <summary>
	/// Stops playing the <see cref="MultiAudioSource"/> using a fade out.
	/// </summary>
	/// <param name="_fadeOutTime">Fade out time.</param>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
	public void FadeOut(float _fadeOutTime=1f,bool disableObject=false)
	{
			if (InsidePool)
				disableObject = true;
			
			channel = -1;
		if (!fadeOut) {
			fadeOutMultiplier = 1;
			fadeOutTime = Mathf.Clamp(_fadeOutTime,0.1f,Mathf.Infinity);
			fadeOut = true;
			fadeOutDisableObject = disableObject;
		}

	}

	/// <summary>
	/// Stops playing the <see cref="MultiAudioSource"/> with a delay specified in seconds.
	/// </summary>
	/// <param name="_delay">Delay time for Stop.</param>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
	public void StopDelayed(float _delay,bool disableObject=false)
	{

			if (_delay > 0) {

				if (InsidePool)
					disableObject = true;

				stopDelayTime = Mathf.Clamp (_delay, 0.1f, Mathf.Infinity);
				stopDelayFadeOut = false;
				stopDelay = true;
				stopDelayDisableObject = disableObject;
			} else {

				Stop (disableObject);
			}

	}

	/// <summary>
	/// Stops playing the <see cref="MultiAudioSource"/> with a delay specified in seconds using a fade out.
	/// </summary>
	/// <param name="_delay">Delay time for Stop.</param>
	/// <param name="_fadeOutTime">Fade out time.</param>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
	public void FadeOutDelayed(float _delay,float _fadeOutTime=1f,bool disableObject=false)
	{

		if (_delay>0) {

				if (InsidePool)
					disableObject = true;

			stopDelayTime = Mathf.Clamp(_delay,0.1f,Mathf.Infinity);
			stopDelay = true;
			stopDelayDisableObject = disableObject;
			stopDelayFadeOut = true;
			stopDelayFadeOutTime = _fadeOutTime;
		}
			else {
				FadeOut (_fadeOutTime,disableObject);
			}

	}

	/// <summary>
	/// Interrupts the delayed stop.
	/// </summary>
	public void InterruptDelayedStop()
	{

		stopDelay = false;

	}

	private bool isApplicationQuitting = false;
	void OnApplicationQuit () {
		isApplicationQuitting = true;
	}

	void OnDestroy()
	{

		if (isApplicationQuitting)
			return;

		if (MultiPoolAudioSystem.audioManager!=null && MultiPoolAudioSystem.audioManager.AudioSources!=null)
		MultiPoolAudioSystem.audioManager.AudioSources.Remove (this);

        if (audioSourceTransform!=null)
        Destroy(audioSourceTransform.gameObject);


	}

		void OnDisable()
		{

			if (isApplicationQuitting)
				return;

			Stop ();

		}

	// Get the Nearest Enabled Multi Audio Listener
	void GetNearestListeners()
	{

		Vector3 closestTargetForward = Vector3.forward;
		Vector3 closestPosition = Vector3.zero;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 tempTransform = Vector3.forward;
		Vector3 tempPosition = Vector3.zero;

		nearestListenerNULL = true;
		secondNearestListenerNULL = true;
		thirdNearestListenerNULL = true;
		fourthNearestListenerNULL = true;

		nearestListenerPosition = Vector3.zero;
		secondNearestListenerPosition = Vector3.zero;
		thirdNearestListenerPosition = Vector3.zero;
		fourthNearestListenerPosition = Vector3.zero;

		nearestListenerForward = Vector3.forward;
		secondNearestListenerForward = Vector3.forward;
		thirdNearestListenerForward = Vector3.forward;
		fourthNearestListenerForward = Vector3.forward;

		int firstFinded = -1;
		int secondFinded = -1;
		int thirdFinded = -1;

		int maxIndex = MultiPoolAudioSystem.audioManager.listenersForwards.Count;

		for (int i = 0; i < maxIndex; i++) {

			tempTransform = MultiPoolAudioSystem.audioManager.listenersForwards [i];
			tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions [i];
            

			Vector3 directionToTarget = new Vector3 (tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
			float dSqrToTarget = directionToTarget.sqrMagnitude;

			if (dSqrToTarget < closestDistanceSqr) {

				firstFinded = i;

				closestTargetForward = tempTransform;

				closestPosition = tempPosition;

				nearestListenerNULL = false;

				nearestListener = MultiPoolAudioSystem.audioManager.listenersComponents [i];

				nearestListenerPosition = closestPosition;

				nearestListenerForward = closestTargetForward;

				closestDistanceSqr = dSqrToTarget;


			}

		}
			
			if (!nearestListenerNULL) {

                nearestListenerDistance = new Vector3(nearestListenerPosition.x - thisPosition.x, nearestListenerPosition.y - thisPosition.y, nearestListenerPosition.z - thisPosition.z).magnitude;

                nearestListenerBlend = Mathf.Clamp01 (1 - (nearestListenerDistance - minDistance) / (maxDistance - minDistance));

                if (spatialMode2D)
					nearestListenerBlend = 1;

				noListener = false;
			} else {

				noListener = true;

			}

		bool notCulled = !nearestListenerNULL && nearestListenerBlend>0;

		if (notCulled)
		{

			closestDistanceSqr = Mathf.Infinity;

			for (int i = 0; i < maxIndex; i++) {

				if (i != firstFinded) {

					tempTransform = MultiPoolAudioSystem.audioManager.listenersForwards [i];
					tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions [i];

					Vector3 directionToTarget = new Vector3 (tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
					float dSqrToTarget = directionToTarget.sqrMagnitude;

					if (dSqrToTarget < closestDistanceSqr) {

						secondFinded = i;

						closestTargetForward = tempTransform;

						closestPosition = tempPosition;

						secondNearestListenerNULL = false;

						secondNearestListenerPosition = closestPosition;

						secondNearestListenerForward = closestTargetForward;

						closestDistanceSqr = dSqrToTarget;

					}
				}

			}

			closestDistanceSqr = Mathf.Infinity;

			for (int i = 0; i < maxIndex; i++) {

				if (i != firstFinded && i != secondFinded) {

					tempTransform = MultiPoolAudioSystem.audioManager.listenersForwards [i];
					tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions [i];

					Vector3 directionToTarget = new Vector3 (tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
					float dSqrToTarget = directionToTarget.sqrMagnitude;

					if (dSqrToTarget < closestDistanceSqr) {

						thirdFinded = i;

						closestTargetForward = tempTransform;

						closestPosition = tempPosition;

						thirdNearestListenerNULL = false;

						thirdNearestListenerPosition = closestPosition;

						thirdNearestListenerForward = closestTargetForward;

						closestDistanceSqr = dSqrToTarget;

					}
				}

			}

			closestDistanceSqr = Mathf.Infinity;

			for (int i = 0; i < maxIndex; i++) {

				if (i != firstFinded && i != secondFinded && i != thirdFinded) {

					tempTransform = MultiPoolAudioSystem.audioManager.listenersForwards [i];
					tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions [i];

					Vector3 directionToTarget = new Vector3 (tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
					float dSqrToTarget = directionToTarget.sqrMagnitude;

					if (dSqrToTarget < closestDistanceSqr) {

						closestTargetForward = tempTransform;
						closestPosition = tempPosition;

						fourthNearestListenerNULL = false;

						fourthNearestListenerPosition = closestPosition;

						fourthNearestListenerForward = closestTargetForward;

						closestDistanceSqr = dSqrToTarget;

					}
				}

			}


			if (!thirdNearestListenerNULL && fourthNearestListenerNULL) {

				Vector3 directionToBetweenNearestAndSecondListeners = new Vector3 (nearestListenerPosition.x - secondNearestListenerPosition.x, nearestListenerPosition.y - secondNearestListenerPosition.y, nearestListenerPosition.z - secondNearestListenerPosition.z);
				float distanceBetweenNearestAndSecondListeners = directionToBetweenNearestAndSecondListeners.sqrMagnitude;

				Vector3 directionToBetweenSecondAndThirdListeners = new Vector3 (secondNearestListenerPosition.x - thirdNearestListenerPosition.x, secondNearestListenerPosition.y - thirdNearestListenerPosition.y, secondNearestListenerPosition.z - thirdNearestListenerPosition.z);
				float distanceBetweenSecondAndThirdListeners = directionToBetweenSecondAndThirdListeners.sqrMagnitude;

				Vector3 directionToBetweenNearestAndThirdListeners = new Vector3 (nearestListenerPosition.x - thirdNearestListenerPosition.x, nearestListenerPosition.y - thirdNearestListenerPosition.y, nearestListenerPosition.z - thirdNearestListenerPosition.z);
				float distanceBetweeNearestAndThirdListeners = directionToBetweenNearestAndThirdListeners.sqrMagnitude;

				distanceBetweenListeners = Mathf.Sqrt ((distanceBetweenNearestAndSecondListeners + distanceBetweenSecondAndThirdListeners + distanceBetweeNearestAndThirdListeners) / 3);


			} else if (!fourthNearestListenerNULL) {

				Vector3 directionToBetweenNearestAndSecondListeners = new Vector3 (nearestListenerPosition.x - secondNearestListenerPosition.x, nearestListenerPosition.y - secondNearestListenerPosition.y, nearestListenerPosition.z - secondNearestListenerPosition.z);
				float distanceBetweenNearestAndSecondListeners = directionToBetweenNearestAndSecondListeners.sqrMagnitude;

				Vector3 directionToBetweenNearestAndThirdListeners = new Vector3 (nearestListenerPosition.x - thirdNearestListenerPosition.x, nearestListenerPosition.y - thirdNearestListenerPosition.y, nearestListenerPosition.z - thirdNearestListenerPosition.z);
				float distanceBetweenNearestAndThirdListeners = directionToBetweenNearestAndThirdListeners.sqrMagnitude;

				Vector3 directionToBetweenNearestAndFourthListeners = new Vector3 (nearestListenerPosition.x - fourthNearestListenerPosition.x, nearestListenerPosition.y - fourthNearestListenerPosition.y, nearestListenerPosition.z - fourthNearestListenerPosition.z);
				float distanceBetweenNearestAndFourthListeners = directionToBetweenNearestAndFourthListeners.sqrMagnitude;

				Vector3 directionToBetweenSecondAndThirdListeners = new Vector3 (secondNearestListenerPosition.x - thirdNearestListenerPosition.x, secondNearestListenerPosition.y - thirdNearestListenerPosition.y, secondNearestListenerPosition.z - thirdNearestListenerPosition.z);
				float distanceBetweenSecondAndThirdListeners = directionToBetweenSecondAndThirdListeners.sqrMagnitude;

				Vector3 directionToBetweenSecondAndFourthListeners = new Vector3 (secondNearestListenerPosition.x - fourthNearestListenerPosition.x, secondNearestListenerPosition.y - fourthNearestListenerPosition.y, secondNearestListenerPosition.z - fourthNearestListenerPosition.z);
				float distanceBetweenSecondAndFourthListeners = directionToBetweenSecondAndFourthListeners.sqrMagnitude;

				Vector3 directionToBetweenThirdAndFourthListeners = new Vector3 (thirdNearestListenerPosition.x - fourthNearestListenerPosition.x, thirdNearestListenerPosition.y - fourthNearestListenerPosition.y, thirdNearestListenerPosition.z - fourthNearestListenerPosition.z);
				float distanceBetweenThirdAndFourthListeners = directionToBetweenThirdAndFourthListeners.sqrMagnitude;

				distanceBetweenListeners = Mathf.Sqrt ((distanceBetweenNearestAndSecondListeners + distanceBetweenNearestAndThirdListeners + distanceBetweenNearestAndFourthListeners +
					distanceBetweenSecondAndThirdListeners + distanceBetweenSecondAndFourthListeners + distanceBetweenThirdAndFourthListeners) / 6);

			} else if (thirdNearestListenerNULL && fourthNearestListenerNULL && !secondNearestListenerNULL) {

				Vector3 directionToBetweenListeners = new Vector3 (nearestListenerPosition.x - secondNearestListenerPosition.x, nearestListenerPosition.y - secondNearestListenerPosition.y, nearestListenerPosition.z - secondNearestListenerPosition.z);
				distanceBetweenListeners = directionToBetweenListeners.magnitude;

			}

			if (!nearestListenerNULL) {
				Vector3 directionToNearestListener = new Vector3 (nearestListenerPosition.x - thisPosition.x, nearestListenerPosition.y - thisPosition.y, nearestListenerPosition.z - thisPosition.z);
				float distance = directionToNearestListener.magnitude;

				nearestListenerDistance = distance;
				nearestListenerBlendDistance = Mathf.Clamp01 (Mathf.Abs(1 - ((distance) / distanceBetweenListeners)));
				nearestListenerBlend =  Mathf.Clamp01 (1 - (nearestListenerDistance - minDistance) / (maxDistance - minDistance));

				if (spatialMode2D) {
					nearestListenerBlend = 1;
					nearestListenerBlendDistance = 1;
				}
			}

			if (!secondNearestListenerNULL) {
				Vector3 directionToSecondNearestListener = new Vector3 (secondNearestListenerPosition.x - thisPosition.x, secondNearestListenerPosition.y - thisPosition.y, secondNearestListenerPosition.z - thisPosition.z);
				float distanceSecond = directionToSecondNearestListener.magnitude;

				secondNearestListenerDistance = distanceSecond;
				secondNearestListenerBlendDistance = Mathf.Clamp01 (Mathf.Abs(1 - ((distanceSecond) / distanceBetweenListeners)));
				secondNearestListenerBlend = Mathf.Clamp01 (1 - ((secondNearestListenerDistance) - minDistance) / (maxDistance - minDistance));

				if (spatialMode2D) {
					secondNearestListenerBlend = 0;
					secondNearestListenerBlendDistance = 0;
				}
			}

			if (!thirdNearestListenerNULL) {
				Vector3 directionToThirdNearestListener = new Vector3 (thirdNearestListenerPosition.x - thisPosition.x, thirdNearestListenerPosition.y - thisPosition.y, thirdNearestListenerPosition.z - thisPosition.z);
				float distanceThird = directionToThirdNearestListener.magnitude;

				thirdNearestListenerDistance = distanceThird;
				thirdNearestListenerBlendDistance = Mathf.Clamp01 (1 - ((distanceThird) / distanceBetweenListeners));
				thirdNearestListenerBlend = Mathf.Clamp01 (1 - (thirdNearestListenerDistance - minDistance) / (maxDistance - minDistance));

				if (spatialMode2D) {
					thirdNearestListenerBlend = 0;
					thirdNearestListenerBlendDistance = 0;
				}
			}

			if (!fourthNearestListenerNULL) {
				Vector3 directionToFourthNearestListener = new Vector3 (fourthNearestListenerPosition.x - thisPosition.x, fourthNearestListenerPosition.y - thisPosition.y, fourthNearestListenerPosition.z - thisPosition.z);
				float distanceFourth = directionToFourthNearestListener.magnitude;

				fourthNearestListenerDistance = distanceFourth;
				fourthNearestListenerBlendDistance = Mathf.Clamp01 (Mathf.Abs(1 - ((distanceFourth) / distanceBetweenListeners)));
				fourthNearestListenerBlend = Mathf.Clamp01 (1 - (fourthNearestListenerDistance - minDistance) / (maxDistance - minDistance));

				if (spatialMode2D) {
					fourthNearestListenerBlend = 0;
					fourthNearestListenerBlendDistance = 0;
				}
			}

		}

	}


	void OnDrawGizmosSelected()
	{

		Gizmos.DrawIcon (transform.position, "AlmenaraGames/MLPAS/AudioSourceIco");

	}

	void OnApplicationFocus(bool hasFocus)
	{
		onFocus = hasFocus;
	}

		public void DrawGizmos()
		{

            #if UNITY_EDITOR

            DrawAudioObjectName((AudioObject!=null?AudioObject.name:"NONE") + (clamped?"\n(CLAMPED)":(Mute?"\n(MUTED)":(LocalPause ? "\n(PAUSED)":(smoothOcclude>0.01f && OccludeSound?"\n(OCCLUDED)":"")))),(Application.isPlaying? emitterTransform.position:transform.position)-Vector3.up*0.35f,new Color(0.7f,0.7f,0.7f,1f));

            if (Application.isPlaying && usingShape)
            {
                Gizmos.DrawIcon(emitterTransform.position, "AlmenaraGames/MLPAS/AudioSourceIco");

                Gizmos.color = new Color(0.5f, 0f, 1f, 1f);

                Gizmos.DrawLine(emitterTransform.position, transform.position);
            }

            #endif 

            if (Application.isPlaying) {

				if (!nearestListenerNULL) {
					Gizmos.color=Color.Lerp(new Color (1, 0, 0f, 0f),new Color (smoothOcclude>0.01f && OccludeSound?1:0, 1, 0f,1f),Mathf.Clamp01(nearestListenerBlend));
					if (spatialMode2D) {
						Gizmos.color = new Color (0, 0.25f, 1f, 1f);
					}
					if (LocalPause) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (0.9f, 0.35f, 0f, 0f), new Color (0.9f, 0.35f, 0, 1), Mathf.Clamp01 (nearestListenerBlend));
						} else {
							Gizmos.color = new Color (0.9f, 0.35f, 0, 1);
						}
					}
					if (Mute || clamped) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (1, 0, 0f, 0f), new Color (1, 0, 0, 1), Mathf.Clamp01 (nearestListenerBlend));
						} else {
							Gizmos.color = new Color (1, 0, 0, 1);
						}
					}
					Gizmos.DrawLine (emitterTransform.position, nearestListenerPosition);
				}
				if (!secondNearestListenerNULL) {
					Gizmos.color=Color.Lerp(new Color (1, 0, 0f, 0f),new Color (smoothOcclude>0.01f && OccludeSound?1:0, 1, 0f,1f),Mathf.Clamp01(secondNearestListenerBlend));
					if (spatialMode2D) {
						Gizmos.color = new Color (0, 0.25f, 1f, 1f);
					}
					if (LocalPause) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (0.9f, 0.35f, 0f, 0f), new Color (0.9f, 0.35f, 0, 1), Mathf.Clamp01 (secondNearestListenerBlend));
						} else {
							Gizmos.color = new Color (0.9f, 0.35f, 0, 1);
						}
					}
					if (Mute || clamped) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (1, 0, 0f, 0f), new Color (1, 0, 0, 1), Mathf.Clamp01 (secondNearestListenerBlend));
						} else {
							Gizmos.color = new Color (1, 0, 0, 1);
						}
					}
					Gizmos.DrawLine (emitterTransform.position, secondNearestListenerPosition);
				}
				if (!thirdNearestListenerNULL) {
					Gizmos.color=Color.Lerp(new Color (1, 0, 0f, 0f),new Color (smoothOcclude>0.01f && OccludeSound?1:0, 1, 0f,1f),Mathf.Clamp01(thirdNearestListenerBlend));
					if (spatialMode2D) {
						Gizmos.color = new Color (0, 0.25f, 1f, 1f);
					}
					if (LocalPause) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (0.9f, 0.35f, 0f, 0f), new Color (0.9f, 0.35f, 0, 1), Mathf.Clamp01 (thirdNearestListenerBlend));
						} else {
							Gizmos.color = new Color (0.9f, 0.35f, 0, 1);
						}
					}
					if (Mute || clamped) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (1, 0, 0f, 0f), new Color (1, 0, 0, 1), Mathf.Clamp01 (thirdNearestListenerBlend));
						} else {
							Gizmos.color = new Color (1, 0, 0, 1);
						}
					}
					Gizmos.DrawLine (emitterTransform.position, thirdNearestListenerPosition);
				}
				if (!fourthNearestListenerNULL) {
					Gizmos.color=Color.Lerp(new Color (1, 0, 0f, 0f),new Color (smoothOcclude>0.01f && OccludeSound?1:0, 1, 0f,1f),Mathf.Clamp01(fourthNearestListenerBlend));
					if (spatialMode2D) {
						Gizmos.color = new Color (0, 0.25f, 1f, 1f);
					}
					if (LocalPause) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (0.9f, 0.35f, 0f, 0f), new Color (0.9f, 0.35f, 0, 1), Mathf.Clamp01 (fourthNearestListenerBlend));
						} else {
							Gizmos.color = new Color (0.9f, 0.35f, 0, 1);
						}
					}
					if (Mute || clamped) {
						if (!spatialMode2D) {
							Gizmos.color = Color.Lerp (new Color (1, 0, 0f, 0f), new Color (1, 0, 0, 1), Mathf.Clamp01 (fourthNearestListenerBlend));
						} else {
							Gizmos.color = new Color (1, 0, 0, 1);
						}
					}
					Gizmos.DrawLine (emitterTransform.position, fourthNearestListenerPosition);
				}
			}

		}

	void OnDrawGizmos()
	{

			DrawGizmos ();

	}

	#if UNITY_EDITOR

		static void DrawAudioObjectName(string text, Vector3 worldPos, Color? colour = null) {
			if (SceneView.lastActiveSceneView!=null && SceneView.lastActiveSceneView.camera!=null && Vector3.Distance(worldPos,SceneView.lastActiveSceneView.camera.transform.position)<19f && UnityEditor.SceneView.currentDrawingSceneView!=null && UnityEditor.SceneView.currentDrawingSceneView.camera!=null)
			{
				UnityEditor.Handles.BeginGUI ();

				var restoreColor = GUI.color;
	


				if (colour.HasValue) {
					if (Vector3.Distance (worldPos, SceneView.lastActiveSceneView.camera.transform.position) > 3f) {
						GUI.color = Color.Lerp (colour.Value, new Color (colour.Value.r, colour.Value.g, colour.Value.b, 0f), Vector3.Distance (worldPos, SceneView.lastActiveSceneView.camera.transform.position) / 18f);
					} else {
						GUI.color = colour.Value;
					}
				}
				var view = UnityEditor.SceneView.currentDrawingSceneView;
				Vector3 screenPos = view.camera.WorldToScreenPoint (worldPos);

				if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
					GUI.color = restoreColor;
					UnityEditor.Handles.EndGUI ();
					return;
				}

				Vector2 size = GUI.skin.label.CalcSize (new GUIContent (text));
				GUIStyle newStyle = new GUIStyle (EditorStyles.whiteLabel);
				newStyle.alignment = TextAnchor.MiddleCenter;
				//newStyle.font = Font.

					GUI.Label (new Rect (screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 2, size.x, size.y), text,newStyle);

				GUI.color = restoreColor;

				UnityEditor.Handles.EndGUI ();
			}
		}

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MultiAudioSource))]
	public class MultiAudioSourceEditor: Editor 
	{

		SerializedObject audioSourceObj;
		SerializedProperty audioObjectProp;
        SerializedProperty defaultChannelProp;
        SerializedProperty playUpdateModeProp;
		SerializedProperty overrideValuesProp;
		SerializedProperty targetToFollowProp;
		SerializedProperty muteProp;
		SerializedProperty overrideAudioClipProp;
		SerializedProperty audioClipOverrideProp;
		SerializedProperty overrideRandomVolumeMultiplierProp;
		SerializedProperty randomVolumeMultiplierProp;
		SerializedProperty overrideVolumeProp;
		SerializedProperty volumeOverrideProp;
		SerializedProperty overrideRandomPitchMultiplierProp;
		SerializedProperty randomPitchMultiplierProp;
		SerializedProperty overridePitchProp;
		SerializedProperty pitchOverrideProp;
		SerializedProperty overrideSpatialModeProp;
		SerializedProperty spatialMode2DOverrideProp;
		SerializedProperty overrideStereoPanProp;
		SerializedProperty stereoPanOverrideProp;
		SerializedProperty overrideSpreadProp;
		SerializedProperty spreadOverrideProp;
		SerializedProperty overrideDopplerLevelProp;
		SerializedProperty dopplerLevelOverrideProp;
		SerializedProperty overrideReverbZoneProp;
		SerializedProperty reverbZoneMixOverrideProp;
		SerializedProperty overrideDistanceProp;
		SerializedProperty minDistanceOverrideProp;
		SerializedProperty maxDistanceOverrideProp;
		SerializedProperty overrideMixerGroupProp;
		SerializedProperty mixerGroupOverrideProp;
		SerializedProperty occludeSoundProp;
		SerializedProperty delayProp;
		SerializedProperty delayUpdateModeProp;
		SerializedProperty overrideVolumeRolloffProp;
		SerializedProperty volumeRolloffOverrideProp;
		SerializedProperty volumeRolloffCurveOverrideProp;
		SerializedProperty overrideSpatializeProp;
		SerializedProperty spatializeOverrideProp;
		SerializedProperty playOnStartProp;
            SerializedProperty fadeInOnStartProp;
            SerializedProperty fadeInOnStartTimeProp;
        SerializedProperty ignoreListenerPauseProp;
		SerializedProperty localPauseProp;
            SerializedProperty startDistanceProp;
           // SerializedProperty shapeProp;
			SerializedProperty masterVolumeProp;
            SerializedProperty callbacks;
            SerializedProperty OnPlay;
            SerializedProperty OnStop;
            SerializedProperty OnLoop;
            SerializedProperty OnRange;
            SerializedProperty OnOutOfRange;
            MultiAudioManager.UpdateModes playUpdate;
			MultiAudioManager.UpdateModes delayUpdate;



        void OnEnable()
		{

			audioSourceObj = new SerializedObject (targets);
			audioObjectProp = audioSourceObj.FindProperty ("audioObject");
            defaultChannelProp = audioSourceObj.FindProperty ("defaultChannel");
            playUpdateModeProp = audioSourceObj.FindProperty ("playUpdateMode");
			overrideValuesProp = audioSourceObj.FindProperty ("OverrideValues");
			targetToFollowProp = audioSourceObj.FindProperty ("TargetToFollow");
			muteProp = audioSourceObj.FindProperty ("Mute");
			overrideAudioClipProp = audioSourceObj.FindProperty ("overrideAudioClip");
			audioClipOverrideProp = audioSourceObj.FindProperty ("audioClipOverride");
                startDistanceProp = audioSourceObj.FindProperty("StartDistance");
            overrideRandomVolumeMultiplierProp = audioSourceObj.FindProperty ("overrideRandomVolumeMultiplier");
			randomVolumeMultiplierProp = audioSourceObj.FindProperty ("randomVolumeMultiplier");
			overrideVolumeProp = audioSourceObj.FindProperty ("overrideVolume");
			volumeOverrideProp = audioSourceObj.FindProperty ("volumeOverride");
			overrideRandomPitchMultiplierProp = audioSourceObj.FindProperty ("overrideRandomPitchMultiplier");
			randomPitchMultiplierProp = audioSourceObj.FindProperty ("randomPitchMultiplier");
			overridePitchProp = audioSourceObj.FindProperty ("overridePitch");
			pitchOverrideProp = audioSourceObj.FindProperty ("pitchOverride");
			overrideSpatialModeProp = audioSourceObj.FindProperty ("overrideSpatialMode");
			spatialMode2DOverrideProp = audioSourceObj.FindProperty ("spatialMode2DOverride");
			overrideStereoPanProp = audioSourceObj.FindProperty ("overrideStereoPan");
			stereoPanOverrideProp = audioSourceObj.FindProperty ("stereoPanOverride");
			overrideSpreadProp = audioSourceObj.FindProperty ("overrideSpread");
			spreadOverrideProp = audioSourceObj.FindProperty ("spreadOverride");
			overrideDopplerLevelProp = audioSourceObj.FindProperty ("overrideDopplerLevel");
			dopplerLevelOverrideProp = audioSourceObj.FindProperty ("dopplerLevelOverride");
			overrideReverbZoneProp = audioSourceObj.FindProperty ("overrideReverbZone");
			reverbZoneMixOverrideProp = audioSourceObj.FindProperty ("reverbZoneMixOverride");
                callbacks = audioSourceObj.FindProperty("callbacksFold");

                overrideMixerGroupProp = audioSourceObj.FindProperty ("overrideMixerGroup");
			mixerGroupOverrideProp = audioSourceObj.FindProperty ("mixerGroupOverride");
			occludeSoundProp = audioSourceObj.FindProperty ("occludeSound");
			delayProp = audioSourceObj.FindProperty ("delay");
			delayUpdateModeProp = audioSourceObj.FindProperty ("delayUpdateMode");
			overrideVolumeRolloffProp = audioSourceObj.FindProperty ("overrideVolumeRolloff");
			volumeRolloffOverrideProp = audioSourceObj.FindProperty ("volumeRolloffOverride");
			volumeRolloffCurveOverrideProp = audioSourceObj.FindProperty ("volumeRolloffCurveOverride");
			overrideSpatializeProp = audioSourceObj.FindProperty ("overrideSpatialize");
			spatializeOverrideProp = audioSourceObj.FindProperty ("spatializeOverride");
			playOnStartProp = audioSourceObj.FindProperty ("playOnStart");
                fadeInOnStartProp = audioSourceObj.FindProperty("fadeInOnStart");
                fadeInOnStartTimeProp = audioSourceObj.FindProperty("fadeInOnStartTime");
                ignoreListenerPauseProp = audioSourceObj.FindProperty ("ignoreListenerPause");

			overrideDistanceProp = audioSourceObj.FindProperty ("overrideDistance");
			minDistanceOverrideProp = audioSourceObj.FindProperty ("minDistanceOverride");
			maxDistanceOverrideProp = audioSourceObj.FindProperty ("maxDistanceOverride");

			localPauseProp = audioSourceObj.FindProperty ("paused");
				masterVolumeProp = audioSourceObj.FindProperty ("masterVolume");

               // shapeProp = audioSourceObj.FindProperty("shape");

                OnPlay = audioSourceObj.FindProperty("OnPlay");
                OnStop = audioSourceObj.FindProperty("OnStop");
                OnLoop = audioSourceObj.FindProperty("OnLoop");
                OnRange = audioSourceObj.FindProperty("OnRange");
                OnOutOfRange = audioSourceObj.FindProperty("OnOutOfRange");

            }




			bool GetSingleBoolValue(SerializedProperty _property)
			{
				foreach(var targetObject in audioSourceObj.targetObjects)
				{
					SerializedObject iteratedObject = new SerializedObject(targetObject);
					SerializedProperty iteratedProperty = iteratedObject.FindProperty(_property.propertyPath);
					if (iteratedProperty.boolValue) {
						return true;
					}
				}

				return false;
			}

			bool GetSingleObjectValue(SerializedProperty _property)
			{
				foreach(var targetObject in audioSourceObj.targetObjects)
				{
					SerializedObject iteratedObject = new SerializedObject(targetObject);
					SerializedProperty iteratedProperty = iteratedObject.FindProperty(_property.propertyPath);
					if (iteratedProperty.objectReferenceValue!=null) {
						return true;
					}
				}

				return false;
			}

			float GetMinDistanceValue(SerializedProperty _property)
			{

				float minDistance = 0;

				foreach(var targetObject in audioSourceObj.targetObjects)
				{
					SerializedObject iteratedObject = new SerializedObject(targetObject);
					SerializedProperty iteratedProperty = iteratedObject.FindProperty(_property.propertyPath);
					if (iteratedProperty.floatValue > minDistance) {
						minDistance = iteratedProperty.floatValue;
					}
				}

				return minDistance;
			}
				

		public override void OnInspectorGUI()
		{
			audioSourceObj.Update();
			
	
				Tools.MLPASExtensionMethods.ObjectField (audioObjectProp,typeof(AudioObject),new GUIContent("Audio Object"));


                if (!audioSourceObj.isEditingMultipleObjects && audioObjectProp.objectReferenceValue != null || audioSourceObj.isEditingMultipleObjects && GetSingleObjectValue(audioObjectProp))
                {


                    Tools.MLPASExtensionMethods.BeginToggleGroup(overrideValuesProp, new GUIContent("Override Values"));


                    if (!audioSourceObj.isEditingMultipleObjects && overrideValuesProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideValuesProp))
                    {

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideAudioClipProp, new GUIContent("Override Audio Clip"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideAudioClipProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideAudioClipProp))
                        {

                            Tools.MLPASExtensionMethods.ObjectField(audioClipOverrideProp, typeof(AudioClip));

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideRandomVolumeMultiplierProp, new GUIContent("Override Random Start Volume Multiplier"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideRandomVolumeMultiplierProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideRandomVolumeMultiplierProp))
                        {

                            Tools.MLPASExtensionMethods.Toggle(randomVolumeMultiplierProp, new GUIContent("Enable Random Start Volume"));

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideVolumeProp, new GUIContent("Override Volume"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideVolumeProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideVolumeProp))
                        {

                            EditorGUILayout.Slider(volumeOverrideProp, 0, 1);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideRandomPitchMultiplierProp, new GUIContent("Override Random Start Pitch Multiplier"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideRandomPitchMultiplierProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideRandomPitchMultiplierProp))
                        {

                            Tools.MLPASExtensionMethods.Toggle(randomPitchMultiplierProp, new GUIContent("Enable Random Start Pitch"));

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overridePitchProp, new GUIContent("Override Pitch"));

                        if (!audioSourceObj.isEditingMultipleObjects && overridePitchProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overridePitchProp))
                        {

                            EditorGUILayout.Slider(pitchOverrideProp, -3, 3);
                            EditorGUILayout.LabelField("A negative pitch value will going to make the Audio Object plays backwards", EditorStyles.miniLabel);

                        }


                        Tools.MLPASExtensionMethods.ToggleLeft(overrideSpreadProp, new GUIContent("Override Spread"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideSpreadProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideSpreadProp))
                        {

                            EditorGUILayout.Slider(spreadOverrideProp, 0, 360);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideSpatialModeProp, new GUIContent("Override Spatial Mode"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideSpatialModeProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideSpatialModeProp))
                        {

                            Tools.MLPASExtensionMethods.Toggle(spatialMode2DOverrideProp, new GUIContent("2D Spatial Mode"));

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideStereoPanProp, new GUIContent("Override 2D Stereo Pan"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideStereoPanProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideStereoPanProp))
                        {

                            EditorGUILayout.Slider(stereoPanOverrideProp, -1, 1);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideDistanceProp, new GUIContent("Override Distance"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideDistanceProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideDistanceProp))
                        {

                            Tools.MLPASExtensionMethods.FloatField(minDistanceOverrideProp, new GUIContent("Min Distance"), 0, Mathf.Infinity);
                            Tools.MLPASExtensionMethods.FloatField(maxDistanceOverrideProp, new GUIContent("Max Distance"), !audioSourceObj.isEditingMultipleObjects ? minDistanceOverrideProp.floatValue : GetMinDistanceValue(minDistanceOverrideProp), float.MaxValue);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideReverbZoneProp, new GUIContent("Override Reverb Zone Mix"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideReverbZoneProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideReverbZoneProp))
                        {

                            EditorGUILayout.Slider(reverbZoneMixOverrideProp, 0, 1.1f);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideDopplerLevelProp, new GUIContent("Override Doppler Level"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideDopplerLevelProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideDopplerLevelProp))
                        {

                            EditorGUILayout.Slider(dopplerLevelOverrideProp, 0, 5f);

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideVolumeRolloffProp, new GUIContent("Override Volume Rolloff"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideVolumeRolloffProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideVolumeRolloffProp))
                        {
                            Tools.MLPASExtensionMethods.Toggle(volumeRolloffOverrideProp, new GUIContent("Use Volume Rolloff"));

                            if (!audioSourceObj.isEditingMultipleObjects && volumeRolloffOverrideProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(volumeRolloffOverrideProp))
                            {

                                Tools.MLPASExtensionMethods.CurveField(volumeRolloffCurveOverrideProp, new GUIContent("Rolloff Curve"));

                                if (GUILayout.Button("Use Logarithmic Rolloff Curve" + (!audioSourceObj.isEditingMultipleObjects ? "" : " (Affects all selected objects)"), EditorStyles.miniButton))
                                {

                                    volumeRolloffCurveOverrideProp.animationCurveValue = new AnimationCurve(new Keyframe[] {
                                        new Keyframe (0, 0, 0, 0),
                                        new Keyframe (0.2f, 0.015f, 0.09f, 0.09f),
                                        new Keyframe (0.6f, 0.1f, 0.3916f, 0.3916f),
                                        new Keyframe (0.8f, 0.25f, 1.33f, 1.33f),
                                        new Keyframe (0.9f, 0.5f, 5f, 5f),
                                        new Keyframe (0.95f, 1f, 14.26f, 14.26f)
                                    });

                                }
                            }

                        }


                        Tools.MLPASExtensionMethods.ToggleLeft(overrideMixerGroupProp, new GUIContent("Override Mixer Group"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideMixerGroupProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideMixerGroupProp))
                        {

                            Tools.MLPASExtensionMethods.ObjectField(mixerGroupOverrideProp, typeof(AudioMixerGroup));

                        }

                        Tools.MLPASExtensionMethods.ToggleLeft(overrideSpatializeProp, new GUIContent("Override Spatialize"));

                        if (!audioSourceObj.isEditingMultipleObjects && overrideSpatializeProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(overrideSpatializeProp))
                        {

                            Tools.MLPASExtensionMethods.Toggle(spatializeOverrideProp, new GUIContent("Spatialize"));

                        }

                        EditorGUILayout.EndVertical();

                    }

                    EditorGUILayout.EndToggleGroup();

                }

			EditorGUILayout.Space ();
			EditorGUILayout.Slider (masterVolumeProp, 0, 1f);

            EditorGUILayout.Space();
            Tools.MLPASExtensionMethods.IntField(defaultChannelProp, new GUIContent("Channel", "To play the Audio Object in a certain channel, use a channel greater than -1 (A Non-Pooled Multi Audio Source can't play overlapping sounds, use a Pooled Multi Audio Source in order to do that)."), -1, int.MaxValue);
            EditorGUILayout.HelpBox("A Non-Pooled Multi Audio Source can't play overlapping sounds.",MessageType.Info);
			EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.UpdateModeEnum (playUpdateModeProp,new GUIContent("Update Mode"),(audioSourceObj.targetObject as MultiAudioSource).PlayUpdateMode);
			
			EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.Toggle (playOnStartProp,new GUIContent("Play On Start"));

                if (!audioSourceObj.isEditingMultipleObjects && playOnStartProp.boolValue || audioSourceObj.isEditingMultipleObjects && GetSingleBoolValue(playOnStartProp))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    Tools.MLPASExtensionMethods.FloatField(delayProp, new GUIContent("Delay Before Start"), 0, Mathf.Infinity);

                    Tools.MLPASExtensionMethods.UpdateModeEnum(delayUpdateModeProp, new GUIContent("Delay Update Mode"), (audioSourceObj.targetObject as MultiAudioSource).DelayUpdateMode);

                    EditorGUILayout.Space();
                    Tools.MLPASExtensionMethods.Toggle(fadeInOnStartProp, new GUIContent("Fade In"));
                    Tools.MLPASExtensionMethods.FloatField(fadeInOnStartTimeProp, new GUIContent("Fade In Time"), 0, Mathf.Infinity);

                    EditorGUILayout.EndVertical();
                }

            EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.Toggle (ignoreListenerPauseProp,new GUIContent("Ignore Listeners Pause"));
			
			EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.Toggle (occludeSoundProp,new GUIContent("Occlude Sound","Occludes the sound if there is an Collider between the source and the listener with one of the MLPASConfig.occludeCheck layers"));
			
			EditorGUILayout.Space ();
	
			Tools.MLPASExtensionMethods.ObjectField (targetToFollowProp, typeof(Transform),new GUIContent("Target to Follow"),true);

            EditorGUILayout.Space ();

                EditorGUILayout.PropertyField(startDistanceProp, new GUIContent("Start Distance", "Distance required from the nearest listener to start listening the source. | Do not confuse with MinDistance"));

           /* EditorGUILayout.PropertyField(shapeProp, new GUIContent("Volumetric Shape"));

                if (!audioSourceObj.isEditingMultipleObjects)
                {
                    if ((audioSourceObj.targetObject as MultiAudioSource).shape!=null && (audioSourceObj.targetObject as MultiAudioSource).shape as MeshCollider!=null)
                    {
                        if (!((audioSourceObj.targetObject as MultiAudioSource).shape as MeshCollider).convex)
                        {
                            EditorGUILayout.HelpBox("Concave Mesh Colliders are not supported",MessageType.Warning);
                        }
                    }

                }*/

            EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.Toggle (localPauseProp,new GUIContent("Pause"));
			
			EditorGUILayout.Space ();
			Tools.MLPASExtensionMethods.Toggle (muteProp,new GUIContent("Mute"));

                EditorGUILayout.Space();

                callbacks.boolValue = EditorGUILayout.Foldout(callbacks.boolValue, new GUIContent("Callbacks"));
              

                if (callbacks.boolValue)
                {

                    EditorGUILayout.PropertyField(OnPlay);
                    EditorGUILayout.PropertyField(OnStop);
                    EditorGUILayout.PropertyField(OnLoop);
                    EditorGUILayout.PropertyField(OnRange);
                    EditorGUILayout.PropertyField(OnOutOfRange,new GUIContent("On OutOfRange"));

                }


                if (!audioSourceObj.isEditingMultipleObjects) {
					EditorGUILayout.Space ();

					if ((target as MultiAudioSource).outOfRange) {
						EditorGUILayout.LabelField ("OUT OF RANGE", EditorStyles.boldLabel);
					}

				}
			

			audioSourceObj.ApplyModifiedProperties();
		}



		private void OnSceneGUI()
			{

				audioSourceObj.Update ();

		#if UNITY_5_6_OR_NEWER
				Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
		#endif

				bool is2D = (audioObjectProp.objectReferenceValue as AudioObject)!=null && (audioObjectProp.objectReferenceValue as AudioObject).spatialMode2D || overrideValuesProp.boolValue && overrideSpatialModeProp.boolValue && spatialMode2DOverrideProp.boolValue;

                Vector3 selfPos = EditorApplication.isPlaying ? (audioSourceObj.targetObject as MultiAudioSource).emitterTransform.position : (audioSourceObj.targetObject as MultiAudioSource).transform.position;

                if (overrideDistanceProp.boolValue) {


					Handles.color=new Color(0.69f,0.89f,1f,1f);

					if (is2D) {
						Handles.color = new Color (0.8f, 0.8f, 0.8f, 0.5f);
					}

                   
                    EditorGUI.BeginChangeCheck ();
					float maxDistance = Handles.RadiusHandle (Quaternion.identity, selfPos, maxDistanceOverrideProp.floatValue);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RecordObject (target, "Changed Max Audible Distance");
						maxDistanceOverrideProp.floatValue = maxDistance;
					}


					EditorGUI.BeginChangeCheck ();
					float minDistance = Handles.RadiusHandle (Quaternion.identity, selfPos, minDistanceOverrideProp.floatValue);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RecordObject (target, "Changed Min Audible Distance");
						minDistanceOverrideProp.floatValue = minDistance;
					}
				}

				if (!overrideDistanceProp.boolValue || overrideDistanceProp.boolValue && !overrideValuesProp.boolValue) {
				
					if (audioObjectProp.objectReferenceValue != null) {
						Handles.color = new Color (0.49f, 0.69f, 0.99f, 1f);

						if (is2D) {
							Handles.color = new Color (0.8f, 0.8f, 0.8f, 0.5f);
						}

						EditorGUI.BeginChangeCheck ();
						float maxDistance = Handles.RadiusHandle (Quaternion.identity, selfPos, (audioObjectProp.objectReferenceValue as AudioObject).maxDistance);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RecordObject (target, "Changed Max Audible Distance");
							maxDistanceOverrideProp.floatValue = maxDistance;
							overrideDistanceProp.boolValue = true;
							overrideValuesProp.boolValue = true;
						}

						EditorGUI.BeginChangeCheck ();
						float minDistance = Handles.RadiusHandle (Quaternion.identity, selfPos, (audioObjectProp.objectReferenceValue as AudioObject).minDistance);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RecordObject (target, "Changed Min Audible Distance");
							minDistanceOverrideProp.floatValue = minDistance;
							overrideDistanceProp.boolValue = true;
							overrideValuesProp.boolValue = true;
						}

					}

				}


				audioSourceObj.ApplyModifiedProperties ();

			}


	}

	#endif

}
}