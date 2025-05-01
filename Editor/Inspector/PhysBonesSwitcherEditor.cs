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
                headerTitle = "操作対象外オブジェクト情報",
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
