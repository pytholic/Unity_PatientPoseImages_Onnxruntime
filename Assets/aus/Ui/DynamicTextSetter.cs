// https://bitbucket.org/alkee/aus

using UnityEngine;
using UnityEngine.UI;

namespace aus.Ui
{
    [RequireComponent(typeof(Text))]
    [DisallowMultipleComponent]
    public class DynamicTextSetter : MonoBehaviour
    {
        [Property.PropertyOrField]
        public Property.ComponentMemberField Source;
        [Tooltip("if the value is null, this NullAlias will be used instead and the Foramt will be ignored")]
        public string NullAlias;
        [Tooltip("c# string format like {0:0.00}")]
        public string Format = "{0}";

        void Awake()
        {
            target = GetComponent<Text>();
        }

        private void Start()
        {
            if (Source == null || Source.IsValid() == false) return;
            var val = Source.GetValue() == null ? null : Source.GetValue().ToString();
            SetText(val);
        }

        void Update()
        {
            if (Source == null || Source.IsValid() == false) return;
            var val = Source.GetValue() == null ? null : Source.GetValue().ToString();
            if (lastText == val) return;
            SetText(val);
        }

        private Text target;
        private string lastText;

        private void SetText(string val)
        {
            if (val == null) target.text = NullAlias;
            else
            {
                lastText = val;
                val = string.Format(Format, val);
                target.text = val;
            }
        }
    }
}
