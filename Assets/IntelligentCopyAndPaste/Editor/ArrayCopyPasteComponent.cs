using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace IntelligentCopyAndPaste
{
    public static class ArrayCopyPasteComponent
    {
        public static System.Collections.Generic.List<Object> _data; //TODO 本当はprivateにしたいな

        //# SHIFT
        //% CTRL
        //& ALT

        /// <summary>
        /// 普通のコピー
        /// </summary>
        [MenuItem("Assets/Copy Assets Paths %c",false,-101)]
        private static void CopyAssetsPaths()
        {
            _data = new System.Collections.Generic.List<Object>(Selection.objects);
            IntelligentClipBoardWinow.Open();
        }

        /// <summary>
        /// 追加コピー
        /// </summary>
        [MenuItem("Assets/AppendCopy Assets Paths #%c", false, -101)]
        private static void AppendCopyAssetsPaths()
        {
            if (_data == null) _data = new System.Collections.Generic.List<Object>();
            _data.AddRange(Selection.objects);
            IntelligentClipBoardWinow.Open();
        }

        /// <summary>
        /// GameObjectを指定してのペースト ALT+V
        /// 可能性が非常に多いのでChoiceWindowを出す必要がある
        /// </summary>
        [MenuItem("Assets/Paste From Assets Paths %v", false, -101)]
        private static void _PasteFromCopiedObjects()
        {
            if (Validate() == false) return;
            var selection = Selection.gameObjects.First();
            var monoBehaviours = selection.GetComponents<MonoBehaviour>();
            ArrayCopyAndPaste(true, monoBehaviours);
        }

        /// <summary>
        /// GameObjectを指定しての追加ペースト ALT+SHIFT+V
        /// 可能性が非常に多いのでChoiceWindowを出す必要がある
        /// </summary>
        [MenuItem("Assets/AppendPaste From Assets Paths #%v", false, -101)]
        private static void _AppendPasteFromCopiedObjects()
        {
            if (Validate() == false) return;
            var selection = Selection.gameObjects.First();
            var monoBehaviours = selection.GetComponents<MonoBehaviour>();
            ArrayCopyAndPaste(false, monoBehaviours);
        }

        /// <summary>
        /// ContextMenuからのペースト ALT+V
        /// </summary>
        [MenuItem("CONTEXT/MonoBehaviour/Intelligent Paste %v", false)]
        private static void _PasetFromCopiedObjects(MenuCommand menuCommand)
        {
            if (Validate() == false) return;
            ArrayCopyAndPaste(true, menuCommand.context as MonoBehaviour);
        }

        /// <summary>
        /// ContextMenuからの追加ペースト ALT+SHIFT+V
        /// </summary>
        [MenuItem("CONTEXT/MonoBehaviour/Intelligent AppendPaste #%v", false)]
        private static void _AppendPasteFromCopiedObjects(MenuCommand menuCommand)
        {
            if (Validate() == false) return;
            ArrayCopyAndPaste(false, menuCommand.context as MonoBehaviour);
        }


        private static void ArrayCopyAndPaste(bool isOverride, params MonoBehaviour[] monoBehaviours)
        {
            var arrayCopyAndPasteInfos = new System.Collections.Generic.List<ArrayCopyAndPasteInfo>();
            foreach (var mono in monoBehaviours)
            {
                var type = mono.GetType();
                var allFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);

                foreach (var info in allFields)
                {
                    //PrivateFieldはSerializeFieldAttributeが指定されてないものは対象にできない
                    if (info.IsPrivate && info.GetCustomAttributes(typeof(SerializeField), false).Any() == false) continue;

                    //複数の場合があるので、どの配列に対して、どのインデックスから（追加か、上書きか）張り付けるかを判断するためのwindowを表示する
                    var pasteInfo = new ArrayCopyAndPasteInfo
                    {
                        IsOverride = isOverride,
                        TargetMonoBehaviour = mono,
                        TargetFieldInfo = info,
                    };

                    //配列かListか単品か判断
                    var arrayType = info.FieldType;
                    if (arrayType.IsGenericType && arrayType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)) pasteInfo.IsTypeList = true;
                    else if (arrayType.IsArray) pasteInfo.IsTypeList = false;
                    else pasteInfo.IsSingle = true;

                    //配列（またはList)の要素のType
                    if (pasteInfo.IsSingle)
                    {
                        pasteInfo.ArrayElementType = info.FieldType;
                    }
                    else
                    {
                        pasteInfo.ArrayElementType = pasteInfo.IsTypeList ? arrayType.GetGenericArguments().FirstOrDefault() : arrayType.GetElementType();
                    }
                    if (pasteInfo.ArrayElementType == null) continue;

                    //対象がこのMonoBehaviourのこのFieldだった場合のコピー元から張り付ける事ができる要素をまとめる
                    pasteInfo.CopySrcDatas = _data.Select(o =>
                    {
                        if(o.GetType() == pasteInfo.ArrayElementType) return o;

                        Debug.Log(pasteInfo.ArrayElementType.Name);
                        if (pasteInfo.ArrayElementType.IsSubclassOf(typeof(Component)))
                        {
                            var attachedComponent = (o as GameObject)?.GetComponent(pasteInfo.ArrayElementType);
                            if (attachedComponent?.GetType() == pasteInfo.ArrayElementType) return attachedComponent;
                        }
                        return AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(o), pasteInfo.ArrayElementType);//TODO Spriteそのものの場合と、Spriteを持っているTextureの場合がある
                    }).Where(o => o != null).ToArray(); 

                    if (pasteInfo.CopySrcDatas.Any() == false) continue; //コピー元が張り付ける先が無かった

                    if (pasteInfo.IsTypeList)
                    {
                        var originalData = pasteInfo.TargetFieldInfo.GetValue(pasteInfo.TargetMonoBehaviour) as IList;
                        pasteInfo.OriginalDatas = new Object[originalData.Count];
                        originalData.CopyTo(pasteInfo.OriginalDatas,0);
                    }
                    else if (pasteInfo.IsSingle)
                    {
                        var originalData = pasteInfo.TargetFieldInfo.GetValue(pasteInfo.TargetMonoBehaviour) as Object;
                        pasteInfo.OriginalDatas = new []{originalData};
                    }
                    else
                    {
                        var originalData = pasteInfo.TargetFieldInfo.GetValue(pasteInfo.TargetMonoBehaviour) as Array;
                        pasteInfo.OriginalDatas = originalData as Object[];
                    }

                    pasteInfo.DisplayName = pasteInfo.TargetFieldInfo.DeclaringType.FullName + Environment.NewLine;

                    if (pasteInfo.IsSingle)
                    {
                        pasteInfo.DisplayName += pasteInfo.ArrayElementType.Name;
                    }
                    else if (pasteInfo.IsTypeList)
                    {
                        pasteInfo.DisplayName += "List<" + pasteInfo.ArrayElementType.Name + "> ";
                    }
                    else
                    {
                        pasteInfo.DisplayName += pasteInfo.ArrayElementType.Name + "[] ";
                    }

                    //継承関係においては、同じフィールドが複数出てきてしまう可能性があるのでチェック
                    if(arrayCopyAndPasteInfos.Any(inf => inf.TargetFieldInfo == pasteInfo.TargetFieldInfo))continue;

                    arrayCopyAndPasteInfos.Add(pasteInfo);
                }
            }

            //TODO 対象が１つしか無かった場合にWindowを出すべきかどうか悩むところ
            if (arrayCopyAndPasteInfos.Any())
            {
                var message = arrayCopyAndPasteInfos.Count == 1 ? "以下のFieldが候補にあがりました。よろしいですか？" : "複数のFieldが候補にあがりました。どちらのFieldにペーストしますか？";
                CopyAndPasteChoiceWindow.Open(message, arrayCopyAndPasteInfos, pasteInfo =>
                {
                    if (pasteInfo != null)
                    {
                        CopyProcess(pasteInfo);
                    }
                });
            }
        }

        private static void CopyProcess(ArrayCopyAndPasteInfo pasteInfo)
        {
            //Undoへ登録
            Undo.RecordObject(pasteInfo.TargetMonoBehaviour,"IntelligentCopyAndPaste - Paste");

            if (pasteInfo.IsSingle)
            {
                //一つしかないので、IsOverride関係ない
                var pastData = pasteInfo.CopySrcDatas.FirstOrDefault();
                if (pastData == null)
                {
                    pasteInfo.TargetFieldInfo.SetValue(pasteInfo.TargetMonoBehaviour, null);
                }
                else
                {
                    pasteInfo.TargetFieldInfo.SetValue(pasteInfo.TargetMonoBehaviour, pastData);
                }
            }
            else if (pasteInfo.IsTypeList)
            {
                var list = pasteInfo.TargetFieldInfo.GetValue(pasteInfo.TargetMonoBehaviour) as IList;

                if (pasteInfo.IsOverride) list.Clear(); //ペーストしたもので全て入れ替える場合
                foreach (var o in pasteInfo.CopySrcDatas)
                {
                    list.Add(o);
                }
            }
            else
            {
                if (pasteInfo.IsOverride) //ペーストしたもので全て入れ替える場合
                {
                    var converted = Array.CreateInstance(pasteInfo.ArrayElementType, pasteInfo.CopySrcDatas.Length);
                    Array.Copy(pasteInfo.CopySrcDatas, 0, converted, 0, pasteInfo.CopySrcDatas.Length);
                    pasteInfo.TargetFieldInfo.SetValue(pasteInfo.TargetMonoBehaviour, converted);
                }
                else //元データを維持して、追加する場合
                {
                    var array = pasteInfo.TargetFieldInfo.GetValue(pasteInfo.TargetMonoBehaviour) as Array;
                    var converted = Array.CreateInstance(pasteInfo.ArrayElementType, pasteInfo.CopySrcDatas.Length + array.Length);
                    Array.Copy(array, 0, converted, 0, array.Length);
                    Array.Copy(pasteInfo.CopySrcDatas, 0, converted, array.Length, pasteInfo.CopySrcDatas.Length);
                    pasteInfo.TargetFieldInfo.SetValue(pasteInfo.TargetMonoBehaviour, converted);
                }
            }
            EditorUtility.SetDirty(pasteInfo.TargetMonoBehaviour);
            pasteInfo.TargetMonoBehaviour.SendMessage("OnValidate");//TODO mmmmmmmmmmmmmm これをやらないとEditor上で画像などが反映されないのツライ
        }


        private static bool Validate(bool isShowAlert = true)
        {
            if (_data == null || !_data.Any()) //C# 6.0 → if(data?.Any() != true)return;
            {
                if (isShowAlert) UnityEditor.EditorUtility.DisplayDialog("Notice", "なにもコピーされていないようです", "OK");
                return false;
            }

            if (Selection.gameObjects.Length >= 2)
            {
                if (isShowAlert) UnityEditor.EditorUtility.DisplayDialog("Notice", "複数GameObjectへの同時ペーストは出来ません", "OK");
                return false;
            }

            if (Selection.gameObjects.Any() == false)
            {
                if (isShowAlert) UnityEditor.EditorUtility.DisplayDialog("Notice", "GameObjectが選択されていません", "OK");
                return false;
            }

            return true;
        }
    }
}