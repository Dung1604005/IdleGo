using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatListAttribute))]
public class StatListDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            property = EnsureStatCount(property);
            if (property == null || !property.isArray)
            {
                EditorGUI.HelpBox(position, "StatList chỉ hỗ trợ List<float> hoặc float array.", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            float currentY = foldoutRect.yMax + VerticalSpacing;
            int visibleStatCount = Mathf.Min(StatTypeUtility.StatCount, property.arraySize);
            for (int i = 0; i < visibleStatCount; i++)
            {
                Rect statRect = new Rect(
                    position.x,
                    currentY,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                SerializedProperty statValue = property.GetArrayElementAtIndex(i);
                if (statValue != null)
                {
                    EditorGUI.PropertyField(statRect, statValue, new GUIContent(((StatType)i).ToString()));
                }
                currentY += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
        {
            height += StatTypeUtility.StatCount
                * (EditorGUIUtility.singleLineHeight + VerticalSpacing);
        }

        return height;
    }

    private static SerializedProperty EnsureStatCount(SerializedProperty property)
    {
        if (property == null || !property.isArray)
        {
            return null;
        }

        int oldSize = property.arraySize;
        if (oldSize == StatTypeUtility.StatCount)
        {
            return property;
        }

        SerializedObject serializedObject = property.serializedObject;
        string propertyPath = property.propertyPath;
        property.arraySize = StatTypeUtility.StatCount;

        // Thay đổi arraySize có thể làm SerializedProperty cũ mất hiệu lực,
        // đặc biệt với nested class hoặc UI Toolkit Inspector. Apply và tìm lại property trước khi truy cập.
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        SerializedProperty resizedProperty = serializedObject.FindProperty(propertyPath);
        if (resizedProperty == null || !resizedProperty.isArray)
        {
            return null;
        }

        for (int i = oldSize; i < StatTypeUtility.StatCount; i++)
        {
            SerializedProperty statValue = resizedProperty.GetArrayElementAtIndex(i);
            if (statValue != null)
            {
                statValue.floatValue = StatTypeUtility.GetDefaultValue((StatType)i);
            }
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        return serializedObject.FindProperty(propertyPath);
    }
}
