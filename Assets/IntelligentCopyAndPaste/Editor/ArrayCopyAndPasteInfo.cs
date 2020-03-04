using System;
using System.Reflection;
using UnityEngine;

namespace IntelligentCopyAndPaste
{
    [Serializable]
    public class ArrayCopyAndPasteInfo
    {
        public MonoBehaviour TargetMonoBehaviour;
        public FieldInfo TargetFieldInfo;
        public Type ArrayElementType;
        public UnityEngine.Object[] CopySrcDatas;
        public UnityEngine.Object[] OriginalDatas;
        public bool IsOverride;
        public bool IsTypeList;
        public bool IsSingle;
        public string DisplayName;
    }
}