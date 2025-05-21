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
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            LanguagePrefs.ApplyFontPreferences(root);

            root.Add(new PropertyField
            {
                bindingPath = "physBoneOffAudioClip",
                label = "PhysBone 無効化時の効果音"
            });

            root.Add(new HelpBox("効果音として設定する AudioClip は Load In Background を true に設定する必要があります", HelpBoxMessageType.Info));

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
    }
}
