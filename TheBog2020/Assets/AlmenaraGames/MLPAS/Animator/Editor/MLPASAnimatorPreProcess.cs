#if UNITY_2018_1_OR_NEWER
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AlmenaraGames.Tools
{
    public class MLPASAnimatorPreProcess : IPreprocessBuildWithReport
    {

        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!MLPASAnimatorSFXController.UpdateValues(true))
            {

                //Stop Build Process

            }

        }
    }
}
#else


using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AlmenaraGames.Tools
{
    public class MLPASAnimatorPreProcess : IPreprocessBuild
    {

        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (!MLPASAnimatorSFXController.UpdateValues(true))
            {

                //Stop Build Process

            }

        }
    }
}

#endif