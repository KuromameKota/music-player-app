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
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class MusicPanelScene : MonoBehaviour
{
    [SerializeField] 
    private string[] mimeTypes;
    [Serializable]
    public class PlayItem
    {
        public AudioClip clip;
        public string path;
        public string title;
        public string artist;

        public PlayItem() { }

        public PlayItem(string path, string title, string artist)
            : this(null, path, title, artist) { }

        public PlayItem(AudioClip clip, string path, string title, string artist)
        {
            this.clip = clip;
            this.path = path;
            this.title = title;
            this.artist = artist;
        }


        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

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

    private HashSet<string> availableExtType = new HashSet<string>()
    {
        ".mp3", ".ogg", ".wav"
    };

    private AudioClip loadedClip = null;
    enum LoadedState
    {
        None,
        Success,
        NotExist,
        LoadFailure,
    }

    private LoadedState loadedState = LoadedState.None;
    private bool isViewDebugLog = true;

    //private string[] MimeTypes
    //{
    //    get
    //    {
    //        if (mimeTypes == null || mimeTypes.Length == 0)
    //            mimeTypes = new string[] { AndroidMimeType.Audio.All };

    //        return mimeTypes;
    //    }
    //    set
    //    {
    //        if (value != null && value.Length > 0)
    //            mimeTypes = value;
    //        else
    //            mimeTypes = new string[] { AndroidMimeType.Audio.All };
    //    }
    //}

    private void Awake()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
        debugLogViewObject.SetActive(isViewDebugLog);
        debugLogViewButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                isViewDebugLog = !isViewDebugLog;
                debugLogViewObject.SetActive(isViewDebugLog);
            })
            .AddTo(this);

        SetButtonAction();
    }

    private void SetButtonAction()
    {
        viewPlayListButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                AndroidPlugin.Instance.GetMusinItemList();
            })
            .AddTo(this);

        prevButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                var neko = AndroidPlugin.Instance.GetTestText();
                Debug.Log(neko);

                AndroidPlugin.Instance.ShowToast("Neko Neko Daisuki");
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
                OpenStorageToAdd();
            })
            .AddTo(this);
    }

    private void OpenStorageToAdd()
    {
//#if UNITY_EDITOR
//        Debug.Log("OpenStorageToAdd called.");
//#elif UNITY_ANDROID
//            AndroidPlugin.OpenStorageAudio(MimeTypes, gameObject.name, "ReceiveAddResult", "ReceiveAddError");
//#endif
    }

    private void ReceiveAddResult(string result)
    {
        //if (result[0] == '{')
        //{
        //    var info = JsonUtility.FromJson<AudioInfo>(result);

        //    Debug.Log(String.Format("ReceiveAddResult path:{0}", info.path));
        //    Debug.Log(String.Format("ReceiveAddResult uri:{0}", info.uri));
        //    Debug.Log(String.Format("ReceiveAddResult fileUri:{0}", info.fileUri));
        //    Debug.Log(String.Format("ReceiveAddResult mimeType:{0}", info.mimeType));
        //    Debug.Log(String.Format("ReceiveAddResult title:{0}", info.title));
        //    Debug.Log(String.Format("ReceiveAddResult artist:{0}", info.artist));
        //    Debug.Log(String.Format("ReceiveAddResult name:{0}", info.name));
        //    Debug.Log(String.Format("ReceiveAddResult size:{0}", info.size));
        //    Debug.Log(String.Format("ReceiveAddResult duration:{0}", info.duration));

        //    if (!string.IsNullOrEmpty(info.path))
        //    {
        //        AddSong(info);
        //        //StartCoroutine(LoadToAudioClip(info.path));
        //    }
        //    else
        //    {
        //        ReceiveAddError("Failed to get path.");
        //    }
        //}
        //else
        //{
        //    ReceiveAddError(result);
        //}
    }

    private void ReceiveAddError(string message)
    {
        Debug.LogError(message);
    }

    //public void AddSong(AudioInfo info)
    //{
    //    if (string.IsNullOrEmpty(info.path))
    //    {
    //        Debug.LogError("path is not empty.");
    //        return;
    //    }

    //    string ext = Path.GetExtension(info.path).ToLower();
    //    if (!availableExtType.Contains(ext))
    //    {
    //        Debug.LogError("extension is not available.");
    //        return;
    //    }

    //    PlayItem data = new PlayItem(info.path, string.IsNullOrEmpty(info.title) ? info.name : info.title, info.artist);
    //    playList.Add(data);
    //    StartCoroutine(LoadAudio(info.path, playList.Count - 1, LoadAudioComplete));
    //}

    private IEnumerator LoadAudio(string path, int idx, Action<int, int> completeCallback, int dir = 0)
    {
        loadedClip = null;
        loadedState = LoadedState.None;

        if (!File.Exists(path))
        {
            Debug.LogError("not exist");
            loadedState = LoadedState.NotExist;
            completeCallback(idx, dir);
            yield break;
        }

        yield return StartCoroutine(LoadToAudioClip(path));

        if (loadedClip == null || loadedClip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogError("load failure.");
            loadedState = LoadedState.LoadFailure;
            loadedClip = null;
            completeCallback(idx, dir);
            yield break;
        }

        loadedState = LoadedState.Success;
        completeCallback(idx, dir);
    }

    private IEnumerator LoadToAudioClip(string path)
    {
        using (WWW www = new WWW("file://" + path))
        {
            while (!www.isDone)
                yield return null;

            loadedClip = www.GetAudioClip(false, true);
        }
    }

    private IEnumerator LoadToAudioClipAndPlay(string uri)
    {
        string[] files = Directory.GetFiles(uri);
        foreach (string path in files)
        {
            var neko = Path.GetExtension(path);
            if (!availableExtType.Contains(neko))
            {
                Debug.LogError("extension is not available.");
                continue;
            }

            var www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    loadedClip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log("Load success : " + www);
                }
            }
        }
    }

    private void LoadAudioComplete(int index, int dir = 1)
    {
        if (loadedState == LoadedState.Success)
        {
            if (IsValidIndex(index))
            {
                playListIndex = index;
                playList[index].clip = loadedClip;
                PlayClip(index);
            }
        }
    }

    private void PlayClip(int index)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        if (playList[index].clip == null || audioSource == null)
        {
            return;
        }

        if (!audioSource.isPlaying || audioSource.clip != playList[index].clip)
        {
            audioSource.clip = playList[index].clip;

            //musicImage = playList[index].clip;
            musicTitleText.text = playList[index].title;
            musicArtistText.text = playList[index].artist;

            audioSource.Play();
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

    private bool IsValidIndex(int idx)
    {
        return (playList != null && 0 <= idx && idx < playList.Count);
    }

    private async UniTask<string> UniTaskTest(string uri)
    {
        var uwr = UnityWebRequest.Get(uri);
        await uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            throw new Exception(uwr.error);
        }

        return uwr.downloadHandler.text;
    }

}
