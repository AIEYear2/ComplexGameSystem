using UnityEditor;
using UnityEngine;

namespace ComboSystem
{
    [CustomPropertyDrawer(typeof(AttackRange))]
    public class AttackDrawer : PropertyDrawer
    {
        private Color activeColor = Color.blue;
        private Color inactiveColor = Color.grey;
        private Color handleColor = Color.black;
        private string nodeArrName = "timeNodes";

        private float[] timeNodes;
        private Rect[] rectHandles;

        private bool isDragging = false;
        private int relevantRect = -1;

        private float HandleSize
        {
            get => EditorGUIUtility.singleLineHeight * 0.7f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            label = EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty nodeArr = property.FindPropertyRelative(nodeArrName);

            Rect barRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };

            timeNodes = new float[nodeArr.arraySize];
            for (int x = 0; x < timeNodes.Length; ++x)
            {
                timeNodes[x] = nodeArr.GetArrayElementAtIndex(x).floatValue;
            }

            rectHandles = new Rect[timeNodes.Length];
            for (int x = 0; x < rectHandles.Length; ++x)
            {
                rectHandles[x] = new Rect()
                {
                    x = barRect.x + (barRect.width * timeNodes[x]) - (HandleSize / 2.0f),
                    y = barRect.y + ((barRect.height - HandleSize) / 2.0f),
                    size = Vector2.one * HandleSize
                };
            }

            // Draw grey / blue bar
            DrawBar(position, barRect, nodeArr);

            Event curEvent = Event.current;

            // Update slider
            Slider(barRect, nodeArr, curEvent);
            // Add or Remove handles
            NodeAdjust(barRect, nodeArr, curEvent);

            // Draw slider handles
            for (int x = 0; x < rectHandles.Length; ++x)
                EditorGUI.DrawRect(rectHandles[x], handleColor);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        private void DrawBar(Rect position, Rect barRect, SerializedProperty nodeArr)
        {
            Rect tmpRect = new Rect(position)
            {
                y = position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight
            };
            EditorGUI.PropertyField(tmpRect, nodeArr, GUIContent.none, true);

            EditorGUI.DrawRect(barRect, inactiveColor);

            float prevX = 0.0f;
            for (int x = 0; x <= timeNodes.Length; ++x)
            {
                // Assign variables
                float tmp = 1.0f;
                SerializedProperty tmpProp = (x < nodeArr.arraySize ? nodeArr.GetArrayElementAtIndex(x) : null);

                // if x has a value it can be in the array assign it, otherwise leave it as 1.0f
                if (tmpProp != null)
                    tmp = tmpProp.floatValue;

                // if tmp has been modified in this frame and is a variable from the array
                if (x < timeNodes.Length && tmp != timeNodes[x])
                {
                    // Get the max value tmp can be
                    float holder = (x + 1 < timeNodes.Length ? timeNodes[x + 1] : 1.0f);

                    // Assign the base variable to be tmp clamped between the smallest value it can be and the largest if it still can be
                    if (tmpProp != null)
                        tmpProp.floatValue = Mathf.Clamp(tmp, prevX, holder);
                }

                // draw the rect where rect.x = prevX and rect.width = x - prevX
                if (x % 2 == 0) // odd values represent the grey box which has already been drawn
                {
                    tmpRect = new Rect(barRect)
                    {
                        x = barRect.x + (barRect.width * prevX),
                        width = barRect.width * (tmp - prevX)
                    };
                    EditorGUI.DrawRect(tmpRect, activeColor);
                }

                prevX = tmp;
            }
        }
        private void Slider(Rect position, SerializedProperty nodeArr, Event curEvent)
        {
            // If we're not dragging check to see if we should start dragging
            if (!isDragging)
            {
                if (curEvent.type == EventType.MouseDown && curEvent.button == 0) // If you pressed the left mouse button
                {
                    for (int x = 0; x < rectHandles.Length; ++x)
                    {
                        if (rectHandles[x].Contains(curEvent.mousePosition))
                        {
                            isDragging = true;
                            relevantRect = x;

                            if (x + 1 < timeNodes.Length && timeNodes[x + 1] == timeNodes[x] &&                 // If there is a handle after this one that is on top of it
                                curEvent.mousePosition.x - rectHandles[x].x >= rectHandles[x].width / 2.0f)     // If the mouse is closer to the right than the left
                                ++relevantRect;                                                                 // Set the relevant index to the higher value

                            break;
                        }
                    }
                }

                return;
            }

            // If we are currently dragging check to see if we should stop dragging
            if (curEvent.type == EventType.MouseUp && curEvent.button == 0)  // If you released the left mouse button
            {
                isDragging = false;
                relevantRect = -1;
                return;
            }

            // Update mouse drag
            if (position.width < 0)
                return;

            // Set the clamp extents
            float min = (relevantRect - 1 < 0 ? 0.0f : timeNodes[relevantRect - 1]);
            float max = (relevantRect + 1 >= timeNodes.Length ? 1.0f : timeNodes[relevantRect + 1]);

            // Calculate the value to assign
            float mouseX = curEvent.mousePosition.x;
            mouseX = Mathf.Clamp((mouseX - position.x) / position.width, min, max);

            // If value has changed update the item
            if (mouseX != timeNodes[relevantRect])
                nodeArr.GetArrayElementAtIndex(relevantRect).floatValue = mouseX;
        }
        private void NodeAdjust(Rect position, SerializedProperty nodeArr, Event curEvent)
        {
            if (isDragging) // Don't change the node count while dragging
                return;
            if (!(curEvent.type == EventType.MouseDown && curEvent.button == 1))    // Both following groups are only called on right mouse down
                return;

            bool addNode = true;
            int nodeIndexHolder = 0;

            // Check to see if the mouse is hovering over a handle
            for (int x = 0; x < rectHandles.Length; ++x)
            {
                if (rectHandles[x].Contains(curEvent.mousePosition))
                {
                    // Remove node from bar
                    addNode = false;
                    nodeIndexHolder = x;
                    break;
                }
            }

            // Return if you aren't clicking in a place to add a node and are not attempting to delete
            if (addNode && !position.Contains(curEvent.mousePosition))
                return;

            GenericMenu menu = new GenericMenu();

            if (addNode) // Enable Add menu item and disable Delete as you are clicking in a place to add an item
            {
                menu.AddItem(new GUIContent("Add"), false, AddNode, new NodeData(nodeArr, (curEvent.mousePosition.x - position.x) / position.width));
                menu.AddDisabledItem(new GUIContent("Delete"));
            }
            else // Enable Delete menu item and disable Add as you are clicking in a place to delete an item
            {
                menu.AddDisabledItem(new GUIContent("Add"));
                menu.AddItem(new GUIContent("Delete"), false, RemoveNode, new NodeData(nodeArr, nodeIndexHolder));
            }

            menu.ShowAsContext(); // Show the popup menu
        }

        private void AddNode(object nodeData)
        {
            if (!(nodeData is NodeData data)) // Convert object to NodeData and weed out errors
                return;

            for (int x = 0; x < timeNodes.Length; ++x) // Put the node in the right place in the array
            {
                if (timeNodes[x] > data.data)
                {
                    data.node.serializedObject.Update();                    // Call Update before changes to make sure changes are applied
                    data.node.InsertArrayElementAtIndex(x);
                    data.node.GetArrayElementAtIndex(x).floatValue = data.data;
                    data.node.serializedObject.ApplyModifiedProperties();   // Call Apply after changes to make sure changes are applied
                    return;
                }
            }

            data.node.serializedObject.Update();                    // Call Update before changes to make sure changes are applied
            data.node.InsertArrayElementAtIndex(timeNodes.Length);
            data.node.GetArrayElementAtIndex(timeNodes.Length).floatValue = data.data;
            data.node.serializedObject.ApplyModifiedProperties();   // Call Apply after changes to make sure changes are applied
        }
        private void RemoveNode(object nodeData)
        {
            if (!(nodeData is NodeData data)) // Convert object to NodeData and weed out errors
                return;

            data.node.serializedObject.Update();                    // Call Apply after changes to make sure changes are applied
            data.node.DeleteArrayElementAtIndex((int)data.data);
            data.node.serializedObject.ApplyModifiedProperties();   // Call Apply after changes to make sure changes are applied
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative(nodeArrName);

            if (!typeProperty.isExpanded)
                return EditorGUIUtility.standardVerticalSpacing + (EditorGUIUtility.singleLineHeight * 2); // single line if not expanded

            if (typeProperty.arraySize == 0)
                return 4 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing); // Three lines if enpty and expanded

            return (3 + typeProperty.arraySize) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing); // if expanded, one line for the label, one for the size, and one for each element
        }

        struct NodeData
        {
            public SerializedProperty node;
            public float data;

            public NodeData(SerializedProperty nodeArr, float nodeIndex)
            {
                this.node = nodeArr;
                this.data = nodeIndex;
            }
        }
    }
}