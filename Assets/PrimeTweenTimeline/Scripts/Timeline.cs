#if PRIME_TWEEN && ODIN_INSPECTOR
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace PTT
{
    public class Timeline : MonoBehaviour
    {
        [HideInInspector]
        public string label = "Timeline";

        /// <summary>
        ///     这个变量在运行时没有作用，仅在编辑器下编辑轨道时记录当前选择的轨道用。
        /// </summary>
        [SerializeField] [HideInInspector]
        internal int selectedIndex = -1;

        [SerializeField] [SerializeReference] [HideInInspector]
        internal List<ITimelineTrack> sequence = new();

        public Sequence Controller { get; private set; }

        public bool IsPlaying => Controller.isAlive;

        public bool IsPaused => Controller.isAlive && Controller.isPaused;

        public Sequence Play()
        {
            if (Controller.isAlive)
            {
                if (Controller.isPaused)
                {
                    Sequence controller = Controller;
                    controller.isPaused = false;
                }

                return Controller;
            }

            if (sequence.Count == 0) return default;

            var seq = Sequence.Create();

            foreach (ITimelineTrack track in sequence)
            {
                if (track.IsActive) track.Create(ref seq, gameObject);
            }

            Controller = seq;

            return Controller;
        }

        public void Paused()
        {
            if (Controller.isAlive)
            {
                Sequence controller = Controller;
                controller.isPaused = true;
            }
        }

        public void Stop()
        {
            if (Controller.isAlive)
            {
                Sequence controller = Controller;
                controller.elapsedTime = 0;
                controller.Stop();
            }
        }

        public void Complete()
        {
            if (Controller.isAlive)
            {
                Controller.Complete();
            }
            else
            {
                Play();
                Controller.Complete();
            }
        }
    }
}
#endif