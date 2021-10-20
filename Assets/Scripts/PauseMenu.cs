using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static PauseMenu Instance { get; protected set; }
    
    public GameObject ui;
    public GameObject settingsUI;
    public GameObject modeButtons;
    public GameObject randomizeBackgroundButton;
    bool changingMode = false;
    public static int modeToChange { get; set; }
    public GameObject changeWarningText;
    private static bool hasRun = false;
    public GameObject pauseMenu;
    //Random num generator
    System.Random rnd = new System.Random();

    List<Color> backgroundColours;
    public GameObject backgroundColourSelector;

    void Start() {
        Instance = this;
        if (hasRun == false) {
            modeToChange = 3;
        }
        
        if (pauseMenu.activeSelf) { pauseMenu.SetActive(false); }
        backgroundColours = new List<Color>();
        
        changeBackgroundColourButtons();
        

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) {

            TogglePause();

        }
    }

    public void TogglePause() {
        if (MoveManager.Instance.endScreen) { return; }
        highlightCurrentMode();
        ui.SetActive(!ui.activeSelf);

        if (ui.activeSelf) {
            Time.timeScale = 0f;
            MouseController.Instance.isPaused = true;
        }
        else {

            Time.timeScale = 1f;
            MouseController.Instance.isPaused = false;
        }
    }

    public void Restart() {
        hasRun = true;
        TogglePause();
        if (changingMode == false) {
            modeToChange = MoveManager.Instance.drawNum;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else {
            //MoveManager.Instance.drawMode = (DrawMode)modeToChange;
            MoveManager.Instance.drawNum = modeToChange;
            changingMode = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            
        }
    }

    public void changeMode(int drawNum) {
        modeToChange = drawNum;
        Debug.Log("Mode changed to" + drawNum);
        
        changingMode = true;
        toggleModeButtonHightlighted();
    }

    public void highlightCurrentMode() {
        int currentMode = StatManager.Instance.drawMode(MoveManager.Instance.drawNum);
        int currentDraw = MoveManager.Instance.drawNum;
        Button[] buttons = modeButtons.GetComponentsInChildren<Button>();
        Button currentButton = buttons[currentMode];
        
        ColorBlock cb = currentButton.colors;
        cb.normalColor = new Color(0f, 0.27f, 0.78f, 0.4f);
        currentButton.colors = cb;
        changeWarningText.SetActive(false);
    }

    public void toggleModeButtonHightlighted() {
        
        int newMode = StatManager.Instance.drawMode(modeToChange);
        Button[] buttons = modeButtons.GetComponentsInChildren<Button>();
        
        foreach(Button b in buttons) {
            ColorBlock colour = b.colors;
            colour.normalColor = new Color(1f, 1f, 1f, 1f);
            b.colors = colour;

        }

        Button selectedButton = buttons[newMode];

        ColorBlock cb = selectedButton.colors;
        cb.normalColor = new Color(0f, 0.57f, 0.78f, 0.4f);
        selectedButton.colors = cb;

        if (modeToChange != MoveManager.Instance.drawNum) {
            changeWarningText.SetActive(true);
        }
        else {
            changeWarningText.SetActive(false);
        }


    }

    public void changeBackgroundColourButtons() {
        Color colour = new Color();
        Button[] buttons = backgroundColourSelector.GetComponentsInChildren<Button>();
        randomiseColours();
        

        for (int i = 6; i < backgroundColours.Count; i++) {
            Button b = buttons[i];
            ColorBlock cb = b.colors;
            cb.normalColor = backgroundColours[i];
            cb.highlightedColor = backgroundColours[i] - new Color (0f, 0f, 0f, 0.3f);
            b.colors = cb;
        }
        
    }


    public void randomiseColours() {
        backgroundColours.Clear();
        for (int i = 0; i < 12; i++) {
            float R = rnd.Next(1, 99) * 0.01f;
            float G = rnd.Next(1, 99) * 0.01f;
            float B = rnd.Next(1, 99) * 0.01f;
            float A = rnd.Next(40, 99) * 0.01f;
            Color randomColour = new Vector4(R, G, B, A);
            backgroundColours.Add(randomColour);
        }

    }

    public void changeBackgroundColour(int colourIndex) {
        Button[] buttons = backgroundColourSelector.GetComponentsInChildren<Button>();
        if (colourIndex <= 5) {
            Camera.main.backgroundColor = buttons[colourIndex].colors.normalColor;
        }

        if (colourIndex > 5) {
            Camera.main.backgroundColor = backgroundColours[colourIndex];
        }


    }

    public void changeCardBackColour(int cardColourIndex) {

        CardSpriteManager.Instance.cardBackSprite = CardSpriteManager.Instance.cardFilenameSpritesMap["CardBack_" + cardColourIndex];
        //Need to redraw all stacks
        foreach (Stack s in Shuffler.Instance.allStacksList) {
            s.RecalculateStack();
        }
        foreach(Image placeholder in CardSpriteManager.Instance.placeholderCards) {
            placeholder.sprite = CardSpriteManager.Instance.cardFilenameSpritesMap["CardBack_" + cardColourIndex];
        }
    }



}
