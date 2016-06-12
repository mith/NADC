using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;

public class TopScoreHUD : NetworkBehaviour 
{

    class Entry {
        public GameObject EntryObject;
        public Text KillCount;
        public Text PlayerName;
    }

    GameObject topScorePanel;

    List<Entry> entries;

    Dictionary<string, int> scores;


    void Awake()
    {
        scores = new Dictionary<string, int>();
        entries = new List<Entry>();
    }

    public void RegisterEvents(GameManager gameManager)
    {
        gameManager.EventDeath += (player, killedBy) => {
            if (player != killedBy) {
                var name = killedBy.GetComponent<PlayerNameHUD>().PlayerName;
                RpcUpdateScores(name);
            }
        };
    }

    Entry createEntry(GameObject entryObject)
    {
        return new Entry {
            EntryObject = entryObject,
            KillCount = entryObject.GetComponent<Text>(),
            PlayerName = entryObject.transform.Find("PlayerName").GetComponent<Text>()
        };
    }

    public override void OnStartLocalPlayer()
    {
        topScorePanel = GameObject.Find("HUD/TopScorePanel");

        var numberOne = topScorePanel.transform.Find("NumberOne").gameObject;
        var numberTwo = topScorePanel.transform.Find("NumberTwo").gameObject;
        var numberThree = topScorePanel.transform.Find("NumberThree").gameObject;
        entries.Add(createEntry(numberOne));
        entries.Add(createEntry(numberTwo));
        entries.Add(createEntry(numberThree));

        foreach (var entry in entries) {
            entry.EntryObject.SetActive(false);
        }

        base.OnStartLocalPlayer();
    }

    [ClientRpc]
    void RpcUpdateScores(string name)
    {
        if (isLocalPlayer) {
            if (scores.ContainsKey(name)) {
                scores[name]++;
            } else {
                scores[name] = 1;
            }

            var sorted = scores.OrderByDescending((KeyValuePair<string, int> arg) => arg.Value);

            for (int i = 0; i < 3; i++) {
                if (sorted.Count() > i) {
                    UpdateEntry(entries[i], scores.ElementAt(i));
                } else {
                    entries[i].EntryObject.SetActive(false);
                }
            }
        }
    }

    void UpdateEntry(Entry entry, KeyValuePair<string, int> score)
    {
        entry.EntryObject.SetActive(true);
        entry.KillCount.text = score.Value.ToString();
        entry.PlayerName.text = score.Key;
    }
}
