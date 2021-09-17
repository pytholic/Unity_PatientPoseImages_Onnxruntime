// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.UI;

namespace aus.Ui
{
    [RequireComponent(typeof(Text))]
    [DisallowMultipleComponent]
    public class FpsCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        Text uiText;
        void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            uiText = GetComponent<Text>();
        }


        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;

                uiText.text = string.Format(display, m_CurrentFps);
            }
        }
    }
}
