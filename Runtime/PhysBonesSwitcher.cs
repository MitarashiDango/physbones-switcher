using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace MitarashiDango.PhysBonesSwitcher.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PhysBones Switcher/PhysBones Switcher")]
    [HelpURL("https://github.com/MitarashiDango/physbones-switcher/blob/master/docs~/components/PhysBonesSwitcher.md")]
    public class PhysBonesSwitcher : MonoBehaviour, IEditorOnly
    {
        /// <summary>
        /// 制御対象外オブジェクト設定
        /// </summary>
        public List<ExcludeObjectSetting> excludeObjectSettings = new List<ExcludeObjectSetting>();

        /// <summary>
        /// カスタム遅延時間（秒）
        /// </summary>
        public int customDelayTime = 0;

        /// <summary>
        /// PhysBone無効化時の効果音
        /// </summary>
        public AudioClip physBoneOffAudioClip = null;

        /// <summary>
        /// Write Defaults 設定
        /// </summary>
        public WriteDefaultsMode writeDefaultsMode = WriteDefaultsMode.MatchAvatarWriteDefaults;

        /// <summary>
        /// PhysBoneの自動分離処理を無効化する
        /// </summary>
        public bool disablePhysBonesSeparation = false;
    }
}
