using UnityEngine;


namespace Dhiraj
{
    public class AudioManager : Singleton<AudioManager>
    {

        public float GetAudioLength()
        {
            return 1;
        }

        public void ControlDialouge(bool isPlay, string path = "null")
        {
            if (isPlay)
            {
                //diaEventInstance = RuntimeManager.CreateInstance(path);
                //diaEventInstance.start();
            }
            else
            {
                //diaEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}
