using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Net;

public class AI_TextToSpeech : MonoBehaviour
{
    [System.Serializable]
    private class ApiKeyData
    {
        public string apiKey;
    }

    [System.Serializable]
    private class TTSRequest
    {
        public TTSInput input;
        public TTSVoice voice;
        public TTSAudioConfig audioConfig;
    }

    [System.Serializable]
    private class TTSInput
    {
        public string text;
    }

    [System.Serializable]
    private class TTSVoice
    {
        public string languageCode;
        public string name;
    }

    [System.Serializable]
    private class TTSAudioConfig
    {
        public string audioEncoding;
    }

    [System.Serializable]
    private class TTSResponse
    {
        public string audioContent;
    }

    private string apiKey;
    private string ttsUrl => $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";

    private AudioSource audioSource;

    private void Awake()
    {
        LoadApiKey();
    }

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void LoadApiKey()
    {
        TextAsset apiKeyFile = Resources.Load<TextAsset>("API_Keys/Google API Key TTS"); //api key .json file path in Resources folder
        if (apiKeyFile != null)
        {
            ApiKeyData data = JsonUtility.FromJson<ApiKeyData>(apiKeyFile.text);
            apiKey = data.apiKey;
            Debug.Log("API Key loaded successfully.");
        }
        else
        {
            Debug.LogError("API Key file not found! Make sure 'google_tts_key.json' is in the Resources folder.");
        }
    }

    public void Speak(string inputText)
    {
        Debug.Log("Speak() called with input: " + inputText);
        StartCoroutine(SendTTSRequest(inputText));
    }

    IEnumerator SendTTSRequest(string inputText)
    {
        var jsonRequest = new TTSRequest
        {
            input = new TTSInput { text = inputText },
            voice = new TTSVoice { languageCode = "en-US", name = "en-US-Wavenet-D" },
            audioConfig = new TTSAudioConfig { audioEncoding = "LINEAR16" }
        };

        string jsonData = JsonUtility.ToJson(jsonRequest);
        Debug.Log("TTS JSON Payload: " + jsonData);

        UnityWebRequest www = new UnityWebRequest(ttsUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending TTS request...");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("TTS request successful!");
            var response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
            byte[] audioBytes = System.Convert.FromBase64String(response.audioContent);
            Debug.Log("Decoded audio bytes length: " + audioBytes.Length);

            var audioClip = WavUtility.ToAudioClip(audioBytes, "TTS");
            if (audioClip == null)
            {
                Debug.LogError("AudioClip is null! WavUtility might not be working.");
            }
            else
            {
                audioSource.clip = audioClip;
                audioSource.volume = 1.0f;
                audioSource.spatialBlend = 0f;
                audioSource.Play();

                StartCoroutine(RemoveClipAfterPlay(audioClip.length));
                Debug.Log("Playing audio via custom AudioSource...");
            }
        }
        else
        {
            Debug.LogError($"TTS Error: {www.error}");
            Debug.LogError($"TTS Full Error: {www.downloadHandler.text}");
        }
    }

    IEnumerator RemoveClipAfterPlay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = null;
        Debug.Log("AudioClip removed from AudioSource.");
    }
}
