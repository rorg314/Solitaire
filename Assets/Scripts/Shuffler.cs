using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Shuffler : MonoBehaviour {
    public static Shuffler Instance { get; protected set; }

    public CardSpriteManager spriteManager;

    //Total number of cards
    int totalCards = 52;

    //Number of stacks on field (7 stacks starting at 1 with ++1 each time leaves 24 in deck)
    int totalStacks = 7;
    //Number of cards to draw
    public int drawNum {get; set;}

    //Random num generator
    System.Random rnd = new System.Random();

    ////.//// Lists and queues ////.////
    
    //List of remaining cards to be shuffled
    List<int> unShuffleList;
    // integers 1-52 in sequence
    int[] unShuffledSequence;
    //Shuffled queue of card numbers
    Queue<int> shuffledCardQueue;
    
    //List of all stacks on field
    public List<Stack> stackList { get; protected set; }
    public List<Stack> aceStackList { get; set; }
    public Stack deckStack;
    public Stack drawStack;
    //List of all stacks in game (first four are ace, last is draw, second last is deck)
    public List<Stack> allStacksList {get; set;}
    

    void Start() {
        if (Instance != null) { Debug.LogError("Can't have more than one shuffler"); }
        Instance = this;

        //Set draw num to 3 by default
        drawNum = 3;

        //Instantiate list of stacks
        stackList = new List<Stack>();
        aceStackList = new List<Stack>();
        allStacksList = new List<Stack>();
        deckStack = new Stack();
        
        drawStack = new Stack(drawNum);
        
        
        //Create empty stacks
        CreateStacks();
        
        //Create shuffled sequence of card IDs 
        CreateShuffleSequence();

        //Tell sprite manager to initialise field stack sprites
        spriteManager.InitialiseFieldStackSprites(stackList);
        //Tell sprite manager to initialise ace stack sprites
        spriteManager.InitialiseAceStackSprites(aceStackList);
        //Tell sprite manager to initialise deck stack sprites
        spriteManager.InitialiseDeckStackSprite(deckStack);
        //Tell sprite manager to initialise draw stack sprites
        spriteManager.InitialiseDrawStackSprites(drawStack);

        //Fill stacks with shuffled cards (deck stacked last)
        FillCardStacks(shuffledCardQueue, stackList, deckStack);

        //Add shuffle move to moveQueue
        MoveManager.Instance.moveSuccessful();
        //Set moves back to zero
        MoveManager.Instance.numberMoves = 0;

        //MoveManager.Instance.DrawCards(3);
        //MoveManager.Instance.DrawCards(3);
    }

    //Create sequence of shuffled card numbers 1-52, where 1-13 are Clubs, 14-26 Diamonds, 27-39 Hearts and 40-52 Spades
    public Queue<int> CreateShuffleSequence() {
        unShuffledSequence = new int[totalCards];
        unShuffleList = new List<int>();
        shuffledCardQueue = new Queue<int>();

        //Create integer sequence
        for (int i = 0; i < totalCards; i++) {
            unShuffledSequence[i] = i + 1;
            unShuffleList.Add(i + 1);
        }

        for (int i = totalCards-1; i >= 0; i--) {
            int cardsLeft = unShuffleList.Count();
            
            int randomInt = rnd.Next(0, cardsLeft);

            shuffledCardQueue.Enqueue(unShuffleList[randomInt]);
            unShuffleList.RemoveAt(randomInt);
        }
        return shuffledCardQueue;
    }

    //Starting stack size is 1, increases by 1 each time
    int stackMax = 1;
    //Total number of cards on field (for calculating deck size)
    int fieldCards = 0;

    //Create empty field, ace and deck stacks 
    public void CreateStacks() {
        //Creating ace stacks
        for (int i = 0; i < 4; i++) {
            //Add four stacks to list with capacity 13
            aceStackList.Add(new Stack(13));
            aceStackList[i].StackID = i;
            aceStackList[i].isAce = true;
            allStacksList.Add(aceStackList[i]);
        }
        //Creating field stacks
        for (int i = 0; i < totalStacks; i++) {
            //Assign stacks to list
            stackList.Add( new Stack(stackMax));
            
            stackList[i].StackID = aceStackList.Count + i;

            allStacksList.Add(stackList[i]);
            //Log total number of cards in stacks so far
            fieldCards += stackMax;
            //Increase size of next stack by 1
            stackMax++;
        }
        

        //Create deck stack (with stackMax = totalCards - fieldCards = deckMax)
        int deckMax = totalCards - fieldCards;

        //Initialise deck and draw stacks
        deckStack.stackMax = deckMax;
        deckStack.isDeck = true;
        drawStack.isDraw = true;
        allStacksList.Add(deckStack);
        allStacksList.Add(drawStack);
        //Set stack IDs of deck and draw
        allStacksList[allStacksList.Count - 2].StackID = allStacksList.Count - 2; //deck
        allStacksList[allStacksList.Count - 1].StackID = allStacksList.Count - 1; //draw
    }

    //Fill stacks based on shuffled sequence 
    public void FillCardStacks(Queue<int> queue, List<Stack> stackList, Stack deckStack) {
        //Fill each card stack from queue
        for (int i = 0; i < stackList.Count; i++) {
            Stack currentStack = stackList[i];
            currentStack.CreateStackFromShuffle(queue, currentStack.stackMax);
            //Debug.Log(currentStack.stackMax + " cards in stack");
            //Debug.Log("Top card is " + getCardInfo(currentStack.CardsInStack[0]));
            currentStack.topCard = currentStack.CardsInStack[currentStack.CardsInStack.Count - 1];
        }
        
        //Put remaining cards in deck stack
        deckStack.CreateStackFromShuffle(queue, deckStack.stackMax);
    }

    public string getCardInfo(Card card) {
        return "Card ID: " + card.cardID + ", Name: " + card.cardName + " of " + card.suit + "s";
    }



}
