using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

namespace AlmenaraGames.Tools
{
    [InitializeOnLoad]
    public static class MLPASAnimatorSFXController
    {

      

        static MLPASAnimatorSFXController()
        {
      
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += StateChange;
#else
            EditorApplication.playmodeStateChanged += StateChangeOlder;
#endif
        }

#if UNITY_2017_2_OR_NEWER
        public static void StateChange(PlayModeStateChange state)
        {

            if (state == PlayModeStateChange.ExitingEditMode)
            {

#if UNITY_2019_3_OR_NEWER
              //  EditorUtility.RequestScriptReload();
#endif
                UpdateValues();

            }

        }
#endif

        public static void StateChangeOlder()
        {

            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {

                UpdateValues();

            }

        }

        public static bool UpdateValues(bool showMessage = false)
        {



            string[] guids = AssetDatabase.FindAssets("t: AnimatorController");
            List<Object> revertState = new List<Object>();
            double startTime = EditorApplication.timeSinceStartup;
            int behavioursCount=0;
            int animatorsCount=0;
            bool error = false;
            float currentIndex = -1;
            int behaviourIndex = -1;
            int behaviourCurrentCount = 0;
            EditorUtility.ClearProgressBar();

            foreach (string guid in guids)
            {
                currentIndex += 1;

                if (showMessage)
                {
                    EditorUtility.DisplayProgressBar("Rebuild Runtime MLPAS Animator State SFX Values", "Rebuilding...  Animator: " + currentIndex.ToString() + "/" + guids.Length.ToString() + (behaviourCurrentCount > 0 ? " | Behaviours: " + (behaviourIndex).ToString() : ""), currentIndex / guids.Length);
                } 

                AnimatorController anim = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(AnimatorController)) as AnimatorController;
                
                if (anim != null)
                {
   
                    animatorsCount += 1;

                    MLPASAnimatorSFX[] behaviours = anim.GetBehaviours<MLPASAnimatorSFX>();
                    behaviourCurrentCount = behaviours.Length;
                    behavioursCount += behaviourCurrentCount;

                    for (int i = 0; i < behaviours.Length; i++)
                    {

                        behaviourIndex += 1;

                        if (!behaviours[i].UpdateRuntimeValues())
                        {
                            error = true;
                            break;
                        }
                        else
                        {
                            StateMachineBehaviourContext[] context = AnimatorController.FindStateMachineBehaviourContext(behaviours[i] as StateMachineBehaviour);

                            /*if (context != null && context.Length > 0)
                            {
                                AnimatorState state = (context[0].animatorObject as AnimatorState);
                                AnimatorStateMachine stateMachine = (context[0].animatorObject as AnimatorStateMachine);

                                if (state != null)
                                {
                                    foreach (var st in state.transitions)
                                    {
                                        if (st.duration == 0)
                                            st.duration = 0.0001f;

                                    }
                                }

                                if (stateMachine != null)
                                {
                                    foreach (var st in stateMachine.anyStateTransitions)
                                    {
                                        if (st.duration == 0)
                                            st.duration = 0.0001f;

                                    }
                                }

                                revertState.Add(context[0].animatorObject);

                            }*/

                        }

                    }
                }

                if (error)
                    break;

            }

            if (!error)
            {

                if (showMessage)
                {
                   
                    Debug.Log("Rebuild Runtime Animator State SFX Values for: <b>" + animatorsCount + " Animators</b> - <b>" + behavioursCount + " Behaviours</b>" + " | <b>Operation Completed!</b>");
                }

                AlmenaraGames.Tools.MLPASConfig config = Resources.Load("MLPASConfig/MLPASConfig", typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;

                if (config != null)
                {
                    config.stateClipboardBuffer = new AlmenaraGames.MLPASAnimatorSFX.StateSFX[0];
                }
            }
            else
            {

                foreach (var item in revertState)
                {
                    AnimatorState state = (item as AnimatorState);
                    AnimatorStateMachine stateMachine = (item as AnimatorStateMachine);

                    if (state != null)
                    {
                        foreach (var st in state.transitions)
                        {
                            if (st.duration == 0)
                                st.duration = 0.0001f;

                        }
                    }

                    if (stateMachine != null)
                    {
                        foreach (var st in stateMachine.anyStateTransitions)
                        {
                            if (st.duration == 0)
                                st.duration = 0.0001f;

                        }
                    }

                }

                Debug.LogError("Rebuild Runtime Animator State SFX Values | <b>Operation Failed!</b>");

            }

            if (showMessage)
            {
                AssetDatabase.SaveAssets();

                EditorUtility.ClearProgressBar();
            }

            return !error;

        }

    }
}