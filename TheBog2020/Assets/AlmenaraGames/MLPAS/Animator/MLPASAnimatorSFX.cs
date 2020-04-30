using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
#endif

namespace AlmenaraGames
{
    ///<summary>
    ///The MLPAS Animator SFX is a State Machine Behaviour that allows playing Audio Objects directly from an Animator State or Animator State Machine.
    ///</summary>
    [DisallowMultipleComponent]
    [HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MLPASAnimatorSFX")]
    public class MLPASAnimatorSFX : StateMachineBehaviour
    {

        public bool first = false;

        public int selectedIndex=0;

        /// <summary>
        /// A StateSFX is like an Animation Event that is controlled directly from an AnimationState or State Machine.
        /// </summary>
        [System.Serializable]
        public class StateSFX
        {

            /// <summary>
            /// Ignores This StateSFX.
            /// </summary>
            public bool ignoreSfx = false;
            /// <summary>
            /// When using a Custom Play Method, play only the StateSFX if the receiver of the Custom Play Method is not NULL.
            /// </summary>
            public bool playOnlyWithReceiver = true;
            /// <summary>
            /// When using a Custom Play Method, shows a warning if the receiver of the Custom Play Method is NULL.
            /// </summary>
            public bool customMethodWarnings = true;

          
            public MLPASAnimatorSFXController.CustomPlayMethod customPlayMethod;

            /// <summary>
            /// Play this StateSFX using a Custom Play Method.
            /// </summary>
            public bool useCustomPlayMethod = false;
            /// <summary>
            /// Custom Play Method name.
            /// </summary>
            public string methodName;
            /// <summary>
            /// Custom Label Name (Only used for visualization)
            /// </summary>
            public string customName;

            public bool unfolded = true;

            /// <summary>
            /// Audio Object to play.
            /// </summary>
            public AudioObject audioObject;
            /// <summary>
            /// Identifier to find the Audio Object to play.
            /// </summary>
            public string audioObjectIdentifier;

            /// <summary>
            /// Use an Audio Object Identifier.
            /// </summary>
            public bool useIdentifier;

            /// <summary>
            /// Use a specific Channel for this StateSFX.
            /// </summary>
            public bool useChannel;

            /// <summary>
            /// The channel on which this StateSFX will play.
            /// </summary>
            public int channel = -1;

            /// <summary>
            /// Makes the Audio Object to follow the Animator position when played.
            /// </summary>
            public bool followAnimator = false;

            /// <summary>
            /// Play this StateSFX only once even if the Animator State Loops.
            /// </summary>
            public bool playOnlyOnce = false;

            /// <summary>
            /// Time on which this StateSFX will play.
            /// </summary>
            public float playTime;
            /// <summary>
            /// Defines if the PlayTime is specified in NormalizedTime or Frames.
            /// </summary>
            public bool playTimeNormalized = true;
            /// <summary>
            /// Frame on which this StateSFX will play.
            /// </summary>
            public int playFrame;
            /// <summary>
            /// Parameter Evaluation Value Threshold.
            /// </summary>
            public float paramThreshold;
            /// <summary>
            /// Parameter Evaluation Value.
            /// </summary
            public float paramExactValue;

            public bool conditionEvaluated = false;

            public float normalizedPlayTime = 0f;
            public bool sfxPlayed = false;
            public float paramOldValue;

            public float lastNormalizedTime = 0f;
            public float startNormalizedTime = 0f;
            public int lastPlayedLoop = -1;
            public bool cycleNotPlayed = false;
            public float lastIgnoredCycleTime;
            public bool started = false;
            public bool exit = true;
            public int curLoop = 0;
            public int lastLoop = 0;
            public bool cycleOffsetHasParameter;
            public int cycleOffsetParameterID;
            public float cycleOffset;
            /// <summary>
            /// Play Only on this Loop Cycle (-1 = any Cycle)
            /// </summary
            public int playCycle = 0;

            public int paramHash;

            public MLPASAnimatorParameter currentParameterRuntime;
            public string currentTransitionName;

            public string lastTransitionName;
            public string lastParameterName;
            public string lastParameterType;

            public string runtimeTransitionName;
            public string runtimeParameterName;
            public int runtimeCurrentStateHash;
            public int runtimeEndStateHash;
            public bool instantTransitionDuration;

            /// <summary>
            /// StateSFX Play Mode.
            /// </summary>
            public MLPASAnimatorPlayMode playMode;
            /// <summary>
            /// StateSFX Parameter Evaluate Mode.
            /// </summary>
            public MLPASParameterEvaluateMode parameterEvaluateMode;

            public bool runtimeError;
            public string runtimeErrorString;

            public bool transitionInit = false;

            public bool parameterInit = false;

            public bool nullParameter = false;
            public bool nullTransition = false;

            /// <summary>
            /// BlendTree Motions.
            /// </summary>
            public List<BlendMotion> blendTree = new List<BlendMotion>();

            /// <summary>
            /// The Custom Optional Parameters of this StateSFX.
            /// </summary>
            public MLPASACustomPlayMethodParameters.CustomParams userParameters = new MLPASACustomPlayMethodParameters.CustomParams();

            public bool showExtraConditions;

            /// <summary>
            /// Additional Play Conditions 
            /// </summary>
            public List<MLPASAnimatorCondition> conditions = new List<MLPASAnimatorCondition>();

            public int lastBlendValue = 0;

            [System.Serializable]
            public class BlendMotion
            {

                public bool blendNode = false;

                public bool unfolded = false;

                public string runTimeName;
                public float threshold;
                public Vector2 position;

                /// <summary>
                /// Use a Different Audio Object for this Motion.
                /// </summary>
                public bool useDifferentAudioObject;

                /// <summary>
                /// Audio Object to play.
                /// </summary>
                public AudioObject audioObject;
                /// <summary>
                /// Identifier to find the Audio Object to play.
                /// </summary>
                public string audioObjectIdentifier;
                /// <summary>
                /// Use an Audio Object Identifier.
                /// </summary>
                public bool useIdentifier;

                /// <summary>
                /// Use a Different Channel for this Motion.
                /// </summary>
                public bool useDifferentChannel;

                public int channel = -1;

                /// <summary>
                /// Use a Different PlayTime for this Motion.
                /// </summary>
                public bool useDifferentPlayTime;

                /// <summary>
                /// Time on which this BlendTree Motion will play.
                /// </summary>
                public float playTime;
                /// <summary>
                /// Defines if the PlayTime is specified in NormalizedTime or Frames.
                /// </summary>
                public bool playTimeNormalized=true;
                /// <summary>
                /// Frame on which this StateSFX will play.
                /// </summary>
                public int playFrame;

                /// <summary>
                /// Don't play the play the StateSFX if this Motion is playing.
                /// </summary>
                public bool ignoreMotion;

                public BlendMotion()
                {

                }

            }
            public string pathName;

#if UNITY_EDITOR
            /// <summary>
            /// Parameter to Evaluate.
            /// </summary>
            public MLPASAnimatorParameter currentParameter = new MLPASAnimatorParameter(true);
            /// <summary>
            /// Transition to Evaluate.
            /// </summary>
            public AnimatorStateTransition currentTransition;
            public bool transitionAssigned;
            public bool parameterAssigned;
#endif


        }

        [System.Serializable]
        public struct MLPASAnimatorParameter
        {
            public string parameterName;
            public AnimatorControllerParameterType type;
            public bool isNull;

            public MLPASAnimatorParameter(bool _null)
            {
                parameterName = "NULL";
                type = AnimatorControllerParameterType.Bool;
                isNull = true;
            }

            public MLPASAnimatorParameter(string paramName, AnimatorControllerParameterType paramType)
            {
                parameterName = paramName;
                type = paramType;
                isNull = false;
            }
            
            public MLPASAnimatorParameter(AnimatorControllerParameter param)
            {
                parameterName = param.name;
                type = param.type;
                isNull = false;
            }

        }

        [System.Serializable]
        public class MLPASAnimatorCondition
        {

            public int index = 0;
            public string parameter;
            public int parameterHash;
            public MLPASAnimatorConditionMode mode;
            public float threshold;
            public AnimatorControllerParameterType parameterType;
            public bool isNULL = true;


            public MLPASAnimatorCondition(string _parameter, MLPASAnimatorConditionMode _mode, float _threshold, AnimatorControllerParameterType _parameterType)
            {
                parameter = _parameter;
                mode = _mode;
                threshold = _threshold;
                parameterType = _parameterType;
            }

            public MLPASAnimatorCondition()
            {

            }

        }

        /// <summary>
        /// StateSFX Condition Evaluation Modes: Greater/true, Less/false, Equals, NotEqual
        /// </summary>
        public enum MLPASAnimatorConditionMode
        {
            Greater,
            Less,
            Equals,
            NotEqual
        }

        /// <summary>
        /// StateSFX Play Mode: OnStart, OnUpdate, OnExit, OnTransitionEnter, OnTransitionTime, OnTransitionExit, OnParameterChange
        /// </summary>
        public enum MLPASAnimatorPlayMode
        {
            OnStart,
            OnUpdate,
            OnExit,
            OnTransitionEnter,
            OnTransitionTime,
            OnTransitionExit,
            OnParameterChange
        }

        /// <summary>
        /// StateSFX Parameter Evaluate Mode: OnChange, OnPositiveChange, OnNegativeChange, OnExactValue
        /// </summary>
        public enum MLPASParameterEvaluateMode
        {
            OnChange,
            OnPositiveChange,
            OnNegativeChange,
            OnExactValue
        }


        public bool onStateMachine = false;

        public int transitionLayer;

        public List<StateSFX> stateSfxs = new List<StateSFX>();

        public Transform trf;

        bool useExternalController;

        public MLPASAnimatorSFXController.ValuesOverride sfxValues;
        public MLPASAnimatorSFXController controller;
        public bool sfxControllerExternalAssigned = false;

        public bool init = false;

        public string runtimeStateName;

        public bool isBlendTree = false;

        public bool blend2D = false;
        public int xParamHash;
        public int yParamHash;

        public int globalAddedSfx = 0;

        //public bool onSyncedLayer = false;
       // public int layerSync;

#if UNITY_EDITOR

        public AnimatorController currentAnimatorController;
        public AnimatorState currentState;
        public AnimatorStateMachine currentStateMachine;

#endif

        public void AssignSFXController(MLPASAnimatorSFXController sfxController,MLPASAnimatorSFXController.ValuesOverride newValues)
        {

            controller = sfxController;
            sfxValues = newValues;
            sfxControllerExternalAssigned = true;
            useExternalController = true;

        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
            if (trf==null)
            {
                trf = animator.transform;
            }

            if (useExternalController)
            {

                if (!init)
                {

                    if (!sfxControllerExternalAssigned && sfxValues == null)
                    {
                        useExternalController = false;
                    }

                    init = true;
                }
            }
            else
            {
                if (!init)
                {

                    init = true;
                }
            }


            for (int i = 0; i < stateSfxs.Count; i++)
            {

                StateSFX stateSFX = stateSfxs[i];

                if (stateSFX.runtimeError)
                {
                    Debug.LogError(stateSFX.runtimeErrorString);
                    return;
                }

                if (stateSFX.ignoreSfx)
                {

                    continue;
                }


               int blendValue = GetBlendValue(stateSFX, animator);

                stateSfxs[i].conditionEvaluated = false;
                stateSfxs[i].sfxPlayed = false;
                stateSfxs[i].normalizedPlayTime = stateSfxs[i].playTime / 100;

                if (isBlendTree)
                {
                    if (stateSFX.blendTree[blendValue].useDifferentPlayTime)
                    {
                        stateSfxs[i].normalizedPlayTime = stateSfxs[i].blendTree[blendValue].playTime / 100;
                    }

                }


                if (!stateSFX.sfxPlayed && stateSFX.playMode == MLPASAnimatorPlayMode.OnStart && !onStateMachine && EvaluateConditions(ref stateSFX.conditions, animator))
                {
                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                    stateSfxs[i].sfxPlayed = true;
                }

                if (stateSFX.playMode == MLPASAnimatorPlayMode.OnParameterChange)
                {
                    stateSfxs[i].paramHash = Animator.StringToHash(stateSFX.currentParameterRuntime.parameterName);

                    switch (stateSFX.currentParameterRuntime.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            stateSfxs[i].paramOldValue = animator.GetFloat(stateSfxs[i].paramHash);
                            break;
                        case AnimatorControllerParameterType.Int:
                            stateSfxs[i].paramOldValue = animator.GetInteger(stateSfxs[i].paramHash);
                            break;
                        case AnimatorControllerParameterType.Bool:
                            stateSfxs[i].paramOldValue = animator.GetBool(stateSfxs[i].paramHash) ? 1 : 0;
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            stateSfxs[i].paramOldValue = 0;
                            break;
                    }
                }

                stateSfxs[i].started = true;

            }


        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            for (int i = 0; i < stateSfxs.Count; i++)
            {

                StateSFX stateSFX = stateSfxs[i];
                 

                if (stateSFX.runtimeError)
                {

                    return;
                }

                if (stateSFX.ignoreSfx)
                {

                    continue;
                }

                if (stateSFX.started)
                {
             
                         stateSfxs[i].curLoop = 0;
                        stateSfxs[i].lastLoop = 0;
                        stateSfxs[i].lastPlayedLoop = -1;
                        stateSfxs[i].lastIgnoredCycleTime = stateInfo.normalizedTime;
                        stateSfxs[i].lastNormalizedTime = stateInfo.normalizedTime;
                        stateSfxs[i].startNormalizedTime = stateInfo.normalizedTime;
                        stateSfxs[i].cycleNotPlayed = true;
 
                        stateSfxs[i].started = false;
                }

                if (stateSFX.exit)
                {
         
                    stateSfxs[i].curLoop = 0;
                    stateSfxs[i].lastLoop = 0;
                    stateSfxs[i].lastPlayedLoop = -1;
                    stateSfxs[i].lastIgnoredCycleTime = stateInfo.normalizedTime;
                    stateSfxs[i].lastNormalizedTime = stateInfo.normalizedTime;
                    stateSfxs[i].startNormalizedTime = stateInfo.normalizedTime;
                    stateSfxs[i].cycleNotPlayed = true;
              
                    stateSfxs[i].exit = false;
                }

                int blendValue = GetBlendValue(stateSFX, animator);

                if (isBlendTree)
                {
                    if (stateSFX.blendTree[blendValue].useDifferentPlayTime)
                    {
                        stateSfxs[i].normalizedPlayTime = stateSfxs[i].blendTree[blendValue].playTime / 100;
                    }
                    else
                    {
                        stateSfxs[i].normalizedPlayTime = stateSfxs[i].playTime / 100;
                    }

                }


                if (stateSFX.playMode == MLPASAnimatorPlayMode.OnUpdate && !onStateMachine)
                {
                    
                   if (stateSFX.lastNormalizedTime > stateInfo.normalizedTime)
                       stateSfxs[i].lastNormalizedTime = stateInfo.normalizedTime;
   
                    stateSfxs[i].curLoop = (int)stateInfo.normalizedTime;
                    stateSfxs[i].lastLoop = (int)stateSFX.lastNormalizedTime;

                    float currentNormalizedTime = stateInfo.normalizedTime;
                    float lastNormalizedTime = stateSFX.lastNormalizedTime;

                    if (stateSfxs[i].cycleOffsetHasParameter)
                    {
                        stateSfxs[i].cycleOffset = animator.GetFloat(stateSfxs[i].cycleOffsetParameterID);
                    }

                    float sfxPlayTime = Mathf.Repeat(Mathf.Clamp((stateSFX.normalizedPlayTime), 0.01f, 0.99f) + stateSfxs[i].cycleOffset, 0.99f) + stateSFX.curLoop;

                    bool playOnceTest = stateSFX.playOnlyOnce && !stateSFX.sfxPlayed && (stateSFX.curLoop == stateSFX.playCycle || stateSFX.playCycle == -1);

                    if (stateSFX.startNormalizedTime > stateSFX.normalizedPlayTime)
                    {
                        stateSfxs[i].cycleNotPlayed = false;
                        stateSfxs[i].startNormalizedTime = -1;
                    }

                    if (!stateSFX.playOnlyOnce || playOnceTest)
                    {

                        if (sfxPlayTime >= lastNormalizedTime && sfxPlayTime < currentNormalizedTime && (sfxPlayTime - lastNormalizedTime <= Time.unscaledDeltaTime * 10))
                        {
                           
                            if (stateSFX.lastPlayedLoop != stateSFX.curLoop && EvaluateConditions(ref stateSFX.conditions, animator))
                            {
    
                                stateSfxs[i].lastPlayedLoop = stateSFX.curLoop;

                                stateSfxs[i].cycleNotPlayed = false;

                                PlaySFX(stateInfo, stateSfxs[i], blendValue, animator);

                                stateSfxs[i].sfxPlayed = true;

             
                            }

                        }


                        if (stateSFX.curLoop != stateSfxs[i].lastLoop && stateSFX.curLoop > 0 && stateSfxs[i].lastLoop == stateSFX.curLoop - 1 && stateSFX.normalizedPlayTime>0.95f)
                        {
                           
                            if (stateSFX.cycleNotPlayed)
                            {
                                if (stateInfo.normalizedTime >= 0f && stateInfo.normalizedTime != stateSFX.lastIgnoredCycleTime)
                                {

                                    if (stateSFX.lastPlayedLoop != stateSFX.curLoop && EvaluateConditions(ref stateSFX.conditions, animator))
                                    {
                                        
                                        stateSfxs[i].lastIgnoredCycleTime = stateInfo.normalizedTime;

                                        stateSfxs[i].lastPlayedLoop = stateSFX.curLoop;

                                        PlaySFX(stateInfo, stateSfxs[i], blendValue, animator);

                                        stateSfxs[i].sfxPlayed = true;
                                    }

                                }

                            }

                            stateSfxs[i].cycleNotPlayed = true;
                        }

                    }


                    stateSfxs[i].lastNormalizedTime = currentNormalizedTime;


                }


                if (!stateSFX.sfxPlayed && (stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionEnter/* || stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionExit*/ || stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionTime))
                {
                    if (!stateSFX.conditionEvaluated && EvaluateConditions(ref stateSFX.conditions, animator))
                    {
                        if (animator.GetNextAnimatorStateInfo(transitionLayer).shortNameHash == stateSFX.runtimeEndStateHash)
                        {
                            stateSfxs[i].conditionEvaluated = true;

                            if (stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionEnter)
                            {
                                PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                stateSfxs[i].sfxPlayed = true;
                            }

                        }
                    }

                }


                if (stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionTime)
                {
                    if (!stateSFX.sfxPlayed && EvaluateConditions(ref stateSFX.conditions, animator))
                    {
                        if (/*stateSFX.conditionEvaluated && */animator.GetNextAnimatorStateInfo(transitionLayer).shortNameHash == stateSFX.runtimeEndStateHash && animator.GetAnimatorTransitionInfo(transitionLayer).normalizedTime >= stateSFX.normalizedPlayTime)
                        {

                            PlaySFX(stateInfo, stateSFX, blendValue, animator);
                            stateSfxs[i].sfxPlayed = true;

                        }
                    }
                }

                if (stateSFX.playMode == MLPASAnimatorPlayMode.OnParameterChange && EvaluateConditions(ref stateSFX.conditions, animator))
                {

                    switch (stateSFX.currentParameterRuntime.type)
                    {
                        case AnimatorControllerParameterType.Float:

                            float valueFloat = animator.GetFloat(stateSFX.paramHash);

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnChange)
                            {
                                if (valueFloat > stateSFX.paramOldValue + stateSFX.paramThreshold || valueFloat < stateSFX.paramOldValue - stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueFloat;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnPositiveChange)
                            {
                                if (valueFloat > stateSFX.paramOldValue + stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueFloat;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnNegativeChange)
                            {
                                if (valueFloat < stateSFX.paramOldValue - stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueFloat;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnExactValue)
                            {
                                bool checkThreshold = valueFloat > stateSFX.paramExactValue - stateSFX.paramThreshold && valueFloat < stateSFX.paramExactValue + stateSFX.paramThreshold;

                                if (checkThreshold || valueFloat == stateSFX.paramExactValue)
                                {
                                    stateSfxs[i].paramOldValue = valueFloat;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            break;

                        case AnimatorControllerParameterType.Int:

                            int valueInteger = animator.GetInteger(stateSFX.paramHash);

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnChange)
                            {
                                if (valueInteger > stateSFX.paramOldValue + stateSFX.paramThreshold || valueInteger < stateSFX.paramOldValue - stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueInteger;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnPositiveChange)
                            {
                                if (valueInteger > stateSFX.paramOldValue + stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueInteger;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnNegativeChange)
                            {
                                if (valueInteger < stateSFX.paramOldValue - stateSFX.paramThreshold)
                                {
                                    stateSfxs[i].paramOldValue = valueInteger;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnExactValue)
                            {
                                bool checkThreshold = valueInteger > stateSFX.paramExactValue - stateSFX.paramThreshold && valueInteger < stateSFX.paramExactValue + stateSFX.paramThreshold;

                                if (checkThreshold || valueInteger == stateSFX.paramExactValue)
                                {
                                    stateSfxs[i].paramOldValue = valueInteger;
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;

                                }
                            }

                            break;

                        case AnimatorControllerParameterType.Bool:

                            bool valueBoolean = animator.GetBool(stateSFX.paramHash);

                            if (stateSFX.paramOldValue != BoolToFloat(valueBoolean))
                            {
                                stateSFX.paramOldValue = BoolToFloat(valueBoolean);

                                if (stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnChange || stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnPositiveChange && valueBoolean || stateSFX.parameterEvaluateMode == MLPASParameterEvaluateMode.OnNegativeChange && !valueBoolean)
                                {
                                    PlaySFX(stateInfo, stateSFX, blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;
                                }
                            }

                            break;

                        case AnimatorControllerParameterType.Trigger:


                            bool valueTrigger = animator.GetBool(stateSfxs[i].paramHash);

                            if (stateSfxs[i].paramOldValue != BoolToFloat(valueTrigger))
                            {
                                stateSfxs[i].paramOldValue = BoolToFloat(valueTrigger);

                                if (valueTrigger)
                                {
                                    PlaySFX(stateInfo, stateSfxs[i], blendValue, animator);
                                    stateSfxs[i].sfxPlayed = true;
                                }
                            }

                            break;
                    }

                }

            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (trf == null)
            {
                return;
            }

            for (int i = 0; i < stateSfxs.Count; i++)
            {

                StateSFX stateSFX = stateSfxs[i];

                if (stateSFX.runtimeError)
                    return;


                if (stateSFX.ignoreSfx)
                {

                    continue;
                }

                stateSfxs[i].exit = true;

                if (!stateSFX.sfxPlayed && stateSFX.playMode == MLPASAnimatorPlayMode.OnExit && !onStateMachine && EvaluateConditions(ref stateSFX.conditions,animator))
                {
                    PlaySFX(stateInfo, stateSFX, GetBlendValue(stateSFX, animator), animator);
                    stateSfxs[i].sfxPlayed = true;
                }

                if (!stateSFX.sfxPlayed && EvaluateConditions(ref stateSFX.conditions, animator) /*&& stateSFX.conditionEvaluated*/ && (stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionExit && true || stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionEnter && stateSFX.instantTransitionDuration || stateSFX.playMode == MLPASAnimatorPlayMode.OnTransitionTime && stateSFX.instantTransitionDuration))
                {
                    PlaySFX(stateInfo, stateSFX, GetBlendValue(stateSFX, animator), animator);
                    stateSfxs[i].sfxPlayed = true;
                }

            }

        }

        bool EvaluateConditions (ref List<MLPASAnimatorCondition> conditions, Animator animator)
        {
            bool evaluated = false;

            for (int i = 0; i < conditions.Count; i++)
            {
                MLPASAnimatorCondition cond = conditions[i];
                AnimatorControllerParameterType pType = cond.parameterType;
                MLPASAnimatorConditionMode mode = cond.mode;
                float threshold = cond.threshold;
                float currentPValue = threshold;

                switch (pType)
                {
                    case AnimatorControllerParameterType.Float:
                        currentPValue = animator.GetFloat(cond.parameterHash);
                        break;
                    case AnimatorControllerParameterType.Int:
                        currentPValue = animator.GetInteger(cond.parameterHash);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        currentPValue = animator.GetBool(cond.parameterHash)?1f:0f;
                        break;
                }

                if (pType== AnimatorControllerParameterType.Bool)
                {
                    threshold = 0.5f;
                }

                switch (mode)
                {
                    case MLPASAnimatorConditionMode.Greater:
                        evaluated = currentPValue > threshold;
                        break;
                    case MLPASAnimatorConditionMode.Less:
                        evaluated = currentPValue < threshold;
                        break;
                    case MLPASAnimatorConditionMode.Equals:
                        evaluated = currentPValue == threshold;
                        break;
                    case MLPASAnimatorConditionMode.NotEqual:
                        evaluated = currentPValue != threshold;
                        break;
                    default:
                        evaluated = true;
                        break;
                }

                if (!evaluated)
                    return false;
            }

            return true;
        }

        float BoolToFloat(bool boolean)
        {
            return boolean ? 1 : 0;
        }

        void PlaySFX(AnimatorStateInfo stateInfo, StateSFX stateSfx, int blendValue, Animator anim)
        {

            if (Object.ReferenceEquals(trf, null))
                return;

           /* if (onSyncedLayer)
            {
                Debug.Log(anim.GetLayerWeight(layerSync));
                if (anim.GetLayerWeight(layerSync)>0f)
                {
                    return;
                }
            }*/

#if UNITY_EDITOR
            if (!stateSfx.useCustomPlayMethod && !isBlendTree)
            {
                if (stateSfx.useIdentifier)
                {
                    if (Object.ReferenceEquals(MultiAudioManager.GetAudioObjectByIdentifier(stateSfx.audioObjectIdentifier, false), null))
                        Debug.LogWarning(stateSfx.pathName + " | <i>Audio Object</i> is missing or invalid");
                }
                else
                {
                    if (Object.ReferenceEquals(stateSfx.audioObject, null))
                        Debug.LogWarning(stateSfx.pathName + " | <i>Audio Object</i> is missing or invalid");
                }
            }
#endif

            bool usingDifferentPlayPos = useExternalController && sfxValues.useDifferentPlayPosition && sfxValues.playPosition != null;
            bool followPos = !usingDifferentPlayPos && stateSfx.followAnimator || usingDifferentPlayPos && sfxValues.followPosition;

            if (!isBlendTree)
            {

                if (stateSfx.useCustomPlayMethod)
                {
                    if (stateSfx.customPlayMethod != null)
                    {
                        if ((stateSfx.customPlayMethod.Target as Object) != null)
                        {
                            Transform trans = usingDifferentPlayPos ? sfxValues.playPosition : trf;
                            MLPASACustomPlayMethodParameters.CustomParams userParameters = stateSfx.userParameters;
                            MLPASACustomPlayMethodParameters parameters = new MLPASACustomPlayMethodParameters(stateSfx.useIdentifier ? MultiAudioManager.GetAudioObjectByIdentifier(stateSfx.audioObjectIdentifier) : stateSfx.audioObject, stateSfx.useChannel ? stateSfx.channel : -1, followPos, trans.position, trans, 0, stateInfo, stateSfx.normalizedPlayTime, userParameters);
                            stateSfx.customPlayMethod.Invoke(parameters);
                            return;
                        }
                        else
                        {
                            if (stateSfx.customMethodWarnings)
                            Debug.LogWarning(stateSfx.pathName + " | Custom Play Method:  <b>" + stateSfx.methodName + "</b> > Receiver has been removed.");

                            if (stateSfx.playOnlyWithReceiver)
                                return;
                        }
                    }
                    else
                    {
                        if (stateSfx.customMethodWarnings)
                            Debug.LogWarning(stateSfx.pathName + " | Custom Play Method:  <b>" + stateSfx.methodName + "</b> > Can't be found");

                        if (stateSfx.playOnlyWithReceiver)
                            return;
                    }
                }

                if (followPos)
                {

                    if (stateSfx.useIdentifier)
                    {
                        MultiAudioSource _source = MultiAudioManager.PlayAudioObjectByIdentifier(stateSfx.audioObjectIdentifier, stateSfx.useChannel ? stateSfx.channel : -1, usingDifferentPlayPos ? sfxValues.playPosition : trf);
                        _source.IgnoreListenerPause = true;
                    }
                    else
                    {
                        MultiAudioSource _source = MultiAudioManager.PlayAudioObject(stateSfx.audioObject, stateSfx.useChannel ? stateSfx.channel : -1, usingDifferentPlayPos ? sfxValues.playPosition : trf);
                        _source.IgnoreListenerPause = true;
                    }

                }

                else
                {

                    if (stateSfx.useIdentifier)
                    {
                        MultiAudioSource _source = MultiAudioManager.PlayAudioObjectByIdentifier(stateSfx.audioObjectIdentifier, stateSfx.useChannel ? stateSfx.channel : -1, usingDifferentPlayPos ? sfxValues.playPosition.position : trf.position);
                        _source.IgnoreListenerPause = true;
                    }
                    else
                    {
                        MultiAudioSource _source = MultiAudioManager.PlayAudioObject(stateSfx.audioObject, stateSfx.useChannel ? stateSfx.channel : -1, usingDifferentPlayPos ? sfxValues.playPosition.position : trf.position);
                        _source.IgnoreListenerPause = true;
                    }

                }

            }
            else
            {

                int index = blendValue;

                StateSFX.BlendMotion blend = stateSfx.blendTree[index];

                if (stateSfx.blendTree[index].ignoreMotion)
                    return;

                int channel = -1;

                if (blend.useDifferentChannel)
                {
                    channel = stateSfx.blendTree[index].channel;
                }
                else
                {
                    if (stateSfx.useChannel)
                        channel = stateSfx.channel;
                }

                if (stateSfx.useCustomPlayMethod)
                {
                    if (stateSfx.customPlayMethod != null)
                    {
                        if ((stateSfx.customPlayMethod.Target as Object) != null)
                        {
                            Transform trans = usingDifferentPlayPos ? sfxValues.playPosition : trf;
                            MLPASACustomPlayMethodParameters.CustomParams userParameters = stateSfx.userParameters;
                            MLPASACustomPlayMethodParameters parameters = new MLPASACustomPlayMethodParameters(stateSfx.useIdentifier || blend.useDifferentAudioObject && blend.useIdentifier ? MultiAudioManager.GetAudioObjectByIdentifier(blend.useDifferentAudioObject ? blend.audioObjectIdentifier : stateSfx.audioObjectIdentifier) : blend.useDifferentAudioObject ? blend.audioObject : stateSfx.audioObject, channel, followPos, trans.position, trans, blendValue, stateInfo, stateSfx.normalizedPlayTime, userParameters);
                            stateSfx.customPlayMethod.Invoke(parameters);
                            return;
                        }
                        else
                        {
                            if (stateSfx.customMethodWarnings)
                                Debug.LogWarning(stateSfx.pathName + " | Custom Play Method:  <b>" + stateSfx.methodName + "</b> > Receiver has been removed.");

                            if (stateSfx.playOnlyWithReceiver)
                                return;
                        }
                    }
                    else
                    {
                        if (stateSfx.customMethodWarnings)
                        {
                            Debug.LogWarning(stateSfx.pathName + " | Custom Play Method:  <b>" + stateSfx.methodName + "</b> > Can't be found");
                        }

                        if (stateSfx.playOnlyWithReceiver)
                            return;
                    }
                }


                if (followPos)
                {
                    if (stateSfx.useIdentifier || blend.useDifferentAudioObject && blend.useIdentifier)
                    {
                        AudioObject ao = MultiAudioManager.GetAudioObjectByIdentifier(blend.useDifferentAudioObject ? blend.audioObjectIdentifier : stateSfx.audioObjectIdentifier, false);
                        if (!Object.ReferenceEquals(ao, null))
                            MultiAudioManager.PlayAudioObject(ao, channel, usingDifferentPlayPos ? sfxValues.playPosition : trf);

                    }
                    else
                    {
                        AudioObject ao = blend.useDifferentAudioObject ? blend.audioObject : stateSfx.audioObject;
                        if (!Object.ReferenceEquals(ao, null))
                            MultiAudioManager.PlayAudioObject(blend.useDifferentAudioObject ? blend.audioObject : stateSfx.audioObject, channel, usingDifferentPlayPos ? sfxValues.playPosition : trf);
                    }
                }

                else
                {
                    if (stateSfx.useIdentifier || blend.useDifferentAudioObject && blend.useIdentifier)
                    {
                        AudioObject ao = MultiAudioManager.GetAudioObjectByIdentifier(blend.useDifferentAudioObject ? blend.audioObjectIdentifier : stateSfx.audioObjectIdentifier, false);
                        if (!Object.ReferenceEquals(ao, null))
                            MultiAudioManager.PlayAudioObject(ao, channel, usingDifferentPlayPos ? sfxValues.playPosition.position : trf.position);

                    }
                    else
                    {
                        AudioObject ao = blend.useDifferentAudioObject ? blend.audioObject : stateSfx.audioObject;
                        if (!Object.ReferenceEquals(ao, null))
                            MultiAudioManager.PlayAudioObject(blend.useDifferentAudioObject ? blend.audioObject : stateSfx.audioObject, channel, usingDifferentPlayPos ? sfxValues.playPosition.position : trf.position);
                    }
                }

            }

        }


        int GetBlendValue(StateSFX stateSfx, Animator animator)
        {
           
            if (!isBlendTree)
                return 0;


            float xParam = animator.GetFloat(xParamHash);
            float yParam = blend2D ? animator.GetFloat(yParamHash) : 0;

            float lastDistance = float.MaxValue;

            Vector2 currentPosition = new Vector2(xParam, yParam);

            int index = 0;

            for (int i = 0; i < stateSfx.blendTree.Count; i++)
            {
                Vector2 targetPosition = blend2D ? stateSfx.blendTree[i].position : new Vector2(stateSfx.blendTree[i].threshold, 0);

                float distance = new Vector2(currentPosition.x - targetPosition.x, currentPosition.y - targetPosition.y).magnitude;

                if (distance <= lastDistance)
                {
                    lastDistance = distance;
                    index = i;
                }

            }

            return index;

        }

        public bool UpdateRuntimeValues()
        {

            bool error = false;
            bool parameterChanges = false;
            bool runtimeError = false;
            string runtimeErrorString = "";
#if UNITY_EDITOR

            bool blendTreeInUse = false;

            for (int i = 0; i < stateSfxs.Count; i++)
            {

                string log = "Animator: <i><b>" + currentAnimatorController.name + "</b></i>" + " - ";
                log += onStateMachine ? "State Machine: " : "State: ";
                log += "<i><b>" + (onStateMachine ? currentStateMachine.name : currentState.name) + "</b></i>";


                if (stateSfxs[i].pathName != log)
                {
                    stateSfxs[i].pathName = log;
                    parameterChanges = true;
                }


                bool runtimeStateNameCheck = false;

                if (!onStateMachine && currentState != null)
                {
                    runtimeStateName = currentState.name;
                    runtimeStateNameCheck = true;
                }

                if (onStateMachine && currentStateMachine != null)
                {
                    runtimeStateName = currentStateMachine.name;
                    runtimeStateNameCheck = true;
                }

                if (!runtimeStateNameCheck)
                {
                    Debug.LogError(log + " | <i>Current State</i> is missing or invalid");
                    error = true;
                    runtimeError = true;
                    runtimeErrorString = log + "<b> | Current State or State Machine is NULL</b>";
                }

                bool isBlendTreeNotNULL = !onStateMachine && (currentState.motion as BlendTree) != null;

                int stateHash = currentState != null ? currentState.nameHash : 0;

                if (stateSfxs[i].runtimeCurrentStateHash != stateHash)
                {
                    stateSfxs[i].runtimeCurrentStateHash = stateHash;
                    parameterChanges = true;
                }


                /*if (onStateMachine)
                {

                    int layerIndex=0;
                    

                    int syncLayer = currentAnimatorController.layers[layerIndex].syncedLayerIndex;

                    if (layerSync != syncLayer)
                    {
                        layerSync = syncLayer;
                        parameterChanges = true;
                    }

                    bool synced = syncLayer >= 0;

                    if (synced != onSyncedLayer)
                    {
                        onSyncedLayer = synced;
                        parameterChanges = true;
                    }

                }*/

   
                bool usingMotionAudioObject = false;

                if (isBlendTreeNotNULL)
                {

                    try
                    {


                        BlendTree tree = currentState.motion as BlendTree;
                        xParamHash = Animator.StringToHash(tree.blendParameter);
                        yParamHash = Animator.StringToHash(tree.blendParameterY);
                        blend2D = tree.blendType != BlendTreeType.Simple1D;

                        for (int iB = 0; iB < stateSfxs[i].blendTree.Count; iB++)
                        {

                            float newThreshold = tree.children[iB].threshold;
                            if (stateSfxs[i].blendTree[iB].threshold != newThreshold)
                            {
                                stateSfxs[i].blendTree[iB].threshold = newThreshold;
                                parameterChanges = true;
                            }

                            Vector2 newPosition = tree.children[iB].position;
                            if (stateSfxs[i].blendTree[iB].position!=newPosition)
                            {
                                stateSfxs[i].blendTree[iB].position = newPosition;
                                parameterChanges = true;
                            }

                            string blendTreeItemName = (!blend2D ? tree.blendParameter + ": " + tree.children[iB].threshold.ToString() : tree.blendParameter + ": " + tree.children[iB].position.x.ToString() + ", " + tree.blendParameterY + ": " + tree.children[iB].position.y.ToString());

                            if (stateSfxs[i].blendTree[iB].runTimeName != blendTreeItemName)
                            {
                                stateSfxs[i].blendTree[iB].runTimeName = blendTreeItemName;
                                parameterChanges = true;
                            }

                            if (stateSfxs[i].blendTree[iB].useDifferentAudioObject || stateSfxs[i].blendTree[iB].useDifferentChannel || stateSfxs[i].blendTree[iB].useDifferentPlayTime || stateSfxs[i].blendTree[iB].ignoreMotion)
                            {
                                blendTreeInUse = true;
                            }

                            if (stateSfxs[i].blendTree[iB].useDifferentAudioObject)
                            {
                                usingMotionAudioObject = true;

                                if (!stateSfxs[i].blendTree[iB].useIdentifier && stateSfxs[i].blendTree[iB].audioObject == null)
                                {
                                    runtimeErrorString = "Blendtree Motion: <b>" + blendTreeItemName + "</b> - " + log + " | <i>Audio Object</i> is missing or invalid";
                                    Debug.LogError(runtimeErrorString);
                                    runtimeError = true;
                                    error = true;
                                }

                            }

                        }



                    }
                    catch (System.Exception)
                    {

                      /*  runtimeError = true;

                        runtimeErrorString = "Blendtree is missing: <b>" + stateSfxs[i].lastTransitionName + "</b> on: " + log + " <b> | WAS REMOVED </b>";
                        Debug.LogError(runtimeErrorString);

                        error = true;*/

                    }

                }


                //Conditions
                bool missingParamCondition=false;
                string parameterConditionName="None";
                AnimatorControllerParameterType parameterConditionType=AnimatorControllerParameterType.Bool;
                List<MLPASAnimatorCondition> toRemove = new List<MLPASAnimatorCondition>();

                    foreach (var item in stateSfxs[i].conditions)
                    {
                        bool finded = false;

                    if (item.isNULL)
                    {
                        toRemove.Add(item);
                        continue;
                    }

                        foreach (var pa in currentAnimatorController.parameters)
                        {

                            if (item.parameter == pa.name && item.parameterType == pa.type)
                            {
                                finded = true;

                            int newHash = Animator.StringToHash(pa.name);

                            if (item.parameterHash != newHash)
                            {
                                item.parameterHash = newHash;
                                parameterChanges = true;
                            }
                                break;
                            }

                        }

                        if (finded)
                            continue;
                        else
                        {
                            missingParamCondition = true;
                          

                        if (!item.isNULL)
                        {
                            parameterConditionName = item.parameter;
                            parameterConditionName = item.parameter;
                            parameterConditionType = item.parameterType;
                        }
                        
                            break;
                        }

                    }

                for (int iR = 0; iR < toRemove.Count; iR++)
                {
                    stateSfxs[i].conditions.Remove(toRemove[iR]);
                    parameterChanges = true;
                }

                if (missingParamCondition)
                {

                    runtimeError = true;

                    runtimeErrorString = "Condition Parameter: <b>" + parameterConditionName + " [" + parameterConditionType.ToString() + "]" + "</b> on: " + log + " <b> | WAS REMOVED OR RENAMED </b>";

                    Debug.LogError(runtimeErrorString);

                    error = true;

                }

                // // // // // //


                if (!onStateMachine && stateSfxs[i].playMode == MLPASAnimatorPlayMode.OnUpdate)
                {
                    if (currentState.cycleOffsetParameterActive != stateSfxs[i].cycleOffsetHasParameter)
                    {
                        stateSfxs[i].cycleOffsetHasParameter = currentState.cycleOffsetParameterActive;
                    }
                    if (Animator.StringToHash(currentState.cycleOffsetParameter) != stateSfxs[i].cycleOffsetParameterID)
                    {
                        stateSfxs[i].cycleOffsetParameterID = currentState.cycleOffsetParameterActive ? Animator.StringToHash(currentState.cycleOffsetParameter) : 0;
                    }
                    if (currentState.cycleOffset != stateSfxs[i].cycleOffset)
                    {
                        stateSfxs[i].cycleOffset = !currentState.cycleOffsetParameterActive ? currentState.cycleOffset : 0;
                    }
                }

                if (stateSfxs[i].playMode == MLPASAnimatorPlayMode.OnTransitionEnter || stateSfxs[i].playMode == MLPASAnimatorPlayMode.OnTransitionTime || stateSfxs[i].playMode == MLPASAnimatorPlayMode.OnTransitionExit)
                {

                    try
                    {
                        string currentTransitionName = stateSfxs[i].currentTransition.GetDisplayName(currentState);
                        bool instantTransition = stateSfxs[i].currentTransition.duration <= 0.0001f;
                        if (currentTransitionName != stateSfxs[i].currentTransition.GetDisplayName(currentState))
                        {
                            stateSfxs[i].currentTransitionName = currentTransitionName;
                            parameterChanges = true;
                        }
                        if (instantTransition != stateSfxs[i].instantTransitionDuration)
                        {
                            stateSfxs[i].instantTransitionDuration = instantTransition;
                            parameterChanges = true;
                        }
                        string lastTransitionName = stateSfxs[i].currentTransition.GetDisplayName(currentState);
                        if (stateSfxs[i].lastTransitionName != lastTransitionName)
                        {
                            stateSfxs[i].lastTransitionName = lastTransitionName;
                            parameterChanges = true;
                        }
                    }

                    catch (System.Exception)
                    {

                     
                        runtimeError = true;

                        if (!string.IsNullOrEmpty(stateSfxs[i].lastTransitionName))
                            runtimeErrorString = "Transition: <b>" + stateSfxs[i].lastTransitionName + "</b> on: " + log + " <b> | WAS REMOVED </b>";
                        else
                            runtimeErrorString = "Transition on: " + log + " <b> | CAN'T BE NULL </b>";

                        Debug.LogError(runtimeErrorString);

                        error = true;

                    }

                    string runtimeTransitionName = stateSfxs[i].currentTransition != null ? stateSfxs[i].currentTransitionName : "Missing";
                    if (stateSfxs[i].runtimeTransitionName != runtimeTransitionName)
                    {
                        stateSfxs[i].runtimeTransitionName = runtimeTransitionName;
                        parameterChanges = true;
                    }

                    int runtimeEndStateHash = stateSfxs[i].currentTransition != null ? stateSfxs[i].currentTransition.destinationState.nameHash : 0;
                    if (runtimeEndStateHash != stateSfxs[i].runtimeEndStateHash)
                    {
                        stateSfxs[i].runtimeEndStateHash = runtimeEndStateHash;
                        parameterChanges = true;
                    }


                }

                if (stateSfxs[i].playMode == MLPASAnimatorPlayMode.OnParameterChange)
                {


                    if (stateSfxs[i].currentParameter.isNull)
                    {

                        for (int ip = 0; ip < currentAnimatorController.parameters.Length; ip++)
                        {

                            if (currentAnimatorController.parameters[ip].name == stateSfxs[i].currentParameter.parameterName && currentAnimatorController.parameters[ip].type == stateSfxs[i].currentParameter.type)
                            {
                                if (stateSfxs[i].currentParameter.isNull)
                                {
                                    stateSfxs[i].currentParameter.isNull = false;
                                    parameterChanges = true;
                                }
                                break;
                            }

                        }

                    }

                    bool paramFinded = false;

                    foreach (var pa in currentAnimatorController.parameters)
                    {

                        if (stateSfxs[i].currentParameter.parameterName == pa.name && stateSfxs[i].currentParameter.type == pa.type)
                        {
                            paramFinded = true;
                            break;
                        }

                    }


                    if (!stateSfxs[i].currentParameter.isNull && paramFinded)
                    {
                        MLPASAnimatorParameter newParam = new MLPASAnimatorParameter(stateSfxs[i].currentParameter.parameterName, stateSfxs[i].currentParameter.type);

                        if (stateSfxs[i].currentParameterRuntime.parameterName!=newParam.parameterName || stateSfxs[i].currentParameterRuntime.type != newParam.type)
                        {
                            stateSfxs[i].currentParameterRuntime = newParam;
                            parameterChanges = true;
                        }

                        if (stateSfxs[i].lastParameterName != newParam.parameterName)
                        {
                            stateSfxs[i].lastParameterName = newParam.parameterName;
                            parameterChanges = true;
                        }

                        string typeStr = newParam.type.ToString();
                        if (stateSfxs[i].lastParameterType != typeStr)
                        {
                            stateSfxs[i].lastParameterType = typeStr;
                            parameterChanges = true;
                        }
                    }

                    else
                    {

                        runtimeError = true;

                        if (!string.IsNullOrEmpty(stateSfxs[i].lastParameterType))
                            runtimeErrorString = "Parameter: <b>" + stateSfxs[i].lastParameterName + " [" + stateSfxs[i].lastParameterType + "]" + "</b> on: " + log + " <b> | WAS REMOVED OR RENAMED </b>";
                        else
                            runtimeErrorString = "Parameter on: " + log + " <b> | CAN'T BE NULL </b>";

                        Debug.LogError(runtimeErrorString);

                        error = true;

                    }

                    string newParamName = !stateSfxs[i].currentParameter.isNull ? stateSfxs[i].currentParameter.parameterName : "Missing";

                    if (stateSfxs[i].runtimeParameterName != newParamName)
                    {
                        stateSfxs[i].runtimeParameterName = newParamName;
                        parameterChanges = true;
                    }
                }

                if (!stateSfxs[i].useCustomPlayMethod && !usingMotionAudioObject)
                {
                    if (!stateSfxs[i].useIdentifier && stateSfxs[i].audioObject == null)
                    {
                        Debug.LogError(log + " | <i>Audio Object</i> is missing or invalid");
                        runtimeErrorString = log + " | <i>Audio Object</i> is missing or invalid";
                        runtimeError = true;
                        error = true;
                    }
                }

                if (stateSfxs[i].runtimeError != runtimeError)
                {
                    stateSfxs[i].runtimeError = runtimeError;
                    parameterChanges = true;
                }

                if (stateSfxs[i].runtimeErrorString != runtimeErrorString)
                {
                    stateSfxs[i].runtimeErrorString = runtimeErrorString;
                    parameterChanges = true;
                }

            }

            isBlendTree = blendTreeInUse;

            isBlendTree = false;


            if (parameterChanges)
            {
                EditorUtility.SetDirty(this);
            }
#endif

            return !error;

        }




#if UNITY_EDITOR
        [CustomEditor(typeof(MLPASAnimatorSFX))]
        public class MLPASAnimatorSFXEditor : Editor
        {

            SerializedObject obj;
            SerializedProperty currentAnimatorControllerProp;
            SerializedProperty currentStateProp;
            SerializedProperty currentStateMachineProp;
            SerializedProperty transitionLayerProp;
            SerializedProperty onStateMachineProp;

            StateMachineBehaviourContext[] context;
            AnimatorState state;
            AnimatorStateMachine stateMachine;
            AnimatorController animController;

            private Texture addIcon;
            private Texture copyIcon;
            private Texture pasteIcon;
            private Texture removeIcon;
            private Texture2D uiBack;
            private Texture2D uiTooltipBack;

            List<AnimatorStateTransition> transitions = new List<AnimatorStateTransition>();
            List<AnimatorControllerParameter> parameters = new List<AnimatorControllerParameter>();
            int transitionIndex = -1;
            int parameterIndex = -1;

            int selectedIndex = 0;
            int prevSelectedIndex = 0;

            bool transitionMissing = false;
            bool parameterMissing = false;

            Color color_selected = new Color32(98, 220, 255, 255);
            Color colorPro_selected = new Color32(28, 128, 170, 255);
            bool valuesModified = false;
            bool dirty = false;

            bool validated = false;

            bool isBlendTree = false;
            BlendTree blendTree = null;

            bool showBlendtree = false;

            AlmenaraGames.Tools.MLPASConfig config;

            bool showCustomParameters;

            bool debugFoldout;

            Object currentMotion;
            Editor motionEditorPreview;

            void OnEnable()
            {

                obj = new SerializedObject(target);

                config = Resources.Load("MLPASConfig/MLPASConfig", typeof(AlmenaraGames.Tools.MLPASConfig)) as AlmenaraGames.Tools.MLPASConfig;
                if (config == null)
                {
                    Debug.LogError("The MLPAS Config file is missing, open the 'Almenara Games/MLPAS/Config' tab to create a new one");
                }

                currentAnimatorControllerProp = obj.FindProperty("currentAnimatorController");
                currentStateProp = obj.FindProperty("currentState");
                currentStateMachineProp = obj.FindProperty("currentStateMachine");
                transitionLayerProp = obj.FindProperty("transitionLayer");
                onStateMachineProp = obj.FindProperty("onStateMachine");

                addIcon = Resources.Load("MLPASImages/addIcon") as Texture;
                copyIcon = Resources.Load("MLPASImages/copyIcon") as Texture;
                pasteIcon = Resources.Load("MLPASImages/pasteIcon") as Texture;
                removeIcon = Resources.Load("MLPASImages/removeIcon") as Texture;
                uiBack = Resources.Load("MLPASImages/guiBack") as Texture2D;
                uiTooltipBack = Resources.Load(EditorGUIUtility.isProSkin ? "MLPASImages/proGuiBack" : "MLPASImages/guiBack") as Texture2D;
             
                if (!EditorApplication.isPlaying)
                {
                    
                        obj.Update();
                        context = AnimatorController.FindStateMachineBehaviourContext(obj.targetObject as StateMachineBehaviour);


                        if (context != null && context.Length > 0)
                        {
                            stateMachine = (context[0].animatorObject as AnimatorStateMachine);
                            state = (context[0].animatorObject as AnimatorState);
                            animController = context[0].animatorController;
                            transitionLayerProp.intValue = context[0].layerIndex;
                 
                            currentAnimatorControllerProp.objectReferenceValue = context[0].animatorController;
                            currentStateProp.objectReferenceValue = (context[0].animatorObject as AnimatorState);
                            currentStateMachineProp.objectReferenceValue = (context[0].animatorObject as AnimatorStateMachine);

                            if (state != null)
                                blendTree = state.motion as BlendTree;

                            if (blendTree != null && blendTree.blendType != BlendTreeType.Direct)
                                isBlendTree = true;

                            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
                            {

                                validated = true;

                                if (state != null)
                                {
                                    foreach (var st in state.transitions)
                                    {
                                        if (st.duration == 0.0001f)
                                            st.duration = 0;

                                    }

                                    

                                    if (animController.layers[transitionLayerProp.intValue].syncedLayerIndex < 0)
                                    {
                                        foreach (var item in state.behaviours)
                                        {

                                            MLPASAnimatorSFX mlpasAnimSfx = item as MLPASAnimatorSFX;

                                            if (mlpasAnimSfx != null && mlpasAnimSfx.first)
                                            {
                                                validated = false;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in animController.layers[transitionLayerProp.intValue].GetOverrideBehaviours(state))
                                        {

                                            MLPASAnimatorSFX mlpasAnimSfx = item as MLPASAnimatorSFX;

                                            if (mlpasAnimSfx != null && mlpasAnimSfx.first)
                                            {
                                                validated = false;
                                            }

                                        }
                                    }

                                }

                                if (stateMachine != null)
                                {
                                    foreach (var st in stateMachine.anyStateTransitions)
                                    {
                                        if (st.duration == 0.0001f)
                                            st.duration = 0;

                                    }

                                    foreach (var item in stateMachine.behaviours)
                                    {

                                        MLPASAnimatorSFX mlpasAnimSfx = item as MLPASAnimatorSFX;

                                        if (mlpasAnimSfx.first)
                                        {
                                            validated = false;
                                        }

                                    }

                                }
                            }

                            onStateMachineProp.boolValue = currentStateMachineProp.objectReferenceValue != null;

                        }

                        obj.ApplyModifiedProperties();
                    

                }

                selectedIndex = (obj.targetObject as MLPASAnimatorSFX).selectedIndex;
                prevSelectedIndex = selectedIndex;



                if (validated && !(obj.targetObject as MLPASAnimatorSFX).first)
                {
                    (obj.targetObject as MLPASAnimatorSFX).first = true;

                    EditorUtility.SetDirty((obj.targetObject as MLPASAnimatorSFX));

                    EditorUtility.SetDirty(animController);

                }

            }

            public override void OnInspectorGUI()
            {

                if (!(obj.targetObject as MLPASAnimatorSFX).first)
                {

                    EditorGUILayout.HelpBox("Can't be multiple instances of this State Machine Behaviour", MessageType.Error);

                    return;
                }

                dirty = false;

                obj.Update();

                bool isPlaying = EditorApplication.isPlaying;

                List<MLPASAnimatorSFX.StateSFX> stateSfxsProp = (obj.targetObject as MLPASAnimatorSFX).stateSfxs;


                if (!isPlaying)
                {

                    GUIStyle smallButton = new GUIStyle(EditorStyles.toolbarButton);
                    smallButton.stretchWidth = false;

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (stateSfxsProp.Count > 0)
                    {

                        if (GUILayout.Button(copyIcon, smallButton))
                        {

                            if (config != null)
                            {
                                config.stateClipboardBuffer = new MLPASAnimatorSFX.StateSFX[] { (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex] };
                            }
                            else
                            {
                                Debug.LogError("State can't be copied because the MLPAS Config file is missing, open the 'Almenara Games/MLPAS/Config' tab to create a new one");
                            }

                        }

                        GUILayout.Label("Copy SFX");
                    }

                    bool prevGUIEnabled = GUI.enabled;
                    bool somethingInClipboard = config.stateClipboardBuffer != null && config.stateClipboardBuffer.Length > 0 && config.stateClipboardBuffer[0] != null;
                    GUI.enabled = somethingInClipboard;


                    if (GUILayout.Button(pasteIcon, smallButton))
                    {

                        if (config != null)
                        {
                            MLPASAnimatorSFX.StateSFX newSFX = new MLPASAnimatorSFX.StateSFX();

                            DuplicateStateSFX(config.stateClipboardBuffer[0], ref newSFX);

                            (obj.targetObject as MLPASAnimatorSFX).globalAddedSfx += 1; 

                            valuesModified = true;

                            Undo.RecordObject(obj.targetObject, "Paste SFX");

                            (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Add(newSFX);

                            stateSfxsProp = (obj.targetObject as MLPASAnimatorSFX).stateSfxs;

                            selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count - 1;
                        }
                        else
                        {
                            Debug.LogError("State can't be pasted because the MLPAS Config file is missing, open the 'Almenara Games/MLPAS/Config' tab to create a new one");
                        }

                    }

                    //EditorGUILayout.BeginVertical();

                    string copiedName = "";
                    bool hasName = false;

                    if (somethingInClipboard)
                    {

                        if (!string.IsNullOrEmpty(config.stateClipboardBuffer[0].customName))
                        {
                            copiedName = config.stateClipboardBuffer[0].customName;
                            hasName = true;
                        }
                        else if (config.stateClipboardBuffer[0].useCustomPlayMethod && !string.IsNullOrEmpty(config.stateClipboardBuffer[0].methodName))
                        {
                            copiedName = config.stateClipboardBuffer[0].methodName;
                            hasName = true;
                        }
                        else
                        {
                            if (config.stateClipboardBuffer[0].useIdentifier)
                            {
                                copiedName = config.stateClipboardBuffer[0].audioObjectIdentifier;
                                hasName = true;
                            }
                            else
                            {
                                string newCopyName = "None";
                                if (config.stateClipboardBuffer[0].audioObject != null)
                                {
                                    newCopyName = config.stateClipboardBuffer[0].audioObject.name;
                                    hasName = true;
                                }

                                copiedName = newCopyName;
                            }

                        }

                        if (!hasName)
                        {
                            bool hasBlendValue=false;

                            foreach (var item in config.stateClipboardBuffer[0].blendTree)
                            {

                                if (item.useDifferentAudioObject)
                                    hasBlendValue = true;

                                if (hasBlendValue)
                                    break;

                            }

                            if (hasBlendValue)
                            {
                                copiedName = "Blend Motion";
                            }

                        }

                    }

                    GUILayout.Label("Paste SFX: " + copiedName);
                    //  EditorGUILayout.EndVertical();


                    GUI.enabled = prevGUIEnabled;

                    EditorGUILayout.EndHorizontal();

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(addIcon, smallButton))
                    {

                        MLPASAnimatorSFX.StateSFX newSFX = new MLPASAnimatorSFX.StateSFX();

                        (obj.targetObject as MLPASAnimatorSFX).globalAddedSfx += 1;

                        valuesModified = true;

                        Undo.RecordObject(obj.targetObject, "Add New Sfx");

                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Add(newSFX);

                        stateSfxsProp = (obj.targetObject as MLPASAnimatorSFX).stateSfxs;

                        selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count - 1;

                    }

                    GUILayout.Label("Add SFX");



                    GUILayout.EndHorizontal();

                    Color color_default = GUI.backgroundColor;


                    GUIStyle itemStyle = new GUIStyle(GUI.skin.box);  //make a new GUIStyle

                    itemStyle.alignment = TextAnchor.MiddleLeft; //align text to the left
                    itemStyle.active.background = itemStyle.normal.background;  //gets rid of button click background style.
                    itemStyle.margin = new RectOffset(0, 0, 0, 0); //removes the space between items (previously there was a small gap between GUI which made it harder to select a desired item)
                    itemStyle.font = EditorStyles.miniFont;
                    itemStyle.fontSize = 10;
                    itemStyle.fixedWidth = 0;
                    itemStyle.stretchWidth = true;
                    itemStyle.wordWrap = true;
                    itemStyle.richText = true;

                    if (EditorGUIUtility.isProSkin)
                    {
                        itemStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                        itemStyle.hover.textColor = itemStyle.normal.textColor;
                        itemStyle.active.textColor = itemStyle.normal.textColor;
                        itemStyle.focused.textColor = itemStyle.normal.textColor;
                        itemStyle.normal.background = uiBack;
                        itemStyle.hover.background = uiBack;
                        itemStyle.active.background = uiBack;
                        itemStyle.focused.background = uiBack;
                       
                    }


                    if (stateSfxsProp.Count > 0 && selectedIndex > stateSfxsProp.Count - 1 || selectedIndex < 0)
                    {
                        selectedIndex = 0;

                        if (prevSelectedIndex != selectedIndex)
                        {
                            Repaint();
                            prevSelectedIndex = selectedIndex;
                            EditorGUI.FocusTextInControl(null);
                            transitionIndex = 0;
                            parameterIndex = 0;
                        }
                    }

                    if (stateSfxsProp.Count > 0)
                    {

                        int transitionsLength = !(obj.targetObject as MLPASAnimatorSFX).onStateMachine ? (obj.targetObject as MLPASAnimatorSFX).currentState.transitions.Length : (obj.targetObject as MLPASAnimatorSFX).currentStateMachine.anyStateTransitions.Length;

                        for (int i = 0; i < stateSfxsProp.Count; i++)
                        {

                            bool valueMissing = true;

                            if (stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                            {
                                for (int i2 = 0; i2 < (obj.targetObject as MLPASAnimatorSFX).currentAnimatorController.parameters.Length; i2++)
                                {

                                    if ((obj.targetObject as MLPASAnimatorSFX).currentAnimatorController.parameters[i2].name == stateSfxsProp[i].currentParameter.parameterName)
                                    {
                                        valueMissing = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i2 = 0; i2 < transitionsLength; i2++)
                                {

                                    if (!(obj.targetObject as MLPASAnimatorSFX).onStateMachine && (obj.targetObject as MLPASAnimatorSFX).currentState.transitions[i2] == stateSfxsProp[i].currentTransition ||
                                        (obj.targetObject as MLPASAnimatorSFX).onStateMachine && (obj.targetObject as MLPASAnimatorSFX).currentStateMachine.anyStateTransitions[i2] == stateSfxsProp[i].currentTransition)
                                    {
                                        valueMissing = false;
                                        break;
                                    }
                                }
                            }

                            GUILayout.BeginHorizontal();

                            if (GUILayout.Button(removeIcon, smallButton))
                            {

                                Undo.RecordObject(obj.targetObject, "Remove Sfx");

                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Remove(stateSfxsProp[i]);

                                stateSfxsProp = (obj.targetObject as MLPASAnimatorSFX).stateSfxs;

                                selectedIndex = Mathf.Clamp(selectedIndex, 0, (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count - 1);

                                valuesModified = true;

                                return;

                            }

                            // Color font_default = GUI.color;
                            GUI.backgroundColor = (selectedIndex == i) ? color_selected : new Color(1, 1, 1, 0.25f);

                            if (EditorGUIUtility.isProSkin)
                                GUI.backgroundColor = (selectedIndex == i) ? colorPro_selected : new Color(0.25f, 0.25f, 0.25f, 0.25f);
                            //  GUI.color = (selectedIndex == i) ? font_selected : font_default;

                            string extraName = "";

                            if (stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter
                                || stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime
                                || stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionExit)
                            {
                                extraName = " | " + (stateSfxsProp[i].currentTransition != null ? stateSfxsProp[i].currentTransition.GetDisplayName(state) : (transitionsLength > 0 || stateSfxsProp[i].transitionAssigned ? "MISSING" : "NONE"));

                            }

                            if (stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                            {
                                extraName = " | " + (!stateSfxsProp[i].currentParameter.isNull && !valueMissing ? stateSfxsProp[i].currentParameter.parameterName + " [" + stateSfxsProp[i].currentParameter.type.ToString() + "]" : ((obj.targetObject as MLPASAnimatorSFX).currentAnimatorController.parameters.Length > 0 || valueMissing && stateSfxsProp[i].parameterAssigned ? "MISSING" : "NONE"));

                            }

                            string aoName = (stateSfxsProp[i].useIdentifier && !string.IsNullOrEmpty(stateSfxsProp[i].audioObjectIdentifier)? "ID: " + stateSfxsProp[i].audioObjectIdentifier : (stateSfxsProp[i].audioObject != null ? stateSfxsProp[i].audioObject.name : "None"));

                            bool named = false;

                            if (!string.IsNullOrEmpty(stateSfxsProp[i].customName))
                            {
                                aoName = stateSfxsProp[i].customName;
                                named = true;
                            }

                            else if (stateSfxsProp[i].useCustomPlayMethod && !string.IsNullOrEmpty(stateSfxsProp[i].methodName))
                            {
                                aoName = "M: "+stateSfxsProp[i].methodName;
                                named = true;
                            }

                            if (!named && aoName=="None")
                            {
                                bool hasBlendValue = false;

                                foreach (var item in stateSfxsProp[i].blendTree)
                                {

                                    if (item.useDifferentAudioObject)
                                        hasBlendValue = true;

                                    if (hasBlendValue)
                                        break;

                                }

                                if (hasBlendValue)
                                {
                                    aoName = "Blend Motion";
                                }

                            }

                            if (GUILayout.Button(aoName + " <b>[" + stateSfxsProp[i].playMode.ToString() + "]</b>" + (extraName), itemStyle))
                            {
                                selectedIndex = i;

                                if (prevSelectedIndex != selectedIndex)
                                {
                                    Repaint();
                                    prevSelectedIndex = selectedIndex;
                                    EditorGUI.FocusTextInControl(null);
                                    transitionIndex = 0;
                                    parameterIndex = 0;
                                }

                                valuesModified = true;
                            }

                            GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list

                            GUILayout.EndHorizontal();

                        }

                    }




                    GUILayout.EndVertical();

                    EditorGUILayout.Space();


                    if (stateSfxsProp.Count > 0 && selectedIndex >= 0 && (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count > 0 && selectedIndex < (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count)
                    {

                        CustomNameField("Custom Label Name", "Custom Label Name (Only used for visualization)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].customName);

                        EditorGUILayout.Space();

                        //EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        if (BoolField("Use Custom Play Method", "Play this StateSFX using a Custom Play Method", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useCustomPlayMethod))
                        {
                            StringField("Method Name", "Custom Play Method name", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].methodName);

                            /* GUIStyle uIStyle = new GUIStyle(EditorStyles.foldout);
                             RectOffset margin = uIStyle.margin;
                             margin.left = 15;
                             uIStyle.margin = margin;*/

                            showCustomParameters = EditorGUILayout.Foldout(showCustomParameters, new GUIContent("Custom Parameters", "The Custom Optional Parameters for this specific StateSFX"));

                            if (showCustomParameters)
                            {
                                bool boolParameter = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.BoolParameter;
                                float floatParameter = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.FloatParameter;
                                int intParameter = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.IntParameter;
                                string stringParameter = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.StringParameter;
                                UnityEngine.Object objectParameter = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.ObjectParameter;

                                EditorGUI.BeginChangeCheck();
                                boolParameter = EditorGUILayout.Toggle("Bool Parameter", boolParameter);
                                floatParameter = EditorGUILayout.FloatField("Float Parameter", floatParameter);
                                intParameter = EditorGUILayout.IntField("Int Parameter", intParameter);
                                stringParameter = EditorGUILayout.TextField("String Parameter", stringParameter);
                                objectParameter = EditorGUILayout.ObjectField("Object Parameter", objectParameter, typeof(UnityEngine.Object), false) as UnityEngine.Object;
                                if (EditorGUI.EndChangeCheck())
                                {
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters = new MLPASACustomPlayMethodParameters.CustomParams(boolParameter, floatParameter, intParameter, stringParameter, objectParameter);
                                    valuesModified = true;
                                }
                            }

                        }

                        // EditorGUILayout.EndVertical();

                        EditorGUILayout.Space();

                        if (BoolField("Use Identifier", "Use an Audio Object Identifier", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useIdentifier))
                        {
                            StringField("Audio Object Identifier", "Identifier to find the Audio Object to play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].audioObjectIdentifier);
                        }
                        else
                        {
                            AudioObjectField("Audio Object","Audio Object to play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].audioObject);
                        }


                        EditorGUILayout.Space();

                        if (BoolField("Use Channel", "Use a specific Channel for this StateSFX", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useChannel))
                        {
                            IntField("Channel", "The channel on which this StateSFX will play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].channel, 0);
                        }

                        EditorGUILayout.Space();

                        if (!isBlendTree)
                        {

                            //NONE
                        }
                        else
                        {


                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count < blendTree.children.Length)
                            {

                                int toAdd = blendTree.children.Length - (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count;

                                for (int iA = 0; iA < toAdd; iA++)
                                {
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Add(new MLPASAnimatorSFX.StateSFX.BlendMotion());
                                }

                            }

                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count > blendTree.children.Length)
                            {

                                int toRemove = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count - blendTree.children.Length;

                                for (int iA = 0; iA < toRemove; iA++)
                                {
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.RemoveAt((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count - 1);
                                }

                            }

                            bool blendTreeValuesOverride = false;

                            for (int iB = 0; iB < blendTree.children.Length; iB++)
                            {

                                if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].ignoreMotion || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentAudioObject || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentChannel || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentPlayTime)
                                    blendTreeValuesOverride = true;

                            }

                            showBlendtree = EditorGUILayout.Foldout(showBlendtree, "BlendTree" + (blendTreeValuesOverride ? " *" : ""));

                            EditorGUILayout.LabelField("*Blend trees are not longer supported", EditorStyles.miniLabel);

                            if (showBlendtree)
                            {

                                for (int iB = 0; iB < blendTree.children.Length; iB++)
                                {


                                    bool blendTreeValueOverride = false;

                                    if (blendTree.blendType == BlendTreeType.Simple1D && (obj.targetObject as MLPASAnimatorSFX).blend2D)
                                    {
                                        (obj.targetObject as MLPASAnimatorSFX).blend2D = false;
                                        valuesModified = true;
                                    }

                                    if (blendTree.blendType != BlendTreeType.Simple1D && !(obj.targetObject as MLPASAnimatorSFX).blend2D)
                                    {
                                        (obj.targetObject as MLPASAnimatorSFX).blend2D = true;
                                        valuesModified = true;
                                    }

                                    string blendTreeItemName = (!(obj.targetObject as MLPASAnimatorSFX).blend2D ? blendTree.blendParameter + ": " + blendTree.children[iB].threshold.ToString() : blendTree.blendParameter + ": " + blendTree.children[iB].position.x.ToString() + ", " + blendTree.blendParameterY + ": " + blendTree.children[iB].position.y.ToString());

                                    if (blendTree.children[iB].motion as BlendTree != null)
                                    {
                                        blendTreeItemName += " | Child Blends can't have individual parameters";
                                    }

                                    if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].ignoreMotion || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentAudioObject || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentChannel || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentPlayTime)
                                        blendTreeValueOverride = true;

                                    Color prevBackColor = GUI.backgroundColor;


                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                    Font oldFont = itemStyle.font;

                                    if (blendTreeValueOverride)
                                    {
                                        itemStyle.font = EditorStyles.boldFont;
                                        blendTreeItemName += " *";
                                    }

                                    if (GUILayout.Button(blendTreeItemName, EditorStyles.toolbarButton))
                                    {

                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded = !(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded;

                                    }

                                    itemStyle.font = oldFont;

                                    GUI.backgroundColor = prevBackColor;

                                    if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded)
                                    {

                                        BoolField("Don't play SFX in this motion", "Don't play the play the StateSFX if this Motion is playing", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].ignoreMotion);

                                        if (LeftBoolField("Use Different Audio Object for this motion", "Use a Different Audio Object for this Motion", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentAudioObject))
                                        {
                                            if (BoolField("Use Identifier", "Use an Audio Object Identifier", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useIdentifier))
                                            {
                                                StringField("Audio Object Identifier", "Identifier to find the Audio Object to play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].audioObjectIdentifier);
                                            }
                                            else
                                            {
                                                AudioObjectField("Audio Object", "Audio Object to play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].audioObject);
                                            }
                                            EditorGUILayout.Space();
                                        }

                                        if (LeftBoolField("Use Different Channel for this motion", "Use a Different Channel for this Motion", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentChannel))
                                        {

                                            IntField("Channel", "The channel on which this motion will play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].channel, 0);

                                        }

                                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate)
                                        {
                                            if (LeftBoolField("Use Different Play Time for this motion", "Use a Different PlayTime for this Motion", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentPlayTime))
                                            {
                                                AnimationClip clip = blendTree.children[iB].motion as AnimationClip;

                                                if (clip != null)
                                                {
                                                    if (LeftBoolField("Normalized Time", "Defines if the PlayTime is specified in NormalizedTime or Frames", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTimeNormalized))
                                                        NormalizedField("Play Time (%)", "Time on which this BlendTree Motion will play (0-100)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTime, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playFrame, clip.length, clip.frameRate);
                                                    else
                                                    {
                                                        FrameField("Play Time (Frame)", "Frame on which this BlendTree Motion will play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playFrame, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTime, clip.length, clip.frameRate);
                                                    }
                                                }
                                                else
                                                {
                                                    NormalizedField("Play Time (%)", "Time on which this BlendTree Motion will play (0-100)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTime, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playFrame, 1f, 30f);
                                                }

                                            }
                                        }

                                    }

                                    EditorGUILayout.EndVertical();




                                }
                            }

                        }

                        EditorGUILayout.Space();

                        BoolField("Follow Animator Position", "Makes the Audio Object to follow the Animator position when played", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].followAnimator);

                        EditorGUILayout.Space();

                        EnumPopupField("Play Mode", "StateSFX Play Mode", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode, onStateMachineProp.boolValue);
                       /* if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter ||
                            (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime)
                        {
                            EditorGUILayout.LabelField("*Use Play Mode: -OnTransitionExit- for transitions with 0 duration", EditorStyles.miniLabel);
                        }*/

                        EditorGUILayout.Space();

                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter ||
                            (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime ||
                            (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionExit)
                        {


                            AnimatorStateTransition[] currentTransitions = onStateMachineProp.boolValue ? stateMachine.anyStateTransitions : state.transitions;

                            bool transitionDontExist = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].transitionInit;

                            for (int i = 0; i < currentTransitions.Length; i++)
                            {

                                if (currentTransitions[i] == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition)
                                    transitionDontExist = false;
                            }

                            if (transitionDontExist)
                                transitionMissing = true;

                            if (transitionMissing)
                            {
                                EditorGUILayout.HelpBox("Transition: '" + (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastTransitionName + "' was removed", MessageType.Warning);

                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition = null;
                            }



                            if (currentTransitions.Length > 0)
                            {
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].nullTransition = false;
                                transitions.Clear();

                                for (int i = 0; i < currentTransitions.Length; i++)
                                {

                                    transitions.Add(currentTransitions[i]);

                                }


                                if (transitions.Exists(x => x == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition))
                                {
                                    transitionIndex = transitions.FindIndex(x => x == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition);
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition = transitions[transitionIndex];

                                }
                                else
                                {
                                    if (!transitionMissing)
                                    {
                                        transitionIndex = 0;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition = transitions[transitionIndex];
                                    }

                                }

                                if (transitionMissing)
                                    transitionIndex = transitions.Count;

                                if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].transitionInit)
                                {

                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].transitionInit = true;

                                    transitionIndex = 0;
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition = transitions[transitionIndex];
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].transitionAssigned = true;

                                }




                                GUIContent[] transitionNames = new GUIContent[transitionMissing ? transitions.Count + 1 : transitions.Count];


                                for (int i = 0; i < transitionNames.Length; i++)
                                {

                                    if (!transitionMissing)
                                    {
                                        transitionNames[i] = new GUIContent((i).ToString() + " - " + (currentTransitions[i].GetDisplayName(state)));
                                    }
                                    else
                                    {
                                        if (i != transitionNames.Length - 1)
                                            transitionNames[i] = new GUIContent((i).ToString() + " - " + (currentTransitions[i].GetDisplayName(state)));
                                        else
                                            transitionNames[i] = new GUIContent((i).ToString() + " - " + ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastTransitionName + " | REMOVED"));
                                    }
                                }

                                EditorGUI.BeginChangeCheck();
                                transitionIndex = EditorGUILayout.Popup(new GUIContent("Transition", "Transition to Evaluate"), transitionIndex, transitionNames);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (transitionIndex < transitions.Count)
                                    {
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFX), "Current Transition");
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition = transitions[transitionIndex];
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].transitionAssigned = true;
                                        transitionMissing = false;
                                    }

                                    valuesModified = true;

                                }

                                if (!transitionMissing)
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastTransitionName = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition.GetDisplayName(state);

                            }
                            else
                            {

                                EditorGUILayout.LabelField(new GUIContent("Transition", "Transition to Evaluate"), new GUIContent(transitionMissing ? ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastTransitionName + " | REMOVED") : "None"));
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].nullTransition = true;

                            }

                        }

                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                        {

                            AnimatorControllerParameter[] currentParameters = animController.parameters;
                            List<AnimatorControllerParameter> notTriggerParameters = new List<AnimatorControllerParameter>();

                            foreach (var param in currentParameters)
                            {

                                if (param.type != AnimatorControllerParameterType.Trigger)
                                    notTriggerParameters.Add(param);

                            }

                            currentParameters = notTriggerParameters.ToArray();

                            bool parameterDontExist = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterInit;

                            for (int i = 0; i < currentParameters.Length; i++)
                            {

                                if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.isNull && currentParameters[i].name == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.parameterName)
                                    parameterDontExist = false;
                            }

                            if (parameterDontExist)
                                parameterMissing = true;

                            if (parameterMissing)
                            {
                                EditorGUILayout.HelpBox("Parameter: '" + (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterName + " [" + (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType + "]" + "' was removed or renamed.", MessageType.Warning);
                            }

                            if (parameterMissing)
                            {
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.isNull = true;

                                for (int i = 0; i < currentParameters.Length; i++)
                                {

                                    if (currentParameters[i].name == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterName && currentParameters[i].type.ToString() == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType)
                                    {

                                        parameterMissing = false;

                                        parameterIndex = i;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter = new MLPASAnimatorSFX.MLPASAnimatorParameter(currentParameters[parameterIndex]);

                                        Repaint();

                                    }


                                }


                            }


                            if (currentParameters.Length > 0)
                            {
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].nullParameter = false;
                                parameters.Clear();

                                for (int i = 0; i < currentParameters.Length; i++)
                                {

                                    parameters.Add(currentParameters[i]);

                                }

                                if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.isNull && parameters.Exists(x => x.name == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.parameterName))
                                {
                                    parameterIndex = parameters.FindIndex(x => x.name == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.parameterName);
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter = new MLPASAnimatorSFX.MLPASAnimatorParameter(parameters[parameterIndex]);
                                }
                                else
                                {

                                    if (!parameterMissing)
                                    {
                                        parameterIndex = 0;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter = new MLPASAnimatorSFX.MLPASAnimatorParameter(parameters[parameterIndex]);
                                    }


                                }

                                if (parameterMissing)
                                    parameterIndex = parameters.Count;

                                if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterInit)
                                {

                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterInit = true;

                                    parameterIndex = 0;
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter = new MLPASAnimatorSFX.MLPASAnimatorParameter(parameters[parameterIndex]);
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterAssigned = true;
                                }


                                GUIContent[] parameterNames = new GUIContent[!parameterMissing ? parameters.Count : parameters.Count + 1];


                                for (int i = 0; i < parameterNames.Length; i++)
                                {

                                    if (!parameterMissing)
                                    {

                                        parameterNames[i] = new GUIContent((i).ToString() + " - " + (currentParameters[i].name) + " [" + currentParameters[i].type.ToString() + "]");
                                    }
                                    else
                                    {

                                        if (i != parameterNames.Length - 1)
                                            parameterNames[i] = new GUIContent((i).ToString() + " - " + (currentParameters[i].name) + " [" + currentParameters[i].type.ToString() + "]");
                                        else
                                            parameterNames[i] = new GUIContent((i).ToString() + " - " + ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterName + " [" + (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType.ToString() + "]" + " | Missing"));

                                    }

                                }

                                EditorGUI.BeginChangeCheck();
                                parameterIndex = EditorGUILayout.Popup(new GUIContent("Parameter", "Parameter to Evaluate"), parameterIndex, parameterNames);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (parameterIndex < parameters.Count)
                                    {
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFX), "Current Parameter");
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter = new MLPASAnimatorSFX.MLPASAnimatorParameter(parameters[parameterIndex]);
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterAssigned = true;
                                        parameterMissing = false;
                                    }

                                    valuesModified = true;

                                }

                                EditorGUILayout.LabelField("*Triggers are not currently supported", EditorStyles.miniLabel);

                                if (!parameterMissing)
                                {
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterName = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.parameterName;
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameter.type.ToString();
                                }

                            }
                            else
                            {

                                EditorGUILayout.LabelField(new GUIContent("Parameter", "Parameter to Evaluate"), new GUIContent(parameterMissing ? ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterName + " | Missing") : "None"));
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].nullParameter = true;
                            }

                        }



                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime)
                        {

                            EditorGUILayout.Space();

                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate && !onStateMachineProp.boolValue)
                            {

                                AnimationClip clip = state.motion as AnimationClip;

                                if (clip != null)
                                {
                                    if (LeftBoolField("Normalized Time", "Defines if the PlayTime is specified in NormalizedTime or Frames", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTimeNormalized))
                                        NormalizedField("Play Time (%)", "Time on which this StateSFX will play (0-100)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playFrame, clip.length, clip.frameRate);
                                    else
                                    {
                                        FrameField("Play Time (Frame)", "Frame on which this StateSFX will play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playFrame, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime, clip.length, clip.frameRate);
                                    }
                                }
                                else
                                {
                                    NormalizedField("Play Time (%)", "Time on which this StateSFX will play", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime, ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playFrame, 1f, 30f);
                                }

                                EditorGUILayout.BeginVertical();

                                Rect timelineRect = GUILayoutUtility.GetRect(100f, 50f);
                                DrawTimeLine(timelineRect, (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime / 100f);

                                showEventInfo = false;

                                for (int i2 = 0; i2 < (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count; i2++)
                                {

                                    if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i2].playMode != MLPASAnimatorPlayMode.OnUpdate || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[i2] == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex])
                                        continue;

                                    DrawEvent(timelineRect, (obj.targetObject as MLPASAnimatorSFX).stateSfxs[i2], (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex]);

                                }

                                if (isBlendTree)
                                {
                                    for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count; i++)
                                    {
                                       if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[i].useDifferentPlayTime)
                                        {
                                            AnimationClip blendClip = blendTree.children[i].motion as AnimationClip;

                                            int maxFrames = Mathf.RoundToInt(blendClip != null ? blendClip.length * blendClip.frameRate : 1f*30f);

                                            float time = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[i].playTime;

                                            if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[i].playTimeNormalized && blendClip!=null)
                                            {
                                                time = ((float)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[i].playFrame / (float)maxFrames) * 100f;
                                            }

                                            string blendTreeItemName = (!(obj.targetObject as MLPASAnimatorSFX).blend2D ? blendTree.blendParameter + ": " + blendTree.children[i].threshold.ToString() : blendTree.blendParameter + ": " + blendTree.children[i].position.x.ToString() + ", " + blendTree.blendParameterY + ": " + blendTree.children[i].position.y.ToString());


                                            DrawEvent(timelineRect, (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex], (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex], true, time, blendTreeItemName, blendClip, (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[i] );
                                        }
                                    }
                                }

                               if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count>selectedIndex && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex]!=null)
                               {

                                    DrawEvent(timelineRect, (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex], (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex]);

                               }

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.Space();
                                if (BoolField("Play Only Once", "Play this StateSFX only once even if the Animator State Loops", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playOnlyOnce))
                                {
                                    IntField("Play On Loop Cycle", "Play Only on this Loop Cycle (-1 = any Cycle)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playCycle, -1);
                                }

                            }
                            else
                            {
                                FloatField("Play Time (%)", "Time on which this StateSFX will play (0-99)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime, 0f, 99f);
                            }
                        }


                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                        {
                            bool showEvaluation = true;

                            AnimatorControllerParameterType tempEnum = AnimatorControllerParameterType.Bool;

                            GUIContent[] evaluationModes = new GUIContent[] { new GUIContent("On Value Change"), new GUIContent("On Positive Value Change"), new GUIContent("On Negative Value Change"), new GUIContent("On Exact Value") };

                            if (!string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType))
                            {
                                tempEnum = (AnimatorControllerParameterType)System.Enum.Parse(typeof(AnimatorControllerParameterType), (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType);

                                switch (tempEnum)
                                {
                                    case AnimatorControllerParameterType.Float:
                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        break;
                                    case AnimatorControllerParameterType.Bool:
                                        evaluationModes = new GUIContent[] { new GUIContent("On Value Change"), new GUIContent("On Enabled"), new GUIContent("On Disabled") };
                                        break;
                                    case AnimatorControllerParameterType.Trigger:
                                        showEvaluation = false;
                                        break;
                                    default:
                                        break;
                                }

                            }
                            else
                            {
                                showEvaluation = false;
                            }


                            if (showEvaluation)
                            {

                                EditorGUILayout.Space();


                                EditorGUI.BeginChangeCheck();
                                int newvalue = EditorGUILayout.Popup(new GUIContent("Evaluation Mode", "StateSFX Parameter Evaluate Mode"), Mathf.Clamp((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode, 0, evaluationModes.Length - 1), evaluationModes);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(obj.targetObject, "Evaluation Mode");
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode = (MLPASAnimatorSFX.MLPASParameterEvaluateMode)newvalue;
                                    valuesModified = true;
                                }


                                switch (tempEnum)
                                {
                                    case AnimatorControllerParameterType.Float:
                                        if ((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode == 3)
                                        {
                                            FloatField("Value", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramExactValue);
                                        }
                                        FloatField("Threshold", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramThreshold, 0);

                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        if ((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode == 3)
                                        {
                                            FloatField("Value", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramExactValue);
                                        }
                                        IntField("Threshold", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramThreshold, 0);
                                        break;
                                }

                            }

                        }

                        EditorGUILayout.Space();

                        bool showExtraConditions = EditorGUILayout.Foldout((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].showExtraConditions, new GUIContent("Conditions", "Additional Play Conditions"));

                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].showExtraConditions = showExtraConditions;

                        if (showExtraConditions)
                        {

                           

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(addIcon, smallButton))
                            {
                                Undo.RecordObject(obj.targetObject, "Add New Condition");
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions.Add(new MLPASAnimatorCondition());
                                valuesModified = true;

                            }

                            GUILayout.Label("Add Condition");
                            EditorGUILayout.EndHorizontal();

                            for (int ci = 0; ci < (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions.Count; ci++)
                            {

                                EditorGUILayout.BeginHorizontal();

                                if (GUILayout.Button(removeIcon, smallButton))
                                {

                                    Undo.RecordObject(obj.targetObject, "Remove Condition");

                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions.Remove((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci]);

                                    valuesModified = true;

                                    return;

                                }

                                AnimatorControllerParameter[] currentParameters = animController.parameters;
                                List<AnimatorControllerParameter> notTriggerParameters = new List<AnimatorControllerParameter>();

                                foreach (var param in currentParameters)
                                {

                                    if (param.type != AnimatorControllerParameterType.Trigger)
                                        notTriggerParameters.Add(param);

                                }


                                GUIContent[] parameterNames = new GUIContent[notTriggerParameters.Count+1];



                                for (int i = 0; i < parameterNames.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        parameterNames[i] = new GUIContent(string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter) ? "None" : (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter + " ["+ (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType.ToString()+"]");
                                    }
                                    else
                                    {
                                        parameterNames[i] = new GUIContent((i-1).ToString() + " - " + (notTriggerParameters[i-1].name + " [" + notTriggerParameters[i - 1].type.ToString() + "]"));
                                    }
   
                                }

                                EditorGUI.BeginChangeCheck();
                                int prevIndex= (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index;
                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index = EditorGUILayout.Popup((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index, parameterNames);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index > 0)
                                    {
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFX), "Condition Parameter");
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter = notTriggerParameters[(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index-1].name;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType = notTriggerParameters[(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index-1].type;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].isNULL = false;
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index = 0;
                                    }
                                    else
                                    {
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index = prevIndex;
                                    }

                                    valuesModified = true;

                                }

                              

                                bool parameterMissing = !notTriggerParameters.Exists(x => x.name == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter && x.type == (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType);
                                bool parameterNull = string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter) || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].isNULL;

                                if (parameterMissing && !parameterNull)
                                EditorGUILayout.LabelField("PARAMETER IS MISSING", EditorStyles.miniLabel);

                                if(parameterNull && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter!="None")
                                {
                                    (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].index = 0;
                                    valuesModified = true;
                                }


                                if (!parameterMissing && !parameterNull)
                                {

                                    bool isBool = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType == AnimatorControllerParameterType.Bool;

                                    string[] modes = new string[isBool?2:4];

                                    if (isBool)
                                    {
                                        modes = new string[] { "true", "false" };
                                    }
                                    else
                                    {
                                        modes = new string[] { "Greater", "Less", "Equals", "Not Equal" };
                                    }

                                    EditorGUI.BeginChangeCheck();
                                    int newvalue = EditorGUILayout.Popup(Mathf.Clamp((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].mode, 0, modes.Length - 1), modes);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(obj.targetObject, "Condition Evaluation Mode");
    
                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].mode = (MLPASAnimatorConditionMode)newvalue;
                                       
                                        valuesModified = true;
                                    }

                                    if (!isBool)
                                    {

                                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType == AnimatorControllerParameterType.Int)
                                            IntField(ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].threshold, "Condition Threshold");
                                        else
                                            FloatField(ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].threshold, "Condition Threshold");

                                    }

                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.LabelField("*Triggers are not currently supported", EditorStyles.miniLabel);
                        }

                        EditorGUILayout.Space();

                        EditorGUILayout.Space();

                        debugFoldout = EditorGUILayout.Foldout(debugFoldout, !(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].ignoreSfx ? "Extra" : "Extra *");

                        if (debugFoldout)
                        {
                            BoolField("Ignore This SFX", "Ignores This StateSFX | Good for Debugging", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].ignoreSfx);
                            bool prevGUI = GUI.enabled;
                            GUI.enabled = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useCustomPlayMethod;
                            LeftBoolField("Play Only if Custom Method Exists", "When using a Custom Play Method, play only the StateSFX if the receiver of the Custom Play Method is not NULL", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playOnlyWithReceiver);
                            LeftBoolField("Show Custom Method Warnings", "When using a Custom Play Method, shows a warning if the receiver of the Custom Play Method is NULL", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].customMethodWarnings);
                            GUI.enabled = prevGUI;
                        }

                    }

                    

                    if (onStateMachineProp.boolValue && (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count > 0 && (int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode < 3)
                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode = MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter;

                    /* if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                     {
                         string infoParameters = (obj.targetObject as MLPASAnimatorSFX).onStateMachine ? "Parameter will be evaluated in all states of the state machine": "Parameter will be evaluated only in this state";
                         EditorGUILayout.HelpBox(infoParameters, MessageType.Info);
                     }*/


                    //   if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode==MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType == "Trigger")
                    //         EditorGUILayout.HelpBox("Triggers are not Supported", MessageType.Warning);

                    (obj.targetObject as MLPASAnimatorSFX).selectedIndex = selectedIndex;

                }
                else
                {

                    if (onStateMachineProp.boolValue)
                        EditorGUILayout.LabelField("From State Machine", EditorStyles.helpBox);

                    if (stateSfxsProp.Count > 0)
                    {

                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        Color color_default = GUI.backgroundColor;


                        GUIStyle itemStyle = new GUIStyle(GUI.skin.box);  //make a new GUIStyle

                        itemStyle.alignment = TextAnchor.MiddleLeft; //align text to the left
                        itemStyle.active.background = itemStyle.normal.background;  //gets rid of button click background style.
                        itemStyle.margin = new RectOffset(0, 0, 0, 0); //removes the space between items (previously there was a small gap between GUI which made it harder to select a desired item)
                        itemStyle.font = EditorStyles.miniFont;
                        itemStyle.fontSize = 10;
                        itemStyle.fixedWidth = 0;
                        itemStyle.stretchWidth = true;
                        itemStyle.wordWrap = true;
                        itemStyle.richText = true;
                        if (EditorGUIUtility.isProSkin)
                        {
                            itemStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : Color.black;
                            itemStyle.hover.textColor = itemStyle.normal.textColor;
                            itemStyle.active.textColor = itemStyle.normal.textColor;
                            itemStyle.focused.textColor = itemStyle.normal.textColor;
                            itemStyle.normal.background = uiBack;
                            itemStyle.hover.background = uiBack;
                            itemStyle.active.background = uiBack;
                            itemStyle.focused.background = uiBack;
                        }

                        for (int i = 0; i < stateSfxsProp.Count; i++)
                        {

                            GUILayout.BeginHorizontal();

                            //  Color font_default = GUI.contentColor;
                            GUI.backgroundColor = (selectedIndex == i) ? color_selected : new Color(1, 1, 1, 0.25f);
                            if (EditorGUIUtility.isProSkin)
                                GUI.backgroundColor = (selectedIndex == i) ? colorPro_selected : new Color(0.25f, 0.25f, 0.25f, 0.25f);
                            // GUI.contentColor = (selectedIndex == i) ? font_selected : font_default;

                            string extraName = "";

                            if (stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter
                                || stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime
                                || stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionExit)
                            {
                                extraName = " | " + (stateSfxsProp[i].runtimeTransitionName);
                            }

                            if (stateSfxsProp[i].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                            {
                                extraName = " | " + (stateSfxsProp[i].runtimeParameterName);
                            }

                            string aoName = (stateSfxsProp[i].useIdentifier && !string.IsNullOrEmpty(stateSfxsProp[i].audioObjectIdentifier) ? "ID: " + stateSfxsProp[i].audioObjectIdentifier : (stateSfxsProp[i].audioObject != null ? stateSfxsProp[i].audioObject.name : "None"));

                            bool named = false;

                            if (!string.IsNullOrEmpty(stateSfxsProp[i].customName))
                            {
                                aoName = stateSfxsProp[i].customName;
                                named = true;
                            }

                            else if (stateSfxsProp[i].useCustomPlayMethod && !string.IsNullOrEmpty(stateSfxsProp[i].methodName))
                            {
                                aoName = "M: " + stateSfxsProp[i].methodName;
                                named = true;
                            }

                            if (!named && aoName == "None")
                            {
                                bool hasBlendValue = false;

                                foreach (var item in stateSfxsProp[i].blendTree)
                                {

                                    if (item.useDifferentAudioObject)
                                        hasBlendValue = true;

                                    if (hasBlendValue)
                                        break;

                                }

                                if (hasBlendValue)
                                {
                                    aoName = "Blend Motion";
                                }

                            }

                            if (GUILayout.Button(aoName + " <b>[" + stateSfxsProp[i].playMode.ToString() + "]</b>" + (extraName), itemStyle))
                            {
                                selectedIndex = i;

                                if (prevSelectedIndex != selectedIndex)
                                {
                                    Repaint();
                                    prevSelectedIndex = selectedIndex;
                                    EditorGUI.FocusTextInControl(null);
                                }
                            }

                            GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list


                            GUILayout.EndHorizontal();

                        }

                        GUILayout.EndVertical();

                        EditorGUILayout.Space();


                        EditorGUI.BeginDisabledGroup(true);

                        StringField("Custom Label Name", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].customName);

                        EditorGUILayout.Space();

                        if (BoolField("Use Custom Play Method", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useCustomPlayMethod))
                        {
                            StringField("Method Name", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].methodName);

                            showCustomParameters = EditorGUILayout.Foldout(showCustomParameters, "Custom Parameters");

                            if (showCustomParameters)
                            {
                                BoolField("Bool Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.BoolParameter);
                                FloatField("Float Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.FloatParameter);
                                IntField("Int Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.IntParameter);
                                StringField("String Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.StringParameter);
                                ObjectField("Object Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].userParameters.ObjectParameter);
                            }

                        }

                        EditorGUILayout.Space();

                        if (BoolField("Use Identifier", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useIdentifier))
                        {
                            StringField("Audio Object Identifier", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].audioObjectIdentifier);
                        }
                        else
                        {
                            AudioObjectField("Audio Object", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].audioObject);
                        }

                        EditorGUILayout.Space();

                        if (BoolField("Use Channel", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].useChannel))
                        {
                            IntField("Channel", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].channel);
                        }

                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.Space();

                        if ((obj.targetObject as MLPASAnimatorSFX).isBlendTree)
                        {

                            bool prevEnabled = GUI.enabled;
                            GUI.enabled = true;

                            showBlendtree = EditorGUILayout.Foldout(showBlendtree, "BlendTree");

                            EditorGUILayout.LabelField("*Blend trees are not longer supported", EditorStyles.miniLabel);

                            if (showBlendtree)
                            {

                                for (int iB = 0; iB < (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree.Count; iB++)
                                {

                                    string blendTreeItemName = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].runTimeName;

                                    Color prevBackColor = GUI.backgroundColor;


                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                                    if (GUILayout.Button(blendTreeItemName, EditorStyles.toolbarButton))
                                    {

                                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded = !(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded;

                                    }

                                    GUI.backgroundColor = prevBackColor;

                                    GUI.enabled = false;


                                    if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].unfolded)
                                    {

                                        BoolField("Don't play SFX in this motion", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].ignoreMotion);

                                        if (LeftBoolField("Use Different Audio Object for this motion", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentAudioObject))
                                        {
                                            if (BoolField("Use Identifier", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useIdentifier))
                                            {
                                                StringField("Audio Object Identifier", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].audioObjectIdentifier);
                                            }
                                            else
                                            {
                                                AudioObjectField("Audio Object", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].audioObject);
                                            }
                                            EditorGUILayout.Space();
                                        }

                                        if (LeftBoolField("Use Different Channel for this motion", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentChannel))
                                        {

                                            IntField("Channel", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].channel);

                                        }

                                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate)
                                        {
                                            if (LeftBoolField("Use Different Play Time for this motion", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].useDifferentPlayTime))
                                            {
                                                if (LeftBoolField("Normalized Time", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTimeNormalized))
                                                    FloatField("Play Time (%)", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playTime);
                                                else
                                                    IntField("Play Time (Frame)", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].blendTree[iB].playFrame);

                                            }
                                        }

                                    }

                                    EditorGUILayout.EndVertical();


                                    GUI.enabled = prevEnabled;

                                }
                            }

                        }

                        EditorGUI.BeginDisabledGroup(true);

                        EditorGUILayout.Space();

                        BoolField("Follow Animator Position", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].followAnimator);

                        EditorGUILayout.Space();

                        EnumPopupField("Play Mode", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode, onStateMachineProp.boolValue);

                        EditorGUILayout.Space();

                        if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameterRuntime.isNull)
                        {
                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                            {
                                EditorGUILayout.LabelField("Parameter", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentParameterRuntime.parameterName);
                            }

                        }

                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransition != null)
                        {
                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionEnter
                                   || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime
                                   || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionExit)
                            {
                                EditorGUILayout.LabelField("Transition", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].currentTransitionName);
                            }

                        }


                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime)
                        {

                            EditorGUILayout.Space();

                            if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate && !onStateMachineProp.boolValue)
                            {


                                if (LeftBoolField("Normalized Time", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTimeNormalized))
                                    FloatField("Play Time (%)", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime);
                                else
                                    IntField("Play Time (Frame)", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playFrame);


                                EditorGUILayout.Space();
                                if (BoolField("Play Only Once", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playOnlyOnce))
                                {
                                    IntField("Play On Loop Cycle", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playCycle);
                                }
                            }
                            else
                            {
                                FloatField("Play Time (%)", ref (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playTime, 0f, 99f);
                            }
                        }


                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnParameterChange)
                        {
                            bool showEvaluation = true;


                            string[] evaluationModes = new string[] { "On Value Change", "On Positive Value Change", "On Negative Value Change", "On Exact Value" };


                            AnimatorControllerParameterType tempEnum = (AnimatorControllerParameterType)System.Enum.Parse(typeof(AnimatorControllerParameterType), (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].lastParameterType);

                            switch (tempEnum)
                            {
                                case AnimatorControllerParameterType.Float:
                                    break;
                                case AnimatorControllerParameterType.Int:
                                    break;
                                case AnimatorControllerParameterType.Bool:
                                    evaluationModes = new string[] { "On Value Change", "On Enabled", "On Disabled" };
                                    break;
                                case AnimatorControllerParameterType.Trigger:
                                    showEvaluation = false;
                                    break;
                                default:
                                    break;
                            }




                            if (showEvaluation)
                            {
                                EditorGUILayout.Space();

                                (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode = (MLPASAnimatorSFX.MLPASParameterEvaluateMode)EditorGUILayout.Popup("Evaluation Mode", (int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode, evaluationModes);

                                switch (tempEnum)
                                {
                                    case AnimatorControllerParameterType.Float:
                                        if ((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode == 3)
                                        {
                                            FloatField("Value", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramExactValue);
                                        }
                                        FloatField("Threshold", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramThreshold);

                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        if ((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].parameterEvaluateMode == 3)
                                        {
                                            FloatField("Value", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramExactValue);
                                        }
                                        IntField("Threshold", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].paramThreshold);
                                        break;
                                }

                            }


                        }

                        EditorGUILayout.Space();

                        bool showExtraConditions = EditorGUILayout.Foldout((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].showExtraConditions, "Extra Conditions");

                        (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].showExtraConditions = showExtraConditions;

                        if (showExtraConditions)
                        {



                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                            for (int ci = 0; ci < (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions.Count; ci++)
                            {

                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.Popup(0, new string[] { (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter });

                                bool parameterMissing = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].runtimeError;
                                bool parameterNull = string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameter) || (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].isNULL;

                                if (parameterMissing && !parameterNull)
                                    EditorGUILayout.LabelField("RUNTIME ERROR", EditorStyles.miniLabel);

                                if (!parameterMissing && !parameterNull)
                                {

                                    bool isBool = (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType == AnimatorControllerParameterType.Bool;

                                    string[] modes = new string[isBool ? 2 : 4];

                                    if (isBool)
                                    {
                                        modes = new string[] { "true", "false" };
                                    }
                                    else
                                    {
                                        modes = new string[] { "Greater", "Less", "Equals", "Not Equal" };
                                    }

                                   
                                    EditorGUILayout.Popup(Mathf.Clamp((int)(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].mode, 0, modes.Length - 1), modes);
                                   

                                    if (!isBool)
                                    {

                                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].parameterType == AnimatorControllerParameterType.Int)
                                            IntField((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].threshold, "Condition Threshold");
                                        else
                                            FloatField((obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].conditions[ci].threshold, "Condition Threshold");

                                    }

                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.LabelField("*Triggers are not currently supported", EditorStyles.miniLabel);
                        }

                        debugFoldout = EditorGUILayout.Foldout(debugFoldout, !(obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].ignoreSfx ? "Extra" : "Extra *");

                        if (debugFoldout)
                        {
                            BoolField("Ignore This SFX", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].ignoreSfx);

                            LeftBoolField("Play Only if Custom Method Exists", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].playOnlyWithReceiver);
                            LeftBoolField("Show Custom Method Warnings", (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex].customMethodWarnings);
                            
                        }

                        EditorGUI.EndDisabledGroup();

                    }

                }


                if (valuesModified && !dirty)
                {

                    dirty = true;
                    valuesModified = false;

                    EditorUtility.SetDirty((obj.targetObject as MLPASAnimatorSFX));

                    EditorUtility.SetDirty(animController);
                }



                obj.ApplyModifiedProperties();

            }

            bool LeftBoolField(string controlName, ref bool value)
            {
                EditorGUI.BeginChangeCheck();
                bool newvalue = EditorGUILayout.ToggleLeft(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }

                return newvalue;
            }

            bool LeftBoolField(string controlName, string tooltip, ref bool value)
            {
                EditorGUI.BeginChangeCheck();
                bool newvalue = EditorGUILayout.ToggleLeft(new GUIContent(controlName, tooltip), value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }

                return newvalue;
            }


            bool BoolField(string controlName, ref bool value)
            {
                EditorGUI.BeginChangeCheck();
                bool newvalue = EditorGUILayout.Toggle(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }

                return newvalue;
            }

            bool BoolField(string controlName, string tooltip, ref bool value)
            {
                EditorGUI.BeginChangeCheck();
                bool newvalue = EditorGUILayout.Toggle(new GUIContent(controlName,tooltip), value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }

                return newvalue;
            }

            void StringField(string controlName, ref string value)
            {
                EditorGUI.BeginChangeCheck();
                string newvalue = EditorGUILayout.TextField(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void StringField(string controlName, string tooltip, ref string value)
            {
                EditorGUI.BeginChangeCheck();
                string newvalue = EditorGUILayout.TextField(new GUIContent(controlName, tooltip), value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void CustomNameField(string controlName, ref string value)
            {
                EditorGUI.BeginChangeCheck();

                GUIStyle boldText = new GUIStyle(GUI.skin.textField);
                boldText.font = EditorStyles.boldFont;

                string newvalue = EditorGUILayout.TextField(controlName, value, !string.IsNullOrEmpty(value) && value.Contains("Copy") ? boldText : GUI.skin.textField);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void CustomNameField(string controlName, string tooltip, ref string value)
            {
                EditorGUI.BeginChangeCheck();

                GUIStyle boldText = new GUIStyle(GUI.skin.textField);
                boldText.font = EditorStyles.boldFont;

                string newvalue = EditorGUILayout.TextField(new GUIContent(controlName, tooltip), value, !string.IsNullOrEmpty(value) && value.Contains("Copy") ? boldText : GUI.skin.textField);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void FloatField(string controlName, ref float value, float min = float.MinValue, float max = float.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                float newvalue = Mathf.Clamp(EditorGUILayout.FloatField(controlName, Mathf.Clamp(value, min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void FloatField(string controlName, string tooltip, ref float value, float min = float.MinValue, float max = float.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                float newvalue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent(controlName, tooltip), Mathf.Clamp(value, min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void NormalizedField(string controlName, ref float value, ref int frameValue, float length, float frameRate)
            {
                int maxFrames = Mathf.RoundToInt(length * frameRate);
                EditorGUI.BeginChangeCheck();
                float newvalue = Mathf.Clamp(EditorGUILayout.Slider(controlName, value, 0f, 100f), 0f, 100f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    frameValue = Mathf.RoundToInt((newvalue / 100f) * maxFrames);
                    valuesModified = true;
                }
            }

            void NormalizedField(string controlName, string tooltip, ref float value, ref int frameValue, float length, float frameRate)
            {
                int maxFrames = Mathf.RoundToInt(length * frameRate);
                EditorGUI.BeginChangeCheck();
                float newvalue = Mathf.Clamp(EditorGUILayout.Slider(new GUIContent(controlName, tooltip), value, 0f, 100f), 0f, 100f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    frameValue = Mathf.RoundToInt((newvalue / 100f) * maxFrames);
                    valuesModified = true;
                }
            }

            void FrameField(string controlName, ref int frameValue, ref float value, float length, float frameRate)
            {
                int maxFrames = Mathf.RoundToInt(length * frameRate);
                EditorGUI.BeginChangeCheck();

                int newvalue = Mathf.RoundToInt(Mathf.Clamp(EditorGUILayout.IntSlider(controlName, frameValue, 0, maxFrames), 0, maxFrames));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    frameValue = newvalue;
                    value = ((float)frameValue / (float)maxFrames) * 100f;
                    valuesModified = true;
                }
            }

            void FrameField(string controlName, string tooltip, ref int frameValue, ref float value, float length, float frameRate)
            {
                int maxFrames = Mathf.RoundToInt(length * frameRate);
                EditorGUI.BeginChangeCheck();

                int newvalue = Mathf.RoundToInt(Mathf.Clamp(EditorGUILayout.IntSlider(new GUIContent(controlName, tooltip), frameValue, 0, maxFrames), 0, maxFrames));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    frameValue = newvalue;
                    value = ((float)frameValue / (float)maxFrames) * 100f;
                    valuesModified = true;
                }
            }

            void IntField(ref float value, string undoText)
            {
                EditorGUI.BeginChangeCheck();
                int newvalue = EditorGUILayout.IntField((int)value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, undoText);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void FloatField(ref float value, string undoText)
            {
                EditorGUI.BeginChangeCheck();
                float newvalue = EditorGUILayout.FloatField(value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, undoText);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void IntField(float value, string undoText)
            {
            
                EditorGUILayout.IntField((int)value);

            }

            void FloatField(float value, string undoText)
            {
                
               EditorGUILayout.FloatField(value);
               
            }

            void IntField(string controlName, ref int value, int min = int.MinValue, int max = int.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                int newvalue = Mathf.Clamp(EditorGUILayout.IntField(controlName, Mathf.Clamp(value, min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void IntField(string controlName, ref float value, int min = int.MinValue, int max = int.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                int newvalue = Mathf.Clamp(EditorGUILayout.IntField(controlName, Mathf.Clamp(Mathf.RoundToInt(value), min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void IntField(string controlName, string tooltip, ref int value, int min = int.MinValue, int max = int.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                int newvalue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(controlName, tooltip), Mathf.Clamp(value, min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void IntField(string controlName, string tooltip, ref float value, int min = int.MinValue, int max = int.MaxValue)
            {
                EditorGUI.BeginChangeCheck();
                int newvalue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(controlName, tooltip), Mathf.Clamp(Mathf.RoundToInt(value), min, max)), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void AudioObjectField(string controlName, ref AudioObject value)
            {
                EditorGUI.BeginChangeCheck();
                AudioObject newvalue = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(AudioObject), false) as AudioObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void AudioObjectField(string controlName, string tooltip, ref AudioObject value)
            {
                EditorGUI.BeginChangeCheck();
                AudioObject newvalue = EditorGUILayout.ObjectField(new GUIContent(controlName, tooltip), value, typeof(AudioObject), false) as AudioObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void EnumPopupField(string controlName, ref MLPASAnimatorSFX.MLPASAnimatorPlayMode value, bool _onStateMachine = false)
            {


                GUIContent[] playModeStr = new GUIContent[] { new GUIContent("On Start"), new GUIContent("On Update"), new GUIContent("On Exit"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change")};

                if (_onStateMachine)
                {

                    playModeStr = new GUIContent[] { new GUIContent("On Start (ONLY FOR STATES)"), new GUIContent("On Update (ONLY FOR STATES)"), new GUIContent("On Exit (ONLY FOR STATES)"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change") };

                }

                EditorGUI.BeginChangeCheck();
                MLPASAnimatorSFX.MLPASAnimatorPlayMode newvalue = (MLPASAnimatorSFX.MLPASAnimatorPlayMode)EditorGUILayout.Popup(new GUIContent(controlName), (int)value, playModeStr);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void EnumPopupField(string controlName, string tooltip, ref MLPASAnimatorSFX.MLPASAnimatorPlayMode value, bool _onStateMachine = false)
            {


                GUIContent[] playModeStr = new GUIContent[] { new GUIContent("On Start"), new GUIContent("On Update"), new GUIContent("On Exit"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change") };

                if (_onStateMachine)
                {

                    playModeStr = new GUIContent[] { new GUIContent("On Start (ONLY FOR STATES)"), new GUIContent("On Update (ONLY FOR STATES)"), new GUIContent("On Exit (ONLY FOR STATES)"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change") };

                }

                EditorGUI.BeginChangeCheck();
                MLPASAnimatorSFX.MLPASAnimatorPlayMode newvalue = (MLPASAnimatorSFX.MLPASAnimatorPlayMode)EditorGUILayout.Popup(new GUIContent(controlName, tooltip), (int)value, playModeStr);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void EnumPopupField(string controlName, ref MLPASAnimatorSFX.MLPASParameterEvaluateMode value)
            {
                EditorGUI.BeginChangeCheck();
                MLPASAnimatorSFX.MLPASParameterEvaluateMode newvalue = (MLPASAnimatorSFX.MLPASParameterEvaluateMode)EditorGUILayout.EnumPopup(new GUIContent(controlName), value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            void EnumPopupField(string controlName, string tooltip, ref MLPASAnimatorSFX.MLPASParameterEvaluateMode value)
            {
                EditorGUI.BeginChangeCheck();
                MLPASAnimatorSFX.MLPASParameterEvaluateMode newvalue = (MLPASAnimatorSFX.MLPASParameterEvaluateMode)EditorGUILayout.EnumPopup(new GUIContent(controlName, tooltip), value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            bool LeftBoolField(string controlName, bool value)
            {

                EditorGUILayout.ToggleLeft(controlName, value);


                return value;
            }

            bool BoolField(string controlName, bool value)
            {

                EditorGUILayout.Toggle(controlName, value);

                return value;

            }

            void StringField(string controlName, string value)
            {


                EditorGUILayout.TextField(controlName, value);


            }

            void FloatField(string controlName, float value)
            {


                EditorGUILayout.FloatField(controlName, value);


            }

            void FrameField(string controlName, int frameValue, float length, float frameRate)
            {
                int maxFrames = Mathf.RoundToInt(length * frameRate);

                Mathf.RoundToInt(Mathf.Clamp(EditorGUILayout.IntSlider(controlName, frameValue, 0, maxFrames), 0, maxFrames));

            }

            void NormalizedField(string controlName, float value)
            {


                Mathf.Clamp(EditorGUILayout.Slider(controlName, value, 0f, 100f), 0f, 100f);

            }

            void IntField(string controlName, int value)
            {


                EditorGUILayout.IntField(controlName, value);


            }

            void IntField(string controlName, float value)
            {

                EditorGUILayout.IntField(controlName, Mathf.RoundToInt(value));

            }

            void AudioObjectField(string controlName, AudioObject value)
            {


                EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(AudioObject), false);

            }

            void ObjectField(string controlName, UnityEngine.Object value)
            {

                EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(UnityEngine.Object), false);

            }

            void EnumPopupField(string controlName, MLPASAnimatorSFX.MLPASAnimatorPlayMode value, bool _onStateMachine = false)
            {


                GUIContent[] playModeStr = new GUIContent[] { new GUIContent("On Start"), new GUIContent("On Update"), new GUIContent("On Exit"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change") };

                if (_onStateMachine)
                {

                    playModeStr = new GUIContent[] { new GUIContent("On Start (ONLY FOR STATES)"), new GUIContent("On Update (ONLY FOR STATES)"), new GUIContent("On Exit (ONLY FOR STATES)"), new GUIContent("On Transition Enter"), new GUIContent("On Transition Time"), new GUIContent("On Transition Exit"), new GUIContent("On Parameter Change") };

                }


                EditorGUILayout.Popup(new GUIContent(controlName), (int)value, playModeStr);


            }

            void EnumPopupField(string controlName, MLPASAnimatorSFX.MLPASParameterEvaluateMode value)
            {

                EditorGUILayout.EnumPopup(new GUIContent(controlName), value);

            }

            void DrawTimeLine(Rect rect, float currentFrame)
            {
                if (Event.current.type != EventType.Repaint)
                {
                    return;
                }

                HandleUtilityWrapper.handleWireMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : Color.black);
                GL.Vertex3(rect.x, rect.y + rect.height -25, 0);
                GL.Vertex3(rect.x + rect.width, rect.y + rect.height - 25, 0);

                GL.Vertex3(rect.x, rect.y + rect.height, 0);
                GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);


                for (int i = 0; i <= 100; i += 1)
                {
                    if (i % 10 == 0)
                    {
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height-25, 0);
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height-25 + 15, 0);
                    }
                    else if (i % 5 == 0)
                    {
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height - 25, 0);
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height - 25 + 10, 0);
                    }
                    else
                    {
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height - 25, 0);
                        GL.Vertex3(rect.x + rect.width * i / 100f, rect.y + rect.height - 25 + 5, 0);
                    }
                }

               
                GL.Color(Color.red);

                GL.Vertex3(rect.x + rect.width * currentFrame, rect.y + rect.height - 25, 0);
                GL.Vertex3(rect.x + rect.width * currentFrame, rect.y + rect.height - 25 + 20, 0);


                GL.Color(!EditorGUIUtility.isProSkin ? new Color(0.37f,0.37f,0.37f) : Color.gray);

                foreach (var item in (obj.targetObject as MLPASAnimatorSFX).stateSfxs)
                {

                    if (item.playMode== MLPASAnimatorPlayMode.OnUpdate && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[selectedIndex]!=item)
                    {
                        GL.Vertex3(rect.x + rect.width * item.playTime/100, rect.y + rect.height - 25, 0);
                        GL.Vertex3(rect.x + rect.width * item.playTime/100, rect.y + rect.height - 25 + 20, 0);
                    }

                }


                GL.End();

            }

            private int hotEventKey = 0;
            bool showEventInfo = false;
            void DrawEvent(Rect rect, StateSFX state, StateSFX currentState)
            {

                if (state.playMode == MLPASAnimatorPlayMode.OnUpdate)
                {
                    float keyTime = state.playTime/100f;
                    Rect keyRect = new Rect((rect.x + rect.width * keyTime - 3), rect.y+6, 6, 14);

                    int eventKeyCtrl = state.GetHashCode();

                    Event e = Event.current;

                    switch (e.type)
                    {
                        case EventType.Repaint:
                            Color savedColor = GUI.color;

                            if (currentState == state)
                            {
                                GUI.color = Color.cyan;
                            }
                            else
                            {
                                GUI.color = Color.grey;
                            }


                            GUIStyle eventStyle = new GUIStyle(GUI.skin.box);
                            eventStyle.normal.background = uiBack;

                            eventStyle.Draw(keyRect, new GUIContent(""), eventKeyCtrl);

                            GUI.color = savedColor;

                            if (hotEventKey == eventKeyCtrl || (hotEventKey == 0 && keyRect.Contains(e.mousePosition)))
                            {
                                string labelName = !string.IsNullOrEmpty(state.customName) ? state.customName : (state.useIdentifier ? (!string.IsNullOrEmpty(state.audioObjectIdentifier) ? state.audioObjectIdentifier : "None") : (state.audioObject != null ? state.audioObject.name : "None"));

                                if (state.useCustomPlayMethod && string.IsNullOrEmpty(state.customName) && !string.IsNullOrEmpty(state.methodName))
                                {
                                    labelName =  state.methodName;
                                }

                                string labelString = /*(state.playTime).ToString("0.00") + " | " + */labelName;
                                Vector2 size = EditorStyles.largeLabel.CalcSize(new GUIContent(labelString));

                                if (hotEventKey != eventKeyCtrl && !showEventInfo)
                                {
                                    Rect infoRect = new Rect(Mathf.Clamp(rect.x + rect.width * keyTime - size.x / 2f, 0, rect.width - rect.x - size.x + 40f), rect.y + 30, Mathf.Clamp(size.x, 20, float.MaxValue), size.y);
                                    GUIStyle eventTooltipStyle = new GUIStyle(GUI.skin.box);
                                    eventTooltipStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : Color.black;
                                    eventTooltipStyle.normal.background = uiTooltipBack;
                                    eventTooltipStyle.Draw(infoRect, new GUIContent(labelString), eventKeyCtrl);

                                    showEventInfo = true;
                                }

                            }
                            break;

                        case EventType.MouseDown:
                            if (keyRect.Contains(e.mousePosition))
                            {

                                hotEventKey = eventKeyCtrl;

                                selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.FindIndex(x=>x==state);

                                e.Use();
                            }
                            break;

                        case EventType.MouseDrag:
                            if (hotEventKey == eventKeyCtrl)
                            {

                 
                                Vector2 guiPos = e.mousePosition;
                                float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
                                float newTime = ((clampedX - rect.x) / rect.width) * 100f;

                                bool updateValues = false;

                                if (e.button == 0)
                                {

                                    updateValues = true;
                                    selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.FindIndex(x => x == state);
                                }

                                e.Use();


                                if (updateValues)
                                {

                                    AnimationClip clip = this.state.motion as AnimationClip;

                                    int maxFrames = Mathf.RoundToInt(clip!=null?clip.length * clip.frameRate:1f*30f);

                                   
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFX), "Play Time");

                                        state.playTime = (float)System.Math.Round((double)newTime, 2);

                                        state.playFrame = Mathf.RoundToInt((state.playTime / 100f) * maxFrames);

                                        valuesModified = true;
                                    
                                }
                            }
                            break;

                        case EventType.MouseUp:
                            if (hotEventKey == eventKeyCtrl)
                            {

                                hotEventKey = 0;

                                e.Use();
                            }
                            break;

                    }
                }
            }

            void DrawEvent(Rect rect, StateSFX state, StateSFX currentState, bool blendMotion, float blendTime, string blendLabel, AnimationClip blendClip, StateSFX.BlendMotion blend)
            {

                if (state.playMode == MLPASAnimatorPlayMode.OnUpdate)
                {
                    float keyTime = blendTime / 100f;
                    Rect keyRect = new Rect((rect.x + rect.width * keyTime - 3), rect.y + 6, 6, 14);

                    int eventKeyCtrl = state.GetHashCode();

                    Event e = Event.current;

                    switch (e.type)
                    {
                        case EventType.Repaint:
                            Color savedColor = GUI.color;

                            if (currentState == state)
                            {
                                GUI.color = Color.cyan;
                            }
                            else
                            {
                                GUI.color = Color.grey;
                            }
                            
                            if (blendMotion)
                            {
                                GUI.color = Color.yellow;
                            }


                            GUIStyle eventStyle = new GUIStyle(GUI.skin.box);
                            eventStyle.normal.background = uiBack;

                            eventStyle.Draw(keyRect, new GUIContent(""), eventKeyCtrl);

                            GUI.color = savedColor;

                            if (hotEventKey == eventKeyCtrl || (hotEventKey == 0 && keyRect.Contains(e.mousePosition)))
                            {
                                string labelName = blendLabel;

                                string labelString = /*(state.playTime).ToString("0.00") + " | " + */labelName;
                                Vector2 size = EditorStyles.largeLabel.CalcSize(new GUIContent(labelString));

                                if (hotEventKey != eventKeyCtrl && !showEventInfo)
                                {
                                    Rect infoRect = new Rect(Mathf.Clamp(rect.x + rect.width * keyTime - size.x / 2f, 0, rect.width - rect.x - size.x + 40f), rect.y + 30, Mathf.Clamp(size.x, 20, float.MaxValue), size.y);
                                    GUIStyle eventTooltipStyle = new GUIStyle(GUI.skin.box);
                                    eventTooltipStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : Color.black;
                                    eventTooltipStyle.normal.background = uiTooltipBack;
                                    eventTooltipStyle.Draw(infoRect, new GUIContent(labelString), eventKeyCtrl);

                                    showEventInfo = true;
                                }

                            }
                            break;

                        case EventType.MouseDown:
                            if (keyRect.Contains(e.mousePosition))
                            {

                                hotEventKey = eventKeyCtrl;

                                selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.FindIndex(x => x == state);

                                e.Use();
                            }
                            break;

                        case EventType.MouseDrag:
                            if (hotEventKey == eventKeyCtrl)
                            {


                                Vector2 guiPos = e.mousePosition;
                                float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
                                float newTime = ((clampedX - rect.x) / rect.width) * 100f;

                                bool updateValues = false;

                                if (e.button == 0)
                                {

                                    updateValues = true;
                                    selectedIndex = (obj.targetObject as MLPASAnimatorSFX).stateSfxs.FindIndex(x => x == state);
                                }

                                e.Use();


                                if (updateValues)
                                {

                                    AnimationClip clip = blendClip;

                                    int maxFrames = Mathf.RoundToInt(clip != null ? clip.length * clip.frameRate : 1f * 30f);


                                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFX), "Play Time");

                                    blend.playTime = (float)System.Math.Round((double)newTime, 2);

                                    blend.playFrame = Mathf.RoundToInt((blend.playTime / 100f) * maxFrames);

                                    valuesModified = true;

                                }
                            }
                            break;

                        case EventType.MouseUp:
                            if (hotEventKey == eventKeyCtrl)
                            {

                                hotEventKey = 0;

                                e.Use();
                            }
                            break;

                    }
                }
            }

            public static class HandleUtilityWrapper
            {
                private static System.Type realType;
                private static System.Reflection.PropertyInfo s_property_handleWireMaterial;

                private static void InitType()
                {
                    if (realType == null)
                    {
                        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(Editor));
                        realType = assembly.GetType("UnityEditor.HandleUtility");

                        s_property_handleWireMaterial = realType.GetProperty("handleWireMaterial", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    }
                }

                public static Material handleWireMaterial
                {
                    get
                    {
                        InitType();
                        return s_property_handleWireMaterial.GetValue(null, null) as Material;
                    }
                }
            }

            void DuplicateStateSFX(MLPASAnimatorSFX.StateSFX original, ref MLPASAnimatorSFX.StateSFX copy)
            {

                copy.currentParameterRuntime = original.currentParameterRuntime;
                copy.currentTransitionName = original.currentTransitionName;
                copy.runtimeParameterName = original.runtimeParameterName;
                copy.runtimeTransitionName = original.runtimeTransitionName;
                copy.runtimeCurrentStateHash = original.runtimeCurrentStateHash;
                copy.runtimeEndStateHash = original.runtimeEndStateHash;
                copy.runtimeErrorString = original.runtimeErrorString;
                copy.runtimeError = original.runtimeError;

                copy.audioObject = original.audioObject;
                copy.audioObjectIdentifier = original.audioObjectIdentifier;
                copy.channel = original.channel;
                copy.currentParameter = original.currentParameter;
                copy.currentTransition = original.currentTransition;
                copy.customPlayMethod = original.customPlayMethod;
                copy.followAnimator = original.followAnimator;
                copy.lastParameterName = original.lastParameterName;
                copy.lastParameterType = original.lastParameterType;
                copy.lastTransitionName = original.lastTransitionName;
                copy.methodName = original.methodName;
                copy.nullParameter = original.nullParameter;
                copy.nullTransition = original.nullTransition;
                copy.parameterAssigned = original.parameterAssigned;
                copy.parameterEvaluateMode = original.parameterEvaluateMode;
                copy.parameterInit = original.parameterInit;
                copy.paramExactValue = original.paramExactValue;
                copy.paramHash = original.paramHash;
                copy.paramThreshold = original.paramThreshold;
                copy.playFrame = original.playFrame;
                copy.playMode = original.playMode;
                copy.playOnlyOnce = original.playOnlyOnce;
                copy.playTime = original.playTime;
                copy.playTimeNormalized = original.playTimeNormalized;
                copy.transitionAssigned = original.transitionAssigned;
                copy.transitionInit = original.transitionInit;
                copy.useChannel = original.useChannel;
                copy.useCustomPlayMethod = original.useCustomPlayMethod;
                copy.useIdentifier = original.useIdentifier;

                copy.blendTree = new List<MLPASAnimatorSFX.StateSFX.BlendMotion>();
                copy.blendTree.Clear();

                foreach (var item in original.blendTree)
                {
                    MLPASAnimatorSFX.StateSFX.BlendMotion blend = new MLPASAnimatorSFX.StateSFX.BlendMotion();

                    blend.audioObject = item.audioObject;
                    blend.audioObjectIdentifier = item.audioObjectIdentifier;
                    blend.channel = item.channel;
                    blend.playFrame = item.playFrame;
                    blend.playTime = item.playTime;
                    blend.playTimeNormalized = item.playTimeNormalized;
                    blend.position = item.position;
                    blend.threshold = item.threshold;
                    blend.useDifferentAudioObject = item.useDifferentAudioObject;
                    blend.useDifferentChannel = item.useDifferentChannel;
                    blend.useDifferentPlayTime = item.useDifferentPlayTime;
                    blend.useIdentifier = item.useIdentifier;
                    blend.ignoreMotion = item.ignoreMotion;
                   
                    copy.blendTree.Add(blend);
                }

                copy.userParameters = original.userParameters;

                copy.conditions = new List<MLPASAnimatorCondition>();

                foreach (var item in original.conditions)
                {
                    MLPASAnimatorCondition condition = new MLPASAnimatorCondition();

                    condition.index = item.index;
                    condition.isNULL = item.isNULL;
                    condition.mode = item.mode;
                    condition.parameter = item.parameter;
                    condition.parameterHash = item.parameterHash;
                    condition.parameterType = item.parameterType;
                    condition.threshold = item.threshold;

                    copy.conditions.Add(condition);

                }

                copy.showExtraConditions = original.showExtraConditions;

                bool sameName = false;

                for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFX).stateSfxs.Count; i++)
                {

                    if (!string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].customName))
                    {
                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].customName == original.customName)
                        {
                            sameName = true;
                            break;
                        }
                    }
                    else if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].useCustomPlayMethod && !string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].methodName))
                    {
                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].methodName == original.methodName)
                        {
                            sameName = true;
                            break;
                        }
                    }
                    else
                    {

                        if ((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].useIdentifier && !string.IsNullOrEmpty(original.audioObjectIdentifier) && !string.IsNullOrEmpty((obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].audioObjectIdentifier) && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].audioObjectIdentifier == original.audioObjectIdentifier)
                        {
                            sameName = true;
                            break;
                        }

                        if (!(obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].useIdentifier && (obj.targetObject as MLPASAnimatorSFX).stateSfxs[i].audioObject == original.audioObject)
                        {
                            sameName = true;
                            break;
                        }

                    }

                }

                if (sameName)
                {
                    if (!string.IsNullOrEmpty(original.customName))
                    {
                        copy.customName = original.customName + " Copy";
                    }
                    else if (original.useCustomPlayMethod && !string.IsNullOrEmpty(original.methodName))
                    {
                        copy.customName = original.methodName + " Copy";
                    }
                    else
                    {
                        bool valueSet = false;

                        if (original.useIdentifier && !string.IsNullOrEmpty(original.audioObjectIdentifier))
                        {
                            copy.customName = original.audioObjectIdentifier + " Copy";
                            valueSet = true;
                        }
                        else
                        {
                            string newCopyName = "None";
                            if (original.audioObject != null)
                            {
                                newCopyName = original.audioObject.name;
                                valueSet = true;
                            }

                            copy.customName = newCopyName + " Copy";


                            if (!valueSet)
                            {

                                bool hasBlendValue = false;

                                foreach (var item in original.blendTree)
                                {

                                    if (item.useDifferentAudioObject)
                                        hasBlendValue = true;

                                    if (hasBlendValue)
                                        break;

                                }

                                if (hasBlendValue)
                                {
                                    copy.customName = "Blend Motion Copy";
                                }

                            }

                        }
                    }
                }
                else
                {
                    copy.customName = original.customName;
                }



            }

        }
#endif
    }
}