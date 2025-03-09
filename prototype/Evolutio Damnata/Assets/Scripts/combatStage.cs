using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CombatStage : MonoBehaviour
{
    public Sprite wizardOutlineSprite;

    [SerializeField]
    public GameObject manaBar;
    [SerializeField]
    public GameObject manaText;
    public int currentMana;

    [SerializeField]
    CardManager cardManager;
    [SerializeField]
    public CardLibrary cardLibrary;

    [SerializeField]
    CardOutlineManager cardOutlineManager;
    [SerializeField]
    CombatManager combatManager;

    [SerializeField]
    Canvas battleField;

    [SerializeField]
    public SpritePositioning spritePositioning;

    [SerializeField]
    DamageVisualizer damageVisualizer;

    [SerializeField]
    GameObject damageNumberPrefab;

    private bool buttonsInitialized = false;

    private CardSelectionHandler cardSelectionHandler;
    private ButtonCreator buttonCreator;
    private AttackHandler attackHandler;
    private EnemySpawner enemySpawner;
    private PlayerCardSpawner playerCardSpawner;

    private void Awake()
    {
        cardSelectionHandler = gameObject.AddComponent<CardSelectionHandler>();
        cardSelectionHandler.Initialize(cardManager, combatManager, cardOutlineManager, spritePositioning, this);

        buttonCreator = gameObject.AddComponent<ButtonCreator>();
        buttonCreator.Initialize(battleField, spritePositioning, cardSelectionHandler);

        attackHandler = new AttackHandler();
        enemySpawner = new EnemySpawner(spritePositioning, cardLibrary, damageVisualizer, damageNumberPrefab, wizardOutlineSprite);
        playerCardSpawner = new PlayerCardSpawner(spritePositioning, cardLibrary, cardOutlineManager, cardManager, combatManager, damageVisualizer, damageNumberPrefab, wizardOutlineSprite, manaBar, manaText, this);
    }

    // This function will be kept
    public void interactableHighlights()
    {
        if (buttonsInitialized) return;

        buttonCreator.AddButtonsToPlayerEntities();
        buttonCreator.AddButtonsToEnemyEntities();

        buttonsInitialized = true;
    }

    public void spawnPlayerCard(string cardName, int whichOutline)
    {
        playerCardSpawner.SpawnPlayerCard(cardName, whichOutline);
    }

    public void HandleMonsterAttack(EntityManager playerEntity, EntityManager enemyEntity)
    {
        attackHandler.HandleMonsterAttack(playerEntity, enemyEntity);
    }

    public void spawnEnemy(string cardName, int whichOutline)
    {
        enemySpawner.SpawnEnemy(cardName, whichOutline);
    }

    void Start()
    {
        // Start the coroutine to wait for room selection
        StartCoroutine(spritePositioning.WaitForRoomSelection());

        // Set all placeholders to be inactive initially
        StartCoroutine(spritePositioning.SetAllPlaceHoldersInactive());

        // Initialize interactable highlights
        StartCoroutine(InitializeInteractableHighlights());
    }

    private IEnumerator InitializeInteractableHighlights()
    {
        // Wait until placeholders are instantiated
        while (spritePositioning.playerEntities.Count == 0)
        {
            yield return null; // Wait for the next frame
        }

        // Initialize interactable highlights
        interactableHighlights();
    }

    public void updateManaUI()
    {
        manaBar.GetComponent<Slider>().value = currentMana;
        manaText.GetComponent<TMP_Text>().text = currentMana.ToString();
    }

    private void Update()
    {
        // Check if a card is selected and update placeholder visibility
        if (cardManager.currentSelectedCard != null)
        {
            EntityManager selectedCardEntityManager = cardManager.currentSelectedCard.GetComponent<EntityManager>();
            if (selectedCardEntityManager != null && selectedCardEntityManager.placed)
            {
                placeHolderActiveState(false);
                availableEnemyTargets(true);

                ReactivateSelectedCardPlaceholder();
            }
            else
            {
                placeHolderActiveState(true);
                availableEnemyTargets(false);
            }
        }
        else
        {
            placeHolderActiveState(false);
            availableEnemyTargets(false);
        }
    }

    private void availableEnemyTargets(bool active)
    {
        for (int i = 0; i < spritePositioning.enemyEntities.Count; i++)
        {
            if (spritePositioning.enemyEntities[i] != null)
            {
                Image placeholderImage = spritePositioning.enemyEntities[i].GetComponent<Image>();
                if (placeholderImage != null && placeholderImage.sprite != null)
                {
                    if (placeholderImage.sprite.name != "wizard_outline")
                    {
                        //apply effect here
                    }
                }
            }
        }
    }

    private void ReactivateSelectedCardPlaceholder()
    {
        for (int i = 0; i < spritePositioning.playerEntities.Count; i++)
        {
            if (spritePositioning.playerEntities[i] == cardManager.currentSelectedCard)
            {
                //apply selection effect here
                break;
            }
        }
    }

    public void placeHolderActiveState(bool active)
    {
        for (int i = 0; i < spritePositioning.playerEntities.Count; i++)
        {
            if (spritePositioning.playerEntities[i] != null)
            {
                Image placeholderImage = spritePositioning.playerEntities[i].GetComponent<Image>();
                if (placeholderImage != null && placeholderImage.sprite != null)
                {
                    if (placeholderImage.sprite.name == "wizard_outline")
                    {
                        spritePositioning.playerEntities[i].SetActive(active);
                    }
                }
            }
        }
    }
}
