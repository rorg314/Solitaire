
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;


//public enum DrawMode { d1, d3, d6};
public class MoveManager : MonoBehaviour {

    //Shuffler shuffler { get { return Shuffler.Instance; } }

    public static MoveManager Instance { get; protected set; }

    public int numberMoves { get; set; }

    public float gameTime { get; set; }
    //public DrawMode drawMode { get; set; }
    //Number of cards to draw
    [HideInInspector]
    public int drawNum;
    public bool gameStarted { get; set; }
    public bool gameFinished { get; set; }
    public Text timeUI;
    public Text movesUI;
    public GameObject endParticles;
    float endTimer;
    //For checking autofills
    int aceCardSum;
    //Testing
    public bool cheat { get; set; }
    public bool endScreen { get; set; }
    //Stack of all previous moves, each move contains list with cardsInStack for each stack
    public Stack<List<Stack>> allMovesStack;

    void Start() {
        Instance = this;
        cheat = false;
        numberMoves = 0;
        gameTime = 0f;
        allMovesStack = new Stack<List<Stack>>();
        //drawMode = (DrawMode) PauseMenu.Instance.modeToChange;
        drawNum = PauseMenu.modeToChange;
        gameStarted = false;
        gameFinished = false;
        endTimer = 0f;
        endScreen = false;
        timeUI.text = "Time: " + string.Format("{0:00:00}", 0);
        movesUI.text = "Moves: " + "0";
    }

    void Update() {
        
        if (gameStarted) {
            gameTime += Time.deltaTime;
            timeUI.text = "Time: " + string.Format("{0:00:00}", gameTime);
        }
        if (gameFinished) {
            endTimer += Time.deltaTime;
            if(endTimer > 15f) { 
                PauseMenu.Instance.TogglePause();
                endScreen = true;
                StopCoroutine("autoFillStacks");
            }
        }

    }

    public void DrawCards() {
        gameStarted = true;
        //Deck drawing from
        Stack deck = Shuffler.Instance.deckStack;
        //Stack of drawn cards
        Stack drawStack = Shuffler.Instance.drawStack;

        //Clear cards in draw stack, add back to deck + clean up sprites and old game objects
        if (drawStack.CardsInStack.Count > 0) {
            //Tell sprite manager to clear drawn card object (needs list of cards still)
            CardSpriteManager.Instance.ClearCardStackObjects(drawStack);

            //Add cards back to deck stack (must add to beginning of list since topcard is last element!)
            foreach(Card oldCard in drawStack.CardsInStack) {
                deck.CardsInStack.Insert(0, oldCard);
                //Set back to not visible
                oldCard.isVisible = false;
            }

            //Only now can clear list of cards in draw stack 
            drawStack.CardsInStack.Clear();
            //drawStack.RecalculateStack();
        }
        //Check if deck has enough cards left - otherwise only draw last few cards
        if(deck.CardsInStack.Count < drawNum) {
            drawNum = deck.CardsInStack.Count;
            if(drawNum == 0) {
                //Reached end of deck with no cards left to draw, reset deck 
            }
        }
        //Draw cards and change stack
        for (int i = 0; i < drawNum; i++) {
            //Add to draw stack
            drawStack.CardsInStack.Add(deck.topCard);
            deck.topCard.isVisible = true;
            //Remove card from deck stack
            deck.CardsInStack.Remove(deck.topCard);
            //Recalculate deck stack if still has cards in 
            if (deck.CardsInStack.Count > 0) {
                deck.RecalculateStack();
            }
            //Repeat until draw stack is full
        }
        
        drawStack.RecalculateStack();
        
        //Debug.Log("Drew cards");
        //Draw counts as move
        moveSuccessful();
        
    }

    public bool IsCardMovementValid(Card placeCard, Card destCard) {
        if (cheat == true) {
            //Debug.Log("cheating");
            return true;
        }

        //Check if placing onto dummy stack
        if (destCard.isDummy) {

            //Check if placing onto dummy ace stack 
            if (destCard.parentStack.isAce) {
                //Only allow move if placeCard is ace
                if (placeCard.cardName == CardName.Ace) {

                    return true;
                }
                return false;
            }

            //Only allow move if placeCard is king
            if (placeCard.cardName == CardName.King) {

                return true;
            }
        }

        //Otherwise placing onto regular stack/partially full ace stack

        //check if stack is origin stack of mover


        //Check if ace stack
        if (destCard.parentStack.isAce) {
            //Only allow move if suit matches, and cardname index is + 1
            if (placeCard.suit == destCard.suit && ((int)placeCard.cardName == ((int)destCard.cardName + 1))) {

                return true;
            }
            return false;
        }
        else { //Placing onto regular stack that already has cards 
            //Check if opposite colour 
            if (destCard.colour != placeCard.colour) {

                //Check if placing cardname index is - 1
                if ((int)placeCard.cardName == ((int)destCard.cardName - 1)) {

                    return true;
                }
            }

        }
        return false;
    }

    public bool IsStackMovementValid(Stack movingStack, Stack targetStack) {

        if (cheat == true){
            //Debug.Log("cheating");
            return true; 
        }

        //Card that is being placed will be first in list 
        Card placeCard = movingStack.CardsInStack[0];
        //Card being placed onto
        Card destCard = targetStack.topCard;

        if (IsCardMovementValid(placeCard, destCard)) { return true; }
        else { return false; }
    }
    
    public void moveSuccessful() {

        //Make deep copy of all stack lists
        List<Stack> copyStackList = new List<Stack>();
        for (int i = 0; i < Shuffler.Instance.allStacksList.Count; i++) {
            //Store copied stack in a new stack  
            Stack copyStack = new Stack();
            copyStack.copyStackInfo(Shuffler.Instance.allStacksList[i]);
            copyStackList.Add(copyStack);
        }

        allMovesStack.Push(copyStackList);
        Instance.numberMoves++;
        if (gameStarted) {
            movesUI.text = "Moves: " + numberMoves;
        }
        CheckForCompleteStacks();
        //Debug.Log(numberMoves);
        
    }

    public void UndoMove() {
        //Debug.Log("Undo");
        if (allMovesStack.Count > 1) {
            //Current game state (already pushed to moves stack) - NOT USED
            List<Stack> currentState = allMovesStack.Pop();
            //Pop again to get previous game state from movesStack
            List<Stack> prevState = allMovesStack.Pop();
            //Replace current stacks with stacks from undo move

            for (int i = 0; i < Shuffler.Instance.allStacksList.Count; i++) {
                Stack currentStack = Shuffler.Instance.allStacksList[i];
                
                if (currentStack.isDraw == false) {
                    for (int j = 0; j < currentStack.CardsInStack.Count; j++) {
                        if (currentStack.CardsInStack[j].isDummy) {
                        
                            CardSpriteManager.Instance.ClearCardStackObjects(currentStack);
                            currentStack.CardsInStack.RemoveAt(j);
                        }
                    }
                }


                Shuffler.Instance.allStacksList[i].copyStackInfo(prevState[i]);

                Shuffler.Instance.allStacksList[i].ReconstructStack();

            }
            moveSuccessful();
        }
        else { Debug.Log("Nothing to undo"); }

    }
        
    public void CheckForCompleteStacks() {
        
        foreach(Stack s in Shuffler.Instance.stackList) {
            //Check if any stack still has non visible cards
            if(s.CardsInStack.Count != s.visibleCards.Count) {
                return;
            }
        }
        //All stacks should now be visible, check if deck and draw are empty
        if(Shuffler.Instance.deckStack.CardsInStack.Count == 0 && Shuffler.Instance.drawStack.CardsInStack.Count <= 1) {
            Debug.Log("WON GAME!");
            bool won = true;
            gameFinished = true;
            InvokeRepeating("autoFillStacks", 0f, 0.08f);


            //Trigger win animation




            //Update stats
            StatManager.Instance.updateStats(won, numberMoves, gameTime);
        }

    }

    public int countAceCards() {
        int aceCardSum = 0;
        foreach (Stack s in Shuffler.Instance.aceStackList) { aceCardSum += s.CardsInStack.Count; }
        return aceCardSum;
    }
    bool finished = false;
    public void autoFillStacks() {
        
        if (finished) { return; }
        //Debug.Log("Autofill");
        //Automatically add all cards to correct stack in order
        List<Stack> aceStacks = Shuffler.Instance.aceStackList;
        List<Stack> fieldStacks = Shuffler.Instance.stackList;
        Stack drawStack = Shuffler.Instance.drawStack;
        aceCardSum = countAceCards();
        
        //Try to autofill until ace stacks are full
        
        //Check each card on field and see if can be placed on any ace stack
        for (int i = 0; i < fieldStacks.Count; i++) {
            Card moveCard = fieldStacks[i].topCard;

            for (int j = 0; j < aceStacks.Count; j++) {
                Card destCard = aceStacks[j].topCard;
                //if (aceStacks[j].CardsInStack.Count <= aceStacks[j].stackMax) {
                //    //Move to next stack if ace stack full (testing)
                //    continue;
                //}
                if (IsCardMovementValid(moveCard, destCard)) {
                    fieldStacks[i].CardsInStack.Remove(moveCard);
                    fieldStacks[i].RecalculateStack();
                    aceStacks[j].CardsInStack.Add(moveCard);
                    aceStacks[j].RecalculateStack();
                    aceCardSum++;
                    if (aceCardSum >= 52) {
                        Debug.Log("Finished");
                        //StopCoroutine("autoFillStacks");
                        ParticleSystem[] particles = endParticles.GetComponentsInChildren<ParticleSystem>();
                        foreach(ParticleSystem p in particles) { p.Play(); }
                        finished = true;
                    }
                    return; //Return, wait for next invoke to continue
                }

                //Check next ace card
            }

            //Move to next field card
        }

        

    }






}
