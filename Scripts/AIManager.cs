using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public MPManager manager;

    public GameObject aiPrefab;
    public AIController[] agents;
    public int agentsRemaining;
    public Text remainingEnemies;

    public int score;



    // Use this for initialization
    void Start() {
        score = 0;

        UpdateAgentList();
        UpdateAgentsRemaining();
        UpdateEnemiesRemainingText();

        agents = gameObject.GetComponentsInChildren<AIController>();
    }


    void Update() {
        UpdateAgentList();
    }

    public void HandleDeath() {
        score += 100;
        manager.UpdateScoreBoard();

        UpdateAgentsRemaining();

        UpdateEnemiesRemainingText();
        if (agentsRemaining == 0) {

            if (score > MainMenuController.highScore) {
                GameObject.Find("RemoteScoreManager").GetComponent<RemoteScoreManager>().SetHighScore(score);
            }

            manager.EndGame();
        }
    }

    public string GetScore() {
        return score.ToString();
    }

    public void SetScore(int score) {
        this.score = score;
    }


    public AIManagerRecord ToRecord() {
        return new AIManagerRecord(score);
    }

    public void LoadAgents(List<AIControllerRecord> agentRecords) {

        UpdateAgentList();
        foreach (AIController agent in agents) {
            if (agent != null) {
                Destroy(agent.gameObject);
            }
        }

        UpdateAgentList();

        foreach (AIControllerRecord record in agentRecords) {
            GameObject newAgent = Instantiate(aiPrefab, record.position, record.rotation);
            newAgent.transform.GetComponent<AIController>().SetHealth(record.health);
            newAgent.transform.SetParent(transform);

            Transform[] restoredWayPoints = new Transform[record.waypoints.Length];

            for (int i = 0; i < record.waypoints.Length; i++) {
                restoredWayPoints[i] = GameObject.Find(record.waypoints[i]).transform;
            }

            newAgent.transform.GetComponent<AIController>().waypoints = restoredWayPoints;           
        }

        UpdateAgentList();
        
        UpdateEnemiesRemainingText();
        manager.UpdateScoreBoard();
    }

    private void UpdateNumberOfAgents() {
        agents = gameObject.GetComponentsInChildren<AIController>();
    }

    private void UpdateEnemiesRemainingText() {
        remainingEnemies.text = "Enemies Remaining: " + agentsRemaining;
    }

    public void UpdateAgentList() {
        agents = gameObject.GetComponentsInChildren<AIController>();
    }

    private void UpdateAgentsRemaining() {
        int wer = 0;
        int called = 0;
        foreach (AIController agent in agents) {
            called++;
            if (agent.state != AIController.AgentState.Dead && agent.state != AIController.AgentState.Disabled) {
                wer++;
            }
            agentsRemaining = wer;
        }
        Debug.Log("call count is " + called);
        Debug.Log(wer);
    }
}


[Serializable]
public struct AIManagerRecord {
    public int score;

    public AIManagerRecord(int score) {
        this.score = score;
    }
}
