using UnityEngine;
using UnityEngine.UI;

// A class that can be used to scrub a value typically using a slider or scrubbing bar.
namespace LoGaCulture.LUTE
{
    public class Timebar : MonoBehaviour
    {
        // Typically we use audiosources but you could extra properties to scrub other values.
        [Tooltip("The audio source to scrub if you desire.")]
        [SerializeField] protected AudioSource audioSource;
        // Typically using a slider which you can set if needed
        [Tooltip("The slider to scrub if this class is not attached to an object with a slider component.")]
        [SerializeField] protected Slider slider;

        protected virtual void Start()
        {
            Initialise();
        }

        protected virtual void Initialise()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (slider == null)
                return;
        }

        public virtual void UpdateSourceTime()
        {
            SoundManager soundManager = LogaManager.Instance.SoundManager;
            if (soundManager == null)
                return;

            soundManager.SetSourceTime(slider.value);
        }

        protected virtual void Update()
        {
            if (audioSource != null && audioSource.clip != null && slider != null)
            {
                slider.value = audioSource.time / audioSource.clip.length;
            }
            else if (slider != null)
            {
                var soundManagerAudioSource = LogaManager.Instance.SoundManager.GetAudioSource();
                if (soundManagerAudioSource == null || soundManagerAudioSource.clip == null)
                    return;

                slider.value = soundManagerAudioSource.time / soundManagerAudioSource.clip.length;
            }
        }
    }
}