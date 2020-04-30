using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AlmenaraGames;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames{
    [HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MultiAudioManager")]
    [AddComponentMenu("")]
    public class MultiAudioManager : MonoBehaviour {

        public static readonly string Version = "3.3";

        public AlmenaraGames.Tools.MLPASConfig config;
        public enum UpdateModes
        {
            ScaledTime,
            UnscaledTime
        }

        private AudioMixerGroup sfxMixerGroup;
        private AudioMixerGroup bgmMixerGroup;
        private LayerMask occludeCheck;
        private float occludeMultiplier = 0.5f;

        public AudioMixerGroup SfxMixerGroup { get { return sfxMixerGroup; } }
        public AudioMixerGroup BgmMixerGroup { get { return bgmMixerGroup; } }
        public LayerMask OccludeCheck { get { return occludeCheck; } }
        public float OccludeMultiplier { get { return occludeMultiplier; } }

        public string RuntimeIdentifierPrefix = "[rt]";

        private bool prevPauseListener;

        private bool paused;

        /// <summary>
        /// The paused state of the audio system.
        ///If set to true, all MultiAudioSources playing will be paused.This works in the same way as pausing the game in the editor. 
        ///While the pause-state is true, the AudioSettings.dspTime will be frozen and further MultiAudioSource play requests will start off paused.
        ///If you want certain sounds to still play during the pause, you need to set the IgnoreListenerPause property on the MultiAudioSource to true for these.
        ///This is typically menu item sounds or background music for the menu.
        /// </summary>
        public static bool Paused { get { return MultiAudioManager.Instance.paused; } set { MultiAudioManager.Instance.paused = value; } }

        private bool ignore;

        private static MultiAudioManager instance;

        private Vector3 audioListenerPosition;
        public Vector3 AudioListenerPosition { get { return audioListenerPosition; } }

        [Space(10f)]
        public List<MultiAudioListener> listenersComponents = new List<MultiAudioListener>();

        [HideInInspector]
        public List<Vector3> listenersForwards = new List<Vector3>();
        [HideInInspector]
        public List<MultiAudioListener> oldListeners = new List<MultiAudioListener>();
        [HideInInspector]
        public List<Vector3> listenersPositions = new List<Vector3>();
        [HideInInspector]
        public List<MultiReverbZone> reverbZones = new List<MultiReverbZone>();
        [HideInInspector] public int reverbZonesCount = 0;


        [HideInInspector] private List<MultiAudioSource> audioSources = new List<MultiAudioSource>();
        public List<MultiAudioSource> AudioSources { get { return audioSources; } set { audioSources = value; } }

        private List<AudioObject> globalAudioObjects = new List<AudioObject>();

        private static string runtimeIdentifierPrefix = "[rt]";

        private static Dictionary<string, AudioObject> runtimeIdentifiers = new Dictionary<string, AudioObject>();

        public Dictionary<string, AudioObject> RuntimeIdentifiers { get { return MultiAudioManager.runtimeIdentifiers; } }

        private static int runtimeIdentifiersCount;

        public static int RuntimeIdentifiersCount { get { return runtimeIdentifiersCount; } }

        private int maxAudioSources = 512;

        [SerializeField] static ulong sessionIndex = 0;

        /// <summary>
        /// DON'T CHANGE THE VALUE
        /// </summary>
        internal static bool noListeners = false;

        [SerializeField] static Scene currentActiveScene;

        /// <summary>
        /// DON'T CHANGE THE VALUE
        /// </summary>
        internal static Dictionary<AudioObject,uint> clampedSources = new Dictionary<AudioObject, uint>();

        /// <summary>
        /// DON'T CHANGE THE VALUE
        /// </summary>
        internal static int clampedSourcesCount;

		static bool instanceNULL = true;

      /*  public static void ResetStaticVars()
        {
            instanceNULL = true;
            sessionIndex = 0;
            noListeners = false;
            currentActiveScene = default(Scene);
            clampedSources = new Dictionary<AudioObject, uint>();
            runtimeIdentifiersCount = 0;
            runtimeIdentifiers = new Dictionary<string, AudioObject>();
            runtimeIdentifierPrefix = "[rt]";
            instance = null;

        }*/

	//Singleton check
	public static MultiAudioManager Instance 
	{
		get {

			if (applicationIsQuitting || !Application.isPlaying)
				return null;
				
			if (MultiAudioManager.instanceNULL) {
				
					GameObject _MultiAudioManager = new GameObject ("MultiAudioManager");
					_MultiAudioManager.AddComponent<MultiAudioManager> ();

					foreach (var item in GameObject.FindObjectsOfType (typeof(AudioListener))) {
						Destroy (item as AudioListener);
					}

					_MultiAudioManager.AddComponent<AudioListener> ();

			}
			return instance;
		}
	}

	// Use this for initialization
	void Awake () {

		currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		
		// if the singleton hasn't been initialized yet
		if (instance != null && instance != this) 
		{
			ignore = true;
			Destroy(this.gameObject);
			return;
		}

		MultiAudioManager.instanceNULL = false;
		instance = this;

		DontDestroyOnLoad(gameObject);



		if (!ignore) {

				sessionIndex = 0;



                if (config == null)
                {

                    config = Resources.Load("MLPASConfig/MLPASConfig", typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;

                }



                if (config == null)
                {

                    Debug.LogError("The MLPAS Config file has been removed, open the 'Almenara Games/MLPAS/Config' tab to create a new one");

                    return;

                }


               

                sfxMixerGroup = config.sfxMixerGroup;
			bgmMixerGroup= config.bgmMixerGroup;
			occludeCheck= config.occludeCheck;
				maxAudioSources=(int)config.maxAudioSources;
			occludeMultiplier = config.occludeMultiplier;
                runtimeIdentifierPrefix = config.runtimeIdentifierPrefix;

			MultiPoolAudioSystem.audioManager = Instance;

			ClearAudioListeners ();

			//Fill The Pool
			for (int i = 0; i < maxAudioSources; i++) {
				GameObject au = new GameObject ("PooledAudioSource_" + (i+1).ToString());
				au.hideFlags = HideFlags.HideInHierarchy;
				MultiAudioSource ssAus = au.AddComponent<MultiAudioSource> ();
				ssAus.InsidePool = true;
				
				MultiPoolAudioSystem.audioManager.AudioSources.Add (ssAus);
				au.transform.parent = this.gameObject.transform;
				au.SetActive (false);
				
			}

			LoadAllGlobalAudioObjects ();



		}

	}

	void OnEnable()
	{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			SceneManager.activeSceneChanged += OnActiveSceneChanged;

			noListeners = listenersForwards.Count < 1;

	}

	void OnDisable()
	{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
	}
	
	void OnSceneUnloaded(Scene scene)
	{

				foreach (var source in audioSources) {

				if (source.InsidePool && !source.PersistsBetweenScenes && source.SessionIndex <= sessionIndex) {
				//	source.Stop ();
				}

				}

			sessionIndex += 1;

	}

	void OnActiveSceneChanged(Scene prevScene,Scene scene)
	{

		currentActiveScene = scene;

	}
	
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{

			foreach (var item in GameObject.FindObjectsOfType (typeof(AudioListener))) {
				if ((item as AudioListener).gameObject!=this.gameObject)
				Destroy (item as AudioListener);
			}

	}

	void Update()
	{

		if (prevPauseListener != Paused) {
			AudioListener.pause = Paused;
			prevPauseListener = Paused;
		}

		transform.position = Vector3.zero;
		audioListenerPosition = transform.position;

	}

	public void ReloadConfig()
	{
		sfxMixerGroup= config.sfxMixerGroup;
		bgmMixerGroup= config.bgmMixerGroup;
		occludeCheck= config.occludeCheck;
			maxAudioSources=(int)config.maxAudioSources;
	}

        void LoadAllGlobalAudioObjects()
        {

            foreach (var obj in Resources.LoadAll<AudioObject>("Global Audio Objects"))
            {

                var asset = obj;

                bool checkSameIdentifier = false;
                AudioObject testObject = asset;

                foreach (var item in globalAudioObjects)
                {

                    if (!string.IsNullOrEmpty(asset.identifier) && !string.IsNullOrEmpty(item.identifier) && item.identifier == asset.identifier)
                    {
                        checkSameIdentifier = true;
                        testObject = item;
                    }

                }

                if (!string.IsNullOrEmpty(asset.identifier) && !checkSameIdentifier)
                {
                    globalAudioObjects.Add(asset);
                }
                else
                {

                    if (!string.IsNullOrEmpty(asset.identifier) && checkSameIdentifier)
                    {

                        Debug.LogError("<b>" + testObject.name + "</b> and " + "<b>" + asset.name + "</b> has the same identifier. Change or remove the " + "<b>" + asset.name + "</b> identifier to avoid conflicts", asset);

                    }

                }

            }

        }

        /// <summary>
        /// Gets the <see cref="AudioObject"/> with the specific identifier.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        /// <param name="showWarning">Shows a warning if the Audio Object can't be finded.</param>
        public static AudioObject GetAudioObjectByIdentifier(string identifier, bool showWarning=true)
	    {

			int listCount = MultiPoolAudioSystem.audioManager.globalAudioObjects.Count;

			for (int i = 0; i < listCount; i++) {

			if (MultiPoolAudioSystem.audioManager.globalAudioObjects[i].identifier == identifier) {

				return MultiPoolAudioSystem.audioManager.globalAudioObjects[i];

			}
			
	        }

            if (runtimeIdentifiersCount > 0)
            {

                if (runtimeIdentifiers.ContainsKey(identifier))
                {
                    if (showWarning)
                    {
                        if (Object.ReferenceEquals(runtimeIdentifiers[identifier], null))
                            Debug.LogWarning("Runtime Identifier: " + "<b>" + identifier + "</b>" + " doesn't have any <b>Audio Object</b> assigned");
                    }

                    return runtimeIdentifiers[identifier];
                }

            }

            if (showWarning)
            {
                if (identifier.Contains(runtimeIdentifierPrefix))
                {
                    Debug.LogWarning("Can't get an <b>Audio Object</b> with the runtime identifier: " + "<b>" + identifier + "</b>" + "\nRemember that the <b>Runtime Identifier</b> needs to be defined");
                }
                else
                {
                    Debug.LogWarning("Can't get an <b>Audio Object</b> with the identifier: " + "<b>" + identifier + "</b>" + "\nRemember that the <b>Audio Object</b> needs to be in the \"Resources\\Global Audio Objects\" folder and the identifier is case sensitive.");
                }
            }
            return null;

	}

        /// <summary>
        /// Determines if the specific Runtime Identifier is currently defined.
        /// </summary>
        /// <returns><c>true</c> if the specified Runtime Identifier is currently defined; otherwise, <c>false</c>.</returns>
        /// <param name="runtimeIdentifier">Runtime Identifier to define. (The prefix for the runtime time identifier will be added automatically to the name)</param>
        /// <param name="checkForAssignment">Returns true only if the assigned Audio Object of the Runtime Identifier is not NULL.</param>
        public static bool IsRuntimeIdentifierDefined(string runtimeIdentifier, bool checkForAssignment)
        {

            string id = runtimeIdentifierPrefix + runtimeIdentifier.Replace(runtimeIdentifierPrefix, "");

            if (runtimeIdentifiers.ContainsKey(id))
            {
                if (checkForAssignment)
                {
                    if (!Object.ReferenceEquals(runtimeIdentifiers[id], null))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Defines a new Runtime Identifier
        /// </summary>
        /// <param name="runtimeIdentifier">Runtime Identifier to define. (The prefix for the runtime time identifier will be added automatically to the name)</param>
        /// <param name="audioObject">Audio Object to assign.</param>
        public static void DefineRuntimeIdentifier(string runtimeIdentifier, AudioObject audioObject=null)
        {

            string id = runtimeIdentifierPrefix + runtimeIdentifier.Replace(runtimeIdentifierPrefix, "");

            if (runtimeIdentifiers.ContainsKey(id))
            {
                Debug.LogWarning("Runtime Identifier: " + "<i>'" + id.Replace(runtimeIdentifierPrefix, "") + "'</i>" + " is already defined, its <b>Audio Object</b> has been changed");
                AssignRuntimeIdentifierAudioObject(runtimeIdentifier, audioObject);
            }
            else
            {
                runtimeIdentifiers.Add(id, audioObject);
            }

            runtimeIdentifiersCount++;

        }

        /// <summary>
        /// Removes a specific Runtime Identifier
        /// </summary>
        /// <param name="runtimeIdentifier">Runtime Identifier to remove</param>
        public static void RemoveRuntimeIdentifier(string runtimeIdentifier)
        {

            string rtId = runtimeIdentifierPrefix + runtimeIdentifier.Replace(runtimeIdentifierPrefix, "");

            if (runtimeIdentifiers.ContainsKey(rtId))
            {
                runtimeIdentifiers.Remove(rtId);
                runtimeIdentifiersCount--;
            }
            else
            {
                Debug.LogWarning("Runtime Identifier: " + "<i>'" + rtId.Replace(runtimeIdentifierPrefix, "") + "'</i>" + " is already removed");
            }

        }

        /// <summary>
        /// Removes all Runtime Identifiers
        /// </summary>
        public static void ClearAllRuntimeIdentifiers()
        {

            runtimeIdentifiers.Clear();
            runtimeIdentifiersCount=0;
        }

        /// <summary>
        /// Assign an <see cref="AudioObject"/>  to the specific Runtime Identifier
        /// </summary>
        /// <param name="runtimeIdentifier"></param>
        /// <param name="audioObject"></param>
        public static void AssignRuntimeIdentifierAudioObject(string runtimeIdentifier, AudioObject audioObject)
        {

            string rtId = runtimeIdentifierPrefix + runtimeIdentifier.Replace(runtimeIdentifierPrefix, "");

            if (runtimeIdentifiers.ContainsKey(rtId))
                runtimeIdentifiers[rtId] =audioObject;
            else
            {
                Debug.LogError("Runtime Identifier: " + "<i>'" + rtId.Replace(runtimeIdentifierPrefix, "") + "'</i>" + " has not been defined");
            }

        }

        /// <summary>
        /// ONLY FOR INTERNAL USE
        /// </summary>
        internal void AddAvailableListener(MultiAudioListener listener)
        {

            listenersForwards.Add(listener.RealListener.right);
            listenersPositions.Add(listener.RealListener.position);
            listenersComponents.Add(listener);
            listener.Index = listenersPositions.Count - 1;

        }
		

	void ClearAudioListeners () {

		listenersComponents.Clear ();
		listenersForwards.Clear ();
		oldListeners.Clear ();
		listenersPositions.Clear ();

	}

        /// <summary>
        /// ONLY FOR INTERNAL USE
        /// </summary>
        internal void RemoveAudioListener (MultiAudioListener listener) {

		oldListeners = new List<MultiAudioListener>(listenersComponents);

		foreach (var item in oldListeners) {

			item.Index = -1;

		}

		listenersForwards.Clear ();
		listenersPositions.Clear ();
		listenersComponents.Clear ();

		foreach (var item in oldListeners) {
			if (item!=listener)
				AddAvailableListener (item);

		}


		noListeners = listenersForwards.Count < 1;

	}


        /// <summary>
        /// ONLY FOR INTERNAL USE
        /// </summary>
        internal static bool ClampedAudioCanBePlayed(AudioObject audioObject)
		{

			if (audioObject.maxSources < 1) {
				return true;
			}

			if (!clampedSources.ContainsKey (audioObject) || clampedSources.ContainsKey (audioObject) && clampedSources [audioObject] < audioObject.maxSources) {
				return true;
			}
		

				return false;

		}


        static void RealPlayAudioObject(MultiAudioSource _audioSource, AudioObject audioObject, int channel = -1, bool changePosition = false, Vector3 position = default(Vector3), Transform targetToFollow = null, AudioMixerGroup mixerGroup = null, bool occludeSound = false, float delay = 0f, float fadeInTime = 0f, bool setMixer=false)
        {


            if (changePosition)
            {
                _audioSource.transform.position = position;
            }

            _audioSource.AudioObject = audioObject;
            _audioSource.gameObject.SetActive(true);
            currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            _audioSource.scene = currentActiveScene;

            if (fadeInTime > 0)
            {
                _audioSource.PlayFadeIn(fadeInTime, channel, targetToFollow, delay);
            }
            else
            {
                _audioSource.Play(channel, targetToFollow, delay);
            }
           
            if (occludeSound)
            _audioSource.OccludeSound = occludeSound;

            if (setMixer)
            _audioSource.MixerGroupOverride = mixerGroup;


        }

        static void RealPlayAudioObjectOverride(MultiAudioSource _audioSource, AudioClip audioClipOverride, AudioObject audioObject, int channel = -1, bool changePosition = false, Vector3 position = default(Vector3), Transform targetToFollow = null, AudioMixerGroup mixerGroup = null, bool occludeSound = false, float delay = 0f, float fadeInTime = 0f, bool setMixer = false)
        {

            if (changePosition)
            {
                _audioSource.transform.position = position;
            }

            currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            _audioSource.scene = currentActiveScene;
            _audioSource.AudioObject = audioObject;
            _audioSource.gameObject.SetActive(true);

            if (fadeInTime > 0)
            {
                _audioSource.PlayFadeInOverride(fadeInTime, audioClipOverride, channel, targetToFollow, delay);
            }
            else
            {
                _audioSource.PlayOverride(audioClipOverride, channel, targetToFollow, delay);
            }

            if (occludeSound)
                _audioSource.OccludeSound = occludeSound;

            if (setMixer)
                _audioSource.MixerGroupOverride = mixerGroup;



        }

        static void RealPlayAudioObject(GameObject caller, MultiAudioSource _audioSource, AudioObject audioObject, int channel = -1, bool changePosition = false, Vector3 position = default(Vector3), Transform targetToFollow = null, AudioMixerGroup mixerGroup = null, bool occludeSound = false, float delay = 0f, float fadeInTime = 0f, bool setMixer = false)
        {

            if (changePosition)
            {
                _audioSource.transform.position = position;
            }

            _audioSource.AudioObject = audioObject;
            _audioSource.gameObject.SetActive(true);
            _audioSource.scene = caller.scene;

            if (fadeInTime > 0)
            {
                _audioSource.PlayFadeIn(fadeInTime, channel, targetToFollow, delay);
            }
            else
            {
                _audioSource.Play(channel, targetToFollow, delay);
            }

            if (occludeSound)
                _audioSource.OccludeSound = occludeSound;

            if (setMixer)
                _audioSource.MixerGroupOverride = mixerGroup;

        }

        static void RealPlayAudioObjectOverride(GameObject caller, MultiAudioSource _audioSource, AudioClip audioClipOverride, AudioObject audioObject, int channel = -1, bool changePosition = false, Vector3 position = default(Vector3), Transform targetToFollow = null, AudioMixerGroup mixerGroup = null, bool occludeSound = false, float delay = 0f, float fadeInTime = 0f, bool setMixer = false)
        {

            if (changePosition)
            {
                _audioSource.transform.position = position;
            }

            _audioSource.AudioObject = audioObject;
            _audioSource.gameObject.SetActive(true);
            _audioSource.scene = caller.scene;

            if (fadeInTime > 0)
            {
                _audioSource.PlayFadeInOverride(fadeInTime, audioClipOverride, channel, targetToFollow, delay);
            }
            else
            {
                _audioSource.PlayOverride(audioClipOverride, channel, targetToFollow, delay);
            }

            if (occludeSound)
                _audioSource.OccludeSound = occludeSound;

            if (setMixer)
                _audioSource.MixerGroupOverride = mixerGroup;


        }

	#region Play Methods

	#region PlayAudioObject Method
	// Normal with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Transform targetToFollow)
	{

            if (audioObject != null)
            {

                MultiAudioSource _audioSource = GetPooledAudio(channel);

                RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false);

                return _audioSource;

            }

            else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;
			
	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}

	/// <summary>
	/// Plays an <see cref="AudioObject"/> at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Vector3 position)
	{
		
		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
        /// <summary>
        /// Plays an <see cref="AudioObject"/> at the specified Position in the specified Channel.
        /// </summary>
        /// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
        /// <param name="audioObject"><see cref="AudioObject"/>.</param>
        /// <param name="channel">Channel.</param>
        /// <param name="position">Position.</param>
        public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Vector3 position)
	    {

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Normal with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, mixerGroup, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObject(AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //
	#endregion

	#region PlayDelayedAudioObject Method

	// Delayed with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Delayed with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	#endregion

	#region PlayAudioObjectOverride Method
	// Normal with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Normal with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //
	#endregion

	#region PlayDelayedAudioObjectOverride Method

	// Delayed with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Delayed with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false, delay,0f,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObject"><see cref="AudioObject"/>.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (audioObject != null) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	#endregion

	#region PlayAudioObjectByIdentifier Method
	// Normal with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier at the specified Position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Normal with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //
	#endregion

	#region PlayDelayedAudioObjectByIdentifier Method

	// Delayed with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Delayed with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObject (_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	#endregion

	#region PlayAudioObjectOverrideByIdentifier Method
	// Normal with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Normal with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Normal with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, 0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //
	#endregion

	#region PlayDelayedAudioObjectOverrideByIdentifier Method

	// Delayed with channel-position/transform
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //


	// Delayed with channel-position/transform-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	// Delayed with channel-position/transform-mixerGroup-occludeSound
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="targetToFollow">Target to follow.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio();

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay,0, true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	/// <summary>
	/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
	/// </summary>
	/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
	/// <param name="audioClipOverride">Audio clip override.</param>
	/// <param name="audioObjectIdentifier">Identifier.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="channel">Channel.</param>
	/// <param name="position">Position.</param>
	/// <param name="mixerGroup">Mixer group.</param>
	/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
	public static MultiAudioSource PlayDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
	{

		if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

			MultiAudioSource _audioSource = GetPooledAudio(channel);

			RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay,0,true);

			return _audioSource;

		}

		 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

	}
	// // // // //

	#endregion

		#region FadeInAudioObject Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime,true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObject(AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObject Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObject(AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectOverride Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectOverride Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverride(AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectByIdentifier(string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectByIdentifier(string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectOverrideByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectOverrideByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource FadeInDelayedAudioObjectOverrideByIdentifier(AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay, fadeInTime, true);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

	#endregion

		#region Scene Play Methods

		#region PlayAudioObject Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region PlayDelayedAudioObject Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region PlayAudioObjectOverride Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region PlayDelayedAudioObjectOverride Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region PlayAudioObjectByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region PlayDelayedAudioObjectByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject (sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region PlayAudioObjectOverrideByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region PlayDelayedAudioObjectOverrideByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with another Audio Clip using a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		public static MultiAudioSource ScenePlayDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObject Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObject(GameObject sceneCaller,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObject Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, -1, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObject(GameObject sceneCaller,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, audioObject, channel, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectOverride Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectOverride Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, -1, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObject"><see cref="AudioObject"/>.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverride(GameObject sceneCaller,AudioClip audioClipOverride,AudioObject audioObject,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (audioObject != null) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,audioObject, channel, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in at the specified Position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in and a delay specified in seconds using a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectByIdentifier(GameObject sceneCaller,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObject(sceneCaller,_audioSource, MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#region FadeInAudioObjectOverrideByIdentifier Method
		// Normal with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Normal with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Normal with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,0,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //
		#endregion

		#region FadeInDelayedAudioObjectOverrideByIdentifier Method

		// Delayed with channel-position/transform
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //


		// Delayed with channel-position/transform-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, null, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, false,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		// Delayed with channel-position/transform-mixerGroup-occludeSound
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified Channel and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds and makes that its pooled <see cref="MultiAudioSource"/> follow a target.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="targetToFollow">Target to follow.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Transform targetToFollow,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, false, Vector3.zero, targetToFollow, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio();

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), -1, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		/// <summary>
		/// Plays an Occludable <see cref="AudioObject"/> by its identifier with a fade in using another Audio Clip and a different AudioMixerGroup with a delay specified in seconds at the specified position in the specified Channel.
		/// </summary>
		/// <returns>The pooled <see cref="MultiAudioSource"/>.</returns>
		/// <param name="sceneCaller">GameObject used to link the pooled <see cref="MultiAudioSource"/> to the correct scene. (Useful to avoid audios not stopping when unloading an Additive Scene)</param>
		/// <param name="audioClipOverride">Audio clip override.</param>
		/// <param name="audioObjectIdentifier">Identifier.</param>
		/// <param name="delay">Delay.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="position">Position.</param>
		/// <param name="mixerGroup">Mixer group.</param>
		/// <param name="occludeSound">If set to <c>true</c> occlude sound.</param>
		/// <param name="fadeInTime">Fade In Time.</param>
		public static MultiAudioSource SceneFadeInDelayedAudioObjectOverrideByIdentifier(GameObject sceneCaller,AudioClip audioClipOverride,string audioObjectIdentifier,float delay,int channel,Vector3 position,AudioMixerGroup mixerGroup,bool occludeSound,float fadeInTime=1f)
		{

			if (!string.IsNullOrEmpty(audioObjectIdentifier)) {

				MultiAudioSource _audioSource = GetPooledAudio(channel);

				RealPlayAudioObjectOverride (sceneCaller,_audioSource, audioClipOverride,MultiAudioManager.GetAudioObjectByIdentifier(audioObjectIdentifier), channel, true, position, null, mixerGroup, occludeSound,delay,fadeInTime);

				return _audioSource;

			}

			 else { Debug.LogWarning("<i>Audio Object</i> to play is missing or invalid"); } return null;

		}
		// // // // //

		#endregion

		#endregion

	/// <summary>
	/// Stops the <see cref="MultiAudioSource"/> playing at the specified channel.
	/// </summary>
	/// <param name="_channel">Channel.</param>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
	public static void StopAudioSource(int _channel,bool disableObject=false)
	{

	

		for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
			if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Channel==_channel) {


					MultiPoolAudioSystem.audioManager.AudioSources [i].Stop(MultiPoolAudioSystem.audioManager.AudioSources [i].InsidePool?true:disableObject);
			}
		}

	}

	/// <summary>
	/// Stops the specified <see cref="MultiAudioSource"/>.
	/// </summary>
	/// <param name="audioSource">Multi Audio source.</param>
	/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void StopAudioSource(MultiAudioSource audioSource,bool disableObject=false)
	{


			audioSource.Stop (audioSource.InsidePool?true:disableObject);

	}

		/// <summary>
		/// Stops the specified<see cref="MultiAudioSource"/> playing at the specified channel with a delay specified in seconds.
		/// </summary>
		/// <param name="_channel">Channel.</param>
		/// <param name="_delay">Delay time for Stop.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void StopAudioSource(int _channel,float _delay,bool disableObject=false)
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Channel==_channel) {

					MultiPoolAudioSystem.audioManager.AudioSources [i].StopDelayed(_delay,MultiPoolAudioSystem.audioManager.AudioSources [i].InsidePool?true:disableObject);
				}
			}

		}

		/// <summary>
		/// Stops the specified<see cref="MultiAudioSource"/> with a delay specified in seconds.
		/// </summary>
		/// <param name="audioSource">Multi Audio source.</param>
		/// <param name="_delay">Delay time for Stop.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void StopAudioSource(MultiAudioSource audioSource,float _delay,bool disableObject=false)
		{

			audioSource.StopDelayed(_delay,audioSource.InsidePool?true:disableObject);

		}

		/// <summary>
		/// Stops the specified<see cref="MultiAudioSource"/> playing at the specified channel with a delay specified in seconds using a fade out.
		/// </summary>
		/// <param name="_channel">Channel.</param>
		/// <param name="_delay">Delay time for Stop.</param>
		/// <param name="_fadeOutTime">Fade out time.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void FadeOutAudioSource(int _channel,float _delay,float _fadeOutTime,bool disableObject=false)
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Channel==_channel) {

					MultiPoolAudioSystem.audioManager.AudioSources [i].FadeOutDelayed(_delay,_fadeOutTime,MultiPoolAudioSystem.audioManager.AudioSources [i].InsidePool?true:disableObject);
				}
			}

		}

		/// <summary>
		/// Stops the specified<see cref="MultiAudioSource"/> with a delay specified in seconds using a fade out.
		/// </summary>
		/// <param name="audioSource">Multi Audio source.</param>
		/// <param name="_delay">Delay time for Stop.</param>
		/// <param name="_fadeOutTime">Fade out time.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void FadeOutAudioSource(MultiAudioSource audioSource,float _delay,float _fadeOutTime,bool disableObject=false)
		{

			audioSource.FadeOutDelayed(_delay,_fadeOutTime,audioSource.InsidePool?true:disableObject);

		}

		/// <summary>
		/// Pauses/Unpauses the <see cref="MultiAudioSource"/> playing at the specified channel.
		/// </summary>
		/// <param name="_channel">Channel.</param>
		/// <param name="pause">If set to <c>true</c> pauses the source.</param>
		public static void PauseAudioSource(int _channel,bool pause=true)
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Channel==_channel) {
					MultiPoolAudioSystem.audioManager.AudioSources [i].LocalPause=pause;
				}
			}

		}

		/// <summary>
		/// Pauses/Unpauses the specified <see cref="MultiAudioSource"/>.
		/// </summary>
		/// <param name="audioSource">Multi Audio source.</param>
		/// <param name="pause">If set to <c>true</c> pauses the source.</param>
		public static void PauseAudioSource(MultiAudioSource audioSource,bool pause=true)
		{

			audioSource.LocalPause=pause;

		}

		/// <summary>
		/// Stops the <see cref="MultiAudioSource"/> playing at the specified channel with a fade out.
		/// </summary>
		/// <param name="_channel">Channel.</param>
		/// <param name="fadeOutTime">Fade out time.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void FadeOutAudioSource(int _channel,float fadeOutTime=1f,bool disableObject=false)
		{
			bool finded = false;
			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (!finded && MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Channel==_channel) {
					MultiPoolAudioSystem.audioManager.AudioSources [i].FadeOut(fadeOutTime,MultiPoolAudioSystem.audioManager.AudioSources [i].InsidePool?true:disableObject);
					finded=true;
				}
			}

		}

		/// <summary>
		/// Stops the specified <see cref="MultiAudioSource"/> with a fade out.
		/// </summary>
		/// <param name="audioSource">Multi Audio source.</param>
		/// <param name="fadeOutTime">Fade out time.</param>
		/// <param name="disableObject">If set to <c>true</c> disable object.</param>
		public static void FadeOutAudioSource(MultiAudioSource audioSource,float fadeOutTime=1f,bool disableObject=false)
		{

			audioSource.FadeOut(fadeOutTime,audioSource.InsidePool?true:disableObject);

		}

	/// <summary>
	/// Stops all playing Multi Audio Sources.
	/// </summary>
	public static void StopAllAudioSources()
	{

		for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
			if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled) {

				MultiPoolAudioSystem.audioManager.AudioSources [i].Stop();
			}
		}

	}

	/// <summary>
	/// Stops all Multi Audio Sources marked as BGM audios.
	/// </summary>
	public static void StopAllBGMAudios()
	{

		for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
			if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].AudioObject.isBGM) {

				MultiPoolAudioSystem.audioManager.AudioSources [i].Stop();
			}
		}

	}

	/// <summary>
	/// Pauses/Unpauses all Multi Audio Sources not marked as BGM audios.
	/// <param name="pause">If set to <c>true</c> pauses the source.</param>
	/// </summary>
		public static void PauseAllNonBGMAudios(bool pause=true)
	{

		for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
			if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && !MultiPoolAudioSystem.audioManager.AudioSources[i].AudioObject.isBGM) {
					MultiPoolAudioSystem.audioManager.AudioSources [i].LocalPause=pause;
			}
		}

	}

		/// <summary>
		/// Pauses/Unpauses all Multi Audio Sources marked as BGM audios.
		/// <param name="pause">If set to <c>true</c> pauses the source.</param>
		/// </summary>
		public static void PauseAllBGMAudios(bool pause=true)
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].AudioObject.isBGM) {
					MultiPoolAudioSystem.audioManager.AudioSources [i].LocalPause=pause;
				}
			}

		}

		/// <summary>
		/// Stops all Multi Audio Sources not marked as BGM audios.
		/// </summary>
		public static void StopAllNonBGMAudios()
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && !MultiPoolAudioSystem.audioManager.AudioSources[i].AudioObject.isBGM) {

					MultiPoolAudioSystem.audioManager.AudioSources [i].Stop();
				}
			}

		}

	/// <summary>
	/// Stops all looped Multi Audio Sources.
	/// </summary>
	public static void StopAllLoopedAudioSources()
	{

		for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
			if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].Loop) {

				MultiPoolAudioSystem.audioManager.AudioSources [i].Stop();
			}
		}

	}

		/// <summary>
		/// Stops all persistent Multi Audio Sources.
		/// </summary>
		public static void StopAllPersistentAudioSources()
		{

			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {
				if (MultiPoolAudioSystem.audioManager.AudioSources[i].isActiveAndEnabled && MultiPoolAudioSystem.audioManager.AudioSources[i].PersistsBetweenScenes) {

					MultiPoolAudioSystem.audioManager.AudioSources [i].Stop();
				}
			}

		}
			
	private static MultiAudioSource GetPooledAudio(int _channel=-1)
	{

			if (MultiAudioManager.Instance.maxAudioSources == 0)
				Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);


			bool finded = false;

			MultiAudioSource toReturn = MultiPoolAudioSystem.audioManager.AudioSources [0];
			int audioSourceListCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

			for (int i = 0; i < audioSourceListCount; i++) {

				MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];
					bool sourceActive = source.isActiveAndEnabled;
					bool sourcePool = source.InsidePool;
                bool sourcePaused = source.LocalPause;

				if (!finded && sourcePool && !sourceActive && !sourcePaused || sourcePool && sourceActive && source.Channel == _channel && _channel < -1 && !sourcePaused) {
					toReturn = source;
					finded = true;
				}
					
			}


			if (finded) {
				toReturn.SessionIndex = sessionIndex;
				return toReturn;
			} else {

				Debug.LogWarning ("Not enought pooled audio sources, returning the first pooled audio source. Consider increasing the MaxAudioSources value in the MLPASConfig.", MultiAudioManager.instance.gameObject);

				toReturn.SessionIndex = sessionIndex;
				return toReturn;
			
			}

	}

	/// <summary>
	/// Gets the <see cref="MultiAudioSource"/> played at the specified Channel (returns NULL if there is no <see cref="MultiAudioSource"/> playing at the specified Channel).
	/// </summary>
	/// <returns>The <see cref="MultiAudioSource"/> played at the specified Channel.</returns>
	/// <param name="_channel">Channel.</param>
	/// <param name="audibleOnly">Check only for audible <see cref="MultiAudioSource"/>.</param>
	public static MultiAudioSource GetAudioSourceAtChannel(int _channel, bool audibleOnly=false)
	{

			if (_channel > -1) {

				if (MultiAudioManager.Instance.maxAudioSources == 0)
					Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);

				MultiAudioSource toReturn = null;

				int sourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < sourcesCount; i++) {
					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audible = audibleOnly && !source.outOfRange || !audibleOnly;
					if (source.isActiveAndEnabled && source.Channel == _channel && source.IsPlaying && audible) {
						toReturn = source;
					}
				}

				return toReturn;

			} else {

				Debug.LogWarning ("<b>GetAudioSourceAtChannel</b><i>(int _channel)</i> has an invalid channel index, returning NULL");
				return null;

			}

	}

		/// <summary>
		/// Determines if a <see cref="MultiAudioSource"/> is playing the specified Audio Object.
		/// </summary>
		/// <returns><c>true</c> if a <see cref="MultiAudioSource"/> is playing the specified Audio Object; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">Identifier.</param>
		/// <param name="audiblesOnly">Check only for audibles <see cref="MultiAudioSource"/>.</param>
		public static bool IsAudioObjectPlaying(string identifier, bool audiblesOnly=false)
		{

			if (!string.IsNullOrEmpty(identifier)) {

				if (MultiAudioManager.Instance.maxAudioSources == 0)
					Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);

				int sourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < sourcesCount; i++) {
					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audible = audiblesOnly && !source.outOfRange || !audiblesOnly;
					if (source.isActiveAndEnabled && source.AudioObject.identifier == identifier && source.IsPlaying && audible) {
						return true;
					}
				}

			} else {

				Debug.LogWarning ("<b>IsAudioObjectPlaying</b><i>(int _channel)</i> has an invalid identifier");
				return false;

			}

			return false;

		}

        /// <summary>
        /// Gets The blend amount of the nearest listener for a specific <see cref="AudioObject"/> in a certain location.
        /// </summary>
        /// <param name="audioObject">Audio Object.</param>
        /// <param name="testPosition">Position to Test</param>
        /// <param name="minDistance">Different Min Distance (-1 default Audio Object value)</param>
        /// <param name="maxDistance">Different Max Distance (-1 default Audio Object value)</param>
        /// <returns></returns>
        public static float GetNearestListenerBlendAt(AudioObject audioObject, Vector3 testPosition, float minDistance=-1f, float maxDistance=-1f)
		{

			int maxIndex = MultiPoolAudioSystem.audioManager.listenersForwards.Count;

			float closestDistanceSqr = Mathf.Infinity;
			Vector3 thisPosition = testPosition;
			Vector3 closestPosition = thisPosition;
			bool nearestListenerNULL = true;

			for (int i = 0; i < maxIndex; i++) {

				Vector3 tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions [i];

				Vector3 directionToTarget = new Vector3 (tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
				float dSqrToTarget = directionToTarget.sqrMagnitude;

				if (dSqrToTarget < closestDistanceSqr) {

					closestPosition = tempPosition;

					nearestListenerNULL = false;

					closestDistanceSqr = dSqrToTarget;


				}

			}

			if (nearestListenerNULL) {
				return 0;
			}
			else {

				Vector3 directionToNearestListener = new Vector3 (closestPosition.x - thisPosition.x, closestPosition.y - thisPosition.y, closestPosition.z - thisPosition.z);
				float distance = directionToNearestListener.magnitude;

				float nearestListenerDistance = distance;
                float minDist = minDistance > -1f ? minDistance: audioObject.minDistance;
                float maxDist = Mathf.Clamp( maxDistance > -1f ? maxDistance: audioObject.maxDistance, minDist, float.MaxValue );
                float nearestListenerBlend = Mathf.Clamp01 (1 - (nearestListenerDistance - minDist) / (maxDist - minDist));


				if (audioObject.spatialMode2D)
					nearestListenerBlend = 1;


				return nearestListenerBlend;
			}

		}

        /// <summary>
        /// Gets The blend amount of the nearest listener for a specific <see cref="MultiAudioSource"/> in a certain location.
        /// </summary>
        /// <param name="audioSource">MultiAudioSource.</param>
        /// <param name="testPosition">Position to Test</param>
        /// <returns></returns>
		public static float GetNearestListenerBlendAt(MultiAudioSource audioSource, Vector3 testPosition)
        {

            int maxIndex = MultiPoolAudioSystem.audioManager.listenersForwards.Count;

            float closestDistanceSqr = Mathf.Infinity;
            Vector3 thisPosition = testPosition;
            Vector3 closestPosition = thisPosition;
            bool nearestListenerNULL = true;

            bool aoNull = Object.ReferenceEquals(audioSource.AudioObject, null);

            if (aoNull) {

                Debug.LogWarning("<b>GetNearestListenerBlendAt</b><i>"+ audioSource.name + "</i> doesn't have an Audio Object");

                return 0;
            }

            for (int i = 0; i < maxIndex; i++)
            {

                Vector3 tempPosition = MultiPoolAudioSystem.audioManager.listenersPositions[i];

                Vector3 directionToTarget = new Vector3(tempPosition.x - thisPosition.x, tempPosition.y - thisPosition.y, tempPosition.z - thisPosition.z);
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {

                    closestPosition = tempPosition;

                    nearestListenerNULL = false;

                    closestDistanceSqr = dSqrToTarget;


                }

            }

            if (nearestListenerNULL)
            {
                return 0;
            }
            else
            {

                Vector3 directionToNearestListener = new Vector3(closestPosition.x - thisPosition.x, closestPosition.y - thisPosition.y, closestPosition.z - thisPosition.z);
                float distance = directionToNearestListener.magnitude;

                float nearestListenerDistance = distance;
                float minDistance = audioSource.OverrideValues && audioSource.OverrideDistance ? audioSource.MinDistance : audioSource.AudioObject.minDistance;
                float maxDistance = audioSource.OverrideValues && audioSource.OverrideDistance ? audioSource.MaxDistance : audioSource.AudioObject.maxDistance;
                float nearestListenerBlend = Mathf.Clamp01(1 - (nearestListenerDistance - minDistance) / (maxDistance - minDistance));

                if (audioSource.AudioObject.spatialMode2D)
                    nearestListenerBlend = 1;


                return nearestListenerBlend;
            }

        }

        /// <summary>
        /// Determines if a <see cref="MultiAudioSource"/> is playing the specified Audio Object.
        /// </summary>
        /// <returns><c>true</c> if a <see cref="MultiAudioSource"/> is playing the specified Audio Object; otherwise, <c>false</c>.</returns>
        /// <param name="audioObject">Audio Object.</param>
        /// <param name="audiblesOnly">Check only for audibles <see cref="MultiAudioSource"/>.</param>
        public static bool IsAudioObjectPlaying(AudioObject audioObject, bool audiblesOnly=false)
		{

			if (audioObject!=null) {

				if (MultiAudioManager.Instance.maxAudioSources == 0)
					Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);


				int sourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < sourcesCount; i++) {
					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audible = audiblesOnly && !source.outOfRange || !audiblesOnly;
					if (source.isActiveAndEnabled && source.AudioObject == audioObject && source.IsPlaying && audible) {
						return true;
					}
				}

			} else {

				Debug.LogWarning ("<b>IsAudioObjectPlaying</b><i>(int _channel)</i> has an invalid Audio Object");
				return false;

			}

			return false;

		}

		/// <summary>
		/// Determines if a <see cref="MultiAudioSource"/> is playing the specified Audio Object at the specific channel.
		/// </summary>
		/// <returns><c>true</c> if a <see cref="MultiAudioSource"/> is playing the specified Audio Object at the specific channel; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">Identifier.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="audiblesOnly">Check only for audibles <see cref="MultiAudioSource"/>.</param>
		public static bool IsAudioObjectPlaying(string identifier, int channel, bool audiblesOnly=false)
		{

			if (!string.IsNullOrEmpty(identifier)) {

				if (MultiAudioManager.Instance.maxAudioSources == 0)
					Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);


				int sourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < sourcesCount; i++) {
					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audible = audiblesOnly && !source.outOfRange || !audiblesOnly;
					if (source.isActiveAndEnabled && source.AudioObject.identifier == identifier && source.IsPlaying && source.Channel==channel && audible) {
						return true;
					}
				}

			} else {

				Debug.LogWarning ("<b>IsAudioObjectPlaying</b><i>(int _channel)</i> has an invalid identifier");
				return false;

			}

			return false;

		}

		/// <summary>
		/// Determines if a <see cref="MultiAudioSource"/> is playing the specified Audio Object at the specific channel.
		/// </summary>
		/// <returns><c>true</c> if a <see cref="MultiAudioSource"/> is playing the specified Audio Object at the specific channel; otherwise, <c>false</c>.</returns>
		/// <param name="audioObject">Audio Object.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="audiblesOnly">Check only for audibles <see cref="MultiAudioSource"/>.</param>
		public static bool IsAudioObjectPlaying(AudioObject audioObject, int channel, bool audiblesOnly=false)
		{

			if (audioObject!=null) {

				if (MultiAudioManager.Instance.maxAudioSources == 0)
					Debug.LogError ("maxAudioSources can't be 0",MultiAudioManager.Instance.gameObject);


				int sourcesCount = MultiPoolAudioSystem.audioManager.AudioSources.Count;

				for (int i = 0; i < sourcesCount; i++) {
					MultiAudioSource source = MultiPoolAudioSystem.audioManager.AudioSources [i];

					bool audible = audiblesOnly && !source.outOfRange || !audiblesOnly;
					if (source.isActiveAndEnabled && source.AudioObject == audioObject && source.IsPlaying && source.Channel==channel && audible) {
						return true;
					}
				}

			} else {

				Debug.LogWarning ("<b>IsAudioObjectPlaying</b><i>(int _channel)</i> has an invalid Audio Object");
				return false;

			}

			return false;

		}

		/// <summary>
		/// Gets the pooled audio sources at the specified scene. (Only returns previously linked Multi Audio Sources)
		/// </summary>
		/// <returns>The pooled audio sources at scene.</returns>
		/// <param name="scene">Scene.</param>
		public static MultiAudioSource[] GetPooledAudioSourcesAtScene(UnityEngine.SceneManagement.Scene scene)
		{

			return MultiPoolAudioSystem.audioManager.AudioSources.FindAll (x => x.scene == scene).ToArray();

		}

		/// <summary>
		/// Gets the pooled audio sources at the same scene of the specified game object. (Only returns previously linked Multi Audio Sources)
		/// </summary>
		/// <returns>The pooled audio sources at scene.</returns>
		/// <param name="gameObject">Game object.</param>
		public static MultiAudioSource[] GetPooledAudioSourcesAtScene(GameObject go)
		{

			return MultiPoolAudioSystem.audioManager.AudioSources.FindAll (x => x.scene == go.scene).ToArray();

		}


		private static bool applicationIsQuitting = false;


		public void OnDestroy () {
			MultiPoolAudioSystem.isNULL = true;
			MultiAudioManager.instanceNULL = true;
			applicationIsQuitting = true;
		}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{

			if (Application.isPlaying && MultiAudioManager.instance.AudioSources!=null) {
			for (int i = 0; i < MultiPoolAudioSystem.audioManager.AudioSources.Count; i++) {

				if (MultiPoolAudioSystem.audioManager.AudioSources [i].InsidePool && MultiPoolAudioSystem.audioManager.AudioSources [i].isActiveAndEnabled) {
					Gizmos.color = Color.blue;
						Gizmos.DrawIcon (MultiPoolAudioSystem.audioManager.AudioSources [i].transform.position, "AlmenaraGames/MLPAS/AudioObjectIco");
						//DrawAudioObjectName(MultiPoolAudioSystem.audioManager.AudioSources [i].AudioObject.name + (MultiPoolAudioSystem.audioManager.AudioSources [i].clamped?"(CLAMPED)":(MultiPoolAudioSystem.audioManager.AudioSources [i].Mute?"(MUTED)":"")),MultiPoolAudioSystem.audioManager.AudioSources [i].transform.position-Vector3.up*0.35f);
						MultiPoolAudioSystem.audioManager.AudioSources [i].DrawGizmos ();
				}

			}
		}

	}

		static void DrawAudioObjectName(string text, Vector3 worldPos, Color? colour = null) {
			if (SceneView.lastActiveSceneView!=null && SceneView.lastActiveSceneView.camera!=null && Vector3.Distance(worldPos,SceneView.lastActiveSceneView.camera.transform.position)<15f && UnityEditor.SceneView.currentDrawingSceneView!=null && UnityEditor.SceneView.currentDrawingSceneView.camera!=null)
			{
				UnityEditor.Handles.BeginGUI ();

				var restoreColor = GUI.color;

				if (colour.HasValue)
					GUI.color = colour.Value;
				var view = UnityEditor.SceneView.currentDrawingSceneView;
				Vector3 screenPos = view.camera.WorldToScreenPoint (worldPos);

				if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
					GUI.color = restoreColor;
					UnityEditor.Handles.EndGUI ();
					return;
				}

				Vector2 size = GUI.skin.label.CalcSize (new GUIContent (text));
				GUI.Label (new Rect (screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 2, size.x, size.y), text);
				GUI.color = restoreColor;
				UnityEditor.Handles.EndGUI ();
			}
		}
		#endif

	void OnDrawGizmosSelected()
	{

		Gizmos.DrawIcon (transform.position, "AlmenaraGames/MLPAS/AudioManagerIco");

	}


	#if UNITY_EDITOR

	[CustomEditor(typeof(MultiAudioManager))]
	public class MultiAudioManagerEditor : Editor
	{

		private Texture logoTex;
		private int specifiedChannel=-1;


			[MenuItem("GameObject/Almenara Games/MLPAS/Multi Audio Source",false,+4)]
			static void CreateMultiAudioSource()
			{

				GameObject multiAudioSourceGo = new GameObject("Multi Audio Source",typeof(MultiAudioSource));
				if (SceneView.lastActiveSceneView!=null && SceneView.lastActiveSceneView.camera) {
					multiAudioSourceGo.transform.position= SceneView.lastActiveSceneView.camera.transform.position + (SceneView.lastActiveSceneView.camera.transform.forward * 10f);
				}
				if (Selection.activeTransform != null) {
					multiAudioSourceGo.transform.parent = Selection.activeTransform;
					multiAudioSourceGo.transform.localPosition = Vector3.zero;
				}
				Selection.activeGameObject = multiAudioSourceGo;

				if (multiAudioSourceGo.name == "Multi Audio Source" && GameObject.Find ("Multi Audio Source")!=null && GameObject.Find ("Multi Audio Source")!=multiAudioSourceGo) {

					for (int i = 1; i < 999; i++) {

						if (GameObject.Find ("Multi Audio Source (" + i.ToString () + ")") == null) {

							multiAudioSourceGo.name = "Multi Audio Source (" + i.ToString () + ")";
							break;

						}
						
					}

				}

			}

			[MenuItem("GameObject/Almenara Games/MLPAS/Multi Audio Listener",false,+4)]
			static void CreateMultiAudioListener()
			{

				GameObject multiAudioListenerGo = new GameObject("Multi Audio Listener",typeof(MultiAudioListener));
				if (SceneView.lastActiveSceneView!=null && SceneView.lastActiveSceneView.camera) {
					multiAudioListenerGo.transform.position= SceneView.lastActiveSceneView.camera.transform.position + (SceneView.lastActiveSceneView.camera.transform.forward * 10f);
				}
				if (Selection.activeTransform != null) {
					multiAudioListenerGo.transform.parent = Selection.activeTransform;
					multiAudioListenerGo.transform.localPosition = Vector3.zero;
				}
				Selection.activeGameObject = multiAudioListenerGo;

				if (multiAudioListenerGo.name == "Multi Audio Listener" && GameObject.Find ("Multi Audio Listener")!=null && GameObject.Find ("Multi Audio Listener")!=multiAudioListenerGo) {

					for (int i = 1; i < 999; i++) {

						if (GameObject.Find ("Multi Audio Listener (" + i.ToString () + ")") == null) {

							multiAudioListenerGo.name = "Multi Audio Listener (" + i.ToString () + ")";
							break;

						}

					}

				}

			}

			[MenuItem("GameObject/Almenara Games/MLPAS/Multi Reverb Zone",false,+4)]
			static void CreateReverbZone()
			{

				GameObject reverZoneGo = new GameObject("Multi Reverb Zone",typeof(MultiReverbZone));
				if (SceneView.lastActiveSceneView!=null && SceneView.lastActiveSceneView.camera) {
					reverZoneGo.transform.position= SceneView.lastActiveSceneView.camera.transform.position + (SceneView.lastActiveSceneView.camera.transform.forward * 10f);
				}
				if (Selection.activeTransform != null) {
					reverZoneGo.transform.parent = Selection.activeTransform;
					reverZoneGo.transform.localPosition = Vector3.zero;
				}
				Selection.activeGameObject = reverZoneGo;

				if (reverZoneGo.name == "Multi Reverb Zone" && GameObject.Find ("Multi Reverb Zone")!=null && GameObject.Find ("Multi Reverb Zone")!=reverZoneGo) {

					for (int i = 1; i < 999; i++) {

						if (GameObject.Find ("Multi Reverb Zone (" + i.ToString () + ")") == null) {

							reverZoneGo.name = "Multi Reverb Zone (" + i.ToString () + ")";
							break;

						}

					}

				}

			}

		void OnEnable()
		{

			logoTex = Resources.Load ("MLPASImages/logoSmall") as Texture;
		}


		public override void OnInspectorGUI()
		{

				if (target.name == "MultiAudioManager") {

					GUILayout.Space (10f);

					var centeredStyle = new GUIStyle (GUI.skin.GetStyle ("Label"));
					centeredStyle.alignment = TextAnchor.UpperCenter;

					GUILayout.Label (logoTex, centeredStyle);

					GUILayout.Space (10f);

						if (GUILayout.Button ("Stop All Audio Sources")) {
							MultiAudioManager.StopAllAudioSources ();
						}

						if (GUILayout.Button ("Stop All Looped Sources")) {
							MultiAudioManager.StopAllLoopedAudioSources ();
						}

						GUILayout.Space (10f);

						specifiedChannel = EditorGUILayout.IntField (specifiedChannel);

			
						if (GUILayout.Button (specifiedChannel > -1 ? "Stop Audio Source at Channel " + specifiedChannel.ToString () : "Stop Audio Sources with no Channel")) {
							MultiAudioManager.StopAudioSource (specifiedChannel);
						}

					MultiAudioManager.Paused=EditorGUILayout.Toggle ("Pause Listeners", MultiAudioManager.Paused);

					GUILayout.Space (5f);

					GUIStyle versionStyle = new GUIStyle (EditorStyles.miniLabel);
					versionStyle.alignment = TextAnchor.MiddleRight;

					EditorGUILayout.LabelField (MultiAudioManager.Version, versionStyle);

				}

				else {
					GUILayout.Space (10f);

					var centeredMiniStyle = new GUIStyle (EditorStyles.miniLabel);
					centeredMiniStyle.alignment = TextAnchor.MiddleCenter;
					EditorGUILayout.LabelField ("Don't add this component manually to a Game Object", centeredMiniStyle);
				}
			

		}

	}
	#endif

}
}