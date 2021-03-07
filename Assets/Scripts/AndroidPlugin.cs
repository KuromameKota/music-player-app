using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AndroidPlugin
{
    public static readonly Lazy<AndroidPlugin> _instance = new Lazy<AndroidPlugin>(() => new AndroidPlugin());
    public static AndroidPlugin Instance => _instance.Value;

    public List<MusicItem> MusicItems => musicItems;

    private List<MusicItem> musicItems = new List<MusicItem>();

    public void GetMusicItemList()
    {
        var androidSystem = new AndroidJavaObject("com.kuromame.musicitemhelper.MusicItemHelper");

        if (Application.platform == RuntimePlatform.Android)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            var text = androidSystem.Call<string>("getTestString");

            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            var json = androidSystem.CallStatic<string>("getMusicItems", context);
            Debug.Log(json);
        }
    }

    public void ShowToast(string text)
    {
        var androidSystem = new AndroidJavaObject("com.kuromame.toastlibrary.ToastLibrary");

        if (Application.platform == RuntimePlatform.Android)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            var javaString = new AndroidJavaObject("java.lang.String", text);
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            androidSystem.Call("showToast", context, javaString);
        }
    }

    private void OnDestroy()
    {
    }
}

public class MusicItem
{
    public long id;
    public string artist;
    public string title;
    public string album;
    public int truck;
    public long duration;
    public string path;
}
