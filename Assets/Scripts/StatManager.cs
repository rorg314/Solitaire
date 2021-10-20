using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatManager : MonoBehaviour {

    public static StatManager Instance { get; protected set; }

    public GameObject statPanel;

    [Serializable]
    public class allStats {
        public List<StatObject> allStatsList;
    }

    public allStats allStatObjects;
    
    public StatObject currentModeStats;

    public StatObject defaultStats = new StatObject {
        totalWins = 0,
        bestMoves = 0,
        bestTime = 0,
        totalGamesPlayed = 0,
        totalLose = 0,
        totalWinPercentage = 0,
        cardBackName = "CardBack_0",
        backgroundColour = new Color(0.2235294f, 0.3764706f, 0.2156863f, 1f)
    };

    public StatObject copyStats(StatObject thiss, StatObject other) {
        thiss.totalWins = other.totalWins;
        thiss.bestMoves = other.bestMoves;
        thiss.bestTime = other.bestTime;
        thiss.totalGamesPlayed = other.totalGamesPlayed;
        thiss.totalLose = other.totalLose;
        thiss.totalWinPercentage = other.totalWinPercentage;
        return thiss;
    }


    public void Start() {
        Instance = this;
        //currentModeStats = loadStats();
        //allStats = loadStats();
        allStatObjects = loadStats();

        //if no previous stats exist set current stats to default
        //if (allStatObjects == null) {
            //List<StatObject> allDefaultStatsList = new List<StatObject>();
            //allStats allStatObjects = new allStats(); 
            
            //allStatObjects.allStatsList = allDefaultStatsList;
        
    }

    public allStats loadStats() {
        string statString = SaveSystem.LoadStats();

        //If found stat file
        if (statString != null) {
            Debug.Log("Found stat file");
            allStats allStats = JsonUtility.FromJson<allStats>(statString);

            //Set card back colour from stat file
            string cardBackName = allStats.allStatsList[0].cardBackName;
            CardSpriteManager.Instance.cardBackSprite = CardSpriteManager.Instance.cardFilenameSpritesMap[cardBackName];
            Camera.main.backgroundColor = allStats.allStatsList[0].backgroundColour;
            return allStats;

            
        }
        
        allStatObjects.allStatsList = new List<StatObject>();
        
        for (int i = 0; i < 3; i++) {
            StatObject dummyStatObject = new StatObject();
            dummyStatObject = copyStats(dummyStatObject, defaultStats);
            allStatObjects.allStatsList.Add(dummyStatObject);
            //allDefaultStatsList.Add(defaultStats);
        }

        return allStatObjects;
    }
    
    //Called after game win or lose
    public void updateStats(bool won, int numMoves, float gameTime) {
        //Update current mode stat object only

        //currentModeStats = allStatObjects.allStatsList[(int)MoveManager.Instance.drawMode];
        currentModeStats = allStatObjects.allStatsList[drawMode(MoveManager.Instance.drawNum)];

        if (currentModeStats.bestTime == 0) { currentModeStats.bestTime = gameTime; }
        if(currentModeStats.bestMoves == 0) { currentModeStats.bestMoves = numMoves; }

        if(numMoves < currentModeStats.bestMoves && numMoves !=0) { currentModeStats.bestMoves = numMoves; }
        if(gameTime < currentModeStats.bestTime && gameTime != 0) { currentModeStats.bestTime = gameTime; }

        currentModeStats.totalGamesPlayed++;
        
        if (won) {
            currentModeStats.totalWins++;
        }
        else {
            currentModeStats.totalLose++;
        }

        float winPercent = (float)currentModeStats.totalWins / currentModeStats.totalGamesPlayed  ;
        
        currentModeStats.totalWinPercentage = (float)winPercent * 100f;

        //Save card back and background colour (saved into each mode stat)
        for (int i = 0; i < allStatObjects.allStatsList.Count; i++) {
            allStatObjects.allStatsList[i].cardBackName = CardSpriteManager.Instance.cardBackSprite.name;
            allStatObjects.allStatsList[i].backgroundColour = Camera.main.backgroundColor;
        }

        saveStats();
        StatUI.Instance.updateStatPanel();
    }


    public void saveStats() {

        string statString = JsonUtility.ToJson(allStatObjects);

        SaveSystem.SaveStats(statString);
    }

    public void resetStats() {
        currentModeStats = defaultStats;
        saveStats();
    }

    [Serializable]
    public class StatObject {
        public int totalWins;
        public int totalLose;
        public int totalGamesPlayed;
        public int bestMoves;
        public float bestTime;
        public float totalWinPercentage;
        public string cardBackName;
        public Color backgroundColour;
    }

    public int drawMode(int drawNum) {
        int drawMode = 0;
        if(drawNum == 1) { drawMode = 0; }
        if (drawNum == 3) { drawMode = 1; }
        if (drawNum == 6) { drawMode = 2; }
        return drawMode;
    }
    


}
