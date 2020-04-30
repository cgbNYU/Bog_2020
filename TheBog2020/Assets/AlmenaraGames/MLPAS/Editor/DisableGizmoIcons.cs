using UnityEditor;
using System;
using System.Reflection;

namespace AlmenaraGames
{

    public class DisableGizmoIcons
    {

        public static void DisableIcons()
        {
            var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var ClassId = Annotation.GetField("classID");
            var ScriptClass = Annotation.GetField("scriptClass");
            var Flags = Annotation.GetField("flags");
            var IconEnabled = Annotation.GetField("iconEnabled");

            Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Array annotations = (Array)GetAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                int classId = (int)ClassId.GetValue(a);
                string scriptClass = (string)ScriptClass.GetValue(a);
                int flags = (int)Flags.GetValue(a);
                int iconEnabled = (int)IconEnabled.GetValue(a);

                // this is done to ignore any built in types
                if (string.IsNullOrEmpty(scriptClass))
                {
                    continue;
                }

                // load a json or text file with class names

                const int HasIcon = 1;
                bool hasIconFlag = (flags & HasIcon) == HasIcon;

                // Added for refrence
                //const int HasGizmo = 2;
                //bool hasGizmoFlag = (flags & HasGizmo) == HasGizmo;

                if (scriptClass == "MultiAudioManager" || scriptClass == "MLPASAnimatorSFXController")
                {
                    if (hasIconFlag && (iconEnabled != 0))
                    {
                        SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
                    }
                }
            }
        }

    }
     
}