using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace AlmenaraGames
{
    public class MLPASAboutWindow : EditorWindow
    {

        private Texture logoTex;

#if UNITY_EDITOR
        [MenuItem("Almenara Games/MLPAS/About", false, +4)]
        public static void ShowWindow()
        {
            GetWindow<MLPASAboutWindow>(false, "MLPAS About", true);
        }

        void OnEnable()
        {

            logoTex = Resources.Load("MLPASImages/logoSmall") as Texture;

        }

        void OnGUI()
        {


            maxSize = new Vector2(390f, 200f);
            minSize = new Vector2(390f, 200f);

            GUILayout.Space(15f);

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;

            GUILayout.Label(logoTex, centeredStyle);

            GUILayout.Space(5f);

            GUIStyle versionStyle = new GUIStyle(EditorStyles.miniLabel);
            versionStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Version: " + AlmenaraGames.MultiAudioManager.Version, versionStyle);

            GUIStyle webPageStyle = new GUIStyle(EditorStyles.label);
            webPageStyle.alignment = TextAnchor.MiddleCenter;
            webPageStyle.fontStyle = FontStyle.Bold;

            GUIStyle aboutStyle = new GUIStyle(EditorStyles.label);
            aboutStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Space(10f);

            EditorGUILayout.LabelField("An asset created by: ALMENARA GAMES", aboutStyle);

            GUILayout.Space(10f);

            Color restoreColor = GUI.contentColor;
            GUI.color = EditorGUIUtility.isProSkin? Color.cyan : Color.blue;

            if (GUILayout.Button("Documentation", webPageStyle))
            {
                Application.OpenURL("https://almenaragames.github.io");
            }

            if (GUILayout.Button("Author Webpage", webPageStyle))
            {
                Application.OpenURL("https://almenara-games.itch.io/");
            }

            GUI.contentColor = restoreColor;

        }

#endif
    }
}