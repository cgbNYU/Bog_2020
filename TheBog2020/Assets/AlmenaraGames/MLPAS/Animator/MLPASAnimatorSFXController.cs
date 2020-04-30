using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace AlmenaraGames
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Almenara Games/MLPAS/Animator SFX Controller",10)]
    [HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MLPASAnimatorSFXController")]
    public class MLPASAnimatorSFXController : MonoBehaviour
    {

        public enum ValueToOverride
        {
            UseDifferentPlayPosition,
            PlayPosition,
            FollowPosition
        }

        [System.Serializable]
        public class ValuesOverride
        {

            public string stateName;

            public int layer;

            public bool useDifferentPlayPosition;

            public Transform playPosition;

            public bool followPosition;

        }

        [System.Serializable]
        public class InspectorDelegate
        {

            public Component target;
            public string methodName;
            public bool removed = false;

        }

        public List<InspectorDelegate> inspectorDelegates = new List<InspectorDelegate>();

        public delegate void CustomPlayMethod(MLPASACustomPlayMethodParameters parameters);

        public List<ValuesOverride> newValues = new List<ValuesOverride>();
        Dictionary<string, CustomPlayMethod> customPlayMethods = new Dictionary<string, CustomPlayMethod>();

        List<MLPASAnimatorSFX.StateSFX> states = new List<MLPASAnimatorSFX.StateSFX>();

        List<CustomPlayMethod> registeredPlayMethods=new List<CustomPlayMethod>();

        Animator anim;

        bool inspectorDelegatesAdded = false;


        /// <summary>
        /// Register a Custom Method to be used by the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="method"></param>
        public void RegisterCustomMethod(CustomPlayMethod method)
        {

            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                Debug.LogWarning("<i>'" + methodName + "'</i>" + " Can't be registered, a Custom Play Method with the same name is already registered.");
            }
            else
            {
                customPlayMethods.Add(methodName, method);
            }

            for (int i = 0; i < states.Count; i++)
            {
                MLPASAnimatorSFX.StateSFX state = states[i];

                if (state.useCustomPlayMethod && state.methodName == methodName)
                    states[i].customPlayMethod = method;
            }

            if (!registeredPlayMethods.Contains(method))
                registeredPlayMethods.Add(method);

        }

        void RegisterCustomMethod(CustomPlayMethod method, bool showWarning)
        {

            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                if (showWarning)
                {
                    Debug.LogWarning("<i>'" + methodName + "'</i>" + " Can't be registered, a Custom Play Method with the same name is already registered.");
                }
            }
            else
            {
                customPlayMethods.Add(methodName, method);
            }

            for (int i = 0; i < states.Count; i++)
            {
                MLPASAnimatorSFX.StateSFX state = states[i];

                if (state.useCustomPlayMethod && state.methodName == methodName)
                {
                    states[i].customPlayMethod = method;


                }
            }

        }

        /// <summary>
        /// Unregister a Custom Play Method in this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="method"></param>
        public void UnregisterCustomMethod(CustomPlayMethod method)
        {
            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                customPlayMethods.Remove(methodName);

                for (int i = 0; i < states.Count; i++)
                {
                    MLPASAnimatorSFX.StateSFX state = states[i];

                    if (state.useCustomPlayMethod && state.methodName == methodName)
                        states[i].customPlayMethod = null;
                }
            }
            else
            {
                Debug.LogWarning("Custom Play Method: " + "<i>'" + methodName + "'</i>" + " is already unregisted.");
            }

            if (registeredPlayMethods.Contains(method))
                registeredPlayMethods.Remove(method);

        }

        /// <summary>
        /// Change a Value of The Custom Position For The Specific StateSFX in the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="valueToOverride"></param>
        /// <param name="value">UseDifferentPlayPosition: needs to be a boolean value | PlayPosition: needs to be a Transform value | FollowPosition: needs to be a boolean value</param>.
        public void SetStateBehaviourCustomPosition(string stateName, int layer, ValueToOverride valueToOverride, object value)
        {

            bool iDValidated = false;

            if (newValues.Exists(x => x.stateName == stateName && x.layer == layer))
                iDValidated = true;

            if (!iDValidated)
                return;

            switch (valueToOverride)
            {
                case ValueToOverride.UseDifferentPlayPosition:

                    if (value is bool)
                        newValues.Find(x => x.stateName == stateName && x.layer == layer).useDifferentPlayPosition = (bool)value;
                    else
                        Debug.LogError("<b>UseDifferentPlayPosition</b> needs to be a boolean value");

                    break;
                case ValueToOverride.PlayPosition:

                    if (value is Transform)
                        newValues.Find(x => x.stateName == stateName && x.layer == layer).playPosition = (Transform)value;
                    else
                        Debug.LogError("<b>PlayPosition</b> needs to be a Transform value");

                    break;
                case ValueToOverride.FollowPosition:

                    if (value is bool)
                        newValues.Find(x => x.stateName == stateName && x.layer == layer).followPosition = (bool)value;
                    else
                        Debug.LogError("<b>FollowPosition</b> needs to be a boolean value");

                    break;
            }



        }

        bool ContainsState(AlmenaraGames.MLPASAnimatorSFX state, out ValuesOverride valuesOverride)
        {

            if (newValues.Exists(x => x.stateName == state.runtimeStateName && x.layer==state.transitionLayer))
            {
                valuesOverride = newValues.Find(x => x.stateName == state.runtimeStateName && x.layer == state.transitionLayer);
                return true;
            }

            valuesOverride = null;
            return false;
        }



        bool previousEnabled = false;

        private void Update()
        {
            
            if (previousEnabled!=anim.enabled)
            {
                if (anim.enabled)
                {
                    UpdateValues();
                }
                previousEnabled = anim.enabled;

            }

 

        }

        void OnDisable()
        {
            previousEnabled = false;
        }

        void OnEnable()
        {
          
            if (previousEnabled != anim.enabled)
            {
                if (anim.enabled)
                {
                    UpdateValues();
                }
                previousEnabled = anim.enabled;
            }
        }

        void Awake()
        {
            anim = GetComponent<Animator>();

            if (anim == null)
            {
                Debug.LogError("Component: <b>MLPASAnimatorSFXControlller</b> needs to be placed next to an <i>Animator Controller</i>");
            }
            else
            {
                UpdateValues();
                previousEnabled = anim.enabled;
            }
        }

        void OnDrawGizmosSelected()
        {

            Gizmos.DrawIcon(transform.position, "AlmenaraGames/MLPAS/AnimatorSFXControllerIco");

        }

        void OnDrawGizmos()
        {

           //HIDE

        }

        public void UpdateValues()
        {

            if (anim != null)
            {
                bool n = false;

                states.Clear();

                foreach (var item in anim.GetBehaviours<AlmenaraGames.MLPASAnimatorSFX>())
                {

                    ValuesOverride newValue = null;

                    item.trf = transform;

                    if (ContainsState(item, out newValue))
                    {

                        newValue.stateName = item.runtimeStateName;
                        newValue.layer = item.transitionLayer;

                        item.AssignSFXController(this,newValue);

                    }

                   // if (!statesAdded)
                    
                        states.AddRange(item.stateSfxs);
                    


                    n = true;
                }

                if (!n)
                {
                    Debug.LogWarning("The Animator from Game Object: <b>" + gameObject.name + "</b> doesn't have any <i>MLPASAnimatorSFX</i> State Machine Behaviour");
                }
                else
                {

                    if (!inspectorDelegatesAdded)
                    {
                        foreach (var item in inspectorDelegates)
                        {

                            if (item.target == null || string.IsNullOrEmpty(item.methodName))
                            {
                                continue;
                            }

                            System.Reflection.MethodInfo[] methods = item.target.GetType().GetMethods();
                            System.Reflection.MethodInfo correctMethod = null;

                            for (int i = 0; i < methods.Length; i++)
                            {

                                bool validMethod = false;
                                System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                                for (int i2 = 0; i2 < parameters.Length; i2++)
                                {

                                    if (item.methodName == methods[i].Name && parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                    {
                                        correctMethod = methods[i];
                                        validMethod = true;
                                        break;
                                    }


                                }

                                if (validMethod)
                                    break;

                            }

                            CustomPlayMethod action = (CustomPlayMethod)System.Delegate.CreateDelegate(typeof(CustomPlayMethod), item.target, correctMethod);

                            if (correctMethod != null && action != null)
                            {
                                registeredPlayMethods.Add(action);
                                RegisterCustomMethod(action, false);
                            }


                        }
                        inspectorDelegatesAdded = true;
                    }
                    

                    foreach (var m in registeredPlayMethods)
                    {
                        RegisterCustomMethod(m, false);
                    }
                }
            }


        }




#if UNITY_EDITOR
        [CustomEditor(typeof(MLPASAnimatorSFXController))]
        public class MLPASAnimatorSFXControllerEditor : Editor
        {

            SerializedObject obj;

            Color color_selected = new Color32(98, 220, 255, 255);
            Color colorPro_selected = new Color32(28, 128, 170, 255);

            private Texture2D uiBack;

            bool valuesModified = false;
            bool dirty = false;

            int selectedIndexStateM = 0;
            int prevSelectedIndexStateM = 0;

            bool animatorValidated = false;

            GameObject customPlayMethodObj;
            int playMethodIndex;

            public UnityEditor.Animations.AnimatorController anim;

            void CheckMethods()
            {
                if (!EditorApplication.isPlaying && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates!=null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count>0)
                {

                    foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates)
                    {

                        System.Reflection.MethodInfo[] methods = item.target.GetType().GetMethods();
                        System.Reflection.MethodInfo correctMethod = null;

                        for (int i = 0; i < methods.Length; i++)
                        {

                            bool validMethod = false;
                            System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                            for (int i2 = 0; i2 < parameters.Length; i2++)
                            {

                                if (item.methodName == methods[i].Name && parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                {
                                    correctMethod = methods[i];
                                    validMethod = true;
                                    break;
                                }


                            }

                            if (validMethod)
                                break;

                        }

                        if (correctMethod == null || item.target == null || string.IsNullOrEmpty(item.methodName))
                        {
                            item.removed = true;
                        }
                        else
                        {
                            item.removed = false;
                        }
                    }

                    SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                }

            }

            void OnDisable()
            {

                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && obj!=null && obj.targetObject!=null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates != null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count > 0)
                {
                    for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count; i++)
                    {
                        if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].removed)
                        {
                            (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.RemoveAt(i);

                        }
                    }

                    SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                }

            }

            void OnEnable()
            {
                uiBack = Resources.Load("MLPASImages/guiBack") as Texture2D;
               
                obj = new SerializedObject(target);



                CheckMethods();

            }

            bool ContainsStateEditor(AlmenaraGames.MLPASAnimatorSFX state, out ValuesOverride valuesOverride)
            {
                
                UnityEditor.Animations.StateMachineBehaviourContext[] context = UnityEditor.Animations.AnimatorController.FindStateMachineBehaviourContext(state);
                UnityEditor.Animations.AnimatorState cState = (context[0].animatorObject as UnityEditor.Animations.AnimatorState);
                UnityEditor.Animations.AnimatorStateMachine cStateMachine = (context[0].animatorObject as UnityEditor.Animations.AnimatorStateMachine);

                string stateName = cState != null ? cState.name : cStateMachine.name;
                int layer = context[0].layerIndex;

                if ((obj.targetObject as MLPASAnimatorSFXController).newValues.Exists(x => x.stateName == stateName && x.layer == layer))
                {
                    valuesOverride = (obj.targetObject as MLPASAnimatorSFXController).newValues.Find(x => x.stateName == stateName && x.layer == layer);
                    return true;
                }

                valuesOverride = null;
                return false;
            }

            public override void OnInspectorGUI()
            {
              
                obj.Update();

                animatorValidated = false;

                Animator animComponent = (obj.targetObject as MLPASAnimatorSFXController).GetComponent<Animator>();

                if (animComponent != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(animComponent.runtimeAnimatorController);

                    UnityEditor.Animations.AnimatorController newAnim = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);

                    if (anim != newAnim) {
                        anim = newAnim;
                    }
                }

                if (anim != null)
                    animatorValidated = true;

                if (!animatorValidated)
                {

                    EditorGUILayout.HelpBox("This Component needs to be placed next to an Animator Component", MessageType.Error);
                    return;

                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                List<MLPASAnimatorSFX> validatedAnimatorSfxes = new List<MLPASAnimatorSFX>();

                foreach (var i in anim.GetBehaviours<MLPASAnimatorSFX>())
                {
                    if (!validatedAnimatorSfxes.Contains(i))
                 validatedAnimatorSfxes.Add(i);
                }

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
                itemStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                itemStyle.hover.textColor = itemStyle.normal.textColor;
                itemStyle.active.textColor = itemStyle.normal.textColor;
                itemStyle.focused.textColor = itemStyle.normal.textColor;
                itemStyle.normal.background = uiBack;
                itemStyle.hover.background = uiBack;
                itemStyle.active.background = uiBack;
                itemStyle.focused.background = uiBack;

                if (validatedAnimatorSfxes.Count > 0)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    for (int i = 0; i < validatedAnimatorSfxes.Count; i++)
                    {

                        // Color font_default = GUI.color;
                        GUI.backgroundColor = (selectedIndexStateM == i) ? color_selected : new Color(1, 1, 1, 0.25f);
                        if (EditorGUIUtility.isProSkin)
                        GUI.backgroundColor = (selectedIndexStateM == i) ? colorPro_selected : new Color(0.25f, 0.25f, 0.25f, 0.25f);
                        //  GUI.color = (selectedIndex == i) ? font_selected : font_default;

                        string layerName = "";

                        for (int iL = 0; iL < anim.layers.Length; iL++)
                        {
                            if (iL == validatedAnimatorSfxes[i].transitionLayer)
                            layerName = anim.layers[iL].name;
                        }

                        string buttonName = "L: " + layerName + " | " + (validatedAnimatorSfxes[i].currentState != null ? "S: " + validatedAnimatorSfxes[i].currentState.name : "SM: " + validatedAnimatorSfxes[i].currentStateMachine.name);


                        if (GUILayout.Button(buttonName, itemStyle))
                        {
                            selectedIndexStateM = i;

                            if (prevSelectedIndexStateM != selectedIndexStateM)
                            {
                                Repaint();
                                prevSelectedIndexStateM = selectedIndexStateM;
                                EditorGUI.FocusTextInControl(null);
                            }

                            valuesModified = true;
                        }

                        GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list


                    }



                    GUILayout.EndVertical();

                    if (EditorApplication.isPlaying)
                        GUI.enabled = false;



                    EditorGUILayout.Space();

                    MLPASAnimatorSFXController.ValuesOverride newValue = null;

                    if (!ContainsStateEditor(validatedAnimatorSfxes[selectedIndexStateM], out newValue))
                    {
                        newValue = new MLPASAnimatorSFXController.ValuesOverride();

                        UnityEditor.Animations.StateMachineBehaviourContext[] context = UnityEditor.Animations.AnimatorController.FindStateMachineBehaviourContext(validatedAnimatorSfxes[selectedIndexStateM]);
                        UnityEditor.Animations.AnimatorState cState = (context[0].animatorObject as UnityEditor.Animations.AnimatorState);
                        UnityEditor.Animations.AnimatorStateMachine cStateMachine = (context[0].animatorObject as UnityEditor.Animations.AnimatorStateMachine);

                        string stateName = cState != null ? cState.name : cStateMachine.name;
                        int layer = context[0].layerIndex;

                        newValue.layer = layer;
                        newValue.stateName = stateName;
                        (obj.targetObject as MLPASAnimatorSFXController).newValues.Add(newValue);
                    }


                    BoolField("Use Different Play Position", ref newValue.useDifferentPlayPosition);

                    if (newValue.useDifferentPlayPosition)
                    {

                        TransformField("Play Position Transform", ref newValue.playPosition);
                        BoolField("Follow Play Position", ref newValue.followPosition);

                    }


                    GUILayout.EndVertical();

                    if (!EditorApplication.isPlaying)
                    {

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Custom Play Methods", EditorStyles.boldLabel);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        customPlayMethodObj = EditorGUILayout.ObjectField(new GUIContent("Target GameObject", ""), customPlayMethodObj, typeof(GameObject), true) as GameObject;
                        if (customPlayMethodObj!=null && EditorUtility.IsPersistent(customPlayMethodObj))
                        {
                            customPlayMethodObj = null;
                        }
                        List<MLPASAnimatorSFXController.InspectorDelegate> inspectorDelegates = new List<MLPASAnimatorSFXController.InspectorDelegate>();



                        if (customPlayMethodObj != null)
                        {

                            Component[] components = customPlayMethodObj.GetComponents<Component>();

                            foreach (var item in components)
                            {

                                System.Reflection.MethodInfo[] methods = item.GetType().GetMethods();


                                for (int i = 0; i < methods.Length; i++)
                                {
                                    System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                                    for (int i2 = 0; i2 < parameters.Length; i2++)
                                    {

                                        if (parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                        {
                                            MLPASAnimatorSFXController.InspectorDelegate del = new MLPASAnimatorSFXController.InspectorDelegate();
                                            del.methodName = methods[i].Name;
                                            del.target = item;
                                            inspectorDelegates.Add(del);
                                            break;
                                        }


                                    }
                                }

                            }


                            string[] methodNames = new string[inspectorDelegates.Count];


                            for (int i = 0; i < inspectorDelegates.Count; i++)
                            {
                                methodNames[i] = i.ToString()+" - " + inspectorDelegates[i].methodName + " (MLPASACustomPlayMethodParameters)";

                            }

                            if (inspectorDelegates.Count > 0)
                            {

                                playMethodIndex = EditorGUILayout.Popup(playMethodIndex, methodNames);

                                bool alreadyExists = false;

                                foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates)
                                {

                                    if (item.methodName == inspectorDelegates[playMethodIndex].methodName)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                bool prevGuiEnabled = GUI.enabled;
                                GUI.enabled = !alreadyExists;
                                Color prevBackground = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                                if (GUILayout.Button(alreadyExists ? inspectorDelegates[playMethodIndex].methodName + " Already Exists" : "Add Custom Play Method", EditorStyles.miniButton))
                                {
                                    if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates == null)
                                        (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates = new List<MLPASAnimatorSFXController.InspectorDelegate>();

                                    (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Add(inspectorDelegates[playMethodIndex]);

                                    valuesModified = true;
                                }
                                GUI.backgroundColor = prevBackground;
                                GUI.enabled = prevGuiEnabled;
                            }
                            else
                            {
                                playMethodIndex = 0;

                                EditorGUILayout.LabelField("No methods found", EditorStyles.miniLabel);
                                bool prevGuiEnabled = GUI.enabled;
                                GUI.enabled = false;
                                Color prevBackground = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                                GUILayout.Button("Select Another GameObject", EditorStyles.miniButton);
                                GUI.backgroundColor = prevBackground;
                                GUI.enabled = prevGuiEnabled;


                            }


                        }
                        else
                        {
                            bool prevGuiEnabled = GUI.enabled;
                            GUI.enabled = false;
                            Color prevBackground = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                            GUILayout.Button("Select a GameObject", EditorStyles.miniButton);
                            GUI.backgroundColor = prevBackground;
                            GUI.enabled = prevGuiEnabled;
                        }

                        EditorGUILayout.EndVertical();

                        for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count; i++)
                        {

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            bool prevGuiEnabled = GUI.enabled;
                            GUI.enabled = false;
                            if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].target == null)
                            {
                                EditorGUILayout.ObjectField("Target GameObject", null, typeof(GameObject), true);
                            }
                            else
                            {
                                EditorGUILayout.ObjectField("Target GameObject", (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].target.gameObject, typeof(GameObject), true);
                            }

                            if (!(obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].removed)
                                EditorGUILayout.Popup(0, new string[] { (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].methodName + " (MLPASACustomPlayMethodParameters)" });
                            else
                                EditorGUILayout.Popup(0, new string[] { (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].methodName + " | MISSING" });

                            GUI.enabled = prevGuiEnabled;

                            Color prevBackground = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(1f, 0.35f, 0.38f);
                            if (GUILayout.Button("Remove Play Method", EditorStyles.miniButton))
                            {
                                (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.RemoveAt(i);
                                valuesModified = true;
                            }
                            GUI.backgroundColor = prevBackground;

                            EditorGUILayout.EndVertical();
                        }



                    }


                    if (valuesModified && !dirty)
                    {

                        dirty = true;
                        valuesModified = false;
                        if (!EditorApplication.isPlaying)
                        {

                            SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("The 'Animator' Next to this 'MLPASAnimatorSFXController' doesn't have any 'MLPASAnimatorSFX' State Machine Behaviour", MessageType.Warning);
                    GUILayout.EndVertical();

                    return;
                }



                GUI.enabled = true;


                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.Space();

                    bool nullMethods = true;
                    EditorGUILayout.LabelField("Registered Custom Play Methods", EditorStyles.boldLabel);

                    foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).customPlayMethods)
                    {
                        if (item.Value != null)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUI.enabled = false;
                            Component comp = ((item.Value.Target) as Component);
                            if (comp != null)
                                nullMethods = false;

                            EditorGUILayout.ObjectField("Target GameObject", comp != null ? comp.gameObject : null, typeof(GameObject), true);

                            if (comp != null)
                            {
                                EditorGUILayout.Popup(0, new string[] { item.Value.Method.Name + " (MLPASACustomPlayMethodParameters)" });
                            }
                            else
                            {
                                EditorGUILayout.Popup(0, new string[] { item.Value.Method.Name + " | MISSING" });
                            }
                            GUI.enabled = true;
                            EditorGUILayout.EndVertical();
                        }
                    }

                    if (nullMethods)
                    {
                        EditorGUILayout.LabelField("No methods found");
                    }
                }

                obj.ApplyModifiedProperties();

            }

            void GameObjectField(string controlName, ref GameObject value)
            {
                EditorGUI.BeginChangeCheck();
                GameObject newvalue = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(GameObject), false) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            public void SetObjectDirty(Component comp)
            {
                EditorUtility.SetDirty(comp);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(comp.gameObject.scene); //This used to happen automatically from SetDirty
            }

            bool BoolField(string controlName, ref bool value)
            {
                EditorGUI.BeginChangeCheck();
                bool newBool = EditorGUILayout.Toggle(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newBool;
                    valuesModified = true;
                }
                return value;

            }

            void TransformField(string controlName, ref Transform value)
            {

                EditorGUI.BeginChangeCheck();
                Transform newTransform = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(Transform), true) as Transform;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newTransform;
                    valuesModified = true;
                }
            }


        }
#endif
    }
}