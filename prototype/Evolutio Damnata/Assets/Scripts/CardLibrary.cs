using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CardLibrary : MonoBehaviour
{
    public Deck playerDeck;
    public Deck enemyDeck;
    public List<CardData> cardDataList = new List<CardData>();

    [Header("Default Sprite for Cards")]
    public Sprite defaultCardSprite;

    private Dictionary<string, Sprite> cardImageDictionary = new Dictionary<string, Sprite>();

    public Card CreateCardFromData(CardData cardData)
    {
        Card newCard;
        if (cardData.IsSpellCard)
        {
            SpellCard spellCard = new GameObject(cardData.CardName).AddComponent<SpellCard>();
            spellCard.CardName = cardData.CardName;
            spellCard.CardImage = cardData.CardImage ?? defaultCardSprite;
            spellCard.Description = cardData.Description;
            spellCard.ManaCost = cardData.ManaCost;
            spellCard.EffectType = cardData.EffectType;  // No need for .Value
            spellCard.EffectValue = cardData.EffectValue; // Direct assignment
            spellCard.Duration = cardData.Duration; // Direct assignment
            newCard = spellCard;
        }
        else
        {
            MonsterCard monsterCard = new GameObject(cardData.CardName).AddComponent<MonsterCard>();
            monsterCard.CardName = cardData.CardName;
            monsterCard.CardImage = cardData.CardImage ?? defaultCardSprite;
            monsterCard.Description = cardData.Description;
            monsterCard.ManaCost = cardData.ManaCost;
            monsterCard.AttackPower = cardData.AttackPower;
            monsterCard.Health = cardData.Health;
            monsterCard.Keywords = cardData.Keywords ?? new List<string>();
            newCard = monsterCard;
        }

        return newCard;
    }

    public List<Card> CreateDeckFromLibrary()
    {
        List<Card> deck = new List<Card>();
        foreach (var cardData in cardDataList)
        {
            Card newCard = CreateCardFromData(cardData);
            deck.Add(newCard);
        }
        return deck;
    }

    void Start()
    {
        if (defaultCardSprite == null)
        {
            Debug.LogError("Default Card Sprite is not assigned! Please assign a sprite in the Inspector.");
            return;
        }

        cardDataList.Add(new CardData("Wizard", null, "A powerful wizard with fire spells", 5, 7, 10, new List<string> { "Spellcaster", "Fire" }));
        cardDataList.Add(new CardData("Warrior", null, "A brave warrior", 3, 6, 8, new List<string> { "Taunt" }));
        cardDataList.Add(new CardData("Archer", null, "An expert archer", 2, 4, 6, new List<string> { "Ranged" }));

        cardDataList.Add(new CardData("Fireball", null, "Deals damage to a single target", 4, 0, 0, null, SpellEffect.Damage, 10));
        cardDataList.Add(new CardData("Healing Light", null, "Heals a single target", 3, 0, 0, null, SpellEffect.Heal, 8));
        cardDataList.Add(new CardData("Burning Flames", null, "Applies burn effect to a single target", 5, 0, 0, null, SpellEffect.Burn, 5, 3));
        cardDataList.Add(new CardData("Frenzy", null, "Allows a monster to attack twice", 6, 0, 0, null, SpellEffect.DoubleAttack, 0, 2));

        List<CardData> validCards = new List<CardData>();

        foreach (var cardData in cardDataList)
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(cardData.CardName))
            {
                Debug.LogWarning("Card skipped due to missing name.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(cardData.Description))
            {
                Debug.LogWarning($"Card {cardData.CardName} skipped due to missing description.");
                isValid = false;
            }
            if (cardData.ManaCost <= 0)
            {
                Debug.LogWarning($"Card {cardData.CardName} skipped due to invalid ManaCost.");
                isValid = false;
            }
            if (cardData.AttackPower < 0)
            {
                Debug.LogWarning($"Card {cardData.CardName} skipped due to invalid AttackPower.");
                isValid = false;
            }
            if (cardData.Health < 0)
            {
                Debug.LogWarning($"Card {cardData.CardName} skipped due to invalid Health.");
                isValid = false;
            }

            if (cardData.CardImage == null)
            {
                Debug.LogWarning($"No specific sprite for {cardData.CardName}. Using default sprite: {defaultCardSprite.name}");
                cardData.CardImage = defaultCardSprite;
            }

            if (isValid)
            {
                validCards.Add(cardData);
            }
        }

        cardDataList = validCards;

        Debug.Log($"Card Library initialized with {cardDataList.Count} valid cards.");

        foreach (var cardData in cardDataList)
        {
            if (!cardImageDictionary.ContainsKey(cardData.CardName))
            {
                cardImageDictionary.Add(cardData.CardName, cardData.CardImage);
            }
        }
    }

    public Sprite cardImageGetter(string cardName)
    {
        if (cardImageDictionary.TryGetValue(cardName, out Sprite cardImage))
        {
            return cardImage;
        }
        else
        {
            Debug.LogWarning($"Card image for '{cardName}' not found in the library.");
            return defaultCardSprite;
        }
    }
}
