using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.SceneManagement;

public class MPManager : MonoBehaviourPunCallbacks {

    public string player_prefab;

    public Transform[] spawn_points;

    public GameObject scoreBoard;
    public GameObject boardElement;
    public GameObject pauseMenu;
    public GameObject gameEndBoard;
    public GameObject gameEndScores;

    public int winningScore = 3;


    public static bool paused = false;
    public static bool gameOver = false;
    private bool disconnecting = false;

    private AIManager aIEnabled;


    private ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();

    public Dictionary<int, Player> players;

    private void Start() {
        Spawn();

        paused = false;
        gameOver = false;
        gameEndBoard.SetActive(false);

        players = new Dictionary<int, Player>();
        players = PhotonNetwork.CurrentRoom.Players;

        playerProps["kills"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProps);

        if (GameObject.Find("AI")) {
            aIEnabled = GameObject.Find("AI").GetComponent<AIManager>();
        } else {
            GameObject.Find("Remaining").GetComponent<Text>().text = "Points to win: 0" + winningScore;
        }

    }

    public void Spawn() {

        if (aIEnabled) {
            EndGame();
            return;
        }

        Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
        PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation); 
    }

    public void UpdateScoreBoard() {

        if (!scoreBoard) {
            return;
        }

        foreach (Transform child in scoreBoard.transform) {
            GameObject.Destroy(child.gameObject);
        }

        foreach (KeyValuePair<int, Photon.Realtime.Player> p in players) {

            GameObject boardElem = Instantiate(boardElement, Vector3.zero, Quaternion.identity);
            boardElem.transform.Find("uiName").GetComponent<Text>().text = p.Value.NickName;

            if (aIEnabled) {
                boardElem.transform.Find("uiScore").GetComponent<Text>().text = aIEnabled.GetScore();
            } else {
                boardElem.transform.Find("uiScore").GetComponent<Text>().text = ((int)p.Value.CustomProperties["kills"]).ToString();
            }        
            boardElem.transform.SetParent(scoreBoard.transform);
        }
    }

    public void EndGame() {
        gameOver = true;

        scoreBoard.SetActive(false);
        gameEndBoard.SetActive(true);

        Cursor.lockState = CursorLockMode.None;

        scoreBoard = gameEndScores;
        scoreBoard.SetActive(true);
        UpdateScoreBoard();
        Cursor.visible = true;

    }

    public void TogglePaused() {
        if (disconnecting) {
            return;
        }
        paused = !paused;

        pauseMenu.transform.GetChild(0).gameObject.SetActive(paused);
        Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    public void Quit() {
        Debug.Log("quit");
        disconnecting = true;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    public void AddNewKill(Player killed, Player killer) {

        int killScore = (int)killer.CustomProperties["kills"];
        killScore++;
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("kills", killScore);
        PhotonNetwork.CurrentRoom.GetPlayer(killer.ActorNumber).SetCustomProperties(hashtable);
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps) {
        UpdateScoreBoard();

        if((int)target.CustomProperties["kills"] >= winningScore) {
            EndGame();
        }
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer) {
        base.OnPlayerEnteredRoom(otherPlayer);
        UpdateScoreBoard();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdateScoreBoard();
    }


}
