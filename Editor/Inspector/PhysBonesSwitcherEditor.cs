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
        private static string _mainUxmlGuid = "4f335a6a01861b7489b303a0f04f963b";

        private VisualElement _physBoneOffAudioClipValidationErrorElement;
        private SerializedProperty _physBoneOffAudioClipProperty;

        private void OnEnable()
        {
            _physBoneOffAudioClipProperty = serializedObject.FindProperty("physBoneOffAudioClip");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var mainUxmlAsset = MiscUtil.LoadVisualTreeAsset(_mainUxmlGuid);
            if (mainUxmlAsset == null)
            {
                Debug.LogError($"Cannot load UXML file: {_mainUxmlGuid}");
                return null;
            }

            var root = mainUxmlAsset.CloneTree();

            var physBoneOffAudioClip = root.Q<PropertyField>("physbone-off-audio-clip-property-field");
            physBoneOffAudioClip.RegisterValueChangeCallback(OnPhysBoneOffAudioClipPropertyFieldChanged);

            _physBoneOffAudioClipValidationErrorElement = root.Q<VisualElement>("physbone-off-audioclip-validation-error-element");

            var _fixPhysBoneOffAudioClipButton = root.Q<Button>("fix-physbone-off-audioclip-validation-button");
            _fixPhysBoneOffAudioClipButton.RegisterCallback<ClickEvent>(OnFixPhysBoneOffAudioClipButtonClick);

            var excludeObjectSettingsListView = root.Q<ListView>("exclude-object-settings-listview");
            excludeObjectSettingsListView.makeItem = MakeExcludeObjectSettingsListViewItem;

            LanguagePrefs.ApplyFontPreferences(root);

            ValidatePhysBoneOffAudioClip();

            return root;
        }

        private VisualElement MakeExcludeObjectSettingsListViewItem()
        {
            var container = new BindableElement
            {
                style = { flexDirection = FlexDirection.Column }
            };

            var objectField = new ObjectField
            {
                bindingPath = "excludeObject",
                style = { flexGrow = 1 }
            };
            container.Add(objectField);

            var toggle = new Toggle
            {
                bindingPath = "withChildren",
                text = "子オブジェクトも対象とする",
                style = { width = 180 }
            };
            container.Add(toggle);

            return container;
        }

        private void ValidatePhysBoneOffAudioClip()
        {
            _physBoneOffAudioClipValidationErrorElement.style.display = DisplayStyle.None;

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
                _physBoneOffAudioClipValidationErrorElement.style.display = DisplayStyle.Flex;
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
