// https://bitbucket.org/alkee/aus

using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using System.Collections;
using UnityEditor;
#endif

namespace aus.Property
{
    // 추가적으로 conditionalSourceField 에 ',' 로 연결해 (and 조건) 사용 가능
    // ** 주의 ** Range 등의 attribute 와 함께 사용할 수 없다.

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    { // ref http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/

        //The name of the bool field that will be in control
        public string ConditionalSourceField = "";
        //TRUE = Hide in inspector / FALSE = Disable in inspector 
        public bool HideInInspector;
        public bool Reversed;

        public ConditionalHideAttribute(string conditionalSourceField, bool hideThanDisable = false, bool reversed = false)
        {
            ConditionalSourceField = conditionalSourceField;
            Reversed = reversed;
            HideInInspector = hideThanDisable;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
            if (condHAtt.Reversed) enabled = !enabled;

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (!condHAtt.HideInInspector || enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
            if (condHAtt.Reversed) enabled = !enabled;

            if (!condHAtt.HideInInspector || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to

            var andFields = condHAtt.ConditionalSourceField.Split(',');

            foreach (var f in andFields)
            {
                string conditionPath = propertyPath.Replace(property.name, f); //changes the path to the conditionalsource property path
                SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
                if (sourcePropertyValue == null)
                {
                    Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + f);
                    continue;
                }
                if (sourcePropertyValue.type != "bool") continue;

                enabled = sourcePropertyValue.boolValue;
                if (enabled == false) break;
            }
            return enabled;
        }
    }
#endif

}