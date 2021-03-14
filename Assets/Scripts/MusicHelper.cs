using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MusicPlayer
{
    public class MusicHelper
    {
        public static readonly Lazy<MusicHelper> _instance = new Lazy<MusicHelper>(() => new MusicHelper());
        public static MusicHelper Instance => _instance.Value;

        public List<MusicItem> GetMusicItems()
        {
            var json = AndroidPlugin.Instance.GetMusicItemList();
            var items = JsonHelper.FromJson<MusicItem>(json);

            return items.ToList();
        }
    }

    public class MusicItem
    {
        public long id;
        public long albumId;
        public long artistId;
        public string path;
        public string title;
        public string album;
        public string artist;
        public string uri;
        public long duration;
        public int trackNo;
    }
}