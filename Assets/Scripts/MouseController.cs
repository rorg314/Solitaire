using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseController : MonoBehaviour {

    public static MouseController Instance { get; protected set; }

    //Stack being moved (needs to be created)
    public Stack movingStack;

    //Mouse positions
    Vector3 lastFramePosition;
    Vector3 currFramePosition;
    Vector3 dragStartPosition;

    public GameObject fieldCardCanvas;
    public GameObject movingStackHolder;
    public GameObject mover { get; set; }
    bool hitSomething = false;
    bool isMoving = false;

    public bool isPaused { get; set; }
    

    Stack sourceStack;
    Stack targetStack;
    Card hitCard;

    void Start() {
        Instance = this;
        
    }

    void Update() {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0f;
        updateClicks();
        
        if (isMoving ) {
            UpdateMovingStack();
        }
        //TESTING
        if (Input.GetKeyDown("a")) { MoveManager.Instance.autoFillStacks(); }

    }

    void updateClicks() {
        //On LMB click
        if (Input.GetMouseButtonDown(0)) {
            
            if(isPaused == true) { return; }

            //Check if valid object is under mouse click
            hitSomething = CheckForValidObjectUnderMouse();

            if (hitSomething) {

                //Check if hit deck - draw cards, otherwise create moving stack
                if (hitCard.parentStack.isDeck) {
                    //Do nothing if clicked deck whilst moving
                    if (isMoving) {
                        return;
                    }
                    else{
                        Debug.Log("draw cards");
                        MoveManager.Instance.DrawCards(); 
                    }
                    
                }
                else {
                    //Hit a stacked card, if not already moving create moving stack
                    if (!isMoving && hitCard != null) {
                        //Stack the moving stack was moved from (for revealing next card)
                        sourceStack = hitCard.parentStack;
                        movingStack = CreateMovingStack(hitCard);
                    }
                    else if (isMoving) {
                        if (hitCard != null) {
                            //Target stack based on card hit
                            targetStack = hitCard.parentStack;
                        }
                        //Otherwise target stack already set from placeholder card

                        //Check if valid movement
                        if (MoveManager.Instance.IsStackMovementValid(movingStack, targetStack)) {

                            targetStack.MergeStacks(targetStack, movingStack);
                            isMoving = false;

                            //Destroy moving stack object
                            Destroy(mover);

                            //Reveal next card from original stack if move successful 
                            if (sourceStack.CardsInStack.Count >= 0 && sourceStack.isDraw == false) {
                                if (sourceStack.topCard.isDummy) { 
                                    sourceStack.CardsInStack.Remove(sourceStack.topCard);
                                    sourceStack.RecalculateStack();
                                }
                                sourceStack.CardsInStack[sourceStack.CardsInStack.Count - 1].isVisible = true;
                                sourceStack.RecalculateStack();
                                
                            }
                            //Move successful so push to all moves stack
                            MoveManager.Instance.moveSuccessful();
                        }
                        else {//Return cards to original stack if unsuccessful
                            sourceStack.MergeStacks(sourceStack, movingStack);
                            isMoving = false;
                            Destroy(mover);
                        }
                    }
                }
            }
        }
    }

    //Find card currently under mouse 
    public bool CheckForValidObjectUnderMouse() {
        MoveManager.Instance.gameStarted = true;
        //These will also be set in this method
        hitCard = null;
        targetStack = null;
        
        //Create raycast
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        
        if (hit.collider != null) {
            //Card was hit
                hitCard = CardSpriteManager.Instance.colliderCardMap[(BoxCollider2D)hit.collider];
                //Debug.Log(hitCard.GenerateCardNameString(hitCard));
                return true;
            }
        //Didn't hit anything valid
        return false;
    }

    //Create moving stack based on card hit
    public Stack CreateMovingStack(Card hitCard) {
        

        movingStack = new Stack(13);
        movingStack.isMoving = true;
        
        //If card not on top, work out visible cards above it (should only have a hit visible card)

        int hitCardIndex = hitCard.parentStack.visibleCards.IndexOf(hitCard);
        for (int i = hitCardIndex; i <= hitCard.parentStack.visibleCards.Count - 1; i++) {
            //Add card to moving stack 
            movingStack.CardsInStack.Add(hitCard.parentStack.visibleCards[i]);
            //Remove from parent stack (still set as base)
            hitCard.parentStack.CardsInStack.Remove(hitCard.parentStack.visibleCards[i]);
        }

        //Recalculate base stack (to create dummy if moved last card in stack)
        hitCard.parentStack.RecalculateStack();

        


        mover = Instantiate(movingStackHolder);
        mover.transform.position = currFramePosition;

        CardSpriteManager.Instance.InitialiseMovingStackSprites(movingStack);
        movingStack.RecalculateStack();
        
        isMoving = true;

        return movingStack;
    }

    public void UpdateMovingStack() {
        mover.transform.position = currFramePosition; 
    }






}
