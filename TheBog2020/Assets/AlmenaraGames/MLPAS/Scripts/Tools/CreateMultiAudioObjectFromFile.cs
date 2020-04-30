using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using AlmenaraGames;

#if UNITY_EDITOR
namespace AlmenaraGames.Tools
{
    public class CreateMultiAudioObjectFromFile : Editor
    {

        [MenuItem("Assets/Multi Listener Pooling Audio System Tools/Create Multi Clip Audio Object From Selection")]
        private static void DoSomethingWithVariable()
        {

            List<AudioClip> clipsToAdd = new List<AudioClip>();

            foreach (Object obj in Selection.objects)
            {
                if (obj.GetType() == typeof(AudioClip))
                {

                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path == "")
                    {
                        path = "Assets/";
                    }
                    else if (Path.GetExtension(path) != "")
                    {
                        path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(obj)), "");
                    }


                    clipsToAdd.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GetAssetPath(obj)));

                }

            }

            Object o = Selection.activeObject;

            if (o.GetType() == typeof(AudioClip))
            {

                string path = AssetDatabase.GetAssetPath(o);
                if (path == "")
                {
                    path = "Assets/";
                }
                else if (Path.GetExtension(path) != "")
                {
                    path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(o)), "");
                }

                AudioObject asset = ScriptableObject.CreateInstance<AudioObject>();
                AssetDatabase.CreateAsset(asset, path + o.name + "_AO.asset");

                asset.clips = clipsToAdd.ToArray();
                EditorUtility.SetDirty(asset);

                Selection.activeObject = asset;
            }

        }

        [MenuItem("Assets/Multi Listener Pooling Audio System Tools/Create Multi Clip Audio Object From Selection", true)]
        private static bool NewMenuOptionValidation()
        {
            // This returns true when the selected object is a Variable (the menu item will be disabled otherwise).

            bool testAllAudioClips = Selection.objects.Length > 1;

            foreach (Object obj in Selection.objects)
            {
                if (obj.GetType() != typeof(AudioClip))
                {
                    testAllAudioClips = false;
                }
            }

            return testAllAudioClips;
        }

    }
}
#endif