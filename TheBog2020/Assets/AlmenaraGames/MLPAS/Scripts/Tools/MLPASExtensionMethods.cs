using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using AlmenaraGames;

namespace AlmenaraGames.Tools
{
    public static class MLPASExtensionMethods
    {

#if UNITY_EDITOR

        public static void ToggleLeft(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ToggleLeft(label, prop.boolValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;
        }

        public static bool CheckToggleLeft(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ToggleLeft(label, prop.boolValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;

            return prop.boolValue;
        }


        public static void BeginToggleGroup(SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.BeginToggleGroup(label, prop.boolValue);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;
        }


        public static void Toggle(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.Toggle(label, prop.boolValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;
        }

        public static bool CheckToggle(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.Toggle(label, prop.boolValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;

            return prop.boolValue;
        }


        public static void FloatField(SerializedProperty prop, GUIContent label, float min, float max, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = Mathf.Clamp(EditorGUILayout.FloatField(label, prop.floatValue, options), min, max);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = newValue;
        }

        public static void IntField(SerializedProperty prop, GUIContent label, int min = int.MinValue, int max = int.MaxValue, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = Mathf.Clamp(EditorGUILayout.IntField(label, prop.intValue, options), min, max);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.intValue = newValue;
        }


        public static void CurveField(SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.CurveField(label, prop.animationCurveValue, prop.hasMultipleDifferentValues ? Color.clear : Color.green, new Rect());
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.animationCurveValue = newValue;
        }

        public static void UpdateModeEnum(SerializedProperty prop, GUIContent label, System.Enum _enum, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = (int)(MultiAudioManager.UpdateModes)EditorGUILayout.EnumPopup(label, _enum, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.enumValueIndex = newValue;
        }

        public static void String(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.TextField(label, prop.stringValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.stringValue = newValue;
        }

        //5.4
        public static void ObjectField(this SerializedProperty prop, System.Type _type, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ObjectField(label, prop.objectReferenceValue, _type, false, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.objectReferenceValue = newValue;
        }

        //5.4
        public static void ObjectField(SerializedProperty prop, System.Type _type, GUIContent label, bool sceneObjects, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ObjectField(label, prop.objectReferenceValue, _type, sceneObjects, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.objectReferenceValue = newValue;
        }

        //5.4
        public static void ObjectField(SerializedProperty prop, System.Type _type, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ObjectField(prop.objectReferenceValue, _type, false, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.objectReferenceValue = newValue;
        }
#endif

    }
}

namespace AlmenaraGames
{
    public static class MLPASRunTimeExtensionMethods
    {

        public static void Play(this AudioObject ao, Vector3 position)
        {
            MultiAudioManager.PlayAudioObject(ao, position);
        }

        public static void Play(this AudioObject ao, Transform targetToFollow)
        {
            MultiAudioManager.PlayAudioObject(ao, targetToFollow);
        }

        public static void Play(this AudioObject ao, int channel, Vector3 position)
        {
            MultiAudioManager.PlayAudioObject(ao, channel, position);
        }

        public static void Play(this AudioObject ao, Vector3 position, MultiAudioManager.UpdateModes updateMode)
        {
            MultiAudioSource _source = MultiAudioManager.PlayAudioObject(ao, position);
            _source.PlayUpdateMode = updateMode;
        }

        public static void Play(this AudioObject ao, Transform targetToFollow, MultiAudioManager.UpdateModes updateMode)
        {
            MultiAudioSource _source = MultiAudioManager.PlayAudioObject(ao, targetToFollow);
            _source.PlayUpdateMode = updateMode;
        }

        public static void Play(this AudioObject ao, int channel, Vector3 position, MultiAudioManager.UpdateModes updateMode)
        {
            MultiAudioSource _source = MultiAudioManager.PlayAudioObject(ao, channel, position);
            _source.PlayUpdateMode = updateMode;
        }

        public static void PlayIgnoringPause(this AudioObject ao, int channel, Vector3 position)
        {
            MultiAudioSource source = MultiAudioManager.PlayAudioObject(ao, channel, position);
            source.IgnoreListenerPause = true;
        }

        public static void PlayIgnoringPause(this AudioObject ao, Vector3 position)
        {
            MultiAudioSource source = MultiAudioManager.PlayAudioObject(ao, position);
            source.IgnoreListenerPause = true;
        }

        public static void PlayIgnoringPause(this AudioObject ao, int channel, Transform trf)
        {
            MultiAudioSource source = MultiAudioManager.PlayAudioObject(ao, channel, trf);
            source.IgnoreListenerPause = true;
        }

        public static void PlayIgnoringPause(this AudioObject ao, Transform trf)
        {
            MultiAudioSource source = MultiAudioManager.PlayAudioObject(ao, trf);
            source.IgnoreListenerPause = true;
        }

        public static void Play(this AudioObject ao, int channel, Transform targetToFollow)
        {
            MultiAudioManager.PlayAudioObject(ao, channel, targetToFollow);
        }

        //

        public static void Play(this AudioObject ao, Vector3 position, out MultiAudioSource audioSource)
        {
            audioSource = MultiAudioManager.PlayAudioObject(ao, position);
        }

        public static void Play(this AudioObject ao, Transform targetToFollow, out MultiAudioSource audioSource)
        {
            audioSource = MultiAudioManager.PlayAudioObject(ao, targetToFollow);
        }

        public static void Play(this AudioObject ao, int channel, Vector3 position, out MultiAudioSource audioSource)
        {
            audioSource = MultiAudioManager.PlayAudioObject(ao, channel, position);
        }

        public static void Play(this AudioObject ao, int channel, Transform targetToFollow, out MultiAudioSource audioSource)
        {
            audioSource = MultiAudioManager.PlayAudioObject(ao, channel, targetToFollow);
        }

    }

}