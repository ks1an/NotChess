using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "CardCollection")]
public class CardCollection : ScriptableObject
{
    [field: SerializeField] public List<Card> CardsInCollection { get; private set; } = new();
    [field: SerializeField] public GameObject cardBack;

    const string CARDS_PATH = "Assets/_Game/Prefabs/Game/Card/UniqueCards/";
    const string BACKS_PATH = "Assets/_Game/Prefabs/Game/Card/UniqueCardBacks/";

    [System.Serializable]
    class SaveData
    {
        public List<string> cardNames = new();
        public string cardBackName;
    }

    public void RemoveCardFromCollection(Card card)
    {
        if (CardsInCollection.Contains(card))
            CardsInCollection.Remove(card);
        else
            Debug.LogWarning("Card is not present in collection, but you try remove card");
    }

    public void AddCardToCollection(Card card)
    {
        CardsInCollection.Add(card);
    }

    public void ClearCollection() => CardsInCollection.Clear();

    public void SaveDataToJson()
    {
        if (CardsInCollection.Count == 0)
        {
            SetStandartDeck();
        }
        var saveData = new SaveData();

        foreach (var card in CardsInCollection)
        {
            if (card != null)
                saveData.cardNames.Add(card.gameObject.name);
        }

        // Сохраняем имя cardBack
        if (cardBack != null)
        {
            saveData.cardBackName = cardBack.name;
        }

        // Сериализуем и сохраняем
        string json = JsonUtility.ToJson(saveData, true);
        string pathToFile = Application.persistentDataPath + "/MyDeck.json";
        File.WriteAllText(pathToFile, json);
    }

    public void LoadDataFromJson()
    {
        string filePath = Application.persistentDataPath + "/MyDeck.json";

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Save file not found");
            SetStandartDeck();
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData == null)
        {
            Debug.LogError("Failed to parse JSON");
            return;
        }

        CardsInCollection.Clear();
        foreach (var cardName in saveData.cardNames)
        {
            Card card = LoadCardByName(cardName);
            if (card != null)
            {
                CardsInCollection.Add(card);
            }
            else
            {
                Debug.LogWarning($"Failed to load card: {cardName}");
            }
        }

        if (CardsInCollection.Count == 0)
            SetStandartDeck();

        if (!string.IsNullOrEmpty(saveData.cardBackName))
        {
            cardBack = LoadCardBackByName(saveData.cardBackName);
        }
    }

    private void SetStandartDeck()
    {
        GlobalCardCollection globalCards = GameController.Instance.globalCards;
        CardsInCollection.Clear();

        for(int i = 0; i < globalCards.GlobalCardsDictionary.Count; i++)
        {
            CardsInCollection.Add(globalCards.GlobalCardsDictionary.Values.ToList()
    [Random.Range(0, globalCards.GlobalCardsDictionary.Count)]);
        }

        cardBack = globalCards.GlobalCardBacks[Random.Range(
            0, globalCards.GlobalCardBacks.Count)];
    }

    private Card LoadCardByName(string cardName)
    {
        foreach(Card card in GameController.Instance.globalCards.GlobalCardsDictionary.Values)
        {
            if (card.gameObject.name == cardName)
                return card;
        }
        Debug.LogWarning($"Card '{cardName}' not found. Check if it exists at path: {CARDS_PATH}");
        return null;
    }

    private GameObject LoadCardBackByName(string backName)
    {
        foreach (GameObject obj in GameController.Instance.globalCards.GlobalCardBacks)
        {
            if (obj.name == backName)
                return obj;
        }
        Debug.LogWarning($"Card back '{backName}' not found. Check if it exists at path: {BACKS_PATH}");
        return null;
    }
}