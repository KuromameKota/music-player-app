using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace MusicPlayer.Scenes.Play
{
    public interface IPlayView
    {
        /// <summary>
        /// 指定のアートワークを表示します。
        /// </summary>
        /// <param name="sprite"></param>
        void DisplayArtwork(Sprite sprite);
        /// <summary>
        /// 指定の曲名を表示します。
        /// </summary>
        /// <param name="songName"></param>
        void DisplaySongName(string songName);
        /// <summary>
        /// アーティスト名を表示します。
        /// </summary>
        /// <param name="artistName"></param>
        void DisplayArtistName(string artistName);
        /// <summary>
        /// 再生ボタンが押下されたことを通知します。
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> OnPlayAsObservable();
        /// <summary>
        /// 指定の時間を現在の再生時間として表示します。
        /// </summary>
        /// <param name="currentTime"></param>
        void DisplayCurrentTime(DateTime currentTime);
        /// <summary>
        /// 指定の時間を楽曲の最大時間として表示します。
        /// </summary>
        /// <param name="maxTime"></param>
        void DisplayMaxTime(DateTime maxTime);
    }

    public class PlayView : MonoBehaviour, IPlayView
    {
        [SerializeField]
        private Image artworkImage = default;
        [SerializeField]
        private Text songNameText = default;
        [SerializeField]
        private Text artistNameText = default;
        [SerializeField]
        private Slider timeSlider = default;
        [SerializeField]
        private Button playButton = default;
        [SerializeField]
        private Text currentTimeText = default;
        [SerializeField]
        private Text maxTimeText = default;

        public void DisplayArtwork(Sprite sprite)
        {
            artworkImage.sprite = sprite;
        }

        public void DisplaySongName(string songName)
        {
            songNameText.text = songName;
        }

        public void DisplayArtistName(string artistName)
        {
            artistNameText.text = artistName;
        }

        public IObservable<Unit> OnPlayAsObservable()
        {
            return playButton.OnClickAsObservable();
        }

        public void DisplayCurrentTime(DateTime currentTime)
        {
            currentTimeText.text = $"{currentTime.Minute}:{currentTime.Second}";
        }

        public void DisplayMaxTime(DateTime maxTime)
        {
            maxTimeText.text = $"{maxTime.Minute}:{maxTime.Second}";
        }
    }
}
