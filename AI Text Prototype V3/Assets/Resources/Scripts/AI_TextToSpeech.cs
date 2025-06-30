using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Net;

public class AI_TextToSpeech : MonoBehaviour
{
    public string languageCode = "en-US";
    public string voiceName = "en-US-Wavenet-F"; // ✅ Change this to female voice

    [System.Serializable]
    private class ApiKeyData { public string apiKey; }
    [System.Serializable]
    private class TTSRequest { public TTSInput input; public TTSVoice voice; public TTSAudioConfig audioConfig; }
    [System.Serializable]
    private class TTSInput { public string text; }
    [System.Serializable]
    private class TTSVoice { public string languageCode; public string name; }
    [System.Serializable]
    private class TTSAudioConfig { public string audioEncoding; }
    [System.Serializable]
    private class TTSResponse { public string audioContent; }

    private string apiKey;
    private string ttsUrl => $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";
    private AudioSource audioSource;

    private void Awake()
    {
        LoadApiKey();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void LoadApiKey()
    {
        TextAsset apiKeyFile = Resources.Load<TextAsset>("API_Keys/Google API Key TTS");
        if (apiKeyFile != null)
        {
            ApiKeyData data = JsonUtility.FromJson<ApiKeyData>(apiKeyFile.text);
            apiKey = data.apiKey;
            Debug.Log("API Key loaded successfully.");
        }
        else
        {
            Debug.LogError("API Key file not found in Resources folder.");
        }
    }

    public void Speak(string inputText)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Cannot speak: API key is missing.");
            return;
        }
        StartCoroutine(SendTTSRequest(inputText));
    }

    IEnumerator SendTTSRequest(string inputText)
    {
        var jsonRequest = new TTSRequest
        {
            input = new TTSInput { text = inputText },
            voice = new TTSVoice { languageCode = languageCode, name = voiceName },
            audioConfig = new TTSAudioConfig { audioEncoding = "LINEAR16" }
        };

        string jsonData = JsonUtility.ToJson(jsonRequest);
        UnityWebRequest www = new UnityWebRequest(ttsUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            TTSResponse response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
            byte[] audioBytes = System.Convert.FromBase64String(response.audioContent);
            AudioClip clip = WavUtility.ToAudioClip(audioBytes, "TTS_Female");
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.volume = 1f;
                audioSource.spatialBlend = 0f;
                audioSource.Play();
                StartCoroutine(RemoveClipAfterPlay(clip.length));
            }
            else Debug.LogError("Failed to create AudioClip.");
        }
        else
        {
            Debug.LogError($"TTS Error: {www.error}");
            Debug.LogError($"Full Error: {www.downloadHandler.text}");
        }
    }

    IEnumerator RemoveClipAfterPlay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = null;
    }
}