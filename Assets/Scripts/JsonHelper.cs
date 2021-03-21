using System;
using UnityEngine;

namespace MusicPlayer
{
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            if (json.StartsWith("[", StringComparison.Ordinal))
            {
                json = "{\"Items\":" + json + "}";
            }
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}