// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.UI;

namespace aus.Ui
{
    public class ButtonSound : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float SoundVolume = 1.0f;
        public AudioClip ClickSound;

        [Tooltip("true 이면 click 을 연속으로 하더라도 중복해서 소리가 나지 않는다.")]
        public bool preventDuplicatedSound = false;

        void Start()
        {
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            var button = GetComponent<Button>();
            Debug.Assert(button != null);
            button.onClick.AddListener(() => { PlaySound(ClickSound); });
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null || enabled == false) return;

            if (source.gameObject.activeInHierarchy == true)
            {
                if (preventDuplicatedSound == false)
                {
                    source.PlayOneShot(clip, SoundVolume);
                }
                else
                {
                    source.volume = SoundVolume;
                    source.Stop();
                    source.clip = clip;
                    source.Play();
                }
            }
            else // "Can not play a disabled audio source" warning 을 피하기 위해
            {
                // 주의 : 이 경우(inactivate 또는 Destroyed)에는 preventDuplicatedSound 을 사용할 수 없다.
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, SoundVolume);
            }
        }

        private AudioSource source;
    }
}
