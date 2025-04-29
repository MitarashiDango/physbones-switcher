using UnityEditor;
using UnityEngine;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    [DisallowMultipleComponent, CustomEditor(typeof(Runtime.PhysBonesSwitcher))]
    public class PhysBonesSwitcherEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("本コンポーネントには設定項目はありません", MessageType.Info);

            if (EditorApplication.isPlaying)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
