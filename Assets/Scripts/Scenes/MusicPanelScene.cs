using UniRx;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


namespace MusicPlayer.Scenes.Play
{
    public class MusicPanelScene : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource = default;
        [Header("MusicPanel")]
        [SerializeField]
        private Image musicImage = default;
        [SerializeField]
        private Text musicTitleText = default;
        [SerializeField]
        private Text musicArtistText = default;
        [SerializeField]
        private Button viewPlayListButton = default;
        [SerializeField]
        private Button prevButton = default;
        [SerializeField]
        private Button stopButton = default;
        [SerializeField]
        private Button playButton = default;
        [SerializeField]
        private Text playButtonText = default;
        [SerializeField]
        private Button nextButton = default;
        [SerializeField]
        private Button addMusicButton = default;
        [SerializeField]
        private Image nowloadingImage = default;

        [Header("DebugObject")]
        [SerializeField]
        private Button debugLogViewButton = default;
        [SerializeField]
        private GameObject debugLogViewObject = default;

        private List<PlayItem> playList = new List<PlayItem>();
        private int playListIndex = 0;
        private List<MusicItem> musicItems = new List<MusicItem>();
        private bool isFirstPlay = true;
        
        private Dictionary<string, AudioType> audioTypeDictionary = new Dictionary<string, AudioType>()
        {
            {".acc", AudioType.ACC},
            {".aiff", AudioType.AIFF},
            {".it", AudioType.IT},
            {".mod", AudioType.MOD},
            {".mpeg", AudioType.MPEG},
            {".m4a", AudioType.MPEG},
            {".mp3", AudioType.MPEG},
            {".mp2", AudioType.MPEG},
            {".ogg", AudioType.OGGVORBIS},
            {".s3m", AudioType.S3M},
            {".wav", AudioType.WAV},
            {".xm", AudioType.XM},
            {".xma", AudioType.XMA},
            {".vag", AudioType.VAG},
            {".audioqueue", AudioType.AUDIOQUEUE}
        };

        private bool isViewDebugLog = true;

        private void Awake()
        {
            RequestUserPermission();
            SetButtonAction();
        }

        private void RequestUserPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
#endif
        }

        private void SetButtonAction()
        {
            viewPlayListButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    RequestUserPermission();
                    nowloadingImage.gameObject.SetActive(true);
                    musicItems = MusicHelper.Instance.GetMusicItems();
                    nowloadingImage.gameObject.SetActive(false);
                    Debug.Log($"musicItems:{musicItems.Count}");
                })
                .AddTo(this);

            prevButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                })
                .AddTo(this);

            stopButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    StopClip();
                })
                .AddTo(this);

            playButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (audioSource != default && audioSource.isPlaying)
                    {
                        PauseClip();
                    }
                    else if (isFirstPlay)
                    {
                        PlayClipAsync().Forget();
                    }
                })
                .AddTo(this);

            nextButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    NextPlaryClipAsync().Forget();
                })
                .AddTo(this);

            addMusicButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                })
                .AddTo(this);

            debugLogViewObject.SetActive(isViewDebugLog);
            debugLogViewButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    isViewDebugLog = !isViewDebugLog;
                    debugLogViewObject.SetActive(isViewDebugLog);
                })
                .AddTo(this);
        }

        private async UniTask<AudioClip> LoadAsync(MusicItem musicItem)
        {
            if (!File.Exists(musicItem.Path))
            {
                Debug.LogError("not exist");
                return null;
            }

            var loadedClip = await LoadAudioClipAsync(musicItem.Path);

            if (loadedClip == null)
            {
                Debug.LogError("load failure.");
                return null;
            }

            return loadedClip;
        }

        private async UniTask<AudioClip> LoadAudioClipAsync(string path)
        {
            AudioClip audioClip = null;
            var extension = Path.GetExtension(path);
            Debug.Log("extension : " + extension);
            var audioType = audioTypeDictionary[extension];

            var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType);
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Load success : " + www);
                audioClip = DownloadHandlerAudioClip.GetContent(www);
            }


            return audioClip;
        }

        private void AddPlayList(AudioClip audioClip, MusicItem musicItem)
        {
            var playItem = new PlayItem
            {
                AudioClip = audioClip,
                MusicItem = musicItem
            };

            playList.Add(playItem);
            Debug.Log($"AddPlayList:{playList.Count}");
        }

        private async UniTask PlayClipAsync()
        {
            Debug.Log("PlayClipAsync");
            playListIndex = 0;
            var musicItem = musicItems[playListIndex];

            var currentAudioClip = await LoadAsync(musicItem);
            AddPlayList(currentAudioClip, musicItem);

            var nextIndex = playListIndex + 1;
            if (musicItems.Count >= nextIndex)
            {
                var nextAudioClip = await LoadAsync(musicItems[nextIndex]);
                AddPlayList(nextAudioClip, musicItems[nextIndex]);
            }

            playButtonText.text = "Pause";
            isFirstPlay = false;
            PlayClip(playListIndex);
        }

        private void PlayClip(int index)
        {
            Debug.Log("PlayClip");
            if (index > playList.Count - 1) return;

            Debug.Log("index > playList.Count - 1");
            var playItem = playList[index];
            if (playItem.AudioClip == null || audioSource == null) return;
            if (audioSource.clip == playItem.AudioClip) return;

            audioSource.clip = playItem.AudioClip;
            musicTitleText.text = playItem.MusicItem.Title;
            musicArtistText.text = playItem.MusicItem.Artist;

            audioSource.Play();
            Debug.Log($"PlayTitle:{playItem.MusicItem.Title}");

            StartCoroutine(Checking(() => {
                NextPlaryClipAsync().Forget();
            }));
        }

        private IEnumerator Checking(Action callback)
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (!audioSource.isPlaying)
                {
                    callback();
                    break;
                }
            }
        }

        private async UniTask NextPlaryClipAsync()
        {
            Debug.Log("NextPlaryClipAsync");
            if (playListIndex + 1 > musicItems.Count - 1) return;
            Debug.Log("playListIndex + 1 > musicItems.Count - 1");
            playListIndex = playListIndex + 1;
            var musicItem = musicItems[playListIndex];

            var playItem = playList.FirstOrDefault(p => p.MusicItem.Id == musicItem.Id);
            if (playItem == null)
            {
                var currentAudioClip = await LoadAsync(musicItems[playListIndex]);
                AddPlayList(currentAudioClip, musicItems[playListIndex]);
            }

            var nextIndex = playListIndex + 1;
            if (nextIndex > musicItems.Count - 1)
            {
                var nextAudioClip = await LoadAsync(musicItems[nextIndex]);
                AddPlayList(nextAudioClip, musicItems[nextIndex]);
            }

            PlayClip(playListIndex);
        }

        private void PauseClip()
        {
            Debug.Log("PauseClip");
            if (audioSource == null)
            {
                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        private void StopClip()
        {
            if (audioSource == null)
            {
                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    public class PlayItem
    {
        public AudioClip AudioClip;
        public MusicItem MusicItem;

        public PlayItem() { }

        public PlayItem(AudioClip clip, MusicItem musicItem)
        {
            AudioClip = clip;
            MusicItem = musicItem;
        }
    }
}