using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class SaveController : MonoBehaviour {

    private static string savefileName = "Assets/Saves/game.xml";

    void Awake() {
        savefileName = "Assets/Saves/" + SceneManager.GetActiveScene().name + ".xml";
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F2)) {
            Load();
        }
    }

    public void Save() {
        XmlDocument xmlDocument = new XmlDocument();
        LevelData levelData = ToRecord();
        XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
        using (MemoryStream stream = new MemoryStream()) {
            serializer.Serialize(stream, levelData);
            stream.Position = 0;
            xmlDocument.Load(stream);
            xmlDocument.Save(savefileName);
        }
    }

    public void Load() {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(savefileName);
        string xmlString = xmlDocument.OuterXml;

        LevelData record;
        using (StringReader read = new StringReader(xmlString)) {
            XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
            using (XmlReader reader = new XmlTextReader(read)) {
                record = (LevelData)serializer.Deserialize(reader);
            }
        }

        ProcessLevelData(record);
    }

    void ProcessLevelData(LevelData levelData) {

        FPSPlayerController player = FindObjectOfType<FPSPlayerController>();
        player.transform.position = levelData.playerRecord.position;
        player.transform.rotation = levelData.playerRecord.rotation;
        player.SetHealth(levelData.playerRecord.health);

        AIManager aiManager = FindObjectOfType<AIManager>();     
        aiManager.LoadAgents(levelData.aiControllers);
        aiManager.SetScore(levelData.aiManagerRecord.score);

    }


    private LevelData ToRecord() {
        FPSPlayerController player = FindObjectOfType<FPSPlayerController>();
        AIManager aiManager = FindObjectOfType<AIManager>();
        aiManager.UpdateAgentList();
        List<AIControllerRecord> aiControllerRecords = new List<AIControllerRecord>();

        foreach (AIController agent in aiManager.agents) {
            aiControllerRecords.Add(agent.ToRecord());
        }

        return new LevelData(player.ToRecord(), aiManager.ToRecord(), aiControllerRecords);
    }

}

[Serializable]
public struct LevelData {
    public PlayerRecord playerRecord;
    public AIManagerRecord aiManagerRecord;
    public List<AIControllerRecord> aiControllers;



    public LevelData(PlayerRecord player, AIManagerRecord aiManager, List<AIControllerRecord> aiControllers) {
        this.playerRecord = player;
        this.aiManagerRecord = aiManager;
        this.aiControllers = aiControllers;
    }
}
