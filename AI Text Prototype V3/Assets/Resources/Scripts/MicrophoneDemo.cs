using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;
using TMPro;
using UnityEngine.InputSystem;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        [HideInInspector] public string inputText;

        [Header("UI")]
        public InputActionReference recordButton;    
        public TMP_Text recordButtonText;

        [Header("LLM")]
        [SerializeField] private LLMChat llmChat;

        private void Awake()
        {
            microphoneRecord.OnRecordStop += OnRecordStop;
            if (recordButton != null && recordButton.action != null)
            {
                recordButton.action.performed += OnGripPressed;
                recordButton.action.canceled += OnGripReleased;
            }
        }

        private void OnGripPressed(InputAction.CallbackContext context)
        {
            microphoneRecord.StartRecord();
            recordButtonText.text = "Stop";
        }

        private void OnGripReleased(InputAction.CallbackContext context)
        {
            microphoneRecord.StopRecord();
            recordButtonText.text = "Record";
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            recordButtonText.text = "Record";

            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null) return;

            inputText = res.Result;

            // Automatically send to Large Language Model
            if (llmChat != null)
            {
                llmChat.SendFromMicrophone(inputText);
            }
            else if (OpenAIChat.Instance != null)
            {
                OpenAIChat.Instance.SendFromMicrophone(inputText);
            }
            else
            {
                Debug.LogError("No LLMChat or OpenAIChat component found.");
            }
        }
    }
}
