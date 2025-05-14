using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AI_TextToSpeech : MonoBehaviour
{
    private string apiKey = "AIzaSyCREbUP8YpJCnpnR1OJBDwivQWa81Cet5s";
    private string ttsUrl => $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";

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
                GameObject audioObject = new GameObject("TTS_Audio");
                AudioSource audioSource = audioObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.volume = 1.0f; // You can increase this above 1.0 if needed (e.g., 2.0)
                audioSource.spatialBlend = 0f; // Ensure it's 2D sound
                audioSource.Play();

                UnityEngine.Object.Destroy(audioObject, audioClip.length);
                Debug.Log("Playing audio via custom AudioSource...");
            }
        }
        else
        {
            Debug.LogError($"TTS Error: {www.error}");
            Debug.LogError($"TTS Full Error: {www.downloadHandler.text}");
        }
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
}
