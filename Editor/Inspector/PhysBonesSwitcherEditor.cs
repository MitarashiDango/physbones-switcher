using nadena.dev.ndmf.localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    [DisallowMultipleComponent, CustomEditor(typeof(Runtime.PhysBonesSwitcher))]
    public class PhysBonesSwitcherEditor : UnityEditor.Editor
    {
        private VisualElement _physBoneOffAudioClipLoadInBackgroundValidationErrorElement;
        private SerializedProperty _physBoneOffAudioClipProperty;

        private void OnEnable()
        {
            _physBoneOffAudioClipProperty = serializedObject.FindProperty("physBoneOffAudioClip");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var physBoneOffAudioClip = new PropertyField
            {
                bindingPath = "physBoneOffAudioClip",
                label = "PhysBone 無効化時の効果音",
            };
            physBoneOffAudioClip.RegisterValueChangeCallback(OnPhysBoneOffAudioClipPropertyFieldChanged);

            root.Add(physBoneOffAudioClip);

            _physBoneOffAudioClipLoadInBackgroundValidationErrorElement = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            var _physBoneOffAudioClipValidationErrorHelpBox = new HelpBox
            {
                text = "Load Type が Decompress On Load である AudioClip は Load In Background を true に設定する必要があります",
                messageType = HelpBoxMessageType.Error,
            };

            _physBoneOffAudioClipLoadInBackgroundValidationErrorElement.Add(_physBoneOffAudioClipValidationErrorHelpBox);

            var _fixPhysBoneOffAudioClipButton = new Button
            {
                text = "修復する",
            };

            _fixPhysBoneOffAudioClipButton.RegisterCallback<ClickEvent>(OnFixPhysBoneOffAudioClipButtonClick);

            _physBoneOffAudioClipLoadInBackgroundValidationErrorElement.Add(_fixPhysBoneOffAudioClipButton);

            root.Add(_physBoneOffAudioClipLoadInBackgroundValidationErrorElement);

            root.Add(new PropertyField
            {
                bindingPath = "customDelayTime",
                label = "カスタム遅延時間(秒)"
            });

            root.Add(new EnumField
            {
                bindingPath = "writeDefaultsMode",
                label = "Write Defaults 設定",
                style = { flexGrow = 1 },
            });

            root.Add(CreateExcludeObjectSettingsListView());

            LanguagePrefs.ApplyFontPreferences(root);

            ValidatePhysBoneOffAudioClip();

            return root;
        }

        private ListView CreateExcludeObjectSettingsListView()
        {
            return new ListView
            {
                bindingPath = "excludeObjectSettings",
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showFoldoutHeader = true,
                reorderable = true,
                headerTitle = "操作対象外オブジェクト",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = () =>
                {
                    var container = new BindableElement { style = { flexDirection = FlexDirection.Column } };

                    var objectField = new ObjectField
                    {
                        name = "ExcludeObject",
                        bindingPath = "excludeObject",
                        style = { flexGrow = 1 }
                    };
                    container.Add(objectField);

                    var toggle = new Toggle
                    {
                        name = "WithChildren",
                        bindingPath = "withChildren",
                        text = "子オブジェクトも対象とする",
                        style = { width = 180 }
                    };
                    container.Add(toggle);

                    return container;
                }
            };
        }

        private void ValidatePhysBoneOffAudioClip()
        {
            _physBoneOffAudioClipLoadInBackgroundValidationErrorElement.style.display = DisplayStyle.None;

            var audioClip = _physBoneOffAudioClipProperty.objectReferenceValue as AudioClip;
            if (audioClip == null)
            {
                return;
            }

            if (audioClip == null)
            {
                return;
            }

            if (audioClip.loadType == AudioClipLoadType.DecompressOnLoad && !audioClip.loadInBackground)
            {
                _physBoneOffAudioClipLoadInBackgroundValidationErrorElement.style.display = DisplayStyle.Flex;
                Debug.LogError("AudioClips with a Load Type of 'Decompress On Load' must have 'Load In Background' set to true.");
            }
        }

        private bool FixPhysBoneOffAudioClip()
        {
            var audioClip = _physBoneOffAudioClipProperty.objectReferenceValue as AudioClip;
            if (audioClip == null)
            {
                Debug.LogError("The AudioClip passed as an argument is null.");
                return false;
            }

            var path = AssetDatabase.GetAssetPath(audioClip);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"Could not get asset path. The AudioClip '{audioClip.name}' may not be a project asset.");
                return false;
            }

            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
            {
                Debug.LogError($"Failed to get AudioImporter. Path: {path}");
                return false;
            }

            if (importer.loadInBackground)
            {
                Debug.Log($"'{audioClip.name}' already has 'Load In Background' enabled.");
                return true;
            }

            importer.loadInBackground = true;
            importer.SaveAndReimport();

            Debug.Log($"Successfully enabled 'Load In Background' for AudioClip '{audioClip.name}'.");
            return true;
        }

        private void OnFixPhysBoneOffAudioClipButtonClick(ClickEvent evt)
        {
            FixPhysBoneOffAudioClip();
            ValidatePhysBoneOffAudioClip();
        }

        private void OnPhysBoneOffAudioClipPropertyFieldChanged(SerializedPropertyChangeEvent evt)
        {
            ValidatePhysBoneOffAudioClip();
        }
    }
}
