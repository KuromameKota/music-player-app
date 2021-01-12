using System;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public static class UniRxExtensions
    {
        /// <summary>
        /// lockTimeを指定した場合クリックした後にlockTimeの終了までクリックをできないようにする。
        /// </summary>
        /// <param name="button"></param>
        /// <param name="lockTime"></param>
        /// <returns></returns>
        public static IObservable<Unit> OnClickAsObservable(this Button button, TimeSpan? lockTime = null)
        {
            var onClick = button.onClick.AsObservable();

            if (lockTime.HasValue && lockTime.Value != TimeSpan.Zero)
            {
                onClick = onClick.ThrottleFirst(lockTime.Value);
            }

            return onClick;
        }
    }
}