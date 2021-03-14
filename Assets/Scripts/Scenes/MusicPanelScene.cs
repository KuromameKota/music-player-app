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
        private Button nextButton = default;
        [SerializeField]
        private Button addMusicButton = default;

        [Header("DebugObject")]
        [SerializeField]
        private Button debugLogViewButton = default;
        [SerializeField]
        private GameObject debugLogViewObject = default;

        private List<PlayItem> playList = new List<PlayItem>();
        private int playListIndex = 0;
        private List<MusicItem> musicItems = new List<MusicItem>();

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
                    musicItems = MusicHelper.Instance.GetMusicItems();
                    Debug.Log($"playList:{musicItems.Count}");
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
                    PlayClip(playListIndex);
                })
                .AddTo(this);

            nextButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
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

        private async UniTask LoadAsync(MusicItem musicItem)
        {
            if (!File.Exists(musicItem.path))
            {
                Debug.LogError("not exist");
                return;
            }

            var loadedClip = await LoadAudioClipAsync(musicItem.path);

            if (loadedClip == null)
            {
                Debug.LogError("load failure.");
                return;
            }

            AddPlayList(loadedClip, musicItem);
        }

        private async UniTask<AudioClip> LoadAudioClipAsync(string path)
        {
            string[] files = Directory.GetFiles(path);
            AudioClip audioClip = null;

            foreach (string file in files)
            {
                var extension = Path.GetExtension(file);
                var audioType = GetAudioType(extension);

                var www = UnityWebRequestMultimedia.GetAudioClip(file, audioType);
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
        }

        private void PlayClip(int index)
        {
            if (playList[index].AudioClip == null || audioSource == null) return;
            if (audioSource.isPlaying || audioSource.clip == playList[index].AudioClip) return;

            audioSource.clip = playList[index].AudioClip;
            musicTitleText.text = playList[index].MusicItem.title;
            musicArtistText.text = playList[index].MusicItem.artist;

            audioSource.Play();
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

        private AudioType GetAudioType(string extension)
        {
            switch (extension)
            {
                case "acc":
                    return AudioType.ACC;
                case "aiff":
                    return AudioType.AIFF;
                case "it":
                    return AudioType.IT;
                case "mod":
                    return AudioType.MOD;
                case "mpeg":
                case "mp3":
                case "mp2":
                    return AudioType.MPEG;
                case "ogg":
                    return AudioType.OGGVORBIS;
                case "s3m":
                    return AudioType.S3M;
                case "wav":
                    return AudioType.WAV;
                case "xm":
                    return AudioType.XM;
                case "xma":
                    return AudioType.XMA;
                case "vag":
                    return AudioType.VAG;
                case "audioqueue":
                    return AudioType.AUDIOQUEUE;
                default:
                    return AudioType.UNKNOWN;
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