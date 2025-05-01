using UnityEngine;


namespace Dhiraj
{
    [CreateAssetMenu(fileName = "Subtitle Data", menuName = "Dhiraj/SubtitleData")]
    public class SubtitleData : ScriptableObject
    {
        [Header("Subtitle Content")]
        [TextArea(3, 10)]
        [Tooltip("The text to display for the subtitle.")]
        public string subtitleText;
        [Tooltip("The audio clip associated with this subtitle.")]
        public string audioClipName;

        [Header("Subtitle Sequence")]
        [Tooltip("The next subtitle in the sequence.")]
        public SubtitleData nextSubtitle;
        [Tooltip("If true, the subtitle automatically continues to the next.")]
        public bool continueText = false;

        [Tooltip("Indicates whether this subtitle is part of a cutscene.")]
        public bool isCutscene = false;

        [Tooltip("Indicates whether this subtitle should rotate or not.")]
        public bool shouldRotate = false;
        public Vector3 playerRotation;
    }
}


