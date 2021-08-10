using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MainMenuController : MonoBehaviourPunCallbacks {

    public Text connectionStatus;
    public InputField playerNameInput;

    public GameObject activeScreen;
    public GameObject mainMenuScreen;
    public GameObject singlePlayerScreen;
    public GameObject multiplayerScreen;
    public GameObject optionsScreen;
    public GameObject controlsScreen;

    public string selectedMap;

    public static string playerName = "DefaultName";
    public static int highScore = 0;

    public Text highScoreText;

    public void Awake() {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerNameInput.text = playerName;

        PhotonNetwork.AutomaticallySyncScene = true;

        singlePlayerScreen.SetActive(false);
        multiplayerScreen.SetActive(false);
        optionsScreen.SetActive(false);
        controlsScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
        activeScreen = mainMenuScreen;

        selectedMap = "SPDusty";

        GameObject.Find("RemoteScoreManager").GetComponent<RemoteScoreManager>().GetHighScore();
    }

    public void ButtonHandlePlay() {
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.CreateRoom("");

    }
    public void ButtonHandleQuit() {
        Application.Quit();
    }

    public void SwitchScreen(GameObject newActiveScreen) {
        activeScreen.SetActive(false);
        activeScreen = newActiveScreen;
        newActiveScreen.SetActive(true);
    }

    public void ButtonHandleSelectItem(Toggle toggle) {

        if (toggle.isOn) {
            toggle.GetComponent<Image>().color = Color.white;
        } else {
            toggle.GetComponent<Image>().color = Color.black;
        }

        selectedMap = toggle.transform.name;

    }

    //network
    public void Disconnect() {
        if (PhotonNetwork.IsConnected && PhotonNetwork.OfflineMode == false) {
            PhotonNetwork.Disconnect();
        }
        
    }


    public void Connect() {
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();       
    }

    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();

        if (PhotonNetwork.OfflineMode) {
            return;
        }
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
        Debug.Log("Connected");
    }

    public void JoinMatch() {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom() {

        if (PhotonNetwork.OfflineMode) {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(selectedMap);
        } else if(PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            PhotonNetwork.LoadLevel("MPDusty");
        }
        base.OnJoinedRoom();
    }

    public void CreateMatch() {
        Create();
    }

    //E1
    public override void OnJoinRandomFailed(short returnCode, string message) {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }

    //E2
    public void Create() {
        PhotonNetwork.CreateRoom("");
    }

    public void Update() {

        //Debug.Log(PhotonNetwork.NetworkClientState);

        if  (!multiplayerScreen.activeSelf) {
            connectionStatus.text = "Offline";
            connectionStatus.color = Color.gray;
            return;
        }


        if (PhotonNetwork.IsConnectedAndReady) {
            connectionStatus.text = "Connected";
            connectionStatus.color = Color.green;
        } else {
            connectionStatus.text = "Connecting...";
            connectionStatus.color = Color.red;
        }
    }

    public void setName(Text name) {
        playerName = name.text;
        Debug.Log(playerName);
    }

    public void UpdateHighScore(int score) {
        highScore = score;
        highScoreText.text = "Highscore : " + score.ToString();
    }








}
