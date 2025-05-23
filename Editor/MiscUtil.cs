using System.Collections.Generic;
using UnityEngine;

namespace MitarashiDango.PhysBonesSwitcher.Editor
{
    public class MiscUtil
    {
        /// <summary>
        /// オブジェクトのヒエラルキー上でのパスを取得する
        /// </summary>
        /// <param name="targetObject">パス取得対象のオブジェクト</param>
        /// <param name="rootObject">パス取得時の基準とするオブジェクト（絶対パスを取得する場合、nullを指定する)</param>
        /// <returns></returns>
        public static string GetPathInHierarchy(GameObject targetObject, GameObject rootObject)
        {
            return GetPathInHierarchy(targetObject?.transform, rootObject?.transform);
        }

        /// <summary>
        /// オブジェクトのヒエラルキー上でのパスを取得する
        /// </summary>
        /// <param name="targetObject">パス取得対象のオブジェクト</param>
        /// <param name="rootObject">パス取得時の基準とするオブジェクト（絶対パスを取得する場合、nullを指定する)</param>
        /// <returns></returns>
        public static string GetPathInHierarchy(Transform targetObjectTransform, Transform rootObjectTransform)
        {
            if (targetObjectTransform == null)
            {
                return null;
            }

            var objectNames = new List<string>();
            var currentTransform = targetObjectTransform;
            var rootTransform = rootObjectTransform;

            while (true)
            {
                if (rootTransform != null && currentTransform == rootTransform)
                {
                    break;
                }

                objectNames.Insert(0, currentTransform.name);

                if (currentTransform.parent == null)
                {
                    break;
                }

                currentTransform = currentTransform.parent;
            }

            return string.Join("/", objectNames);
        }
    }
}
