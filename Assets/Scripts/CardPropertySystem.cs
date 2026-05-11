using UnityEngine;
using System.Collections.Generic;

public static class CardPropertySystem
{

    public static int CalculateDamage(CardController attacker, int baseDamage, bool IsTargetFace)
    {
        if (attacker.Data.property == CardProperty.Venom && !IsTargetFace)
            return 999;
        return baseDamage;
    }


    public static int CalculateLifesteal(CardController attacker, int damageDealt)
    {
        return attacker.Data.property == CardProperty.Lifesteal ? damageDealt : 0;
    }


    public static CardSlot FindTarget(int attackerSlotIndex, IReadOnlyList<CardSlot> defenderSlots)
    {

        if (attackerSlotIndex < defenderSlots.Count)
        {
            CardSlot oppositeSlot = defenderSlots[attackerSlotIndex];

            foreach (var slot in defenderSlots)
                if (slot.currentCard != null && slot.currentCard.Data.property == CardProperty.Taunt)
                    return slot;



            if (oppositeSlot != null && oppositeSlot.currentCard != null)
            {



                return oppositeSlot;
            }
        }


        return null;
    }





}