//using TMPro;
//using UnityEngine;

//namespace LoGaCulture.LUTE
//{
//    public class ObjectInfoPanel : MonoBehaviour
//    {
//        [Header("UI Components")]
//        [SerializeField] protected TextMeshProUGUI titleText;
//        [SerializeField] protected TextMeshProUGUI bodyText;
//        [SerializeField] protected UnityEngine.UI.Image infoImage; // Eventually replaced by spinning object
//        [SerializeField] protected UnityEngine.UI.Button playButton;
//        [SerializeField] protected Sprite playSprite;
//        [SerializeField] protected Sprite pauseSprite;
//        [SerializeField] protected UnityEngine.UI.Button stopButton;
//        [SerializeField] protected CanvasGroup audioPlayerGroup;
//        [SerializeField] protected CanvasGroup panelGroup; // For fading in and out
//        [SerializeField] protected Transform objectSpawn;
//        [SerializeField] protected bool hideDescription;
//        [SerializeField] protected AudioClip unlockSound; // Typically played when player has unlocked all info

//        protected LTDescr fadeTween;

//        protected static bool infoPanelActive = false;

//        private ObjectSpinner spawnedObject;
//        private bool stopAudioOnClose = false;
//        private bool hasAudio = false;

//        public static ObjectInfoPanel ActiveInfoPanel;
//        public static LocationInfoPanel ActiveLocationInfoPanel;

//        [HideInInspector]
//        public static LocationInfoPanel CustomLocationPrefab;

//        public static ObjectInfoPanel GetInfoPanel()
//        {
//            if (ActiveInfoPanel == null)
//            {
//                GameObject prefab = Resources.Load<GameObject>("Prefabs/ObjectInfoPanel");
//                if (prefab != null)
//                {
//                    GameObject go = Instantiate(prefab);
//                    go.SetActive(false);
//                    go.name = "ObjectInfoPanel";
//                    ActiveInfoPanel = go.GetComponent<ObjectInfoPanel>();
//                }
//            }
//            return ActiveInfoPanel;
//        }

//        public virtual HiddenObjectInteraction GetHiddenInteraction()
//        {
//            if (spawnedObject == null)
//            {
//                return null;
//            }
//            var hiddenObject = spawnedObject.GetComponentInChildren<HiddenObjectInteraction>();
//            if (hiddenObject == null)
//            {
//                return null;
//            }
//            return hiddenObject;
//        }

//        protected virtual void Awake()
//        {
//            ActiveInfoPanel = this;
//        }

//        protected virtual void OnDestroy()
//        {
//            ActiveInfoPanel = null;
//        }

//        protected virtual void Start()
//        {
//            if (!infoPanelActive && panelGroup != null)
//            {
//                panelGroup.alpha = 0.0f;
//                panelGroup.interactable = false;
//                panelGroup.blocksRaycasts = false;
//            }

//            if (hideDescription && bodyText != null)
//            {
//                bodyText.alpha = 0;
//            }
//        }

//        public virtual void SetActive(bool state) => gameObject.SetActive(state);

//        public virtual void SetInfo(ObjectInfo info)
//        {
//            if (info == null)
//            {
//                return;
//            }

//            if (titleText == null || bodyText == null || infoImage == null || objectSpawn == null)
//            {
//                return;
//            }

//            titleText.text = info.ObjectName;
//            bodyText.text = info.ObjectDescription;
//            infoImage.sprite = info.ObjectIcon;
//            var spinner = info.SpinningObject;
//            hasAudio = false;
//            audioPlayerGroup.alpha = 0; audioPlayerGroup.interactable = false; audioPlayerGroup.blocksRaycasts = false;

//            //if (playButton != null && stopButton != null)
//            //{
//            //    if (info.VoiceOverClip != null)
//            //        SetAudioInteraction(info.VoiceOverClip);
//            //}

//            if (playButton != null && playSprite != null && pauseSprite != null)
//            {
//                if (info.VoiceOverClip != null)
//                {
//                    SetAudioInteractionBool(info.VoiceOverClip);
//                }
//            }
//            stopAudioOnClose = info.StopAudioOnClose;

//            if (spawnedObject != null)
//                Destroy(spawnedObject.gameObject);
//            if (spinner != null)
//            {
//                spawnedObject = Instantiate(spinner, infoImage.transform.parent.transform, false);
//                spawnedObject.transform.localPosition = objectSpawn.localPosition;
//                spawnedObject.transform.localScale = objectSpawn.localScale;
//                spawnedObject.transform.rotation = objectSpawn.rotation;
//            }

//            var hiddenObject = GetHiddenInteraction();
//            if (hiddenObject != null)
//                hiddenObject.ObjectInfo = info;

//            if (info.Unlocked)
//            {
//                RevealInfo();
//                if (hiddenObject != null)
//                {
//                    hiddenObject.SetActive(false);
//                }
//            }
//            else
//            {
//                HideInfo();
//            }
//        }


//        protected virtual void SetAudioInteraction(AudioClip clip)
//        {
//            hasAudio = true;
//            playButton.onClick.AddListener(() =>
//            {
//                SoundManager soundManager = LogaManager.Instance.SoundManager;
//                if (soundManager == null)
//                    return;

//                // needs to play from the point of the clip
//                var audiosource = soundManager.GetAudioSource();
//                if (audiosource != null)
//                {
//                    if (audiosource.clip != null)
//                    {
//                        soundManager.PlayMusic(clip, false, 0.5f, audiosource.time, true);
//                    }
//                    else
//                    {
//                        soundManager.PlayMusic(clip, false, 0.5f, 0);
//                    }
//                }

//            });

//            stopButton.onClick.AddListener(() =>
//            {
//                SoundManager soundManager = LogaManager.Instance.SoundManager;
//                if (soundManager == null)
//                    return;

//                soundManager.PauseMusic();
//            });
//        }

//        protected virtual void SetAudioInteractionBool(AudioClip clip)
//        {
//            bool isPlaying = false; // Track whether the audio is currently playing
//            hasAudio = true;

//            // Set initial button sprite to play icon
//            playButton.image.sprite = playSprite;

//            playButton.onClick.AddListener(() =>
//            {
//                SoundManager soundManager = LogaManager.Instance.SoundManager;
//                if (soundManager == null)
//                    return;

//                var audiosource = soundManager.GetAudioSource();
//                if (audiosource != null)
//                {
//                    if (isPlaying)
//                    {
//                        // Pause the music if it is currently playing
//                        soundManager.PauseMusic();
//                        playButton.image.sprite = playSprite;
//                        isPlaying = false;
//                    }
//                    else
//                    {
//                        // Play the music from where it was left off
//                        if (audiosource.clip != null)
//                        {
//                            soundManager.PlayMusic(clip, false, 0.5f, audiosource.time, true);
//                        }
//                        else
//                        {
//                            soundManager.PlayMusic(clip, false, 0.5f, 0);
//                        }
//                        playButton.image.sprite = pauseSprite;
//                        isPlaying = true;
//                    }
//                }
//            });
//        }

//        // Reveals the rest of the information about the object
//        public virtual void RevealInfo()
//        {
//            if (fadeTween != null)
//            {
//                LeanTween.cancel(fadeTween.id, true);
//                fadeTween = null;
//            }

//            // Fade the text in
//            LeanTween.value(bodyText.gameObject, bodyText.alpha, 1f, 0.75f)
//    .setEase(LeanTweenType.easeOutQuint)
//    .setOnUpdate((t) =>
//    {
//        if (audioPlayerGroup != null && hasAudio)
//            audioPlayerGroup.alpha = t;
//        bodyText.alpha = t;
//    }).setOnComplete(() =>
//    {
//        if (audioPlayerGroup != null && hasAudio)
//        {
//            audioPlayerGroup.alpha = 1f;
//            audioPlayerGroup.interactable = true;
//            audioPlayerGroup.blocksRaycasts = true;
//        }
//        bodyText.alpha = 1f;
//    });
//        }

//        public virtual void HideInfo()
//        {
//            if (fadeTween != null)
//            {
//                LeanTween.cancel(fadeTween.id, true);
//                fadeTween = null;
//            }

//            // Fade the text in
//            LeanTween.value(bodyText.gameObject, bodyText.alpha, 0f, 0.0f)
//    .setEase(LeanTweenType.easeOutQuint)
//    .setOnUpdate((t) =>
//    {
//        if (audioPlayerGroup != null && hasAudio)
//            audioPlayerGroup.alpha = t;
//        bodyText.alpha = t;
//    }).setOnComplete(() =>
//    {
//        if (audioPlayerGroup != null && hasAudio)
//        {
//            audioPlayerGroup.alpha = 0f;
//            audioPlayerGroup.interactable = false;
//            audioPlayerGroup.blocksRaycasts = false;
//        }
//        bodyText.alpha = 0f;
//    });
//        }

//        public virtual void UnlockInfo()
//        {
//            if (unlockSound != null)
//            {
//                LogaManager.Instance.SoundManager.PlaySound(unlockSound, 1.0f);
//            }
//        }

//        public virtual void ToggleMenu()
//        {
//            if (fadeTween != null)
//            {
//                LeanTween.cancel(fadeTween.id, true);
//                fadeTween = null;
//            }

//            if (infoPanelActive)
//            {
//                //Fade menu out
//                LeanTween.value(panelGroup.gameObject, panelGroup.alpha, 0f, 0.4f)
//        .setEase(LeanTweenType.easeOutQuint)
//        .setOnUpdate((t) =>
//        {
//            panelGroup.alpha = t;
//            if (spawnedObject != null && objectSpawn != null)
//                spawnedObject.transform.localScale = new Vector3(t, t, t);
//        }).setOnComplete(() =>
//        {
//            panelGroup.alpha = 0f;
//            panelGroup.interactable = false;
//            panelGroup.blocksRaycasts = false;
//            if (stopAudioOnClose)
//                LogaManager.Instance.SoundManager.StopMusic();
//            SetActive(false);
//        });
//            }
//            else
//            {
//                SetActive(true);
//                //Fade menu in
//                LeanTween.value(panelGroup.gameObject, panelGroup.alpha, 1f, 0.4f)
//        .setEase(LeanTweenType.easeOutQuint)
//        .setOnUpdate((t) =>
//        {
//            panelGroup.alpha = t;
//            if (spawnedObject != null && objectSpawn != null)
//            {
//                float x = t * objectSpawn.localScale.x;
//                spawnedObject.transform.localScale = new Vector3(x, x, x);
//            }
//        }).setOnComplete(() =>
//        {
//            panelGroup.alpha = 1f;
//            panelGroup.interactable = true;
//            panelGroup.blocksRaycasts = true;
//        });
//            }
//            infoPanelActive = !infoPanelActive;
//        }
//    }
//}
