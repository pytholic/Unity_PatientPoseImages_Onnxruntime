// https://bitbucket.org/alkee/aus

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Property
{
    // modified by alkee
    // from http://www.grapefruitgames.com/blog/2013/11/a-min-max-range-for-unity/

    // Unity에서 기본 제공하는 RangeAttribute 는 float 또는 int 에서만 사용할 수 있다.
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float minLimit;
        public float maxLimit;
        public bool showSlider;

        public MinMaxRangeAttribute(float minLimit, float maxLimit, bool showSlider = false)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
            this.showSlider = showSlider;
        }
    }

    [System.Serializable]
    public class MinMaxRange
    {
        public float rangeStart;
        public float rangeEnd;

        public MinMaxRange()
        {
        }

        public MinMaxRange(float min, float max)
        {
            rangeStart = min;
            rangeEnd = max;
        }

        public float GetRandomValue()
        {
            return Random.Range(rangeStart, rangeEnd);
        }

        public bool IsIn(float value, bool include = false)
        {
            if (!include && value > rangeStart && value < rangeEnd) return true;
            else if (include && value >= rangeEnd && value <= rangeEnd) return true;
            return false;
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var range = attribute as MinMaxRangeAttribute;
            return base.GetPropertyHeight(property, label) + (range.showSlider ? 16 : 0);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Now draw the property as a Slider or an IntSlider based on whether it’s a float or integer.
            if (property.type != "MinMaxRange")
                Debug.LogWarning("Use only with MinMaxRange type");
            else
            {
                var range = attribute as MinMaxRangeAttribute;
                var minValue = property.FindPropertyRelative("rangeStart");
                var maxValue = property.FindPropertyRelative("rangeEnd");
                var newMin = minValue.floatValue;
                var newMax = maxValue.floatValue;

                var xDivision = position.width * 0.33f;
                var yDivision = position.height * (range.showSlider ? 0.5f : 1.0f);

                // tooltip 표시 안되는건 버그 인 듯? https://issuetracker.unity3d.com/issues/when-using-custom-propertydrawers-the-tooltip-field-of-the-serializedproperty-is-always-empty

                // titile
                EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, yDivision), label);

                // slider area
                if (range.showSlider)
                {
                    const float INDENT = 130;

                    EditorGUI.LabelField(new Rect(position.x + INDENT, position.y + yDivision, position.width - INDENT, yDivision)
                        , range.minLimit.ToString("0.##"));
                    EditorGUI.LabelField(new Rect(position.x + position.width - 28f, position.y + yDivision, position.width - INDENT, yDivision)
                        , range.maxLimit.ToString("0.##"));
                    EditorGUI.MinMaxSlider(new Rect(position.x + INDENT + 24f, position.y + yDivision, position.width - INDENT - 58f, yDivision)
                        , ref newMin, ref newMax, range.minLimit, range.maxLimit);
                }
                // text input area
                EditorGUI.LabelField(new Rect(position.x + xDivision, position.y, xDivision, yDivision)
                    , "Min: ");
                newMin = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.x + xDivision + 30, position.y, xDivision - 30, yDivision), newMin)
                    , range.minLimit, newMax);
                EditorGUI.LabelField(new Rect(position.x + xDivision * 2f, position.y, xDivision, yDivision)
                    , "Max: ");
                newMax = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.x + xDivision * 2f + 30, position.y, xDivision - 30, yDivision), newMax)
                    , newMin, range.maxLimit);

                minValue.floatValue = newMin;
                maxValue.floatValue = newMax;

            }
        }
    }
#endif

}
