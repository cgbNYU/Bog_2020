using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AlmenaraGames
{
    [InitializeOnLoad]
    public static class MLPASUpdater
    {

        static MLPASUpdater()
        {

            EditorApplication.update += Initialize;

        }

        static void Initialize()
        {

            //Change Icons
            if (EditorGUIUtility.isProSkin)
            {
                ChangeIcons("MLPASAnimatorSFXController", "AnimatorSFXControllerProIco");
                ChangeIcons("MLPASAnimatorSFX", "AnimatorSFXProIco");
                ChangeIcon("MultiAudioManager", "AudioManagerIcoPro");
            }
            else
            {
                ChangeIcons("MLPASAnimatorSFXController", "AnimatorSFXControllerLightIco");
                ChangeIcons("MLPASAnimatorSFX", "AnimatorSFXLightIco");
                ChangeIcon("MultiAudioManager", "AudioManagerIcoLight");
            }

            
            AlmenaraGames.Tools.MLPASConfig config = Resources.Load("MLPASConfig/MLPASConfig", typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;

            if (config == null || config!=null && !config.updated)
            {
                DisableGizmoIcons.DisableIcons();
                MLPASConfigWindow window = (MLPASConfigWindow)EditorWindow.GetWindow<MLPASConfigWindow>(false, "MLPAS Config", true);
                window.Show();
            }

            EditorApplication.update -= Initialize;

        }

        static void ChangeIcons(string componentName, string iconName)
        {
             
            var iconData = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/AlmenaraGames/MLPAS/" + iconName + ".png");
            Object obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/AlmenaraGames/MLPAS/Animator/" + componentName + ".cs");

            if (iconData != null && obj!=null)
            {
                GUIContent icon = new GUIContent(iconData);
                var egu = typeof(EditorGUIUtility);
                var flags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                var args = new object[] { obj, icon.image };
                var setIcon = egu.GetMethod("SetIconForObject", flags, null, new System.Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
                setIcon.Invoke(null, args);
            }

        }

        static void ChangeIcon(string componentName, string iconName)
        {

            var iconData = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/AlmenaraGames/MLPAS/" + iconName + ".png");
            Object obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/AlmenaraGames/MLPAS/Scripts/" + componentName + ".cs");

            if (iconData != null && obj != null)
            {
                GUIContent icon = new GUIContent(iconData);
                var egu = typeof(EditorGUIUtility);
                var flags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                var args = new object[] { obj, icon.image };
                var setIcon = egu.GetMethod("SetIconForObject", flags, null, new System.Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
                setIcon.Invoke(null, args);
            }

        }

    }
}
