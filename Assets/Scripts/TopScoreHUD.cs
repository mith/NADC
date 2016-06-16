using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;

public class TopScoreHUD : NetworkBehaviour 
{

    struct ScoreHudLine {
        public GameObject EntryObject;
        public Text KillCount;
        public Text PlayerName;
    }

    struct Score {
        public string PlayerName;
        public int KillCount;
    }

    GameObject topScorePanel;

    List<ScoreHudLine> hudObjects;

    Dictionary<string, int> scores;

    class SyncListScore : SyncListStruct<Score> { }

    SyncListScore displayScores;

    void Awake()
    {
        scores = new Dictionary<string, int>();
        hudObjects = new List<ScoreHudLine>();
        displayScores = new SyncListScore();
    }

    public void RegisterEvents(GameManager gameManager)
    {
        gameManager.EventDeath += (player, killedBy) => {
            if (player != killedBy) {
                var name = killedBy.GetComponent<PlayerNameHUD>().PlayerName;
                if (scores.ContainsKey(name)) {
                    scores[name]++;
                } else {
                    scores[name] = 1;
                }

                var sorted = scores.OrderByDescending((KeyValuePair<string, int> arg) => arg.Value)
                    .Take(3).Select(s => new Score { PlayerName = s.Key, KillCount = s.Value });
                displayScores.Clear();
                foreach (var score in sorted) {
                    displayScores.Add(score);
                }
            }
        };
    }

    ScoreHudLine createHudLine(GameObject entryObject)
    {
        return new ScoreHudLine {
            EntryObject = entryObject,
            KillCount = entryObject.GetComponent<Text>(),
            PlayerName = entryObject.transform.Find("PlayerName").GetComponent<Text>()
        };
    }

    public void Start()
    {
        if (!isClient)
            return;
        
        topScorePanel = GameObject.Find("HUD/TopScorePanel");

        var numberOne = topScorePanel.transform.Find("NumberOne").gameObject;
        var numberTwo = topScorePanel.transform.Find("NumberTwo").gameObject;
        var numberThree = topScorePanel.transform.Find("NumberThree").gameObject;
        hudObjects.Add(createHudLine(numberOne));
        hudObjects.Add(createHudLine(numberTwo));
        hudObjects.Add(createHudLine(numberThree));

        foreach (var entry in hudObjects) {
            entry.EntryObject.SetActive(false);
        }

        displayScores.Callback = (SyncList<Score>.Operation op, int itemIndex) => {
            for (int i = 0; i < 3; i++) {
                if (displayScores.Count() > i) {
                    updateHudLine(hudObjects[i], displayScores.ElementAt(i));
                } else {
                    hudObjects[i].EntryObject.SetActive(false);
                }
            }
        };

        base.OnStartClient();
    }


    void updateHudLine(ScoreHudLine entry, Score score)
    {
        entry.EntryObject.SetActive(true);
        entry.KillCount.text = score.KillCount.ToString();
        entry.PlayerName.text = score.PlayerName;
    }
}
