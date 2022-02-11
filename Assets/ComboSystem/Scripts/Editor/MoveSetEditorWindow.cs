using UnityEditor;
using UnityEngine;

namespace ComboSystem
{
    public class MoveSetEditorWindow : EditorWindow
    {
        private static MoveSet currentMoveSet = null;
        public MoveSet curSet = null;

        Vector2 scrollPosition;
        int tab = 0;
        int prevTab = 0;
        bool listening = false;
        bool deleteBlock = false;

        string simpleMoveName = "basicMoves";
        string comboMoveName = "comboMoves";
        Color horizontalLineColor = new Color(25f / 255f, 25f / 255f, 25f / 255f);

        KeyCode[] comboHold;
        int keyCodePropIndex = -1;

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is MoveSet set)
            {
                // Open window
                currentMoveSet = set;
                ShowWindow();
                return true;
            }

            return false;
        }

        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(MoveSetEditorWindow));
            window.minSize = new Vector2(380.0f, 0);
        }

        private void SelectionChanged()
        {
            if ((Selection.activeObject is MoveSet set) && set != currentMoveSet)
            {
                currentMoveSet = set;
                Repaint();
            }
        }

        private void AddButton(bool isPressed, SerializedProperty prop)
        {
            if (!isPressed)
                return;

            prop.InsertArrayElementAtIndex(prop.arraySize);
        }

        private void BeginListening(SerializedProperty prop)
        {
            listening = true;
            prevTab = tab;

            if(tab == 0)
                return;

            comboHold = new KeyCode[prop.arraySize];
            for(int x = 0; x < comboHold.Length; ++x)
            {
                comboHold[x] = (KeyCode)prop.GetArrayElementAtIndex(x).intValue;
            }
            prop.ClearArray();
        }
        private void KeyListening(SerializedProperty prop)
        {
            if (prevTab != tab)
            {
                CancelListening(prop);
                return;
            }

            // SimpleMove Listener

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
            {
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    CancelListening(prop);
                    return;
                }

                // SimpleMove Listener
                if (tab == 0)
                {
                    prop.intValue = (int)Event.current.keyCode;
                    CancelListening(prop);
                    return;
                }

                // ComboMove Listener
                int index = prop.arraySize;
                prop.InsertArrayElementAtIndex(index);
                prop.GetArrayElementAtIndex(index).intValue = (int)Event.current.keyCode;
                Repaint();
            }
            else if(Event.current.type == EventType.MouseDown)
            {
                if (tab == 0)
                {
                    prop.intValue = (int)KeyCode.Mouse0 + Event.current.button;
                    CancelListening(prop);
                    return;
                }

                int index = prop.arraySize;
                prop.InsertArrayElementAtIndex(index);
                prop.GetArrayElementAtIndex(index).intValue = (int)KeyCode.Mouse0 + Event.current.button;
            }
        }

        private void CancelListening(SerializedProperty prop)
        {
            listening = false;

            if(tab == 0)
            {
                Repaint();
                return;
            }

            if (prop.arraySize == 0)
            {
                for(int x = 0; x < comboHold.Length; ++x)
                {
                    prop.InsertArrayElementAtIndex(x);
                    prop.GetArrayElementAtIndex(x).intValue = (int)comboHold[x];
                }

                comboHold = new KeyCode[0];
                Repaint();
                return;
            }

            Repaint();
            comboHold = new KeyCode[0];
        }

        private void OnGUI()
        {
            curSet = currentMoveSet;
            if (!curSet)
                return;

            SerializedObject baseObj = new SerializedObject(curSet);
            SerializedProperty simpleMoves = baseObj.FindProperty(simpleMoveName);
            SerializedProperty comboMoves = baseObj.FindProperty(comboMoveName);

            tab = GUILayout.Toolbar(tab, new string[] { simpleMoves.displayName, comboMoves.displayName });
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            baseObj.Update();

            if (listening)
                GUI.enabled = false;

            switch (tab)
            {
                case 0:
                    for (int x = 0; x < simpleMoves.arraySize; ++x)
                    {
                        ShowBasicMove(simpleMoves.GetArrayElementAtIndex(x), x);

                        if (deleteBlock)
                        {
                            simpleMoves.DeleteArrayElementAtIndex(x);
                            deleteBlock = false;
                        }

                        if (x + 1 < simpleMoves.arraySize)
                        {
                            EditorGUILayout.Space();
                            DrawUILine(horizontalLineColor, 1, 10);
                        }
                    }

                    break;
                case 1:
                    for (int x = 0; x < comboMoves.arraySize; ++x)
                    {
                        ShowComboMove(comboMoves.GetArrayElementAtIndex(x), x);

                        if (deleteBlock)
                        {
                            comboMoves.DeleteArrayElementAtIndex(x);
                            deleteBlock = false;
                        }

                        if (x + 1 < comboMoves.arraySize)
                        {
                            EditorGUILayout.Space();
                            DrawUILine(horizontalLineColor, 1, 10);
                        }
                    }

                    break;
            }

            EditorGUILayout.Space();
            DrawUILine(horizontalLineColor, 1, 10);

            if (GUILayout.Button("Add New Move", GUILayout.Width(EditorGUIUtility.currentViewWidth - 21), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3)))
            {
                switch (tab)
                {
                    case 0:
                        simpleMoves.InsertArrayElementAtIndex(simpleMoves.arraySize);
                        break;
                    case 1:
                        comboMoves.InsertArrayElementAtIndex(comboMoves.arraySize);
                        break;
                }
            }

            baseObj.ApplyModifiedProperties();

            GUI.enabled = true;

            GUILayout.EndScrollView();
        }

        private void ShowBasicMove(SerializedProperty move, int index)
        {
////////////// Move Name /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(true);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(move.displayName);

            GUILayout.FlexibleSpace();

            deleteBlock = GUILayout.Button("Remove", GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(move, GUIContent.none);
////////////// Move Attack Key ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(false);

            if (listening && keyCodePropIndex == index)
            {
                GUI.enabled = true;
                KeyListening(move);
                GUI.enabled = false;
            }

            // TODO: finish listen funtctions
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(move.displayName);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button((listening && keyCodePropIndex == index ? "Esc" : "Listen"), GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                keyCodePropIndex = index;
                BeginListening(move);
            }

            EditorGUILayout.PropertyField(move, GUIContent.none);
            EditorGUILayout.EndHorizontal();
////////////// Move Attacks //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(false);

            EditorGUILayout.PropertyField(move, true);
        }
        private void ShowComboMove(SerializedProperty move, int index)
        {
////////////// Move Name /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(true);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(move.displayName);

            GUILayout.FlexibleSpace();

            deleteBlock = GUILayout.Button("Remove", GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(move, GUIContent.none);
////////////// Move Combo Array //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(false);

            // TODO: fix glitch where the keys stop adding once reaching the right wall of the window rather than wrapping down
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(move.displayName);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button((listening && keyCodePropIndex == index ? "esc" : "listen"), GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                keyCodePropIndex = index;
                BeginListening(move);
            }

            if (GUILayout.Button("Clear", GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                move.ClearArray();
            }

            EditorGUILayout.EndHorizontal();
            // increase arraysize by 1 for the add button
            int arrSize = move.arraySize + 1;

            int horCount = Mathf.Min(arrSize, (int)EditorGUIUtility.currentViewWidth / 55);
            float tmp = arrSize / (float)horCount;
            int vertCount = (int)tmp;
            vertCount += (tmp > vertCount ? 1 : 0);

            EditorGUILayout.BeginVertical();
            for (int x = 0; x < vertCount; ++x)
            {
                EditorGUILayout.BeginHorizontal();
                for(int y = 0; y < Mathf.Min(arrSize - (horCount * x), horCount); ++y)
                {
                    int i = (horCount * x) + y;
                    if (i < move.arraySize)
                        EditorGUILayout.PropertyField(move.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(50));
                    else
                        AddButton(GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight)), move);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            if (listening && keyCodePropIndex == index)
            {
                GUI.enabled = true;
                KeyListening(move);
                GUI.enabled = false;
            }
////////////// Move Attacks //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            move.Next(false);

            EditorGUILayout.PropertyField(move);
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 4;
            r.width += 8;
            EditorGUI.DrawRect(r, color);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += SelectionChanged;
        }
        private void OnDestroy()
        {
            Selection.selectionChanged -= SelectionChanged;
        }
    }
}