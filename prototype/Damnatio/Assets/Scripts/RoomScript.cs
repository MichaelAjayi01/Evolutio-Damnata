using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
public class RoomScript : MonoBehaviour
{
    public enum _roomsType {
        standard,
        shop,
        boss
    }

    enum _objectives
    {
        clear,
        boss
    }

    [SerializeField]
    GameObject backgroundImg; //<--- this has been set in the editor
    Sprite newBackgroundImage;
    [SerializeField]
    GameObject monsterPrefab;

    [SerializeField]
    Sprite placeHolderSprites;
    [SerializeField]
    int placeHolderSpriteCount = 3;

    public _roomsType roomsType;
    static float _playAreaHeight = 423f; //<--this is the play area height
    static float _playAreaWidth = 527f; //<--this is the play area height
    [SerializeField]
    float _initalOffsetX = 153f;
    [SerializeField]
    float _initalOffsetY = 220f;

    //this needs to be set in generate room, need to be even
    //also used to show where the monster is on the map reference design doc
    //index 0 == player them selves
    //index last == enemy if we have attacking wizards
    //the rest are monsters split evenyl
    //  \-so player side monster are 1 -> (size-2)/2
    //  |-enemys are ((size-2)/2)+1 -> size size - 2
    [SerializeField]
    GameObject[] entities = new GameObject[8];
    int numberOfEntites = 8;

    //this is a self setup function, use enemy generate
    //will need to somehow scale the backgroun obj with the screen size
    public void generateRoom(_roomsType roomType) {
        this.roomsType = roomType;
        //get room image(random background image)
        GlobalResources globalResources = GameObject.Find("ResourceManagaer").GetComponent<GlobalResources>();
        newBackgroundImage = globalResources.dungeonRooms[Random.Range(0, globalResources.dungeonRooms.Count)];
        backgroundImg = GameObject.Find("Canvas");
        backgroundImg.GetComponent<Image>().sprite = newBackgroundImage;

        //add room image to resizer (all background will be resized once rooms have been generated) 

        //set number of entites(make sure its even)
        entities = new GameObject[numberOfEntites];

        //needs to generate room monsters here
        int numMonsters = Random.Range(1, (numberOfEntites - 2) - ((numberOfEntites - 2) / 2) + 1);
        float spaceBetweenMonsters = _playAreaHeight / numMonsters;
        float spaceBetweenMonstersX = _playAreaWidth / numMonsters;

        if (roomsType == _roomsType.shop) { numMonsters = 0; }
        if (roomsType == _roomsType.boss) { numMonsters = 1; }
        for (int i = 0; i < numMonsters; i++) {

            GameObject canv = GameObject.Find("Canvas");
            RectTransform canvRect = canv.GetComponent<RectTransform>();
            Vector2 centre = new Vector2((canvRect.rect.width / 2) * canv.transform.localScale.x, (canvRect.rect.height / 2) * canv.transform.localScale.y);

            float newy = centre.y - (_initalOffsetY + (spaceBetweenMonsters * i));
            float newx = centre.x + _initalOffsetX + (spaceBetweenMonstersX * i);

            GameObject newMonster = Instantiate(monsterPrefab, canv.transform);

            if (roomsType == _roomsType.boss) {
                newMonster.GetComponent<MonsterScript>().GenerateMonster(gameObject, entities.Length-1, MonsterScript._monsterType.Boss);//goes in thta last one so it appears centre
                entities[numberOfEntites-1] = newMonster;
                break; //makes sure that only one monster, the boss, exists
            }
            else {
                newMonster.GetComponent<MonsterScript>().GenerateMonster(gameObject, (((numberOfEntites - 2) / 2) + 1) + i, MonsterScript._monsterType.Enemy);
                entities[(((numberOfEntites - 2) / 2) + 1) + i] = newMonster;
                newMonster.transform.position = new Vector3(newx, newy, 0);
                newMonster.transform.localScale = new Vector3(-7, 7, 7);
            }
            Debug.Log(newx+ " :newx -- newy: "+newy);
        }



        /*
         
         
         
         
         <---- DISPLAY CHOICE HIGHLIGHTS CODE --->
         
         
         
         
         */

        spaceBetweenMonsters = _playAreaHeight / 3;
        spaceBetweenMonstersX = _playAreaWidth / 3;

        for (int i = 0; i < placeHolderSpriteCount; i++)
        {
            GameObject canv = GameObject.Find("Canvas");
            RectTransform canvRect = canv.GetComponent<RectTransform>();
            Vector2 centre = new Vector2((canvRect.rect.width / 2) * canv.transform.localScale.x, (canvRect.rect.height / 2) * canv.transform.localScale.y);

            float newy = centre.y - (_initalOffsetY + (spaceBetweenMonsters * i));
            float newx = centre.x - _initalOffsetX - (spaceBetweenMonstersX * i);

            GameObject newMonster = Instantiate(monsterPrefab, GameObject.Find("Canvas").transform);
            newMonster.GetComponent<MonsterScript>().GenerateMonster(gameObject, i, MonsterScript._monsterType.Friendly, placeHolderSprites);
            entities[i] = newMonster;
            newMonster.transform.position = new Vector3(newx, newy, 0);
            newMonster.transform.localScale = new Vector3(7, 7, 7);
            //newMonster.GetComponent<SpriteRenderer>().flipX = true;
        }


        //deactivates current room, main room will be activated by map script
        unloadRoom();
    }


    public void loadRoom() {
        gameObject.SetActive(true);
        foreach ( GameObject a in entities) {
            if (a != null)
            {
                a.GetComponent<MonsterScript>().loadMonster();
            }
        }
        backgroundImg.GetComponent<Image>().sprite = newBackgroundImage;
    }

    public void unloadRoom() {
        gameObject.SetActive(false);
        foreach (GameObject a in entities)
        {
            if (a != null)
            {
                a.GetComponent<MonsterScript>().unloadMonster();
            }
        }
    }

    //----------events used by monsters to affect other monsters-----------------
    public void attackEvent(int AttackingID, int AttackedID, float Damage) {
        float atkDamage = entities[AttackedID].GetComponent<MonsterScript>().getAttackDamage();
        entities[AttackedID].GetComponent<MonsterScript>().takeDamage(atkDamage);
    }
    public void attackBuffEvent(int BuffingID, float buff) {
        entities[BuffingID].GetComponent<MonsterScript>().attackBuff(buff);
    }
    public void healEvent(int healingID, float health)
    {
        entities[healingID].GetComponent<MonsterScript>().heal(health);
    }

    //---------------------------fucntions used by mosnters to get information about other monsters
    //returns array with all of enemy monsters
    public GameObject[] returnEnemys() {
        int size = entities.Length;
        GameObject[] enemysObj = new GameObject[(size - 2) / 2];
        for (int i = ((size - 2) / 2) + 1; i < size - 2; i++) {
            enemysObj[i - ((size - 2) / 2) + 1] = entities[i];
        }
        return enemysObj;
    }
    //returns array with all of the player's spawned monsters
    public GameObject[] returnPlayerMonsters() {
        int size = entities.Length;
        GameObject[] playerMonsters = new GameObject[(size - 2) / 2];
        for (int i = 1; i < (size - 2) / 2; i++) {
            playerMonsters[i - 1] = entities[i];
        }
        return playerMonsters;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(backgroundImg.transform.localScale.y / backgroundImg.GetComponent<SpriteRenderer>().sprite.bounds.size.y);
    }
}
