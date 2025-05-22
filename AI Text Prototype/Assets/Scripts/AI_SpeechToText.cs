using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading.Tasks;

public class AI_SpeechToText : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    public string ConvertedText { get; private set; }

    [System.Serializable]
    private class ApiKeyData
    {
        public string apiKey;
    }

    private string apiKey;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    private void Update()
    {
        if (recording)
        {
            Debug.Log("Mic position: " + Microphone.GetPosition(null));

            if (Microphone.GetPosition(null) >= clip.samples)
            {
                StopRecording();
            }
        }
    }

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found.");
            return;
        }

        string micDevice = Microphone.devices[0];
        clip = Microphone.Start(micDevice, false, 10, 44100);
        StartCoroutine(WaitForMicrophoneStart(micDevice));
    }

    private IEnumerator WaitForMicrophoneStart(string micDevice)
    {
        int safetyCounter = 0;

        while (Microphone.GetPosition(micDevice) <= 0)
        {
            safetyCounter++;
            if (safetyCounter > 100)
            {
                Debug.LogError("Microphone failed to start after waiting.");
                yield break;
            }

            yield return null;
        }

        Debug.Log("Microphone recording started.");
        text.color = Color.white;
        text.text = "Recording...";
        startButton.interactable = false;
        stopButton.interactable = true;
        recording = true;
    }

    public void StopRecording()
    {
        if (!Microphone.IsRecording(null))
        {
            Debug.LogWarning("Microphone was not recording.");
            return;
        }

        int position = Microphone.GetPosition(null);
        if (position <= 0)
        {
            Debug.LogWarning("No audio was captured.");
            Microphone.End(null);
            return;
        }

        Microphone.End(null);

        if (clip == null)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }

        int samplesCount = position * clip.channels;
        float[] samples = new float[samplesCount];

        try
        {
            clip.GetData(samples, 0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error extracting audio samples: " + ex.Message);
            return;
        }

        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecordingAsync();
    }
    private void LoadApiKey()
    {
        TextAsset apiKeyFile = Resources.Load<TextAsset>("HuggingFaceConfig");
        if (apiKeyFile != null)
        {
            ApiKeyData data = JsonUtility.FromJson<ApiKeyData>(apiKeyFile.text);
            apiKey = data.apiKey;
            Debug.Log("API Key loaded successfully.");
        }
        else
        {
            Debug.LogError("API Key file not found");
        }
    }
    private async void SendRecordingAsync()
    {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;

        try
        {
            string apiKey = "{apiKey}";
            string result = await HuggingFaceAPI.SendAudio(bytes, apiKey);
            text.color = Color.white;
            text.text = result;
            ConvertedText = result;
        }
        catch (System.Exception ex)
        {
            text.color = Color.red;
            text.text = $"Error: {ex.Message}";
        }

        startButton.interactable = true;
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
   
}
