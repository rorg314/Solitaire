using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;




public class GameMaster : MonoBehaviour {


    public static GameMaster Instance { get; protected set; }

    public List<StackList> allStackList;

    private void Start() {
        SaveSystem.Init();
        Instance = this;
    }

    [Serializable]
    public class StackList {
        public List<int> cardIDStackList;
        public List<bool> isVisibleList;
        public StackType stackType;
        
    }
    

    public void Save() {

        SaveObject saveObject = new SaveObject {
            //Store all saved items here
            numMoves = MoveManager.Instance.numberMoves,
            gameTime = MoveManager.Instance.gameTime,
            allStacks = serialiseCurrentGameState()

        };
        string saveString = JsonUtility.ToJson(saveObject);

        SaveSystem.SaveGame(saveString);
        Debug.Log("Saved");
    }

    public void Load() {
        string saveString = SaveSystem.LoadGame();
        Debug.Log("Loading");
        if(saveString != null) { 
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            //Setup everything based on save object contents

            for (int i = 0; i < saveObject.allStacks.Count; i++) {

                //Get current stack from stack of all stacks
                StackList currentStackList = saveObject.allStacks[i];

                Stack dummyStack = new Stack();
                
                dummyStack.setStackType(currentStackList.stackType);
                for (int j = 0; j < currentStackList.cardIDStackList.Count; j++) {
                    Card card = new Card();
                    
                    Card dummyCard = card.CreateCardFromID(currentStackList.cardIDStackList[j]);
                    if (currentStackList.isVisibleList[j] == true) { 
                        dummyCard.isVisible = true;
                        dummyStack.visibleCards.Add(dummyCard);
                    }
                    if(dummyCard.cardID == 0) { dummyCard.isDummy = true; }
                    dummyStack.CardsInStack.Add(dummyCard);
                }
                dummyStack.RecalculateStack();

                CardSpriteManager.Instance.ClearCardStackObjects(Shuffler.Instance.allStacksList[i]);
                Shuffler.Instance.allStacksList[i].copyStackInfo(dummyStack);
                Shuffler.Instance.allStacksList[i].RecalculateStack();
            }
            MoveManager.Instance.allMovesStack.Clear();
            MoveManager.Instance.moveSuccessful();

           

            MoveManager.Instance.numberMoves = saveObject.numMoves;
            MoveManager.Instance.gameTime = saveObject.gameTime;
            Debug.Log("Loaded");
        }


    }

    public StackList serialiseCurrentStack(Stack s) {
        List<int> cardList = new List<int>();
        List<bool> visibleList = new List<bool>();

        foreach (Card c in s.CardsInStack) {
            int cardID = c.getIDFromCard();
            cardList.Add(cardID);
            visibleList.Add(c.isVisible);
        }
        StackList cardsInStack = new StackList { 
            cardIDStackList = cardList,
            stackType = s.getStackType(),
            isVisibleList = visibleList
        };
        
        return cardsInStack;
    }

    public List<StackList> serialiseCurrentGameState() {

        StackList currentStackList;
        List<StackList> allStackList = new List<StackList>();
        for (int i = 0; i < Shuffler.Instance.allStacksList.Count; i++) {
            Stack s = Shuffler.Instance.allStacksList[i];
            currentStackList = serialiseCurrentStack(s);
            allStackList.Add(currentStackList);
        }
        Debug.Log("serialised game state");
        return allStackList;
    }

    public void Restart() {
        //Update stats with loss if game started
        if (MoveManager.Instance.gameStarted) {
            StatManager.Instance.updateStats(false, 0, 0);
        }
        
        
        PauseMenu.Instance.Restart();

    }


    public class SaveObject {
        public int numMoves;
        public float gameTime;
        public List<StackList> allStacks;
        
    }

    
    

    

    



 


}
