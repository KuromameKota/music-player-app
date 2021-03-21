using Newtonsoft.Json;
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
            var items = JsonConvert.DeserializeObject<MusicItem[]>(json);

            return items.ToList();
        }
    }
}