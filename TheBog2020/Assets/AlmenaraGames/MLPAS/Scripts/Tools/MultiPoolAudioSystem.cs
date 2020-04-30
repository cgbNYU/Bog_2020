using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames
{
    public class MultiPoolAudioSystem
    {
        bool reset = false;
        public static bool isNULL = true;
        static MultiAudioManager m_audioManager;
        public static MultiAudioManager audioManager
        {
            get
            {
                if (isNULL)
                {
                    m_audioManager = MultiAudioManager.Instance; isNULL = false;
                }
                return m_audioManager;
            }
            set { m_audioManager = value; }
        }

      /*  public static void ResetStaticVars()
        {
            isNULL = true;
            m_audioManager = null;
        }*/

    }
}