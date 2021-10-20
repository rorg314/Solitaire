using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveSystem {

    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";

    public static void Init() {
        //Test if save folder exists
        if (!Directory.Exists(SAVE_FOLDER)) {
            //Create save folder
            Directory.CreateDirectory(SAVE_FOLDER);
        }

    }

    public static void SaveGame(string saveString) {

        File.WriteAllText(SAVE_FOLDER + "save.txt", saveString);
    }

    public static void SaveStats (string statString) {

        File.WriteAllText(SAVE_FOLDER + "stats.txt", statString);
    }

    public static string LoadGame() {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles();
        
        if(File.Exists(SAVE_FOLDER + "save.txt")) {
            string saveString = File.ReadAllText(SAVE_FOLDER + "save.txt");
            Debug.Log("Found save");
            return saveString;
        }
        else { 
            return null; 
        }

    }

    public static string LoadStats() {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles();

        if (File.Exists(SAVE_FOLDER + "stats.txt")) {
            string statString = File.ReadAllText(SAVE_FOLDER + "stats.txt");
            Debug.Log("Found stats");
            return statString;
        }
        else {
            return null;
        }

    }


}
