using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Reflection;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames {
	[CreateAssetMenu(fileName = "New Audio Object", menuName = "Multi Listener Pooling Audio System/Audio Object")]
	[HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.AudioObject")]
public class AudioObject : ScriptableObject {

		[Tooltip("The identifier to get this Audio Object.")]
	/// <summary>
	/// The identifier for get this Audio Object.
	/// </summary>
	public string identifier=string.Empty;

	/// <summary>
	/// Gets or sets the clips.
	/// </summary>
	/// <value>The clips.</value>
	public AudioClip[] clips=new AudioClip[0];

	/// <summary>
	/// Returns a random clip.
	/// </summary>
	public AudioClip RandomClip
	{
		get
		{
			//Some other code
			return clips.Length>0?clips[Random.Range(0,clips.Length)]:null;
		}
	}

	[Space(10)]

	[Tooltip("Sets the overall volume of the sound")]
	[Range(0,1)]
	/// <summary>
	/// The overall volume of the sound.
	/// </summary>
	public float volume = 1;
	[Space(5)]

	[Tooltip("Sets the spread of a 3d sound in speaker space")]
	[Range(0,360)]
	/// <summary>
	/// The spread of a 3d sound in speaker space.
	/// </summary>
	public float spread = 0f;

	[Space(5)]

	[Tooltip("Sets the frequency of the sound. Use this to slow down or speed up the sound. A negative pitch value will going to make the sound plays backwards")]
	[Range(-3,3)]
	/// <summary>
	/// The frequency of the sound. Use this to slow down or speed up the sound. A negative pitch value will going to make the sound plays backwards.
	/// </summary>
	public float pitch = 1;

	[Space(5)]

	[Tooltip("A 2D sound ignores all types of spatial attenuation, thats include spatial position and spread")]
	/// <summary>
	/// A 2D sound ignores all types of spatial attenuation, thats include spatial position and spread.
	/// </summary>
	public bool spatialMode2D = false;

	[Space(5)]

	[Tooltip("Only for 2D sound")]
	[Range(-1,1)]
	/// <summary>
	/// The 2D Stereo pan.
	/// </summary>
	public float stereoPan = 0;

	[Space(10)]
	[Tooltip("Sets the source to loop")]
	/// <summary>
	/// Is the audio clip looping?. If you disable looping on a playing source the sound will stop after the end of the current loop.
	/// </summary>
	public bool loop = false;
	
    /// <summary>
    /// The source will going to automatically changes the current clip to the next clip on the list when this Audio Object repeats.
    /// </summary>
    [Tooltip("The source will going to automatically changes the current clip to the next clip on the list when this Audio Object repeats")]
    [UnityEngine.Serialization.FormerlySerializedAs("loopClipsSequentially")]
    public bool playClipsSequentially = false;

	[Space(10)]

	[Tooltip("Sets the priority of the sound. Note that a sound with a larger priority value will more likely be stolen by sounds with smaller priority values")]
	[Range(0,256)]
	/// <summary>
	/// The priority of the sound. Note that a sound with a larger priority value will more likely be stolen by sounds with smaller priority values.
	/// </summary>
	public int priority = 128;

		[Space(5)]

		[Tooltip("Limits how many Multi Audio Sources can be playing this Audio Object at the same time (0 = No Limits)")]
		/// <summary>
		/// Limits how many Multi Audio Sources can be playing this Audio Object at the same time (0 = No Limits).
		/// </summary>
		public uint maxSources = 0;

		[Space(10)]

	[Tooltip("Withing the Min Distance, the volume will stay at the loudest possible. Outside of this Min Distance it begins to attenuate")]
	/// <summary>
	/// Withing the Min Distance, the volume will stay at the loudest possible. Outside of this Min Distance it begins to attenuate.
	/// </summary>
	public float minDistance = 1;
	[Tooltip("Max Distance is the distance where the sound is completely inaudible")]
	/// <summary>
	/// Max Distance is the distance where the sound is completely inaudible.
	/// </summary>
	public float maxDistance = 20;
        
	[Space(10)]

	[Tooltip("Min random value for multiply the pitch of the sound")]
	[Range(0.75f,1)]
	/// <summary>
	/// Min random value for multiply the pitch of the sound.
	/// </summary>
	public float minPitchMultiplier = 0.9f;
	[Tooltip("Max random value for multiply the pitch of the sound")]
	[Range(1,1.25f)]
	/// <summary>
	/// Max random value for multiply the pitch of the sound.
	/// </summary>
	public float maxPitchMultiplier = 1.1f;

	[Space(10)]

	[Tooltip("Min random value for multiply the volume of the sound")]
	[Range(0.25f,1)]
	/// <summary>
	/// Min random value for multiply the volume of the sound.
	/// </summary>
	public float minVolumeMultiplier = 0.8f;
	[Tooltip("Max random value for multiply the volume of the sound")]
	[Range(1,1.75f)]
	/// <summary>
	/// Max random value for multiply the volume of the sound.
	/// </summary>
	public float maxVolumeMultiplier = 1.2f;

	[Space(10)]

	[Tooltip("The amount by which the signal from the sound will be mixed into the global reverb associated with the Reverb from the listeners. The range from 0 to 1 is linear (like the volume property) while the range from 1 to 1.1 is an extra boost range that allows you to boost the reverberated signal by 10 dB")]
	[Range(0,1.1f)]
	/// <summary>
	/// The amount by which the signal from the sound will be mixed into the global reverb associated with the Reverb from the listeners. The range from 0 to 1 is linear (like the volume property) while the range from 1 to 1.1 is an extra boost range that allows you to boost the reverberated signal by 10 dB.
	/// </summary>
	public float reverbZoneMix = 1;

	[Space(10)]
	[Tooltip("Specifies how much the pitch is changed based on the relative velocity between the listener and the source")]
	[Range(0,5f)]
	/// <summary>
	/// Specifies how much the pitch is changed based on the relative velocity between the listener and the source.
	/// </summary>
	public float dopplerLevel = 0.25f;

	[Space(10)]
	[Tooltip("Enables or disables sound attenuation over distance")]
	/// <summary>
	/// Enables or disables sound attenuation over distance.
	/// </summary>
	public bool volumeRolloff = true;
	[Tooltip("Sets how the sound attenuates over distance")]
	/// <summary>
	/// The volume rolloff curve. Sets how the sound attenuates over distance.
	/// </summary>
	public AnimationCurve volumeRolloffCurve=AnimationCurve.EaseInOut(0,0,1,1);

	[Space(10)]

	[Tooltip("Set whether the sound should play through an Audio Mixer first or directly to the Listener. Leave NULL to use the default SFX/BGM output specified in the <b>Multi Listener Pooling Audio System Config</b>.")]
	/// <summary>
	/// Set whether the sound should play through an Audio Mixer first or directly to the Listener.
	/// </summary>
	public AudioMixerGroup mixerGroup;

	[Space(10)]
	[Tooltip("Enables or disables custom spatialization for the source")]
	/// <summary>
	/// Enables or disables custom spatialization for the source.
	/// </summary>
	public bool spatialize;

	[Space(10)]
	[Tooltip("is the sound a BGM?")]
	/// <summary>
	/// Is the sound a BGM?.
	/// </summary>
	public bool isBGM = false;


#if UNITY_EDITOR
        [InitializeOnLoad]
        [CanEditMultipleObjects]
        [CustomEditor(typeof(AudioObject))]
        public class AudioObjectEditor : Editor
        {

            SerializedObject audioObj;
            SerializedProperty _clips;
            SerializedProperty rollOffCurve;
            SerializedProperty minDistance;
            SerializedProperty maxDistance;
            SerializedProperty minPitchMultiplier;
            SerializedProperty maxPitchMultiplier;
            SerializedProperty minVolumeMultiplier;
            SerializedProperty maxVolumeMultiplier;
            SerializedProperty identifier;
            SerializedProperty loopClipsSequentially;

            private Texture playIcon;
            private Texture stopIcon;
            private Texture addIcon;
            private Texture removeIcon;
            private Texture emptyIcon;

            private static bool unfolded = true;
            int currentPickerWindow = -1;

            private static readonly string[] _dontIncludeMe_0 = new string[] { "m_Script", "playClipsSequentially", "clips", "identifier", "maxDistance", "minDistance", "playClipsSequentially", "priority", "maxSources", "minDistance", "maxDistance", "minVolumeMultiplier", "maxVolumeMultiplier", "minPitchMultiplier", "maxPitchMultiplier", "reverbZoneMix", "dopplerLevel", "volumeRolloff", "volumeRolloffCurve", "mixerGroup", "spatialize", "isBGM" };
            private static readonly string[] _dontIncludeMe_1 = new string[] { "m_Script", "playClipsSequentially", "clips", "identifier", "maxDistance", "minDistance", "volume", "spread", "pitch", "spatialMode2D", "stereoPan", "loop", "minVolumeMultiplier", "maxVolumeMultiplier", "minPitchMultiplier", "maxPitchMultiplier", "reverbZoneMix", "dopplerLevel", "volumeRolloff", "volumeRolloffCurve", "mixerGroup", "spatialize", "isBGM" };
            private static readonly string[] _dontIncludeMe_2 = new string[] { "m_Script", "playClipsSequentially", "clips", "identifier", "maxDistance", "minDistance", "volume", "spread", "pitch", "spatialMode2D", "stereoPan", "loop", "playClipsSequentially", "priority", "maxSources", "minDistance", "maxDistance", "minVolumeMultiplier", "maxVolumeMultiplier", "minPitchMultiplier", "maxPitchMultiplier", "reverbZoneMix", "dopplerLevel", "volumeRolloff", "volumeRolloffCurve", "mixerGroup", "spatialize", "isBGM" };
            private static readonly string[] _dontIncludeMe_3 = new string[] { "m_Script", "playClipsSequentially", "clips", "identifier", "maxDistance", "minDistance", "volume", "spread", "pitch", "spatialMode2D", "stereoPan", "loop", "playClipsSequentially", "priority", "maxSources", "minDistance", "maxDistance", "minPitchMultiplier", "maxPitchMultiplier", "minVolumeMultiplier", "maxVolumeMultiplier", "mixerGroup", "spatialize", "isBGM" };
            private static readonly string[] _dontIncludeMe_4 = new string[] { "m_Script", "playClipsSequentially", "clips", "identifier", "maxDistance", "minDistance", "volume", "spread", "pitch", "spatialMode2D", "stereoPan", "loop", "playClipsSequentially", "priority", "maxSources", "minDistance", "maxDistance", "minPitchMultiplier", "maxPitchMultiplier", "minVolumeMultiplier", "maxVolumeMultiplier", "reverbZoneMix", "dopplerLevel", "volumeRolloff", "volumeRolloffCurve" };

            private object[] droppedObjects;

            bool showError = false;
            AudioObject lastSameId;

            void OnEnable()
            {
                audioObj = new SerializedObject(targets);

                _clips = audioObj.FindProperty("clips");

                rollOffCurve = audioObj.FindProperty("volumeRolloffCurve");

                minDistance = audioObj.FindProperty("minDistance");
                maxDistance = audioObj.FindProperty("maxDistance");
                minPitchMultiplier = audioObj.FindProperty("minPitchMultiplier");
                maxPitchMultiplier = audioObj.FindProperty("maxPitchMultiplier");
                minVolumeMultiplier = audioObj.FindProperty("minVolumeMultiplier");
                maxVolumeMultiplier = audioObj.FindProperty("maxVolumeMultiplier");
                identifier = audioObj.FindProperty("identifier");
                loopClipsSequentially = audioObj.FindProperty("playClipsSequentially");

                playIcon = Resources.Load("MLPASImages/playIcon") as Texture;
                stopIcon = Resources.Load("MLPASImages/pauseIcon") as Texture;
                addIcon = Resources.Load("MLPASImages/addIcon") as Texture;
                removeIcon = Resources.Load("MLPASImages/removeIcon") as Texture;
                emptyIcon = Resources.Load("MLPASImages/emptyIcon") as Texture;

                globalAudioObjects = Resources.LoadAll<AudioObject>("Global Audio Objects");



                if (!string.IsNullOrEmpty(identifier.stringValue))
                {
                    foreach (var itemTest in globalAudioObjects)
                    {
                        if (itemTest != (audioObj.targetObject as AudioObject) && itemTest.identifier == identifier.stringValue)
                        {
                            showError = true;
                        }

                    }
                }

            }

            private static bool _isRegistered = false;
            private static bool _didSelectionChange = false;

            private static Object prevSelection;

            private AudioObject[] globalAudioObjects;

            private void OnSelectionChanged()
            {
                _didSelectionChange = true;
            }

            private void OnEditorUpdate()
            {

                if (Selection.activeObject != null)
                    prevSelection = Selection.activeObject;

                if (_didSelectionChange)
                {
                    _didSelectionChange = false;

                    if (!Application.isPlaying)
                    {
                        if (prevSelection != null && prevSelection.GetType() == typeof(AudioObject))
                            StopAllClips();
                    }

                }


            }

            bool GetSingleBoolValue(SerializedProperty _property)
            {
                foreach (var targetObject in audioObj.targetObjects)
                {
                    SerializedObject iteratedObject = new SerializedObject(targetObject);
                    SerializedProperty iteratedProperty = iteratedObject.FindProperty(_property.propertyPath);
                    if (iteratedProperty.boolValue)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override void OnInspectorGUI()
            {

                audioObj.Update();

                if (!_isRegistered)
                {
                    _isRegistered = true;

                    Selection.selectionChanged += OnSelectionChanged;
                    EditorApplication.update += OnEditorUpdate;
                    Undo.undoRedoPerformed += CheckIdentifier;
#if UNITY_2017_2_OR_NEWER
                    EditorApplication.playModeStateChanged += PlayModeChange;
#else
				EditorApplication.playmodeStateChanged += PlayModeChangeOlder;
#endif
                }

                //GUILayout.Space (15f);

                var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.stretchWidth = true;

                GUIStyle playButton = new GUIStyle(EditorStyles.toolbarButton);
                playButton.stretchWidth = false;



                if (!audioObj.isEditingMultipleObjects)
                {

                    if (GUILayout.Button(audioObj.targetObject.name.ToUpper() + " Properties", EditorStyles.miniLabel))
                    {

                        EditorGUIUtility.PingObject(audioObj.targetObject);

                    }

                    GUILayout.Space(15f);

                    unfolded = EditorGUILayout.Foldout(unfolded, "Audio Clips" + " (" + _clips.arraySize.ToString() + ")");
                }
                else
                {
                    GUILayout.Space(15f);
                    EditorGUILayout.LabelField("Audio Clips", "(Not Supported while Multi-Editing)");
                }

                if (unfolded && !audioObj.isEditingMultipleObjects)
                {

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();


                    currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 10;

                    if (GUILayout.Button(addIcon, playButton))
                    {

                        _clips.InsertArrayElementAtIndex(_clips.arraySize);

                        EditorGUIUtility.ShowObjectPicker<AudioClip>(null, false, "", currentPickerWindow);

                    }

                    if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
                    {

                        currentPickerWindow = -1;

                        AudioClip newAudioClip = EditorGUIUtility.GetObjectPickerObject() as AudioClip;

                        _clips.GetArrayElementAtIndex(_clips.arraySize - 1).objectReferenceValue = newAudioClip;

                    }

                    GUILayout.Label("Add Audio Clip", EditorStyles.miniLabel);

                    GUILayout.EndHorizontal();
                    for (int i = 0; i < _clips.arraySize; i++)
                    {

                        bool hasClip = _clips.GetArrayElementAtIndex(i).objectReferenceValue != null;


                        GUILayout.BeginHorizontal();


                        if (GUILayout.Button(removeIcon, playButton))
                        {

                            StopAllClips();

                            _clips.GetArrayElementAtIndex(i).objectReferenceValue = null;
                            _clips.DeleteArrayElementAtIndex(i);

                        }

                        if (hasClip)
                        {
                            if (GUILayout.Button(hasClip ? playIcon : emptyIcon, playButton))
                            {
                                if (hasClip)
                                {
                                    StopAllClips();
                                    PlayClip(_clips.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip);
                                }
                            }
                            if (GUILayout.Button(hasClip ? stopIcon : emptyIcon, playButton))
                            {
                                if (hasClip)
                                {
                                    StopClip(_clips.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip);
                                }
                            }
                        }



                        if (_clips.arraySize > i)
                        {
                            _clips.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(_clips.GetArrayElementAtIndex(i).objectReferenceValue, typeof(AudioClip), false);
                        }


                        GUILayout.EndHorizontal();

                    }

                    droppedObjects = DropZone();

                    if (droppedObjects != null && droppedObjects.Length > 0)
                    {
                        foreach (var item in droppedObjects)
                        {

                            if (item.GetType() == typeof(AudioClip))
                            {

                                _clips.InsertArrayElementAtIndex(_clips.arraySize);
                                _clips.GetArrayElementAtIndex(_clips.arraySize - 1).objectReferenceValue = item as AudioClip;

                            }

                        }
                    }

                    GUILayout.EndVertical();

                }

                GUILayout.Space(10f);

                if (!audioObj.isEditingMultipleObjects)
                {

                    string oldIdentifier = identifier.stringValue;

                    identifier.stringValue = EditorGUILayout.TextField(new GUIContent("Identifier", "The identifier to get this Audio Object.\nRemember that the Audio Object needs to be in the \"Resources\\Global Audio Objects\" folder in order to be accessed via this identifier"), identifier.stringValue);

                    if (showError)
                    {
                        EditorGUILayout.HelpBox("Some Audio Object has the same identifier, the identifier needs to be unique.", MessageType.Error);

                        if (lastSameId != null && lastSameId.identifier != identifier.stringValue)
                            showError = false;
                    }

                    if (!string.IsNullOrEmpty(identifier.stringValue))
                    {

                        if (oldIdentifier != identifier.stringValue)
                        {

                            showError = false;

                            foreach (var itemTest in globalAudioObjects)
                            {
                                if (itemTest != (audioObj.targetObject as AudioObject) && itemTest.identifier == identifier.stringValue)
                                {
                                    showError = true;
                                    lastSameId = itemTest;
                                }

                            }

                        }

                        string path = AssetDatabase.GetAssetPath(audioObj.targetObject);

                        if (!path.Contains("Resources/Global Audio Objects"))
                        {
                            EditorGUILayout.HelpBox("The AudioObject needs to be inside the \"Resources\\Global Audio Objects\" folder in order to be accessed via this identifier.\n\nIt can be also in a subfolder if it is inside the \"Global Audio Objects\" folder", MessageType.Warning);

                            if (!path.Contains("Resources") || !path.Contains("Global Audio Objects") && path.Contains("Resources/" + Path.GetFileName(AssetDatabase.GetAssetPath(audioObj.targetObject))))
                            {
                                if (GUILayout.Button("Try to fix Asset Path", EditorStyles.miniButton))
                                {

                                    string assetName = Path.GetFileName(AssetDatabase.GetAssetPath(audioObj.targetObject));
                                    string newPath = (!path.Contains("Resources") ? "Resources/" : "") + "Global Audio Objects" + "/" + assetName;

                                    if (!path.Contains("Resources") && !path.Contains("Global Audio Objects"))
                                    {
                                        if (!AssetDatabase.IsValidFolder(path.Replace(assetName, newPath).Replace("/" + assetName, "")))
                                        {


                                            if (!AssetDatabase.IsValidFolder(path.Replace(assetName, newPath.Replace("/" + assetName, "").Replace("/Global Audio Objects", ""))))
                                            {
                                                AssetDatabase.CreateFolder(path.Replace("/" + assetName, ""), "Resources");
                                            }


                                            if (!AssetDatabase.IsValidFolder(path.Replace(assetName, newPath).Replace("/" + assetName, "")))
                                            {
                                                AssetDatabase.CreateFolder(path.Replace("/" + assetName, "") + "/Resources", "Global Audio Objects");
                                            }

                                        }
                                    }
                                    else if (!path.Contains("Global Audio Objects"))
                                    {
                                        if (!AssetDatabase.IsValidFolder(path.Replace("/" + assetName, "") + "/Global Audio Objects"))
                                        {
                                            AssetDatabase.CreateFolder(path.Replace("/" + assetName, ""), "Global Audio Objects");
                                        }
                                    }

                                    string testMove = AssetDatabase.MoveAsset(path, path.Replace(assetName, newPath));

                                    if (testMove == "")
                                    {
                                        EditorGUIUtility.PingObject(audioObj.targetObject);
                                        Selection.activeObject = audioObj.targetObject;
                                        Debug.Log(assetName + " Moved to: " + "Resources/Global Audio Objects");
                                    }
                                    else
                                    {
                                        Debug.LogError("Error while trying to fix path for: " + assetName);
                                    }

                                }
                            }

                        }

                    }
                }
                else
                {

                    EditorGUILayout.LabelField("Identifier", "(Not Supported while Multi-Editing)");

                }


                DrawPropertiesExcluding(audioObj, _dontIncludeMe_0);

                EditorGUILayout.Space();

                Tools.MLPASExtensionMethods.Toggle(loopClipsSequentially, new GUIContent("Play Clips Sequentially", "The source will going to automatically changes the current clip to the next clip on the list when this Audio Object repeats"));

                DrawPropertiesExcluding(audioObj, _dontIncludeMe_1);

                EditorGUILayout.Space();

                Tools.MLPASExtensionMethods.FloatField(minDistance, new GUIContent("Min Distance", "Withing the Min Distance, the volume will stay at the loudest possible. Outside of this Min Distance it begins to attenuate"), 0, float.MaxValue);
                Tools.MLPASExtensionMethods.FloatField(maxDistance, new GUIContent("Max Distance", "Max Distance is the distance where the sound is completely inaudible"), !audioObj.isEditingMultipleObjects ? minDistance.floatValue : GetMinDistanceValue(minDistance), float.MaxValue);

                EditorGUILayout.Space();

                EditorGUILayout.Slider(minPitchMultiplier, 0.75f, 1f, new GUIContent("Min Pitch Multiplier" + " (" + Mathf.RoundToInt(minPitchMultiplier.floatValue * 100 - 100).ToString() + "%)", "Min random value for multiply the pitch of the sound"));
                EditorGUILayout.Slider(maxPitchMultiplier, 1f, 1.25f, new GUIContent("Max Pitch Multiplier" + " (" + Mathf.RoundToInt(maxPitchMultiplier.floatValue * 100 - 100).ToString() + "%)", "Max random value for multiply the pitch of the sound"));


                if (GUILayout.Button("Disable Random Pitch Multiplier at Start", EditorStyles.miniButton))
                {

                    minPitchMultiplier.floatValue = 1;
                    maxPitchMultiplier.floatValue = 1;

                }

                DrawPropertiesExcluding(audioObj, _dontIncludeMe_2);

                EditorGUILayout.Space();

                EditorGUILayout.Slider(minVolumeMultiplier, 0.25f, 1f, new GUIContent("Min Volume Multiplier" + " (" + Mathf.RoundToInt(minVolumeMultiplier.floatValue * 100 - 100).ToString() + "%)", "Min random value for multiply the volume of the sound"));
                EditorGUILayout.Slider(maxVolumeMultiplier, 1f, 1.75f, new GUIContent("Max Volume Multiplier" + " (" + Mathf.RoundToInt(maxVolumeMultiplier.floatValue * 100 - 100).ToString() + "%)", "Max random value for multiply the volume of the sound"));

                if (GUILayout.Button("Disable Random Volume Multiplier at Start", EditorStyles.miniButton))
                {

                    minVolumeMultiplier.floatValue = 1;
                    maxVolumeMultiplier.floatValue = 1;

                }

                DrawPropertiesExcluding(audioObj, _dontIncludeMe_3);




                if (GUILayout.Button("Use Logarithmic Rolloff Curve", EditorStyles.miniButton))
                {

                    rollOffCurve.animationCurveValue = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(0.2f, 0.015f, 0.09f, 0.09f), new Keyframe(0.6f, 0.1f, 0.3916f, 0.3916f), new Keyframe(0.8f, 0.25f, 1.33f, 1.33f), new Keyframe(0.9f, 0.5f, 5f, 5f), new Keyframe(0.95f, 1f, 14.26f, 14.26f) });

                }

                DrawPropertiesExcluding(audioObj, _dontIncludeMe_4);

                audioObj.ApplyModifiedProperties();




                if (target != null)
                    target.name = Path.GetFileName(AssetDatabase.GetAssetPath(target)).Replace(".asset", "");

            }

            float GetMinDistanceValue(SerializedProperty _property)
            {

                float minDistance = 0;

                foreach (var targetObject in audioObj.targetObjects)
                {
                    SerializedObject iteratedObject = new SerializedObject(targetObject);
                    SerializedProperty iteratedProperty = iteratedObject.FindProperty(_property.propertyPath);
                    if (iteratedProperty.floatValue > minDistance)
                    {
                        minDistance = iteratedProperty.floatValue;
                    }
                }

                return minDistance;
            }

            void CheckIdentifier()
            {

                if (Selection.activeObject != audioObj.targetObject)
                    return;

                showError = false;

                if (!string.IsNullOrEmpty(identifier.stringValue))
                {
                    foreach (var itemTest in globalAudioObjects)
                    {
                        if (itemTest != (audioObj.targetObject as AudioObject) && itemTest.identifier == (audioObj.targetObject as AudioObject).identifier)
                        {
                            showError = true;
                        }

                    }
                }

            }

            public static void PlayClip(AudioClip clip)
            {

                if (Application.isPlaying)
                    return;

                Assembly assembly = typeof(AudioImporter).Assembly;
                System.Type audioUtilType = assembly.GetType("UnityEditor.AudioUtil");

                System.Type[] typeParams = { typeof(AudioClip), typeof(int), typeof(bool) };
                object[] objParams = { clip, 0, false };

                MethodInfo method = audioUtilType.GetMethod("PlayClip", typeParams);
                method.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, objParams, null);
            }

            public static void StopClip(AudioClip clip)
            {
                Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
                System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
                MethodInfo method = audioUtilClass.GetMethod(
                    "StopClip",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new System.Type[] {
                    typeof(AudioClip)
                    },
                    null
                );
                method.Invoke(
                    null,
                    new object[] {
                    clip
                    }
                );
            }

#if UNITY_2017_2_OR_NEWER
            void PlayModeChange(PlayModeStateChange state)
            {

                if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingEditMode)
                {

                    StopAllClips();

                }

            }
#endif

            void PlayModeChangeOlder()
            {

                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
                {

                    StopAllClips();

                }

            }

            public static void StopAllClips()
            {


                Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
                System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
                MethodInfo method = audioUtilClass.GetMethod(
                    "StopAllClips",
                    BindingFlags.Static | BindingFlags.Public
                );

                method.Invoke(
                    null,
                    null
                );
            }

            public static object[] DropZone()
            {

                EventType eventType = Event.current.type;
                bool isAccepted = false;

                if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        isAccepted = true;
                    }
                    Event.current.Use();
                }

                return isAccepted ? DragAndDrop.objectReferences : null;
            }

        }
#endif

    }
}
