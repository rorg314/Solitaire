using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour {

    public static StatUI Instance { get; protected set; }

    public GameObject panel;
    public GameObject dropdown;
    Text[] allText;
    StatManager.StatObject stats;

    private void Start() {
        Instance = this;
        updateStatPanel();
    }

    public void updateStatPanel() {
        allText = panel.GetComponentsInChildren<Text>();
        //Display current stats for game mode in progress
        stats = StatManager.Instance.allStatObjects.allStatsList[0];
        foreach (Text text in allText) {
            if(text.name == "Played") {
                text.text = "GAMES PLAYED: " + stats.totalGamesPlayed;
            }
            if (text.name == "Wins") {
                text.text = "GAMES WON: " + stats.totalWins;
            }
            if (text.name == "Lose") {
                text.text = "GAMES LOST: " + stats.totalLose;
            }
            if (text.name == "Percentage") {
                text.text = "WIN RATE: " + string.Format("{0:00.00}", stats.totalWinPercentage)+"%";
            }
            if (text.name == "Moves") {
                text.text = "BEST MOVES: " + stats.bestMoves;
            }
            if (text.name == "Time") {
                text.text = "BEST TIME: " + string.Format("{0:00:00}", stats.bestTime);
            }
        }

    }
    
    public void dropdownChanged(int val) {
        if(val == 0) { viewStats(0); }
        if (val == 1) { viewStats(1); }
        if (val == 2) { viewStats(2); }
    }
    
    //Used to select currently displayed stats
    public void viewStats(int drawMode) {
        //View stats for another game mode
        allText = panel.GetComponentsInChildren<Text>();
        //Display current stats for game mode selected
        stats = StatManager.Instance.allStatObjects.allStatsList[drawMode];
        foreach (Text text in allText) {
            if (text.name == "Played") {
                text.text = "GAMES PLAYED: " + stats.totalGamesPlayed;
            }
            if (text.name == "Wins") {
                text.text = "GAMES WON: " + stats.totalWins;
            }
            if (text.name == "Lose") {
                text.text = "GAMES LOST: " + stats.totalLose;
            }
            if (text.name == "Percentage") {
                text.text = "WIN RATE: " + string.Format("{0:00.00}", stats.totalWinPercentage) + "%";
            }
            if (text.name == "Moves") {
                text.text = "BEST MOVES: " + stats.bestMoves;
            }
            if (text.name == "Time") {
                text.text = "BEST TIME: " + string.Format("{0:00:00}", stats.bestTime);
            }
        }
    }

}
