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

    public void GetMusinItemList()
    {
        var androidSystem = new AndroidJavaObject("com.kuromame.musicitem.MusicItem");

        if (Application.platform == RuntimePlatform.Android)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(
              () =>
              {
                  var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                  var result = androidSystem.Call<string>("getItems", context);
                  Debug.Log($"result:{result}");

                  musicItems = JsonUtility.FromJson<List<MusicItem>>(result);

                  Debug.Log($"MusicItem:{musicItems.Count()}");
              }
              ));
        }
    }

    public void ShowToast(string text)
    {
        var androidSystem = new AndroidJavaObject("com.kuromame.toastlibrary.ToastLibrary");

        if (Application.platform == RuntimePlatform.Android)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(
              () =>
              {
                  var Toast = new AndroidJavaClass("android.widget.Toast");
                  var javaString = new AndroidJavaObject("java.lang.String", text);
                  var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                  androidSystem.Call("showToast", context, javaString);
              }
              ));
        }
    }

    public string GetTestText()
    {
        var _javaClass = new AndroidJavaObject("com.kuromame.toastlibrary.ToastLibrary");

        if (_javaClass != null)
        {
            return _javaClass.Call<string>("getText");
        }
        else
        {
            return "error: java calss is null";
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
