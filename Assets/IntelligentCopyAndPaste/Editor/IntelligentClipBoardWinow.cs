using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IntelligentCopyAndPaste
{
    public class IntelligentClipBoardWinow : EditorWindow
    {
        private ReorderableList _reorderableList;
        private Vector2 scrollPos;

        public static void Open()
        {
            var window = GetWindow<IntelligentClipBoardWinow>(typeof(SceneView));
            window.Init();
            window.Repaint();
        }

        void Init()
        {
            _reorderableList = new ReorderableList(ArrayCopyPasteComponent._data, typeof(Object));
            //変更があったら元データに反映させる
            _reorderableList.onChangedCallback += list => {
                ArrayCopyPasteComponent._data = list.list.Cast<Object>().ToList();
            };

            //個々にドロップできるようにする
            _reorderableList.drawElementCallback += (rect, index, active, focused) => {
                ArrayCopyPasteComponent._data[index] = EditorGUI.ObjectField(rect, ArrayCopyPasteComponent._data[index], typeof(Object), false);
                _reorderableList.list = ArrayCopyPasteComponent._data;
            };

            _reorderableList.elementHeightCallback += index => {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            };

            _reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "clipboard data");
        }
        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
            if (_reorderableList != null)
            {
                _reorderableList.DoLayoutList();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}