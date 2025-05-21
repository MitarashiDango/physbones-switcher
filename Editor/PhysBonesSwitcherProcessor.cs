using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherProcessor
    {
        private static readonly string LayerNamePbsPhysBoneOffParamController = "PBS_PHYS_BONES_OFF_PARAM_CONTROLLER";
        private static readonly string LayerNamePbsPhysBonesSwitcher = "PBS_PHYS_BONES_SWITCHER";
        private static readonly string LayerNamePbsPhysBoneDisableSound = "PBS_PHYS_BONES_DISABLE_SOUND";
        private static readonly string StateNamePhysBonesON = "PhysBones_ON";
        private static readonly string StateNamePhysBonesOFF = "PhysBones_OFF";
        private static readonly string StateNamePhysBoneDisableSoundON = "PhysBoneDisableSound_ON";
        private static readonly string StateNamePhysBoneDisableSoundOFF = "PhysBoneDisableSound_OFF";
        private static readonly string EditorOnly = "EditorOnly";

        public void GeneratingProcess(BuildContext ctx)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();

            var physBonesSwitcher = ctx.AvatarRootObject.GetComponentInChildren<Runtime.PhysBonesSwitcher>();
            if (physBonesSwitcher == null)
            {
                return;
            }

            state.excludeObjectSettings = physBonesSwitcher.excludeObjectSettings;
            state.customDelayTime = physBonesSwitcher.customDelayTime;
            state.physBoneOffAudioClip = physBonesSwitcher.physBoneOffAudioClip;
            state.writeDefaultsMode = physBonesSwitcher.writeDefaultsMode;
#if AVATAR_OPTIMIZER
            state.NeedOptimizingPhaseProcessing = true;
#endif

            var physBonesSwitcherGameObject = CreatePhysBonesSwitcherGameObject(ctx);
            var physBoneOffAudioSourceGameObject = AddPhysBoneDisableAudioSource(ctx, physBonesSwitcherGameObject);

            AddParameters(physBonesSwitcher);
            AddMenuItems(ctx, physBonesSwitcher);

            OptimizeVRCPhysBones(ctx);

            var animatorController = GeneratePhysBonesSwitcherAnimatorController();
            animatorController.AddLayer(GeneratePhysBoneOffParamControllerLayer(ctx, physBonesSwitcherGameObject));
            animatorController.AddLayer(GeneratePhysBonesSwitcherLayer(ctx));
            if (physBoneOffAudioSourceGameObject != null)
            {
                animatorController.AddLayer(GeneratePhysBoneDisableSoundLayer(ctx, physBoneOffAudioSourceGameObject));
            }

            var mergeAnimator = physBonesSwitcher.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animatorController;
            mergeAnimator.layerType = AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = physBonesSwitcher.writeDefaultsMode == Runtime.WriteDefaultsMode.MatchAvatarWriteDefaults;

            Object.DestroyImmediate(physBonesSwitcher);
        }

        public void OptimizingProcess(BuildContext ctx)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();
            if (!state.NeedOptimizingPhaseProcessing)
            {
                return;
            }

#if AVATAR_OPTIMIZER
            var (blankAnimationClip, toEnableAnimationClip, toDisableAnimationClip) = GenerateAnimationClips(ctx);

            var baseAnimationLayers = ctx.AvatarDescriptor.baseAnimationLayers;
            foreach (var customAnimationLayer in baseAnimationLayers)
            {
                if (customAnimationLayer.type != AnimLayerType.FX)
                {
                    continue;
                }

                if (customAnimationLayer.animatorController is AnimatorController ac)
                {
                    ac.layers = ac.layers.ToList()
                    .Select((layer, index) =>
                    {
                        var stateMachine = layer.stateMachine;
                        if (layer.name == LayerNamePbsPhysBonesSwitcher)
                        {
                            foreach (var childAnimationState in stateMachine.states)
                            {
                                if (childAnimationState.state.name == StateNamePhysBonesON)
                                {
                                    if (childAnimationState.state.motion is AnimationClip animationClip)
                                    {
                                        childAnimationState.state.motion = MergeAnimationClip(animationClip, toEnableAnimationClip);
                                    }
                                }
                                else if (childAnimationState.state.name == StateNamePhysBonesOFF)
                                {
                                    if (childAnimationState.state.motion is AnimationClip animationClip)
                                    {
                                        childAnimationState.state.motion = MergeAnimationClip(animationClip, toDisableAnimationClip);
                                    }
                                }
                            }
                        }

                        return new AnimatorControllerLayer()
                        {
                            name = layer.name,
                            defaultWeight = layer.defaultWeight,
                            avatarMask = layer.avatarMask,
                            blendingMode = layer.blendingMode,
                            iKPass = layer.iKPass,
                            stateMachine = stateMachine,
                            syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,
                            syncedLayerIndex = layer.syncedLayerIndex,
                        };
                    }).ToArray();
                }
            }
#endif
        }

        private AnimationClip MergeAnimationClip(AnimationClip animationClip1, AnimationClip animationClip2)
        {
            var mergedAnimationClip = new AnimationClip()
            {
                frameRate = animationClip1.frameRate,
                hideFlags = animationClip1.hideFlags,
                legacy = animationClip1.legacy,
                localBounds = animationClip1.localBounds,
                name = animationClip1.name,
                wrapMode = animationClip1.wrapMode,
            };

            CopyCurves(animationClip1, mergedAnimationClip);
            CopyCurves(animationClip2, mergedAnimationClip);

            CopyEvents(animationClip1, mergedAnimationClip);
            CopyEvents(animationClip2, mergedAnimationClip);

            return mergedAnimationClip;
        }

        private void CopyCurves(AnimationClip sourceClip, AnimationClip destinationClip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

            foreach (var binding in bindings)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                if (curve != null)
                {
                    AnimationUtility.SetEditorCurve(destinationClip, binding, curve);
                }
                else
                {
                    ObjectReferenceKeyframe[] objectReferenceKeyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                    if (objectReferenceKeyframes != null)
                    {
                        AnimationUtility.SetObjectReferenceCurve(destinationClip, binding, objectReferenceKeyframes);
                    }
                }
            }
        }

        private static void CopyEvents(AnimationClip sourceClip, AnimationClip destinationClip)
        {
            AnimationEvent[] sourceEvents = AnimationUtility.GetAnimationEvents(sourceClip);

            if (sourceEvents.Length > 0)
            {
                List<AnimationEvent> existingEvents = AnimationUtility.GetAnimationEvents(destinationClip).ToList();
                existingEvents.AddRange(sourceEvents);
                AnimationUtility.SetAnimationEvents(destinationClip, existingEvents.ToArray());
            }
        }

        private void AddParameters(Runtime.PhysBonesSwitcher physBonesSwitcher)
        {
            var parameters = new PhysBonesSwitcherParameters();
            var modularAvatarParameters = physBonesSwitcher.gameObject.AddComponent<ModularAvatarParameters>();
            modularAvatarParameters.parameters = parameters.GetParameterConfigs();
        }

        private void AddMenuItems(BuildContext ctx, Runtime.PhysBonesSwitcher physBonesSwitcher)
        {
            var menuItem = physBonesSwitcher.gameObject.AddComponent<ModularAvatarMenuItem>();

            menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            menuItem.MenuSource = SubmenuSource.Children;

            AddTogglePhysBonesMenuItem(physBonesSwitcher.gameObject);
            AddDelaySettingsSubMenuItem(ctx, physBonesSwitcher.gameObject);
        }

        private GameObject AddTogglePhysBonesMenuItem(GameObject parentObject)
        {
            var go = new GameObject("PhysBones OFF");

            var menuItem = go.AddComponent<ModularAvatarMenuItem>();
            menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            menuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter
            {
                name = PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn,
            };
            menuItem.Control.value = 1;

            go.transform.SetParent(parentObject.transform);

            return go;
        }

        private GameObject AddDelaySettingsSubMenuItem(BuildContext ctx, GameObject parentObject)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();
            var delaySettingsGameObject = new GameObject("Delay Settings");

            var delaySettingsMenuItem = delaySettingsGameObject.AddComponent<ModularAvatarMenuItem>();
            delaySettingsMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            delaySettingsMenuItem.MenuSource = SubmenuSource.Children;

            delaySettingsGameObject.transform.SetParent(parentObject.transform);

            var choices = GetDelayOptionChoices(state.customDelayTime);

            // 遅延設定用メニュー項目生成 (即時モード)
            var immediateMenuGameObject = new GameObject("Immediate");
            var immediateMenuItem = immediateMenuGameObject.AddComponent<ModularAvatarMenuItem>();
            immediateMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            immediateMenuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter
            {
                name = PhysBonesSwitcherParameters.DelayType,
            };
            immediateMenuItem.Control.value = 0;
            immediateMenuGameObject.transform.SetParent(delaySettingsGameObject.transform);

            // 遅延設定用メニュー項目生成 (遅延モード)
            foreach (var (choice, index) in choices.Select((choice, index) => (choice, index)))
            {
                var toggleMenuGameObject = new GameObject($"{choice} sec");
                var toggleMenuItem = toggleMenuGameObject.AddComponent<ModularAvatarMenuItem>();
                toggleMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                toggleMenuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = PhysBonesSwitcherParameters.DelayType,
                };
                toggleMenuItem.Control.value = index + 1;

                toggleMenuGameObject.transform.SetParent(delaySettingsGameObject.transform);
            }

            return delaySettingsGameObject;
        }

        private List<int> GetDelayOptionChoices(int customDelayTime)
        {
            var choices = new List<int> { 1, 3, 5 };
            if (customDelayTime > 0 && !choices.Exists(c => c == customDelayTime))
            {
                choices.Add(customDelayTime);
            }

            choices.Sort((a, b) => a - b);

            return choices;
        }

        private AnimatorController GeneratePhysBonesSwitcherAnimatorController()
        {
            var animatorController = new AnimatorController
            {
                name = "PHYS_BONES_SWITCHER_ANIMATOR_CONTROLLER",
                parameters = new AnimatorControllerParameter[]{
                    new AnimatorControllerParameter{
                        name = PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = false,
                    },
                    new AnimatorControllerParameter{
                        name = PhysBonesSwitcherParameters.PhysBonesOff,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = false,
                    },
                    new AnimatorControllerParameter{
                        name = PhysBonesSwitcherParameters.DelayType,
                        type = AnimatorControllerParameterType.Int,
                        defaultInt = 0,
                    },
                    new AnimatorControllerParameter{
                        name = VRCParameters.IS_LOCAL,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = false,
                    },
                },
            };

            if (animatorController.layers.Length == 0)
            {
                animatorController.AddLayer("DUMMY_LAYER");
            }

            return animatorController;
        }

        private List<GameObject> FilterExcludeObjects(BuildContext ctx, List<GameObject> sourceObjects)
        {
            if (sourceObjects.Count == 0)
            {
                return sourceObjects;
            }

            var state = ctx.GetState<PhysBonesSwitcherState>();
            var excludeObjectSettings = state.excludeObjectSettings.Where(eos => eos != null && eos.excludeObject != null).ToList();

            // タグが "EditorOnly" であるオブジェクトを除外
            var filterdObjects = sourceObjects.Where(obj => obj.tag != EditorOnly);

            // 直接指定されているオブジェクトを除外
            filterdObjects = sourceObjects.Where(
                obj => !excludeObjectSettings.Exists(eos => eos.excludeObject == obj));

            // 子オブジェクトも除外対象とするオブジェクトの子オブジェクトを除外
            filterdObjects = filterdObjects.Where(
                obj => !excludeObjectSettings.Where(eos => eos.withChildren)
                    .Select(eos => eos.excludeObject)
                    .ToList()
                    .Exists(src => obj.transform.IsChildOf(src.transform)));

            return filterdObjects.ToList();
        }

        private void OptimizeVRCPhysBones(BuildContext ctx)
        {
            var gameObjects = FilterExcludeObjects(ctx, GetGameObjectsWithVRCPhysBone(ctx.AvatarRootObject));
            if (gameObjects.Count == 0)
            {
                return;
            }

            foreach (var go in gameObjects)
            {
                var physBones = go.GetComponents<VRCPhysBone>();
                if (physBones.Length <= 1)
                {
                    continue;
                }

                var attachedComponentsCount = go.GetComponents<Component>().Where(c => c.GetType() != typeof(Transform)).Count();

                if (physBones.Length == attachedComponentsCount)
                {
                    // VRC PhysBone だけアタッチされているゲームオブジェクトの場合、何もしない
                    continue;
                }

                // VRC PhysBone 以外のコンポーネントもアタッチされているゲームオブジェクトの場合、新たにゲームオブジェクトを作成し、VRC PhysBone だけ新規作成したオブジェクトへ移動する
                var physBonesCount = 1;
                foreach (var srcPhysBone in physBones)
                {
                    var physBonesGameObject = new GameObject($"$$PhysBones_{physBonesCount++}");
                    physBonesGameObject.transform.SetParent(go.transform);

                    var type = typeof(VRCPhysBone);
                    var destPhysBone = physBonesGameObject.AddComponent<VRCPhysBone>();

                    // VRC PhysBone の各種設定値をコピーする
                    foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.IsDefined(typeof(System.NonSerializedAttribute), true))
                        {
                            continue;
                        }

                        field.SetValue(destPhysBone, field.GetValue(srcPhysBone));
                    }

                    foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (!property.CanWrite || !property.CanRead || property.Name == "name")
                        {
                            continue;
                        }

                        property.SetValue(destPhysBone, property.GetValue(srcPhysBone, null), null);
                    }

                    if (srcPhysBone.rootTransform == null)
                    {
                        destPhysBone.rootTransform = srcPhysBone.transform;
                    }
                }

                // コピー元の VRC PhysBone は消す
                foreach (var p in physBones)
                {
                    Object.DestroyImmediate(p);
                }
            }
        }

        private List<GameObject> GetGameObjectsWithVRCPhysBone(GameObject avatarRootObject)
        {
            var vrcPhysBones = avatarRootObject.GetComponentsInChildren<VRCPhysBone>();
            return vrcPhysBones
                .Select(vrcPhysBone => vrcPhysBone.gameObject)
                .Distinct()
                .ToList();
        }

        private GameObject CreatePhysBonesSwitcherGameObject(BuildContext ctx)
        {
            var go = new GameObject("$$PhysBonesSwitcher");
            go.transform.SetParent(ctx.AvatarRootTransform);
            return go;
        }

        private GameObject AddPhysBoneDisableAudioSource(BuildContext ctx, GameObject parentObject)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();
            if (state.physBoneOffAudioClip == null)
            {
                return null;
            }

            var go = new GameObject("$$PhysBoneOffAudioSource");
            go.SetActive(false);

            go.transform.SetParent(parentObject.transform);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = true;
            audioSource.clip = state.physBoneOffAudioClip;
            audioSource.dopplerLevel = 0;
            audioSource.spread = 0;
            audioSource.volume = 1;

            var vrcSpatialAudioSource = go.AddComponent<VRCSpatialAudioSource>();
            vrcSpatialAudioSource.Gain = 10;
            vrcSpatialAudioSource.Far = 40;
            vrcSpatialAudioSource.Near = 0;
            vrcSpatialAudioSource.VolumetricRadius = 0;
            vrcSpatialAudioSource.EnableSpatialization = false;

            return go;
        }

        private (AnimationClip, AnimationClip, AnimationClip) GenerateAnimationClips(BuildContext ctx)
        {
            var vrcPhysBones = ctx.AvatarRootObject.GetComponentsInChildren<VRCPhysBone>();

            var blankAnimationClip = new AnimationClip
            {
                name = "blank"
            };

            var toEnableAnimationClip = new AnimationClip
            {
                name = StateNamePhysBonesON,
                frameRate = 60
            };

            var toDisableAnimationClip = new AnimationClip
            {
                name = StateNamePhysBonesOFF,
                frameRate = 60
            };

            var physBonesAttachedObjects = vrcPhysBones
                .Select(vrcPhysBone => vrcPhysBone.gameObject)
                .Distinct();

            physBonesAttachedObjects = FilterExcludeObjects(ctx, physBonesAttachedObjects.ToList());

            foreach (var physBonesAttachedObject in physBonesAttachedObjects)
            {
                var path = MiscUtil.GetPathInHierarchy(physBonesAttachedObject.transform, ctx.AvatarRootObject.transform);

                var toEnableCurve = new AnimationCurve();
                toEnableCurve.AddKey(0, 1);

                var toDisableCurve = new AnimationCurve();
                toDisableCurve.AddKey(0, 0);

                toEnableAnimationClip.SetCurve(path, typeof(VRCPhysBone), "m_Enabled", toEnableCurve);
                toDisableAnimationClip.SetCurve(path, typeof(VRCPhysBone), "m_Enabled", toDisableCurve);
            }

            return (blankAnimationClip, toEnableAnimationClip, toDisableAnimationClip);
        }

        private AnimatorControllerLayer GeneratePhysBoneOffParamControllerLayer(BuildContext ctx, GameObject physBonesSwitcherGameObject)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();

            var layer = new AnimatorControllerLayer
            {
                name = LayerNamePbsPhysBoneOffParamController,
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine(),
            };

            layer.stateMachine.entryPosition = new Vector3(0, 0, 0);
            layer.stateMachine.exitPosition = new Vector3(0, -40, 0);
            layer.stateMachine.anyStatePosition = new Vector3(0, -80, 0);

            var blankAnimationClip = new AnimationClip
            {
                name = "blank"
            };

            var initialState = layer.stateMachine.AddState("Initial State", new Vector3(-20, 60, 0));
            initialState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            initialState.motion = blankAnimationClip;

            var setPhysBoneOnState = layer.stateMachine.AddState("Set PhysBone ON", new Vector3(220, 60, 0));
            setPhysBoneOnState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            setPhysBoneOnState.motion = blankAnimationClip;
            setPhysBoneOnState.behaviours = new StateMachineBehaviour[]
            {
                GenerateVRCAvatarParameterLocalSetDriver(PhysBonesSwitcherParameters.PhysBonesOff, 0)
            };

            AnimatorTransitionUtil.AddTransition(initialState, setPhysBoneOnState)
                .If(VRCParameters.IS_LOCAL)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(initialState, setPhysBoneOnState)
                .If(VRCParameters.IS_LOCAL)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            var setPhysBoneOffState = layer.stateMachine.AddState("Set PhysBone OFF", new Vector3(-20, 140, 0));
            setPhysBoneOffState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            setPhysBoneOffState.motion = blankAnimationClip;
            setPhysBoneOffState.behaviours = new StateMachineBehaviour[]
            {
                GenerateVRCAvatarParameterLocalSetDriver(PhysBonesSwitcherParameters.PhysBonesOff, 1)
            };

            AnimatorTransitionUtil.AddTransition(initialState, setPhysBoneOffState)
                .If(VRCParameters.IS_LOCAL)
                .If(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(initialState, setPhysBoneOffState)
                .If(VRCParameters.IS_LOCAL)
                .If(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(setPhysBoneOnState, setPhysBoneOffState)
                .If(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .Equals(PhysBonesSwitcherParameters.DelayType, 0)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(setPhysBoneOffState, setPhysBoneOnState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .SetImmediateTransitionSettings();

            var sleepAnimationClip = new AnimationClip
            {
                frameRate = 60, // 60fps
            };
            var dummyBinding = EditorCurveBinding.FloatCurve(physBonesSwitcherGameObject.name, typeof(Transform), "m_LocalPosition.x");
            var dummyCurve = AnimationCurve.Constant(0, 1, 0); // 値0を1秒間維持
            AnimationUtility.SetEditorCurve(sleepAnimationClip, dummyBinding, dummyCurve);

            var sleepState = layer.stateMachine.AddState("Sleep", new Vector3(220, 140, 0));
            sleepState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            sleepState.motion = sleepAnimationClip;

            AnimatorTransitionUtil.AddTransition(setPhysBoneOnState, sleepState)
                .If(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .NotEqual(PhysBonesSwitcherParameters.DelayType, 0)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(sleepState, setPhysBoneOffState)
                .If(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .Equals(PhysBonesSwitcherParameters.DelayType, 0)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(sleepState, setPhysBoneOnState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                .SetImmediateTransitionSettings();

            var choices = GetDelayOptionChoices(state.customDelayTime);
            foreach (var (choice, index) in choices.Select((c, i) => (c, i)))
            {
                AnimatorTransitionUtil.AddTransition(sleepState, setPhysBoneOffState)
                    .If(PhysBonesSwitcherParameters.PhysBonesOffMenuItemOn)
                    .Equals(PhysBonesSwitcherParameters.DelayType, index + 1)
                    .Exec((builder) =>
                    {
                        var transition = builder.Transition;
                        transition.hasExitTime = true;
                        transition.exitTime = choice;
                        transition.hasFixedDuration = true;
                        transition.duration = 0;
                        transition.offset = 0;
                        transition.interruptionSource = TransitionInterruptionSource.None;
                        transition.orderedInterruption = true;
                    });
            }

            return layer;
        }

        private AnimatorControllerLayer GeneratePhysBonesSwitcherLayer(BuildContext ctx)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();

            var layer = new AnimatorControllerLayer
            {
                name = LayerNamePbsPhysBonesSwitcher,
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine(),
            };

            layer.stateMachine.entryPosition = new Vector3(0, 0, 0);
            layer.stateMachine.exitPosition = new Vector3(0, -40, 0);
            layer.stateMachine.anyStatePosition = new Vector3(0, -80, 0);

            var (blankAnimationClip, toEnableAnimationClip, toDisableAnimationClip) = GenerateAnimationClips(ctx);

            var initialState = layer.stateMachine.AddState("Initial State", new Vector3(-20, 60, 0));
            initialState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            initialState.motion = blankAnimationClip;

            var physBonesOnState = layer.stateMachine.AddState(StateNamePhysBonesON, new Vector3(220, 60, 0));
            physBonesOnState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            physBonesOnState.motion = toEnableAnimationClip;

            AnimatorTransitionUtil.AddTransition(initialState, physBonesOnState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            var physBonesOffState = layer.stateMachine.AddState(StateNamePhysBonesOFF, new Vector3(-20, 140, 0));
            physBonesOffState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            physBonesOffState.motion = toDisableAnimationClip;

            AnimatorTransitionUtil.AddTransition(initialState, physBonesOffState)
                .If(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(physBonesOnState, physBonesOffState)
                .If(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(physBonesOffState, physBonesOnState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            return layer;
        }

        private AnimatorControllerLayer GeneratePhysBoneDisableSoundLayer(BuildContext ctx, GameObject physBoneOffAudioSourceGameObject)
        {
            var state = ctx.GetState<PhysBonesSwitcherState>();
            if (state.physBoneOffAudioClip == null)
            {
                return null;
            }

            var layer = new AnimatorControllerLayer
            {
                name = LayerNamePbsPhysBoneDisableSound,
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine(),
            };

            layer.stateMachine.entryPosition = new Vector3(0, 0, 0);
            layer.stateMachine.exitPosition = new Vector3(0, -40, 0);
            layer.stateMachine.anyStatePosition = new Vector3(0, -80, 0);

            var blankAnimationClip = new AnimationClip
            {
                name = "blank"
            };

            var toEnableAnimationClip = new AnimationClip
            {
                name = StateNamePhysBoneDisableSoundON,
                frameRate = 60
            };

            var toDisableAnimationClip = new AnimationClip
            {
                name = StateNamePhysBoneDisableSoundOFF,
                frameRate = 60
            };

            var path = MiscUtil.GetPathInHierarchy(physBoneOffAudioSourceGameObject.transform, ctx.AvatarRootObject.transform);

            var toEnableCurve = new AnimationCurve();
            toEnableCurve.AddKey(0, 1);

            var toDisableCurve = new AnimationCurve();
            toDisableCurve.AddKey(0, 0);

            toEnableAnimationClip.SetCurve(path, typeof(GameObject), "m_IsActive", toEnableCurve);
            toDisableAnimationClip.SetCurve(path, typeof(GameObject), "m_IsActive", toDisableCurve);

            var initialState = layer.stateMachine.AddState("Initial State", new Vector3(-20, 60, 0));
            initialState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            initialState.motion = blankAnimationClip;

            var physBoneDisableSoundOnState = layer.stateMachine.AddState(StateNamePhysBoneDisableSoundON, new Vector3(220, 60, 0));
            physBoneDisableSoundOnState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            physBoneDisableSoundOnState.motion = toEnableAnimationClip;

            AnimatorTransitionUtil.AddTransition(initialState, physBoneDisableSoundOnState)
                .If(VRCParameters.IS_LOCAL)
                .If(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            var physBoneDisableSoundOffState = layer.stateMachine.AddState(StateNamePhysBoneDisableSoundOFF, new Vector3(-20, 140, 0));
            physBoneDisableSoundOffState.writeDefaultValues = state.writeDefaultsMode == Runtime.WriteDefaultsMode.WriteDefaultsOn;
            physBoneDisableSoundOffState.motion = toDisableAnimationClip;

            AnimatorTransitionUtil.AddTransition(initialState, physBoneDisableSoundOffState)
                .If(VRCParameters.IS_LOCAL)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(physBoneDisableSoundOnState, physBoneDisableSoundOffState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            AnimatorTransitionUtil.AddTransition(physBoneDisableSoundOffState, physBoneDisableSoundOnState)
                .If(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            return layer;
        }

        private VRCAvatarParameterDriver GenerateVRCAvatarParameterLocalSetDriver(string parameterName, float value)
        {
            var vrcAvatarParameterDriver = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            vrcAvatarParameterDriver.localOnly = true;
            vrcAvatarParameterDriver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter
                {
                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                    name = parameterName,
                    value = value,
                }
            };

            return vrcAvatarParameterDriver;
        }
    }
}
