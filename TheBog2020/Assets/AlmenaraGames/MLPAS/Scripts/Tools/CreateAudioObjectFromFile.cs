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
    public class CreateAudioObjectFromFile : Editor
    {

        [MenuItem("Assets/Multi Listener Pooling Audio System Tools/Create Audio Object From Selection")]
        private static void DoSomethingWithVariable()
        {

            foreach (Object o in Selection.objects)
            {
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

                    asset.clips = new AudioClip[] { AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GetAssetPath(o)) };
                    EditorUtility.SetDirty(asset);

                    Selection.activeObject = asset;
                }

            }

        }

        [MenuItem("Assets/Multi Listener Pooling Audio System Tools/Create Audio Object From Selection", true)]
        private static bool NewMenuOptionValidation()
        {
            bool testAllAudioClips = true;

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