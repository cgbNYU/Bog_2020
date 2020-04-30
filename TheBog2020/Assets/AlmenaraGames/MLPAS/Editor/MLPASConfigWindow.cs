using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

namespace AlmenaraGames
{
    public class MLPASConfigWindow : EditorWindow
    {

        private Texture logoTex;
        AlmenaraGames.Tools.MLPASConfig config;

#if UNITY_EDITOR
        [MenuItem("Almenara Games/MLPAS/Rebuild Runtime Animator State SFX Values")]
        public static void RebuildRuntimeAnimatorSfx()
        {
            Tools.MLPASAnimatorSFXController.UpdateValues(true);
        }

        [MenuItem("Almenara Games/MLPAS/Config", false, +3)]
        public static void ShowWindow()
        {
            GetWindow<MLPASConfigWindow>(false, "MLPAS Config", true);
        }

        void UpdateFiles()
        {

            MonoScript ms = MonoScript.FromScriptableObject(this);
            string oldPath = AssetDatabase.GetAssetPath(ms);

            oldPath = oldPath.Replace("Editor/MLPASConfigWindow.cs", "Resources/Images");

            bool updated = false;

            if ((Resources.Load("Images/logoSmall") as Texture) != null)
            {

                FileUtil.DeleteFileOrDirectory(oldPath);
                FileUtil.DeleteFileOrDirectory(oldPath + ".meta");
                updated = true;
            }


            if ((Resources.Load("Multi Listener Pooling Audio System Config") as GameObject) != null)
            {
                AlmenaraGames.Tools.MultiAudioManagerConfig oldConfig = (Resources.Load("Multi Listener Pooling Audio System Config") as GameObject).GetComponent<AlmenaraGames.Tools.MultiAudioManagerConfig>();

                config.bgmMixerGroup = oldConfig.bgmMixerGroup;
                config.maxAudioSources = oldConfig.maxAudioSources;
                config.occludeCheck = oldConfig.occludeCheck;
                config.occludeMultiplier = oldConfig.occludeMultiplier;
                config.sfxMixerGroup = oldConfig.sfxMixerGroup;

                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(oldConfig));
                updated = true;
            }

            string oldDemoScenesPath = AssetDatabase.GetAssetPath(ms).Replace("MLPAS/Editor/MLPASConfigWindow.cs", "Demos");

            if (AssetDatabase.IsValidFolder(oldDemoScenesPath))
            {
                FileUtil.DeleteFileOrDirectory(oldDemoScenesPath);
                FileUtil.DeleteFileOrDirectory(oldDemoScenesPath + ".meta");
                updated = true;
            }

            string oldDocs = AssetDatabase.GetAssetPath(ms).Replace("Editor/MLPASConfigWindow.cs", "DOCS");

            if (AssetDatabase.IsValidFolder(oldDocs))
            {
                FileUtil.DeleteFileOrDirectory(oldDocs);
                FileUtil.DeleteFileOrDirectory(oldDocs + ".meta");
                updated = true;
            }

            //Remove old Gizmos

            string[] oldGizmosNames = new string[] { "AnimatorSFXControllerIco", "AnimatorSFXIco", "AudioListenerIco", "AudioListenerNoCenterIco", "AudioManagerIco", "AudioObjectIco", "AudioSourceIco", "ReverbZoneIco", "ReverbZoneNoCenterIco" };
            string gizmosPath = "Assets/Gizmos/";

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(gizmosPath + oldGizmosNames[2] + ".png") != null)
            {
                for (int i = 0; i < oldGizmosNames.Length; i++)
                {
                    if (AssetDatabase.LoadAssetAtPath<Texture2D>(gizmosPath + oldGizmosNames[i] + ".png") != null)
                    {
                        AssetDatabase.DeleteAsset(gizmosPath + oldGizmosNames[i] + ".png");
                        updated = true;
                    }
                }
            }

            // // // // // // //



            if (updated)
            {
                Debug.Log("Multi Listener Pooling Audio System | <b> HAS BEEN UPDATED</B>");

                config.updated = true;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                if (!config.updated)
                {

                    ForceRebuild();

                    config.updated = true;
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Revert();
                }
            }



        }

        public static void ForceRebuild()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!definesString.Contains("MLPAS"))
            {
                definesString += ";MLPAS";

                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    definesString);
            }


            UnityEditor.Animations.AnimatorController anim = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/AlmenaraGames/Demo Scenes/MLPAS/Basic Animator Sfx/Demo_mlpasAnimatorTest.controller");

            if (anim != null)
            {

                AudioObject au1 = AssetDatabase.LoadAssetAtPath<AudioObject>("Assets/AlmenaraGames/Demo Scenes/MLPAS/Basic Animator Sfx/Demo_boink_AO.asset");
                AudioObject au2 = AssetDatabase.LoadAssetAtPath<AudioObject>("Assets/AlmenaraGames/Demo Scenes/MLPAS/Basic Animator Sfx/Demo_landing_AO.asset");

                if (au1 != null && au2 != null)
                {
                    anim.GetBehaviours<MLPASAnimatorSFX>()[0].stateSfxs[0].audioObject = au1;
                    anim.GetBehaviours<MLPASAnimatorSFX>()[0].stateSfxs[1].audioObject = au2;

                    EditorUtility.SetDirty(anim);
                    EditorUtility.SetDirty(anim.GetBehaviours<MLPASAnimatorSFX>()[0]);
                    AssetDatabase.Refresh();
                }

            }


            anim = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/AlmenaraGames/Demo Scenes/MLPAS/Advanced Animator Sfx/Anims/Demo_mlpasAnimatorWalk.controller");


            if (anim != null)
            {
                AudioObject au3 = AssetDatabase.LoadAssetAtPath<AudioObject>("Assets/AlmenaraGames/Demo Scenes/MLPAS/Advanced Animator Sfx/Demo_footstep_AO.asset");

                if (au3 != null)
                {
                    anim.GetBehaviours<MLPASAnimatorSFX>()[0].stateSfxs[0].audioObject = au3;
                    anim.GetBehaviours<MLPASAnimatorSFX>()[0].stateSfxs[1].audioObject = au3;

                    EditorUtility.SetDirty(anim);
                    EditorUtility.SetDirty(anim.GetBehaviours<MLPASAnimatorSFX>()[0]);
                    AssetDatabase.Refresh();
                }
            }

        }

        public static void Revert()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
               EditorUserBuildSettings.selectedBuildTargetGroup);
            if (definesString.Contains(";MLPAS"))
            {
                definesString = definesString.Replace(";MLPAS", "");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    definesString);
            }
        }

        void OnEnable()
        {

            logoTex = Resources.Load("MLPASImages/logoSmall") as Texture;

            config = null;

            MonoScript ms = MonoScript.FromScriptableObject(this);

            string docsPath = AssetDatabase.GetAssetPath(ms).Replace("Editor/MLPASConfigWindow.cs", "Docs.asset");

            if (!AssetDatabase.LoadAssetAtPath<ScriptableObject>(docsPath))
            {
                AlmenaraGames.Tools.Documentation mlpasDocs = AlmenaraGames.Tools.Documentation.CreateInstance("Docs") as AlmenaraGames.Tools.Documentation;
                AssetDatabase.CreateAsset(mlpasDocs, docsPath);
                AssetDatabase.Refresh();
            }

            if (config == null)
            {

                config = Resources.Load("MLPASConfig/MLPASConfig", typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;

            }

#if UNITY_EDITOR
            if (config == null)
            {
                AlmenaraGames.Tools.MLPASConfig mlpasConfig = AlmenaraGames.Tools.MLPASConfig.CreateInstance("MLPASConfig") as AlmenaraGames.Tools.MLPASConfig;


                string path = AssetDatabase.GetAssetPath(ms);

                path = path.Replace("Editor/MLPASConfigWindow.cs", "Resources/MLPASConfig/");

                AssetDatabase.CreateAsset(mlpasConfig, path + "MLPASConfig.asset");


                string[] guids2 = AssetDatabase.FindAssets("t: MLPASConfig");

                foreach (string guid in guids2)
                {
                    AlmenaraGames.Tools.MLPASConfig ao = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;

                    if (ao != null)
                    {

                        config = ao;
                        Debug.LogWarning("<b>Multi Listener Pooling Audio System Config</b> is missing, a <i>New One</i> has been created", ao);
                        break;

                    }


                }

                AssetDatabase.Refresh();

            }
#endif


            UpdateFiles();

        }

        void OnGUI()
        {

            maxSize = new Vector2(390f, 315f);
            minSize = maxSize;

            GUILayout.Space(10f);

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;

            GUILayout.Label(logoTex, centeredStyle);

            GUIStyle contentStyle = new GUIStyle(EditorStyles.label);
            contentStyle.alignment = TextAnchor.MiddleCenter;

            UnityEngine.Audio.AudioMixerGroup prevSfxMixerGroup = config.sfxMixerGroup;
            UnityEngine.Audio.AudioMixerGroup prevBgmMixerGroup = config.bgmMixerGroup;
            LayerMask prevOccludeCheck = config.occludeCheck;
            float prevOccludeMultiplier = config.occludeMultiplier;
            uint prevMaxAudioSources = config.maxAudioSources;
            string prevRuntimeIdentifierPrefix = config.runtimeIdentifierPrefix;


            GUIStyle versionStyle = new GUIStyle(EditorStyles.miniLabel);
            versionStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Global Config", versionStyle);

            GUILayout.Space(5f);

            EditorGUI.BeginChangeCheck();
            UnityEngine.Audio.AudioMixerGroup sfxMixerGroup = EditorGUILayout.ObjectField(new GUIContent("SFX Mixer Group", "Default SFX Mixer Group Output"), config.sfxMixerGroup, typeof(UnityEngine.Audio.AudioMixerGroup), false) as UnityEngine.Audio.AudioMixerGroup;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed SFX Mixer Group");
                config.sfxMixerGroup = sfxMixerGroup;
            }

            EditorGUI.BeginChangeCheck();
            UnityEngine.Audio.AudioMixerGroup bgmMixerGroup = EditorGUILayout.ObjectField(new GUIContent("BGM Mixer Group", "Default BGM Mixer Group Output"), config.bgmMixerGroup, typeof(UnityEngine.Audio.AudioMixerGroup), false) as UnityEngine.Audio.AudioMixerGroup;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed BGM Mixer Group");
                config.bgmMixerGroup = bgmMixerGroup;
            }

            EditorGUI.BeginChangeCheck();
            LayerMask occludeCheck = LayerMaskField(new GUIContent("Occlude Check Layer", "Layer Mask used for check whether or not a collider occludes the sound. Tip: Use a unique layer for the occludable colliders, then you can have more control putting invisible triggers on the objects that you want to occludes sound"), config.occludeCheck);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed Occlude Check Layer");
                config.occludeCheck = occludeCheck;
            }

            EditorGUI.BeginChangeCheck();
            float occludeMultiplier = Mathf.Clamp01(EditorGUILayout.Slider(new GUIContent("Occlude Multiplier", "The higher the value, the less the audio is heard when occluded"), Mathf.Clamp01(config.occludeMultiplier), 0f, 1f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed Occlude Multiplier");
                config.occludeMultiplier = occludeMultiplier;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            string prefix = EditorGUILayout.TextField(new GUIContent("Runtime Identifier Prefix", "The prefix used to define Runtime Identifiers"), config.runtimeIdentifierPrefix);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed Runtime Identifier Prefix");
                config.runtimeIdentifierPrefix = prefix;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            uint maxAudioSources = (uint)Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Max Audio Sources", "Max pooled Multi Audio Sources"), (int)Mathf.Clamp(config.maxAudioSources, 1, 2048)), 1, 2048);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed Max Audio Sources");
                config.maxAudioSources = maxAudioSources;
            }

            EditorGUILayout.HelpBox("To get the best performance only increase this value if you are having problems when playing pooled audios, otherwise keep this value as low as possible.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            float dopplerFactor = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Doppler Factor", "Set how audible the Doppler effect is. Use 0 to disable it. Use 1 make it audible for fast moving objects."), Mathf.Clamp(config.dopplerFactor, 0, 10)), 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Changed Doppler Factor");
                config.dopplerFactor = dopplerFactor;
            }

            if (prevRuntimeIdentifierPrefix != config.runtimeIdentifierPrefix || prevSfxMixerGroup != config.sfxMixerGroup || prevBgmMixerGroup != config.bgmMixerGroup || config.occludeCheck != prevOccludeCheck || prevOccludeMultiplier != config.occludeMultiplier || prevMaxAudioSources != config.maxAudioSources)
            {
                EditorUtility.SetDirty(config);
            }

        }

        public static LayerMask LayerMaskField(GUIContent label, LayerMask layer)
        {
            LayerMask[] layers = new LayerMask[32];
            int m = 0;
            // search all leyers [32 is max layer count for Unity]
            for (int i = 0; i < 32; i++)
            {
                int layerID = i;
                string name = LayerMask.LayerToName(layerID);
                if (name != null && name.Length > 0)
                {
                    layers[m] = layerID;
                    m++;
                }
            }

            string[] names = new string[m];
            for (int i = 0; i < m; i++)
            {
                names[i] = LayerMask.LayerToName(layers[i]);
            }


            LayerMask result = EditorGUILayout.MaskField(label, layer.value, names);

            return result;
        }

#endif
    }
}