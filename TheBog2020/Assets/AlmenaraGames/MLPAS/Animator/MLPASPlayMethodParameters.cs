using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AlmenaraGames
{
    /// <summary>
    /// Contains information about the specific StateSFX in the Animator State Invoking the corresponding Custom Play Method.
    /// The values returned will take in consideration any Per-Motion value (BlendTree) and State Behaviour Custom Position change.
    /// </summary>
    [System.Serializable]
    public struct MLPASACustomPlayMethodParameters
    {

        [SerializeField] private AudioObject audioObject;
        [SerializeField] private int channel;
        [SerializeField] private bool followPosition;
        [SerializeField] private Vector3 position;
        [SerializeField] private Transform transform;
        [SerializeField] private int blendTreeMotionIndex;
        [SerializeField] private AnimatorStateInfo animatorStateInfo;
        [SerializeField] private float playTime;

        private CustomParams customParameters;

        public AudioObject AudioObject
        {
            get
            {
                return audioObject;
            }

        }

        public int Channel
        {
            get
            {
                return channel;
            }

        }


        public bool FollowPosition
        {
            get
            {
                return followPosition;
            }

        }

        public Vector3 Position
        {
            get
            {
                return position;
            }

        }

        public Transform Transform
        {
            get
            {
                return transform;
            }

        }

        public int BlendTreeMotionIndex
        {
            get
            {
                return blendTreeMotionIndex;
            }

        }

        public AnimatorStateInfo AnimatorStateInfo
        {
            get
            {
                return animatorStateInfo;
            }

        }

        public float PlayTime
        {
            get
            {
                return playTime;
            }
        }

        /// <summary>
        /// The Custom Optional Parameters for this specific StateSFX.
        /// </summary>
        public CustomParams CustomParameters
        {
            get
            {
                return customParameters;
            }

        }

        public MLPASACustomPlayMethodParameters(AudioObject audioObject, int channel, bool followPosition, Vector3 position, Transform transform, int blendTreeMotionIndex, AnimatorStateInfo animatorStateInfo, float playTime, CustomParams userParameters)
        {

            this.audioObject = audioObject;
            this.channel = channel;
            this.followPosition = followPosition;
            this.position = position;
            this.transform = transform;
            this.blendTreeMotionIndex = blendTreeMotionIndex;
            this.animatorStateInfo = animatorStateInfo;
            this.playTime = playTime;
            this.customParameters = userParameters; 

        }

        /// <summary>
        /// The Custom Optional Parameters for this specific StateSFX.
        /// </summary>
        [System.Serializable]
        public struct CustomParams
        {
            [SerializeField] private bool boolParameter;
            [SerializeField] private float floatParameter;
            [SerializeField] private int intParameter;
            [SerializeField] private string stringParameter;
            [SerializeField] private UnityEngine.Object objectParameter;

            public bool BoolParameter
            {
                get
                {
                    return boolParameter;
                }

            }

            public float FloatParameter
            {
                get
                {
                    return floatParameter;
                }

            }

            public int IntParameter
            {
                get
                {
                    return intParameter;
                }

            }

            public string StringParameter
            {
                get
                {
                    return stringParameter;
                }

            }

            public Object ObjectParameter
            {
                get
                {
                    return objectParameter;
                }

            }

            public CustomParams(bool boolParameter, float floatParameter, int intParameter, string stringParameter, UnityEngine.Object objectParameter)
            {
                this.boolParameter = boolParameter;
                this.floatParameter = floatParameter;
                this.intParameter = intParameter;
                this.stringParameter = stringParameter;
                this.objectParameter = objectParameter;
            }
        }

    }
}
