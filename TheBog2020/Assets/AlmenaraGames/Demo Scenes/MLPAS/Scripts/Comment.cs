using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames.Demo
{
    public class Comment : MonoBehaviour
    {
        [Multiline]
        public string comment;

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Comment))]
    public class CommentEditor : Editor
    {

        SerializedObject obj;
        SerializedProperty comment;
   
        private void OnEnable()
        {
            obj = new SerializedObject(target);
            comment = obj.FindProperty("comment");
        }

        public override void OnInspectorGUI()
        {
            obj.Update();

            EditorGUILayout.Space();
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(200,255,255,255);

            GUIStyle textBox = new GUIStyle(GUI.skin.textArea);

            textBox.wordWrap = true;

            EditorGUILayout.LabelField(comment.stringValue, textBox);

            GUI.backgroundColor = prevColor;

            EditorGUILayout.Space();


        }

    }
#endif
}
