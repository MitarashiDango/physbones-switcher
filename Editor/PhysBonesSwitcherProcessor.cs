using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class PhysBonesSwitcherProcessor
    {
        private static readonly string LayerNamePbsPhysBonesSwitcher = "PBS_PHYS_BONES_SWITCHER";
        private static readonly string StateNamePhysBonesON = "PhysBones_ON";
        private static readonly string StateNamePhysBonesOFF = "PhysBones_OFF";
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
#if AVATAR_OPTIMIZER
            state.NeedOptimizingPhaseProcessing = true;
#endif

            AddParameters(physBonesSwitcher);
            AddMenuItems(physBonesSwitcher);

            OptimizeVRCPhysBones(ctx);

            var animatorController = GeneratePhysBonesSwitcherAnimatorController();
            animatorController.AddLayer(GeneratePhysBonesSwitcherLayer(ctx));

            var mergeAnimator = physBonesSwitcher.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animatorController;
            mergeAnimator.layerType = AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

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
                events = animationClip1.events,
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

        public void Run(BuildContext ctx)
        {
            var physBonesSwitcher = ctx.AvatarRootObject.GetComponentInChildren<Runtime.PhysBonesSwitcher>();
            if (physBonesSwitcher == null)
            {
                return;
            }

            AddParameters(physBonesSwitcher);
            AddMenuItems(physBonesSwitcher);

            OptimizeVRCPhysBones(ctx);

            var animatorController = GeneratePhysBonesSwitcherAnimatorController();
            animatorController.AddLayer(GeneratePhysBonesSwitcherLayer(ctx));

            var mergeAnimator = physBonesSwitcher.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animatorController;
            mergeAnimator.layerType = AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            Object.DestroyImmediate(physBonesSwitcher);
        }

        private void AddParameters(Runtime.PhysBonesSwitcher physBonesSwitcher)
        {
            var parameters = new PhysBonesSwitcherParameters();
            var modularAvatarParameters = physBonesSwitcher.gameObject.AddComponent<ModularAvatarParameters>();
            modularAvatarParameters.parameters = parameters.GetParameterConfigs();
        }

        private void AddMenuItems(Runtime.PhysBonesSwitcher physBonesSwitcher)
        {
            var modularAvatarMenuItem = physBonesSwitcher.gameObject.AddComponent<ModularAvatarMenuItem>();

            modularAvatarMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            modularAvatarMenuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter
            {
                name = PhysBonesSwitcherParameters.PhysBonesOff,
            };
            modularAvatarMenuItem.Control.value = 1;
        }

        private AnimatorController GeneratePhysBonesSwitcherAnimatorController()
        {
            var animatorController = new AnimatorController
            {
                name = "PHYS_BONES_SWITCHER_ANIMATOR_CONTROLLER",
                parameters = new AnimatorControllerParameter[]{
                    new AnimatorControllerParameter{
                        name = PhysBonesSwitcherParameters.PhysBonesOff,
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

        private AnimatorControllerLayer GeneratePhysBonesSwitcherLayer(BuildContext ctx)
        {
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
            initialState.writeDefaultValues = false;
            initialState.motion = blankAnimationClip;

            var physBonesOnState = layer.stateMachine.AddState(StateNamePhysBonesON, new Vector3(220, 60, 0));
            physBonesOnState.motion = toEnableAnimationClip;

            AnimatorTransitionUtil.AddTransition(initialState, physBonesOnState)
                .IfNot(PhysBonesSwitcherParameters.PhysBonesOff)
                .SetImmediateTransitionSettings();

            var physBonesOffState = layer.stateMachine.AddState(StateNamePhysBonesOFF, new Vector3(-20, 140, 0));
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
    }
}
