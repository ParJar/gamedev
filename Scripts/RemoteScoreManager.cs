using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class HighScoreResult {
    public int Score;
    public string code; // error code
    public string message; // error message
}
public class RemoteScoreManager : MonoBehaviour {

    HighScoreResult currentScore;

    public string APPLICATION_ID = "78E16DFC-AF2F-63A9-FF4A-D5196AE5D400";
    public string REST_SECRET_KEY = "C6F8324A-8D1A-4B7D-8636-84BCFC7E5826";

    public static RemoteScoreManager Instance { get; private set; }
    void Awake() {

        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        DontDestroyOnLoad(gameObject);

        //StartCoroutine("GetHighScoreCR");

    }


    public void GetHighScore() {
        StartCoroutine("GetHighScoreCR");
    }

    public void SetHighScore(int newScore) {
        this.currentScore.Score = newScore;
        StartCoroutine("SetHighScoreCR");
    }

    public IEnumerator GetHighScoreCR() {


        string url = "https://api.backendless.com/78E16DFC-AF2F-63A9-FF4A-D5196AE5D400/C6F8324A-8D1A-4B7D-8636-84BCFC7E5826/data/HighScore/B40A603B-678C-3CE2-FFD1-6E60A97B1800";


        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();

        if (request.isNetworkError || request.isHttpError) {
            Debug.Log(request.error);
        }
        if (request.isDone) {
            this.currentScore = JsonUtility.FromJson<HighScoreResult>(request.downloadHandler.text);
        }

        if (GameObject.Find("Canvas").GetComponent<MainMenuController>()) {
            GameObject.Find("Canvas").GetComponent<MainMenuController>().UpdateHighScore(this.currentScore.Score);
        }
    }
    


    public IEnumerator SetHighScoreCR() {

        string url = "https://api.backendless.com/78E16DFC-AF2F-63A9-FF4A-D5196AE5D400/C6F8324A-8D1A-4B7D-8636-84BCFC7E5826/data/HighScore/B40A603B-678C-3CE2-FFD1-6E60A97B1800";

        string toSend = JsonUtility.ToJson(this.currentScore);
        Debug.Log(toSend);

        UnityWebRequest request = UnityWebRequest.Put(url, toSend);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();

        if (request.isNetworkError || request.isHttpError) {
            Debug.Log(request.error);
        }
        if (request.isDone) {
            Debug.Log("high score updated");
        }
    }

    void UpdateMainMenuScore() {
        MainMenuController.highScore = this.currentScore.Score;
    }
}