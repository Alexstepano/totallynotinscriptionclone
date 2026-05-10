using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public bool IsOccupied => currentCard != null;
    public CardController currentCard { get; set; }


    //spawnPoint=pos+rotation
    public Transform spawnPoint;

    public void OnCardDied(CardController card)
    {
        if (currentCard == card) currentCard = null;
    }


}
