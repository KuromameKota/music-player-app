using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace MusicPlayer.Scenes.Play
{
    public interface IPlayView
    {
        /// <summary>
        /// �w��̃A�[�g���[�N��\�����܂��B
        /// </summary>
        /// <param name="sprite"></param>
        void DisplayArtwork(Sprite sprite);
        /// <summary>
        /// �w��̋Ȗ���\�����܂��B
        /// </summary>
        /// <param name="songName"></param>
        void DisplaySongName(string songName);
        /// <summary>
        /// �A�[�e�B�X�g����\�����܂��B
        /// </summary>
        /// <param name="artistName"></param>
        void DisplayArtistName(string artistName);
        /// <summary>
        /// �Đ��{�^�����������ꂽ���Ƃ�ʒm���܂��B
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> OnPlayAsObservable();
        /// <summary>
        /// �w��̎��Ԃ����݂̍Đ����ԂƂ��ĕ\�����܂��B
        /// </summary>
        /// <param name="currentTime"></param>
        void DisplayCurrentTime(DateTime currentTime);
        /// <summary>
        /// �w��̎��Ԃ��y�Ȃ̍ő厞�ԂƂ��ĕ\�����܂��B
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
