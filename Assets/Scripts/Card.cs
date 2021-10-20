using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Suit { Club, Diamond, Heart, Spade }

public enum Colour { Black, Red }

public enum CardName { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }


public class Card {
    public Suit suit;
    public Colour colour;
    public CardName cardName;
    //Card ID number
    public int cardID { get; set; }
    public bool isTopCard { get; set; }
    public bool isVisible { get; set; }
    public bool isDummy { get; set; }
    //Stack the card is currently in 
    public Stack parentStack { get; set; }
    //Dummy constructor
    public Card() {

    }

    public Card(int cardID, Suit suit, Colour colour, CardName cardName) {
        this.suit = suit;
        this.colour = colour;
        this.cardName = cardName;
        this.cardID = cardID;
        if (cardID != 0) {
            this.isTopCard = false;
            this.isVisible = false;
            this.isDummy = false;
        }
    }
    //Card from ID constructor
    public Card(int cardID) {
        CreateCardFromID(cardID);
        this.cardID = cardID;
        if (cardID != 0) {
            this.isTopCard = false;
            this.isVisible = false;
            this.isDummy = false;
        }
    }
    //Copy constructor
    public Card(Card other) {
        cardID = other.cardID;
        CreateCardFromID(cardID);
        isTopCard = other.isTopCard;
        isVisible = other.isVisible;
        isDummy = other.isDummy;
    }

    //Copy info from other card to this card
    public void copyCardInfo (Card other) {
        this.cardID = other.cardID;
        this.suit = other.suit;
        this.colour = other.colour;
        this.cardName = other.cardName;
        this.isTopCard = other.isTopCard;
        this.isVisible = other.isVisible;
        this.isDummy = other.isDummy;

    }

    //Create card from card ID number
    public Card CreateCardFromID(int ID) {
        
        int index = ID - 1;
        //For dummy card
        if (ID == 0) {
            suit = Suit.Club;
            colour = Colour.Black;
            cardName = CardName.Ace;
        }
        
        //Clubs
        if(ID > 0 && ID <= 13) {
            suit = Suit.Club;
            colour = Colour.Black;
            cardName = (CardName)(index); // -1 since enum index start at 0
        }
        //Diamonds
        if (ID > 13 && ID <= 26) {
            suit = Suit.Diamond;
            colour = Colour.Red;
            cardName = (CardName)(index - 13);
        }
        //Hearts
        if (ID > 26 && ID <= 39) {
            suit = Suit.Heart;
            colour = Colour.Red;
            cardName = (CardName)(index - 26);
        }
        //Spades
        if (ID > 39 && ID <= 52) {
            suit = Suit.Spade;
            colour = Colour.Black;
            cardName = (CardName)(index - 39);
        }

        Card cardFromID = new Card(ID, suit, colour, cardName);
        
        if(ID == 0) {
            isDummy = true;
            isVisible = true;
            isTopCard = true;
        }
    
        return cardFromID;
    }

    public string GenerateCardNameString(Card card) {

        return suit.ToString() + "_" + ((int)cardName +1);

    }

    public int getIDFromCard() {
        int ID;
        int offset = 0;
        if(suit == Suit.Club) {
            offset = 0; 
        }
        if (suit == Suit.Diamond) {
            offset = 13;
        }
        if (suit == Suit.Heart) {
            offset = 26;
        }
        if (suit == Suit.Spade) {
            offset = 39;
        }
        if (isDummy) { ID = 0; }
        else {
            ID = offset + (int)cardName + 1;
        }
            return ID;
    }




}
