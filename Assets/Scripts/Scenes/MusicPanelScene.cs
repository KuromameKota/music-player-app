using UniRx;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.IO;

public class MusicPanelScene : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    private string path = "/storage/emulated/0/Music/sample.mp3";

    private void Start()
    {
        LoadAudio(path);
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

    private IEnumerator LoadToAudioClipAndPlay(string path)
    {
        if (audioSource == null || string.IsNullOrEmpty(path))
            yield break;

        if (!File.Exists(path))
        {
            Debug.Log("File not found.");
            yield break;
        }

        using (WWW www = new WWW("file://" + path))
        {
            while (!www.isDone)
            {
                yield return null;
            }

            AudioClip audioClip = www.GetAudioClip(false, true);
            if (audioClip.loadState != AudioDataLoadState.Loaded)
            {
                Debug.Log("Failed to load AudioClip.");
                yield break;
            }

            audioSource.clip = audioClip;
            audioSource.Play();
            Debug.Log("Load success : " + path);
        }
    }


}
