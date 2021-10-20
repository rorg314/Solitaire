using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StackType { Field, Ace, Deck, Draw }

public class Stack {
    //List of cards in current stack
    public List<Card> CardsInStack;
    //ID of stack in all stack list, set when creating stacks in shuffler (0-3 aces, last is draw second last deck etc)
    public int StackID { get; set; }
    public Card topCard { get; set; }
    public List<Card> visibleCards { get; set; }
    //Game object attached to stack
    public GameObject stack_go { get; set; }

    public event Action<Stack> cbOnStackChanged;

    

    //Max number of cards in stack
    public int stackMax { get; set; }

    //Number of cards in stack
    int stackNum = 0;

    //True if stack is deck stack
    public bool isDeck {get; set;}
    //True if draw stack (to display all cards ) 
    public bool isDraw { get; set; }
    //Is this a moving stack?
    public bool isMoving { get; set; }
    public bool isAce { get; set; }

    //Paramaterless constructor
    public Stack() {
        this.stackMax = 52;
        this.isDeck = false;
        this.isDraw = false;
        this.isMoving = false;
        this.isAce = false;
        this.CardsInStack = new List<Card>();
        this.visibleCards = new List<Card>();
    }

    public Stack(int stackMax) {
        this.stackMax = stackMax;
        //Defaults to false 
        this.isDeck = false;
        this.isDraw = false;
        this.isMoving = false;
        this.isAce = false;
        this.CardsInStack = new List<Card>();
        this.visibleCards = new List<Card>();
    }

    //Copy stack info
    public void copyStackInfo (Stack other) {
        this.stackMax = other.stackMax;
        this.isDeck = other.isDeck;
        this.isDraw = other.isDraw;
        this.isMoving = other.isMoving;
        this.isAce = other.isAce;

        Card[] copiedCardListArray = other.CardsInStack.ToArray();
        this.CardsInStack = copiedCardListArray.ToList();
        Card[] visibleCardArray = other.visibleCards.ToArray();
        this.visibleCards = visibleCardArray.ToList();

    }


    public void CreateStackFromShuffle(Queue<int> queue, int stackMax) {
        
        for (int i = 0; i < stackMax; i++) {
            //Create card based on next ID number from queue
            Card currentCard = new Card(queue.Dequeue());
            //Add card to stack
            CardsInStack.Add(currentCard);
            //Increase number of cards in stack
            //stackNum++;

        }
        CardsInStack[CardsInStack.Count - 1].isTopCard = true;
        if (!isDeck) {
            CardsInStack[CardsInStack.Count - 1].isVisible = true;
        }
        RecalculateStack();

        //After shuffle let stack have max size 52
        this.stackMax = 52;
    }

    public void RecalculateStack() {
        //Number of cards in stack
        stackNum = CardsInStack.Count;
        
        //If stack empty create dummy card and add to stack
        if (stackNum == 0 && isDraw == false && isDeck == false) {
            Card dummyCard = new Card(0);
            

            CardsInStack.Add(dummyCard);
        }
        
        //Assign parentStack 
        foreach(Card c in CardsInStack) {
            c.parentStack = this;
            c.isTopCard = false;
        }

        topCard = null;
        
            //Clear old cards from visibleCards list
            if (visibleCards.Count > 0 && isDraw == false) {
                //Clear visible card list
                visibleCards.Clear();
            }

            if (isDraw) {
                //Set visible cards to be all cards in draw stack
                visibleCards = CardsInStack;
            }
            else {
                //Calculate new visible cards based on isVisible property
                foreach (Card c in CardsInStack) {
                    if (c.isVisible) {
                        visibleCards.Add(c);
                    }
                }
            
        }
        
        //Set top card
        if (CardsInStack.Count > 0) {
            topCard = CardsInStack[CardsInStack.Count - 1];
            topCard.isTopCard = true;
        }
        

        //Invoke stackchanged callback
        cbOnStackChanged?.Invoke(this);
        
    }


    public void ReconstructStack() {
        //Remove old dummy cards
        

        //Reconstruct visibility flag from visible cards list (stored for each stack)
        foreach(Card c in CardsInStack) {
            
            if(visibleCards.Contains(c) == false) {
                c.isVisible = false;
            }
            else { c.isVisible = true; }
        }
        //Then can safely call recalculate stack as visibility flags should be set correctly
        RecalculateStack();
    }

    

    //Merge moving stack mStack into baseStack
    public void MergeStacks(Stack baseStack, Stack mStack) {
        Card baseTopCard = baseStack.CardsInStack[baseStack.CardsInStack.Count - 1];
        //Count only zero for fully empty draw stack
        if (baseStack.CardsInStack.Count > 0) {
            //Remove dummy card if it exists
            if (baseTopCard.isDummy == true) {
                CardSpriteManager.Instance.ClearCardStackObjects(baseStack);
                baseStack.CardsInStack.Remove(baseTopCard);
                baseStack.RecalculateStack();
            }
        }
        baseStack.CardsInStack.AddRange(mStack.CardsInStack);
        //baseStack.visibleCards.AddRange(mStack.CardsInStack);
        mStack.CardsInStack.Clear();
        baseStack.isMoving = false;
        mStack.isMoving = false;
        baseStack.RecalculateStack();
        

    }

    public StackType getStackType() {
        if (isAce) { return StackType.Ace; }
        if (isDeck) { return StackType.Deck; }
        if (isDraw) { return StackType.Draw; }
        return StackType.Field;
    }

    public void setStackType(StackType type) {
        if(type == StackType.Ace) { isAce = true; return; }
        if (type == StackType.Deck) { isDeck = true; return; }
        if (type == StackType.Draw) { isDraw = true; return; }
        if (type == StackType.Field) {  return; }
    }


    
}
