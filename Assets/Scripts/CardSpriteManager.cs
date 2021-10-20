using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSpriteManager : MonoBehaviour {
    public static CardSpriteManager Instance { get; protected set; }

    //Card -> game object
    public Dictionary<Card, GameObject> cardGameObjectMap { get; protected set; }
    //Filename string -> card sprite
    public Dictionary<string, Sprite> cardFilenameSpritesMap { get; set; }
    //CardID -> sprite
    Dictionary<int, Sprite> cardIDSpriteMap;
    //Stack -> stack game object
    public Dictionary<Stack, GameObject> stackGameObjectMap;
    //Collider -> card
    public Dictionary<BoxCollider2D, Card> colliderCardMap { get; set; }

    //Vertical stack prefab object
    public GameObject verticalStackPrefab;
    //Flat stack holder (aces + deck)
    public GameObject flatStackPrefab;
    //Horizontal stack holder for draw (eg draw 3)
    public GameObject horizStackPrefab;
    //Holders for stacks
    public GameObject allStacksHolder;
    public GameObject aceStackHolder;
    public GameObject deckStackHolder;
    public GameObject drawStackHolder;
    public List<Image> placeholderCards { get; protected set; }
    public Sprite cardBackSprite { get; set; }
    
    float cardHeight = 5f;
    float cardWidth = 3f;
 
    void Start() {
        Instance = this;
        //Initialise dictionaries
        cardFilenameSpritesMap = new Dictionary<string, Sprite>();
        cardGameObjectMap = new Dictionary<Card, GameObject>();
        stackGameObjectMap = new Dictionary<Stack, GameObject>();
        colliderCardMap = new Dictionary<BoxCollider2D, Card>();
        placeholderCards = new List<Image>();
        
        LoadSprites();
        cardBackSprite = cardFilenameSpritesMap["CardBack_0"];
    }

    public void LoadSprites() {
        //Array of all sprites
        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Sprites/Cards");
        //Assign each sprite to correct card name
        foreach (Sprite s in cardSprites) {
            cardFilenameSpritesMap[s.name] = s;
            //Debug.Log(s.name);
        }
        //Create dictionary of card objects -> sprite
        cardIDSpriteMap = new Dictionary<int, Sprite>();
        //Parse card info from filename
        foreach (Sprite s in cardSprites) {
            int ID = GetCardIDFromFilename(s.name);
            cardIDSpriteMap[ID] = s;
        }
    }
    public int GetCardIDFromFilename(string name) {
        
        
        int pos = name.IndexOf("_");
        
        string fileCardName = name.Substring(0, pos);
        
        if (fileCardName == "CardBack") { return 0; }

        int fileCardNumber = int.Parse(name.Substring(pos + 1));
        int cardID = 0;
        if(fileCardName == "club") {
            cardID = fileCardNumber;
        }
        if (fileCardName == "diamond") {
            cardID = fileCardNumber + 13;
        }
        if (fileCardName == "heart") {
            cardID = fileCardNumber + 26;
        }
        if (fileCardName == "spade") {
            cardID = fileCardNumber + 39;
        }
        return cardID;
    }

    public void InitialiseFieldStackSprites(List<Stack> stackList) {
        for (int i = 0; i < stackList.Count ; i++) {
            //Register onStackChanged callback to each stack
            stackList[i].cbOnStackChanged += onStackChanged;
            //Initialise stack prefab
            GameObject stack_go = Instantiate(verticalStackPrefab);
            stack_go.transform.SetParent(allStacksHolder.transform);
            stack_go.transform.localPosition = Vector3.zero;

            //Set card back sprite
            Image placeholderImage = stack_go.GetComponentInChildren<Image>();
            placeholderImage.sprite = cardBackSprite;
            placeholderCards.Add(placeholderImage);
            stackList[i].stack_go = stack_go;
            stack_go.name = "Stack_" + i;
        }  
    }

    public void InitialiseAceStackSprites(List<Stack> aceStacks) {
        for (int i = 0; i < aceStacks.Count; i++) {
            //Register onStackChanged callback to each stack
            aceStacks[i].cbOnStackChanged += onStackChanged;
            //Initialise stack prefab
            GameObject aceStack_go = Instantiate(flatStackPrefab);
            aceStack_go.transform.SetParent(aceStackHolder.transform);
            aceStack_go.transform.localPosition = Vector3.zero;
            Image placeholderImage = aceStack_go.GetComponentInChildren<Image>();
            placeholderImage.sprite = cardBackSprite;
            placeholderCards.Add(placeholderImage);
            aceStacks[i].stack_go = aceStack_go;
            aceStacks[i].RecalculateStack();
        }
    }

    public void InitialiseDeckStackSprite(Stack deckStack) {
        //Register callbacks
        deckStack.cbOnStackChanged += onStackChanged;
        
        //Initialise deck prefab
        GameObject deckStack_go = Instantiate(flatStackPrefab);
        deckStack_go.transform.SetParent(deckStackHolder.transform);
        deckStack_go.transform.localPosition = Vector3.zero;
        //Set card back sprite
        Image placeholderImage = deckStack_go.GetComponentInChildren<Image>();
        placeholderImage.sprite = cardBackSprite;
        placeholderCards.Add(placeholderImage);
        deckStack.stack_go = deckStack_go;
    }

    public void InitialiseDrawStackSprites(Stack drawStack) {
        //Register callback
        drawStack.cbOnStackChanged += onStackChanged;
        //Initialise draw (horiz) prefab
        GameObject drawStack_go = Instantiate(horizStackPrefab);
        drawStack_go.transform.SetParent(drawStackHolder.transform);
        drawStack_go.transform.localPosition = Vector3.zero;
        //stackGameObjectMap.Add(drawStack, drawStack_go);
        drawStack.stack_go = drawStack_go;
    }
    
    public void InitialiseMovingStackSprites(Stack movingStack) {
        //Register callback
        movingStack.cbOnStackChanged += onStackChanged;
        //Find mover object from mousecontroller
        GameObject mover = MouseController.Instance.mover;
        //Initialise vertical prefab
        GameObject movingStack_go = Instantiate(verticalStackPrefab);
        movingStack_go.transform.SetParent(mover.transform);
        movingStack_go.transform.localPosition = Vector3.zero;
        //Set card back sprite
        Image placeholderImage = movingStack_go.GetComponentInChildren<Image>();
        placeholderImage.sprite = cardBackSprite;
        placeholderCards.Add(placeholderImage);
        movingStack.stack_go = movingStack_go;
    }
    public void onStackChanged(Stack currentStack) {
        ClearCardStackObjects(currentStack);
        if(currentStack.CardsInStack.Count == 0) { 
        //Stack was empty so just return
            return; 
        }
        
        GameObject stack_go = currentStack.stack_go;
        
        //Debug.Log(stack_go);
        Transform _stack = FindStackHolder(stack_go);

        if (_stack != null) {
            //Create new card objects 
            if (currentStack.isDeck) {
                //Only create single card object for top card
                Card card = currentStack.topCard;
                
                CreateCardWithEmptySprite(_stack, card);

                onVisibleCardsChanged(currentStack);
            }
            else {
                //Create correct number of stacked card game objects (if not deck)
                for (int j = 0; j < currentStack.CardsInStack.Count; j++) {
                    //Current card from card list
                    Card card = currentStack.CardsInStack[j];

                    //Create card object and return its image renderer
                    Image sr = CreateCardWithEmptySprite(_stack, card);

                    if (currentStack.visibleCards.Contains(card)) {
                        //call top card changed 
                        if (cardIDSpriteMap.ContainsKey(card.cardID)) {
                            onVisibleCardsChanged(currentStack);
                        }
                        else { Debug.LogError("No such card in dictionary"); }
                    }
                    else {
                        //Set to card back sprite
                        sr.sprite = cardBackSprite;
                    }
                }
            }
        }
    }

    //Called whenever top card changes
    public void onVisibleCardsChanged(Stack currentStack) {
        List<Card> visibleCards = currentStack.visibleCards;
        if (currentStack.isDeck) { visibleCards.Add(currentStack.topCard); }
        GameObject vCard_go = null;
        
        for (int i = 0; i < visibleCards.Count; i++) {
            Card vCard = visibleCards[i];
              
            if (cardGameObjectMap.ContainsKey(vCard)) {
                vCard_go = cardGameObjectMap[vCard];
            }
            else {//Card doesn't exist yet (probably because deck only creates one card - eg when drawing two or more need to create new card objects)
                //CreateCardWithEmptySprite(FindStackHolder(stackGameObjectMap[currentStack]), vCard);
                CreateCardWithEmptySprite(FindStackHolder(currentStack.stack_go), vCard);
                vCard_go = cardGameObjectMap[vCard];
            }
            if(vCard_go == null) { Debug.LogError("Card object hasn't been created yet!"); return; }
            
            Image sr = vCard_go.GetComponent<Image>();

            if (currentStack.isDeck == true) {
                //set top cards to card back for deck stack
                sr.sprite = cardBackSprite;
            }
            else {
                if (vCard.isDummy == false) {
                    sr.sprite = cardIDSpriteMap[vCard.cardID];
                }
                else { sr.enabled = false; }//disable sprite renderer for dummy card
            }
            RectTransform rt = vCard_go.GetComponent<RectTransform>();
            //For moving stack only 
            if (currentStack.isMoving) {
                //Place infront of all objects
                rt.localPosition = new Vector3(0f, 0f, -5f);
            }
            else {
                //offset z slightly for physics raycasting 
                Vector3 offset = new Vector3(0f, 0f, -(i + 1) * 0.05f);
                rt.localPosition = offset;
            }
            if (currentStack.isMoving == false ) {
                //Add 2D collider to visible cards only
                if (currentStack.isDraw == false) {
                    BoxCollider2D collider = vCard_go.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(cardWidth, cardHeight);
                    //Add collider to dictionary
                    colliderCardMap.Add(collider, vCard);
                }
                else {
                    //Only add collider to top visible card in draw stack
                    if (vCard.isTopCard == true) {
                        BoxCollider2D collider = vCard_go.AddComponent<BoxCollider2D>();
                        collider.size = new Vector2(cardWidth, cardHeight);
                        //Add collider to dictionary
                        colliderCardMap.Add(collider, vCard);
                    }
                }
            }
        }
    }
    //Creates card game object (child of _stack), and returns the empty image renderer
    public Image CreateCardWithEmptySprite(Transform _stack, Card card ) {
        GameObject stackedCard_go = new GameObject();

        if (cardGameObjectMap.ContainsKey(card)) {
            //Remove old card object 
            Destroy(cardGameObjectMap[card]);
            cardGameObjectMap.Remove(card);
        }
        //Add to card game object dictionary
        cardGameObjectMap.Add(card, stackedCard_go);

        //Resize card and force to origin of parent
        stackedCard_go.AddComponent<RectTransform>();
        RectTransform rt = stackedCard_go.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardWidth);

        //Add card image
        Image sr = stackedCard_go.AddComponent<Image>();
        sr.preserveAspect = true;
        //Make children of current stack
        stackedCard_go.transform.SetParent(_stack.transform);
        stackedCard_go.name = card.GenerateCardNameString(card);
        return sr;
    }

    public Transform FindStackHolder(GameObject stack_go) {
        Transform _stack = null;
        
        Transform[] children = stack_go.GetComponentsInChildren<Transform>();
        
        foreach (Transform child in children) {
            if (child.gameObject.name == "StackHolder") {
                //Debug.Log("Found a stack holder");
                _stack = child;
            }
        }
        return _stack;
    }

    public void ClearCardStackObjects(Stack currentStack) {

        //Remove old card objects from stack
        foreach (Card oldCard in currentStack.CardsInStack) {
            
            if (cardGameObjectMap.ContainsKey(oldCard)) {
                if (currentStack.visibleCards.Contains(oldCard)) {
                    //Remove collider from dictionary
                    //Debug.Log("got here");
                    BoxCollider2D oldCollider = cardGameObjectMap[oldCard].GetComponent<BoxCollider2D>();
                    if (oldCollider != null && colliderCardMap.ContainsKey(oldCollider)) {
                        //Debug.Log("Found old collider");
                        colliderCardMap.Remove(oldCollider);
                        //Debug.Log("Removed old collider");
                    }
                }
                //Debug.Log("trying to destroy ");
                Destroy(cardGameObjectMap[oldCard]);
                //Debug.Log("card destroyed ");
                cardGameObjectMap.Remove(oldCard);
                //Debug.Log("removed card from dictionary");
            }
        }
    }

    
    

    




}
