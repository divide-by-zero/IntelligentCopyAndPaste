using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IntelligentCopyAndPaste
{
    public class CopyAndPasteChoiceWindow : EditorWindow
    {
        private static CopyAndPasteChoiceWindow _window;

        public static void Open(string title, List<ArrayCopyAndPasteInfo> arrayCopyAndPasteInfos, Action<ArrayCopyAndPasteInfo> action)
        {
            if (_window == null)
            {
                _window = CreateInstance<CopyAndPasteChoiceWindow>();
            }

            _window.Init(title, arrayCopyAndPasteInfos, action);
            _window.ShowUtility();
        }


        private string message;
        private List<ArrayCopyAndPasteInfo> infos;
        private Action<ArrayCopyAndPasteInfo> callback;
        private List<ReorderableList> lists;
        private Vector2 scrollPos;

        private int selected;

        void Init(string message, List<ArrayCopyAndPasteInfo> infos, Action<ArrayCopyAndPasteInfo> callback)
        {
            this.message = message;
            this.infos = infos;
            this.callback = callback;

            lists = infos.Select(info =>
            {
                var reorderableList = new ReorderableList(info.CopySrcDatas.ToList(), info.ArrayElementType);

                //変更があったら元データをいじる
                reorderableList.onChangedCallback += list => { info.CopySrcDatas = list.list.Cast<Object>().ToArray(); };
                reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "clipboard data");

                return reorderableList;
            }).ToList();
        }

        void OnGUI()
        {
            EventRegist();
            if (infos == null || infos.Count == 0) return;
            if (lists == null || lists.Count == 0) return;

            if (string.IsNullOrEmpty(message) == false)
            {
                EditorGUILayout.LabelField(message);
            }

            selected = GUILayout.SelectionGrid(selected, infos.Select(info => info.DisplayName).ToArray(), 3);

            var targetInfo = infos[selected];
            var targetlist = lists[selected];

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);

            EditorGUILayout.LabelField("current data");
            if (targetInfo.OriginalDatas != null && targetInfo.OriginalDatas.Any())
            {
                foreach (var targetInfoOriginalData in targetInfo.OriginalDatas)
                {
                    EditorGUILayout.ObjectField(targetInfoOriginalData, targetInfo.ArrayElementType, false);
                }
            }
            else
            {
                EditorGUILayout.LabelField("empty");
            }

            targetlist.DoLayoutList();
            EditorGUILayout.EndScrollView();

            if (targetInfo.IsSingle)
            {
                EditorGUILayout.LabelField("ペースト対象が配列(List)じゃない場合は、コピーされた1番上のデータがペーストされます");
            }
            else
            {
                targetInfo.IsOverride = EditorGUILayout.ToggleLeft("IsOverride", targetInfo.IsOverride); //上書きかどうか
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.Separator();

            // 何かしら入力しないとOKボタンを押せないようにするDisableGroup
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CANCEL", GUILayout.Height(30f)))
            {
                Close();
            }

            if (GUILayout.Button("OK", GUILayout.Height(30f)))
            {
                callback(targetInfo);
                callback = null;
                Close();
            }

            GUILayout.EndHorizontal();

        }

        void OnDestroy()
        {
            if (callback != null) callback(null);
        }

        private void EventRegist()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                this.Close();
            }
        }

    }
}