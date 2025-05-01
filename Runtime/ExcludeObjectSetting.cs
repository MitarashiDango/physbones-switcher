using System;
using UnityEngine;

namespace MitarashiDango.PhysBonesSwitcher.Runtime
{
    [Serializable]
    public class ExcludeObjectSetting
    {
        /// <summary>
        /// 操作対象外オブジェクト
        /// </summary>
        public GameObject excludeObject;

        /// <summary>
        /// 子オブジェクトも対象とするか
        /// </summary>
        public bool withChildren;
    }
}
