using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

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
        public InputActionReference leftRecordButton;
        public InputActionReference rightRecordButton;   
        public TMP_Text recordButtonText;

        [Header("LLM")]
        [SerializeField] private LLMChat llmChat;

        private void Awake()
        {
            microphoneRecord.OnRecordStop += OnRecordStop;

            if (leftRecordButton.action != null && rightRecordButton.action != null)
            {
                // Ensure the action is enabled
                if (!leftRecordButton.action.enabled)
                    leftRecordButton.action.Enable();

                // Ensure the action is enabled
                if (!rightRecordButton.action.enabled)
                    rightRecordButton.action.Enable();

                leftRecordButton.action.performed += OnGripPressed;
                leftRecordButton.action.canceled += OnGripReleased;

                rightRecordButton.action.performed += OnGripPressed;
                rightRecordButton.action.canceled += OnGripReleased;
            }
        }

        private void OnDestroy()
        {
            if (leftRecordButton.action != null && rightRecordButton.action != null)
            {
                leftRecordButton.action.performed -= OnGripPressed;
                leftRecordButton.action.canceled -= OnGripReleased;

                rightRecordButton.action.performed -= OnGripPressed;
                rightRecordButton.action.canceled -= OnGripReleased;
            }

            microphoneRecord.OnRecordStop -= OnRecordStop;
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
