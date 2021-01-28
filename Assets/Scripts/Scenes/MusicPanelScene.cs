using UniRx;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using FantomLib;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class MusicPanelScene : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource = null;
    [SerializeField]
    private Button openStorageButton = null;
    [SerializeField]
    private Button debugLogViewButton = null;
    [SerializeField]
    private GameObject debugLogViewObject = null;
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

    private List<PlayItem> playlist = new List<PlayItem>();
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
    private bool isViewDebugLog = false;

    private string[] MimeTypes
    {
        get
        {
            if (mimeTypes == null || mimeTypes.Length == 0)
                mimeTypes = new string[] { AndroidMimeType.Audio.All };

            return mimeTypes;
        }
        set
        {
            if (value != null && value.Length > 0)
                mimeTypes = value;
            else
                mimeTypes = new string[] { AndroidMimeType.Audio.All };
        }
    }

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

        openStorageButton.OnClickAsObservable()
            .Subscribe(_ => 
            {
                OpenStorageToAdd();
            })
            .AddTo(this);
    }

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

    private void LoadAudioComplete(int idx, int dir = 1)
    {
        if (loadedState == LoadedState.Success)
        {
            if (IsValidIndex(idx))
            {
                playlist[idx].clip = loadedClip;
                PlayClip(idx);
            }
        }
    }

    private void PlayClip(int idx)
    {
        if (!IsValidIndex(idx))
            return;

        if (playlist[idx].clip == null || audioSource == null)
            return;

        if (!audioSource.isPlaying || audioSource.clip != playlist[idx].clip)
        {
            audioSource.clip = playlist[idx].clip;
            audioSource.Play();
        }
    }

    private bool IsValidIndex(int idx)
    {
        return (playlist != null && 0 <= idx && idx < playlist.Count);
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

    private void OpenStorageToAdd()
    {
#if UNITY_EDITOR
        Debug.Log("OpenStorageToAdd called.");
#elif UNITY_ANDROID
            AndroidPlugin.OpenStorageAudio(MimeTypes, gameObject.name, "ReceiveAddResult", "ReceiveAddError");
#endif
    }

    private void ReceiveAddResult(string result)
    {
        Debug.Log(String.Format("ReceiveAddResult:{0}", result));

        if (result[0] == '{')
        {
            var info = JsonUtility.FromJson<AudioInfo>(result);
            
            Debug.Log(String.Format("ReceiveAddResult path:{0}", info.path));
            Debug.Log(String.Format("ReceiveAddResult uri:{0}", info.uri));
            Debug.Log(String.Format("ReceiveAddResult fileUri:{0}", info.fileUri));
            Debug.Log(String.Format("ReceiveAddResult mimeType:{0}", info.mimeType));
            Debug.Log(String.Format("ReceiveAddResult title:{0}", info.title));
            Debug.Log(String.Format("ReceiveAddResult artist:{0}", info.artist));
            Debug.Log(String.Format("ReceiveAddResult name:{0}", info.name));
            Debug.Log(String.Format("ReceiveAddResult size:{0}", info.size));
            Debug.Log(String.Format("ReceiveAddResult duration:{0}", info.duration));

            if (!string.IsNullOrEmpty(info.path))
            {
                AddSong(info);
                //StartCoroutine(LoadToAudioClip(info.path));
                //StartCoroutine(LoadToAudioClipAndPlay(info.path));
            }
            else
            {
                ReceiveAddError("Failed to get path.");
            }
        }
        else
        {
            ReceiveAddError(result);
        }
    }

    private void ReceiveAddError(string message)
    {
        Debug.LogError(message);
    }

    public void AddSong(AudioInfo info)
    {
        if (string.IsNullOrEmpty(info.path))
        {
            Debug.LogError("path is not empty.");
            return;
        }

        string ext = Path.GetExtension(info.path).ToLower();
        if (!availableExtType.Contains(ext))
        {
            Debug.LogError("extension is not available.");
            return;
        }

        PlayItem data = new PlayItem(info.path, string.IsNullOrEmpty(info.title) ? info.name : info.title, info.artist);
        playlist.Add(data); 
        StartCoroutine(LoadAudio(info.path, playlist.Count - 1, LoadAudioComplete));
    }
}
