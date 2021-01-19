using UniRx;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.UI;

public class MusicPanelScene : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource = null;
    [SerializeField]
    private Button debugLogViewButton = null;
    [SerializeField]
    private GameObject debugLogViewObject = null;


    private string path = "/storage/emulated/0/Music/sample.mp3";

    private bool isViewDebugLog = false;
    private void Awake()
    {
        LoadAudio(path);

        debugLogViewObject.SetActive(isViewDebugLog);
        debugLogViewButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                isViewDebugLog = !isViewDebugLog;
                debugLogViewObject.SetActive(isViewDebugLog);
            })
            .AddTo(this);
    }

    public void LoadAudio(string path)
    {
        if (Path.GetExtension(path) == ".m4a")
        {
            Debug.Log("Not supported audio format.");
            return;
        }

        StartCoroutine(LoadToAudioClipAndPlay(path));
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

    private IEnumerator LoadToAudioClipAndPlay(string uri)
    {
        string[] files = Directory.GetFiles(uri);
        foreach (string path in files)
        {
            if (Path.GetExtension(path) != ".wav")
            {
                continue;
            }

            var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV);
            {
                yield return www.Send();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    var downloadClip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = downloadClip;
                    audioSource.Play();
                    Debug.Log("Load success : " + www);
                }
            }
        }
    }


}
