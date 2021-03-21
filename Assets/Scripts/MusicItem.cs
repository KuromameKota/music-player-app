using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace MusicPlayer
{
    [JsonObject]
    public class MusicItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("albumId")]
        public long AlbumId { get; set; }

        [JsonProperty("artistId")]
        public long ArtistId { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("trackNo")]
        public int TrackNo { get; set; }
    }
}
