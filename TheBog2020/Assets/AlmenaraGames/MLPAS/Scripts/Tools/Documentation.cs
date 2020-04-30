using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames.Tools
{

    public class Documentation : ScriptableObject
    {


#if UNITY_EDITOR

        [CustomEditor(typeof(Documentation))]
        public class DocumentationEditor : Editor
        {

            private Texture logoTex;

            void OnEnable()
            {

                logoTex = Resources.Load("MLPASImages/logoSmall") as Texture;

            }

            void OpenDocumentation()
            {

                Application.OpenURL("https://almenaragames.github.io");

            }

            public override void OnInspectorGUI()
            {

                var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                centeredStyle.alignment = TextAnchor.UpperCenter;

                GUILayout.Label(logoTex, centeredStyle);

                if (GUILayout.Button("Open Online Documentation"))
                {
                    OpenDocumentation();
                }

            }
        }
#endif

    }
}