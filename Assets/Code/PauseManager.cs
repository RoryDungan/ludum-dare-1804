using System;
using UnityEngine;

namespace Assets
{
    class PauseManager : Singleton<PauseManager>
    {
        private int pauseCount;

        public void Pause()
        {
            pauseCount++;

            Time.timeScale = 0f;
        }

        public void Unpause()
        {
            pauseCount = Math.Max(0, pauseCount - 1);

            if (pauseCount <= 0)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
