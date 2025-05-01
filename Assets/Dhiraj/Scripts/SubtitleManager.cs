using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEditor.Profiling.RawFrameDataView;


namespace Dhiraj
{
    public class SubtitleManager : MonoBehaviour
    {
        [Header("Dialogue UIs")]
        public CanvasGroup _DialogueUI;
        public TextMeshProUGUI _DialogueTextUI;

        public float audioLengthExpander = .35f;
        public bool isAudioPlaying = false;

        [SerializeField] SubtitleData demoData;

        private void Start()
        {
            //GetSubtitleTextsData(demoData);
        }

        public void GetSubtitleTextsData(SubtitleData data)
        {
            PlayAudioForCutScene(data);

        }

        private void PlayAudioForCutScene(SubtitleData data)
        {
            GameManager.Instance.CutSceneStatus(data.isCutscene);   // Un-comment this code once the game manager is implemented and you have integrated the cutscene part


            if (data.shouldRotate) LeanTween.rotate(GameManager.Instance._PlayerObject.gameObject, data.playerRotation, .3f).setEaseInCubic();    // Un-comment this code once the player is integrated and attach the player game object here

            _DialogueUI.alpha = 1;
            _DialogueTextUI.text = data.subtitleText;

            AudioManager.Instance.ControlDialouge(true, "FModEvent.FModDiaPrefix + data.audioClipName"); // Need to integr

            if (data.continueText)
            {
                LeanTween.delayedCall((AudioManager.Instance.GetAudioLength(/*AudioManager.Instance.diaEventInstance*/) + .1f), () =>
                {
                    PlayAudioForCutScene(data.nextSubtitle);
                });
                return;
            }
            LeanTween.delayedCall((AudioManager.Instance.GetAudioLength(/*AudioManager.Instance.diaEventInstance*/) + .1f), () =>
            {
                StopPlaying();

                // Add your code to execute after audio is played.

            });
            LeanTween.delayedCall(audioLengthExpander + 0.5f, () =>
            {
                StopPlaying();

                // Add your code to execute after audio is played.

            });
            isAudioPlaying = true;
        }

        private void StopPlaying()
        {
            GameManager.Instance.CutSceneStatus(false);
            _DialogueUI.alpha = 0;
            _DialogueTextUI.text = "";
            //AudioManager.Instance.ControlDialouge(false);
            isAudioPlaying = false;
        }

    }
}
