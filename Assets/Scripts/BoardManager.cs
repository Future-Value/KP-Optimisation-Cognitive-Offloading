using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour
{
    // Resoultion width and Height
    // CAUTION! Modifying this does not modify the Screen resolution.
    // items will be displayed inside a 1600x900 box
    public static int resolutionWidth = 1600;
    public static int resolutionHeight = 900;

    // leave some margin for the "TooHeavy" text and centre timer
    public static int bottommargin = 100;
    public static int centremargin = 200;
    
    // Number of Columns and rows of the grid (the possible positions of the items).
    // 1920 x 1080; 16:9; 100 pixels should be sufficient
    public static int columns;
    public static int rows;

    //Prefab of the item interface configuration
    public static GameObject KSItemPrefab;

    //A canvas where all the board is going to be placed
    private GameObject canvas;

    //If randomization of buttons:
    //1: No/Yes 0: Yes/No
    public static int ReverseButtons;

    // Current counters
    public static Text ValueText;
    public static Text WeightText;
    public Text WeightLeft;
    public Text TooHeavy;

    public Text Note1;
    public Text Note2;
    public Text Note3;
    public Text Note4;
    public Text Note5;
    public Text Note6;
    public Text Note7;
    public Text Note8;
    public Text Note9;
    public Text Note10;
    public Text Note11;
    public Text Note12;
    public Text Note13;
    public Text Note14;
    public Text Note15;
    public Text Note16;
    public Text Note17;
    public Text Note18;
    public Text Note19;
    public Text Note20;
    public Text Note21;
    public Text Note22;
    public Text Note23;
    public Text Note24;
    public Text Note25;
    public Text Note26;
    public Text Note27;
    public Text Note28;
    public Text Note29;
    public Text Note30;

    public static List<string> notelist;
    public static int notelistindex;

    public static int submitcounter;


    //The possible positions of the items;
    private List<Vector2> gridPositions = new List<Vector2>();

    //Weights and value vectors for this trial. CURRENTLY SET UP TO ALLOW ONLY INTEGERS.
    //ws and vs must be of the same length
    public static int[] ws;
    public static int[] vs;
    public static int nitems;
    public static string question;
    // to record optimal value
    public static int solution;

    public static int confidence_trialinfo;

    // A list to store the previous item numbers
    public static List<int> previousitems = new List<int>();

    // Reset button
    public Button Reset;

    public Button TestButton;

    // Answer button
    public Button Answer;

    //These variables shouldn't be modified. They just state that the area of the value part of the item and the weight part are assumed to be 1.
    public static float minAreaBill = 1f;
    public static float minAreaWeight = 1f;

    //The total area of all the items. Separated by the value part and the weighy part. A good initialization for this variables is the number of items plus 1.
    public static int totalAreaBill = 8;
    public static int totalAreaWeight = 8;

    // Input fields, this helps with auto focus later on
    public static InputField pID;
    public static InputField rID;
    public static InputField EnterNum;

    //Structure with the relevant parameters of an item.
    //gameItem: is the game object
    //coorValue1: The coordinates of one of the corners of the encompassing rectangle of the Value Part of the Item. The coordinates are taken relative to the center of the item.
    //coorValue2: The coordinates of the diagonally opposite corner of the Value Part of the Item.
    //coordWeight1 and coordWeight2: Same as before but for the weight part of the item.
    private struct Item
    {
        public GameObject gameItem;
        public Vector2 center;
        public int ItemNumber;
        public Button ItemButton;
    }

    //The items for the scene are stored here.
    private static Item[] items;


    // The list of all the button clicks. Each event contains the following information:
    // ItemNumber (a number between 1 and the number of items.)
    // Item is being selected In=1; Out=0 
    // Time of the click with respect to the beginning of the trial 
    public static List<Click> itemClicks = new List<Click>();

    public struct Click
    {
        // itemnumber (itemnumber or 100=Reset). State: In(1)/Out(0)/Invalid(2)/Reset(3). Time in seconds
        public int ItemNumber;
        public string description;
        public string offloading;
        public string confidence;
        public int State;
        public float time;
    }

    // To keep track of the number of items visited
    public static int itemsvisited = 0;

    // Current Instance number
//    public static int currInstance;
    public static int currOffloading;

    /// Macro function that initializes the Board
    public void SetupTrial()
    {
        previousitems.Clear();
        itemClicks.Clear();
        GameManager.valueValue = 0;
        GameManager.weightValue = 0;
        itemsvisited = 0;

        canvas = GameObject.Find("Canvas");

        SetKPInstance();

        //If the bool returned by LayoutObjectAtRandom() is false, then retry again:
        //Destroy all items. Initialize list again and try to place them once more.
        int nt = 0;
        bool itemsPlaced = false;
        while (nt < 10 && !itemsPlaced)
        {
            GameObject[] items1 = GameObject.FindGameObjectsWithTag("Item");

            foreach (GameObject item in items1)
            {
                Destroy(item);
            }

            InitialiseList();

            itemsPlaced = LayoutObjectAtRandom();
            nt++;
        }
    }
    //Initializes the instance for this trial:
    //1. Sets the question string using the instance (from the .txt files)
    //2. The weight and value vectors are uploaded
    //3. The instance prefab is uploaded
    void SetKPInstance()
    {
        KSItemPrefab = (GameObject)Resources.Load("KSItem3");

//        currInstance = GameManager.Randomization[GameManager.TotalTrials - 1];

        currOffloading = GameManager.offloadingRandomization[GameManager.TotalTrials - 1];
        question = GameManager.kpinstances[GameManager.TotalTrials - 1].capacity + "kg?";


        // Instance information
        Debug.Log("Setting up  Instance: Block " + (GameManager.block) + "/" + GameManager.numberOfBlocks + 
            ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + 
            GameManager.TotalTrials + " , Current Instance " + GameManager.Randomization[GameManager.TotalTrials - 1]);

        ws = GameManager.kpinstances[GameManager.TotalTrials - 1].weights;
        vs = GameManager.kpinstances[GameManager.TotalTrials - 1].values;

        solution = GameManager.kpinstances[GameManager.TotalTrials - 1].solution;

        confidence_trialinfo = 0;

        // Display current value
        ValueText = GameObject.Find("ValueText").GetComponent<Text>();
        
        // Display current weight
        WeightText = GameObject.Find("WeightText").GetComponent<Text>();
        
        // Display current weight left
        WeightLeft = GameObject.Find("WeightLeft").GetComponent<Text>();
        
        // Show when weight is excessive
        TooHeavy = GameObject.Find("TooHeavy").GetComponent<Text>();
        
        // make reset button clickable
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);
        
        // make calculation buttons clickable
        Button ShowCalcValue = GameObject.Find("ShowCalcValue").GetComponent<Button>();
        ShowCalcValue.onClick.AddListener(TotalValue);
        Button ShowCalcWeight = GameObject.Find("ShowCalcWeight").GetComponent<Button>();
        ShowCalcWeight.onClick.AddListener(TotalWeight);
        
        // make store buttons clickable
        Button StoreValue = GameObject.Find("StoreValue").GetComponent<Button>();
        StoreValue.onClick.AddListener(MemoriseValue);
        Button StoreWeight = GameObject.Find("StoreWeight").GetComponent<Button>();
        StoreWeight.onClick.AddListener(MemoriseWeight);
        Button StoreItem = GameObject.Find("StoreItem").GetComponent<Button>();
        StoreItem.onClick.AddListener(MemoriseItem);
        

        // make answer button clickable
        Answer = GameObject.Find("Answer").GetComponent<Button>();
        Answer.onClick.AddListener(FinishTrial);

        Button SubmitAnswer = GameObject.Find("SubmitAnswer").GetComponent<Button>();
        SubmitAnswer.onClick.AddListener(RemoveEverything);


        // transfer stored text to textboxes
        Note1 = GameObject.Find("Note1").GetComponent<Text>();
        Note2 = GameObject.Find("Note2").GetComponent<Text>();
        Note3 = GameObject.Find("Note3").GetComponent<Text>();
        Note4 = GameObject.Find("Note4").GetComponent<Text>();
        Note5 = GameObject.Find("Note5").GetComponent<Text>();
        Note6 = GameObject.Find("Note6").GetComponent<Text>();
        Note7 = GameObject.Find("Note7").GetComponent<Text>();
        Note8 = GameObject.Find("Note8").GetComponent<Text>();
        Note9 = GameObject.Find("Note9").GetComponent<Text>();
        Note10 = GameObject.Find("Note10").GetComponent<Text>();
        Note11 = GameObject.Find("Note11").GetComponent<Text>();
        Note12 = GameObject.Find("Note12").GetComponent<Text>();        
        Note13 = GameObject.Find("Note13").GetComponent<Text>();
        Note14 = GameObject.Find("Note14").GetComponent<Text>();
        Note15 = GameObject.Find("Note15").GetComponent<Text>();
        Note16 = GameObject.Find("Note16").GetComponent<Text>();
        Note17 = GameObject.Find("Note17").GetComponent<Text>();
        Note18 = GameObject.Find("Note18").GetComponent<Text>();
        Note19 = GameObject.Find("Note19").GetComponent<Text>();
        Note20 = GameObject.Find("Note20").GetComponent<Text>();
        Note21 = GameObject.Find("Note21").GetComponent<Text>();
        Note22 = GameObject.Find("Note22").GetComponent<Text>();
        Note23 = GameObject.Find("Note23").GetComponent<Text>();
        Note24 = GameObject.Find("Note24").GetComponent<Text>();
        Note25 = GameObject.Find("Note25").GetComponent<Text>();
        Note26 = GameObject.Find("Note26").GetComponent<Text>();
        Note27 = GameObject.Find("Note27").GetComponent<Text>();
        Note28 = GameObject.Find("Note28").GetComponent<Text>();
        Note29 = GameObject.Find("Note29").GetComponent<Text>();
        Note30 = GameObject.Find("Note30").GetComponent<Text>();

        notelist = new List<string>();
        notelistindex = 0;

        submitcounter = 0;

        ValueText.text = "TOTAL VALUE: $0";
        WeightText.text = "TOTAL WEIGHT: 0kg";

        if (GameManager.size == 1)
        {
            GameObject.Find("Timer1").SetActive(false);
        }
        else
        {
            GameObject.Find("Timer").SetActive(false);
        }

        // set question text
        Text Quest = GameObject.Find("Question").GetComponent<Text>();
        Quest.text = question;

        // Disable answer button if this is optimisation.
        if (GameManager.decision == 0)
        {
            GameObject answerbutton = GameObject.Find("Answer") as GameObject;
            answerbutton.SetActive(false);
        }


        if (currOffloading == 0)
        {
            GameObject.Find("ShowCalcValue").SetActive(false);
            GameObject.Find("ShowCalcWeight").SetActive(false);
            GameObject.Find("StoreValue").SetActive(false);
            GameObject.Find("StoreWeight").SetActive(false);
            GameObject.Find("StoreItem").SetActive(false);
            GameObject.Find("Reset").SetActive(false);
            GameObject.Find("ValueText").SetActive(false);
            GameObject.Find("WeightText").SetActive(false);
            GameObject.Find("Note1").SetActive(false);
            GameObject.Find("Note2").SetActive(false);
            GameObject.Find("Note3").SetActive(false);
            GameObject.Find("Note4").SetActive(false);
            GameObject.Find("Note5").SetActive(false);
            GameObject.Find("Note6").SetActive(false);
            GameObject.Find("Note7").SetActive(false);
            GameObject.Find("Note8").SetActive(false);
            GameObject.Find("Note9").SetActive(false);
            GameObject.Find("Note10").SetActive(false);
            GameObject.Find("Note11").SetActive(false);
            GameObject.Find("Note12").SetActive(false);
            GameObject.Find("Note13").SetActive(false);
            GameObject.Find("Note14").SetActive(false);
            GameObject.Find("Note15").SetActive(false);
            GameObject.Find("Note16").SetActive(false);
            GameObject.Find("Note17").SetActive(false);
            GameObject.Find("Note18").SetActive(false);
            GameObject.Find("Note19").SetActive(false);
            GameObject.Find("Note20").SetActive(false);
            GameObject.Find("Note21").SetActive(false);
            GameObject.Find("Note22").SetActive(false);
            GameObject.Find("Note23").SetActive(false);
            GameObject.Find("Note24").SetActive(false);
            GameObject.Find("Note25").SetActive(false);
            GameObject.Find("Note26").SetActive(false);
            GameObject.Find("Note27").SetActive(false);
            GameObject.Find("Note28").SetActive(false);
            GameObject.Find("Note29").SetActive(false);
            GameObject.Find("Note30").SetActive(false);
            GameObject.Find("Notepad").SetActive(false);
        }
    }

    //This Initializes the GridPositions which are the possible places where the items will be placed.
    void InitialiseList()
    {
        gridPositions.Clear();

        int radius = 300;
            for (int i = 0; i < ws.Length; i++)
            {
                // Generate a new item every this many radians...
                double radian_separation = (360f / ws.Length * Math.PI) / 180;
                //Debug.Log((float)Math.Sin(radian_separation * i) * radius + " " +
                //    (float)Math.Cos(radian_separation * i) * radius);
                gridPositions.Add(new Vector2((float)Math.Sin(radian_separation * i) * radius, 
                    (float)Math.Cos(radian_separation * i) * radius));
            }

        /*
        if (GameManager.reward == 1 || GameManager.cost == 1)
        {
            int radius = 350;
            for (int i = 0; i < ws.Length; i++)
            {
                // Generate a new item every this many radians...
                double radian_separation = (360f / ws.Length * Math.PI) / 180;
                //Debug.Log((float)Math.Sin(radian_separation * i) * radius + " " +
                //    (float)Math.Cos(radian_separation * i) * radius);
                gridPositions.Add(new Vector2((float)Math.Sin(radian_separation * i) * radius, 
                    (float)Math.Cos(radian_separation * i) * radius));
            }
        }
        else
        {
            // Generate a list of possible positions, this is shaped like a box with a centre cut out
            for (int x = -resolutionWidth / 2; x < resolutionWidth / 2; x += resolutionWidth / columns)
            {
                for (int y = -resolutionHeight / 2 + bottommargin; y < resolutionHeight / 2; y += ((resolutionHeight - bottommargin) / rows))
                {
                    if (Math.Abs(x) > centremargin || Math.Abs(y) > centremargin)
                    {
                        gridPositions.Add(new Vector2(x, y));
                    }
                }
            }
        }
        */
        //Debug.Log("Number of possible positions: " + gridPositions.Count);
    }


    //Returns a random position from the grid and removes the item from the list.
    Vector2 RandomPosition()
    {
        // int randomIndex = Random.Range(0, gridPositions.Count);
        int randomIndex = 0;
        Vector2 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    // Places all the objects from the instance (ws,vs) on the canvas. 
    // Returns TRUE if all items where positioned, FALSE otherwise.
    private bool LayoutObjectAtRandom()
    {
        int objectCount = ws.Length;
        items = new Item[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            bool objectPositioned = false;

            Item itemToLocate = GenerateItem(i, new Vector2(-2000, -2000));
            //Debug.Log("Local: " + itemToLocate.gameItem.transform.localPosition);
            //Debug.Log("Global: " + itemToLocate.gameItem.transform.position);
            while (!objectPositioned && gridPositions.Count > 0)
            {
                Vector2 randomPosition = RandomPosition();
                //Instantiates the item and places it.
                itemToLocate.gameItem.transform.localPosition = randomPosition;
                itemToLocate.center = new Vector2(itemToLocate.gameItem.transform.localPosition.x,
                    itemToLocate.gameItem.transform.localPosition.y);

                items[i] = itemToLocate;
                objectPositioned = true;
            }

            if (!objectPositioned)
            {
                Debug.Log("Not enough space to place all items... " +
                    "ran out of randomPositions");
                return false;
            }
        }
        return true;
    }

    // Instantiates an Item and places it on the position from the input
    // The item placing here is temporary; The real placing is done by the LayoutObjectAtRandom() method.
    Item GenerateItem(int itemNumber, Vector2 tempPosition)
    {
        //Instantiates the item and places it.
        GameObject instance = Instantiate(KSItemPrefab, tempPosition,
            Quaternion.identity) as GameObject;
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);

        //Gets the subcomponents of the item 
        GameObject bill = instance.transform.Find("Bill").gameObject;
        GameObject weight = instance.transform.Find("Weight").gameObject;
        GameObject itemcircle = instance.transform.Find("Item Number").gameObject;

        //Sets the Text of the items
        bill.GetComponentInChildren<Text>().text = "$" + vs[itemNumber];
        weight.GetComponentInChildren<Text>().text = ws[itemNumber].ToString() + "kg";
        itemcircle.GetComponentInChildren<Text>().text = (itemNumber + 1).ToString();

        // circle....text = itemNumber.ToString()

        /*        
        if (GameManager.size == 1)
        {
            // Calculates the area of the Value and Weight sections of the item according to approach 2 
            // and then Scales the sections so they match the corresponding area.
            Vector3 curr_billscale = bill.transform.localScale;
            float billscale = (float)Math.Pow(vs[itemNumber] / vs.Average(), 0.6) * curr_billscale.x - 0.15f;

            if (billscale < 0.7f * curr_billscale.x)
            {
                billscale = 0.7f * curr_billscale.x;
            }
            else if (billscale > 1.0f * curr_billscale.x)
            {
                billscale = 1.0f * curr_billscale.x;
            }

            bill.transform.localScale = new Vector3(billscale,
                billscale, billscale);

            Vector3 curr_weightscale = weight.transform.localScale;
            float weightscale = (float)Math.Pow(ws[itemNumber] / ws.Average(), 0.6) * curr_weightscale.x - 0.15f;

            if (weightscale < 0.7f * curr_weightscale.x)
            {
                weightscale = 0.7f * curr_weightscale.x;
            }
            else if (weightscale > 1.0f * curr_weightscale.x)
            {
                weightscale = 1.0f * curr_weightscale.x;
            }

            weight.transform.localScale = new Vector3(weightscale,
                weightscale, weightscale);
        }
        */

        Item itemInstance = new Item
        {
            gameItem = instance,
        };

        itemInstance.ItemButton = itemInstance.gameItem.GetComponent<Button>();
        itemInstance.ItemNumber = itemNumber;

        itemInstance.ItemButton.onClick.AddListener(delegate {
            GameManager.gameManager.boardScript.ClickOnItem(itemInstance);
        });

        return (itemInstance);
    }

    void ClickOnItem(Item itemToLocate)
    {
        // Check if click is valid
        // Debug.Log("Item Clicked: " + itemToLocate.ItemNumber);
        Light myLight = itemToLocate.gameItem.GetComponent<Light>();

        if (myLight.enabled == true)
        {
            if ((GameManager.weightValue - ws[itemToLocate.ItemNumber]) <=
            GameManager.kpinstances[GameManager.TotalTrials - 1].capacity)
            {
                TooHeavy.text = " ";
            }
            myLight.enabled = false;
            RemoveItem(itemToLocate);
        }
        else
        {
            if (ClickValid(itemToLocate) == true)
            {
                myLight.enabled = true;
                AddItem(itemToLocate);
            }
        }
    }

    bool ClickValid(Item itemToLocate)
    {
        if ((GameManager.weightValue + ws[itemToLocate.ItemNumber]) >
           GameManager.kpinstances[GameManager.TotalTrials - 1].capacity)
        {
            TooHeavy.text = "Weight Limit Exceeded!";
            WeightLeft.text = "Exceeded";
        }
        else
        {
            TooHeavy.text = " ";
            WeightLeft.text = "Excess Capacity: " + (GameManager.kpinstances[GameManager.TotalTrials - 1].capacity -
            GameManager.weightValue).ToString() + "kg";
        }

        return true;
    }

    //Updates the timer rectangle size accoriding to the remaining time.
    public void UpdateTimer()
    {
        if (GameManager.size != 1 && GameManager.escena == "Trial")
        {
            Image timer = GameObject.Find("TimerBar").GetComponent<Image>();
            timer.fillAmount = GameManager.tiempo / GameManager.totalTime;

            if (timer.fillAmount > 0.5)
            {
                Byte Red = (byte)Convert.ToInt32(255 * ((1 - timer.fillAmount) * 2));
                Byte Green = 255;
                timer.color = new Color32(Red, Green, 128, 255);
            }
            else
            {
                Byte Red = 255;
                Byte Green = (byte)Convert.ToInt32(255 * (timer.fillAmount * 2));
                timer.color = new Color32(Red, Green, 128, 255);
            }
        }
        else
        {
            Image timer = GameObject.Find("Timer").GetComponent<Image>();
            timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
        }
    }

    //Sets the triggers for pressing the corresponding keys
    //123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
    //changeToNextScene(answer) on game manager
    private void SetKeyInput()
    {
        /*
        if (GameManager.escena == "TrialAnswer")
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Left
                AnswerSelect("left");
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                //Right
                AnswerSelect("right");
            }
        }
        */

        if (GameManager.escena == "SetUp")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.SetTimeStamp();
                GameManager.ChangeToNextScene(itemClicks, false);
            }
        }
    }

    public void SetupInitialScreen()
    {
        //Button
        GameObject start = GameObject.Find("Start") as GameObject;
        start.SetActive(false);

        GameObject rand = GameObject.Find("RandomisationID") as GameObject;
        rand.SetActive(false);

        //Participant Input
        pID = GameObject.Find("ParticipantID").GetComponent<InputField>();


        InputField.SubmitEvent se = new InputField.SubmitEvent();
        se.AddListener((value) => SubmitPID(value, start, rand));
        pID.onEndEdit = se;

        //Randomisation Input
        rID = rand.GetComponent<InputField>();

        InputField.SubmitEvent se2 = new InputField.SubmitEvent();
        se2.AddListener((value) => SubmitRandID(value, start));
        rID.onEndEdit = se2;
    }

    private void SubmitPID(string pIDs, GameObject start, GameObject rand)
    {
        GameObject pID = GameObject.Find("ParticipantID");
        GameObject pIDT = GameObject.Find("ParticipantIDText");
        pID.SetActive(false);
        pIDT.SetActive(true);

        Text inputID = pIDT.GetComponent<Text>();
        inputID.text = "Randomisation Number";

        //Set Participant ID
        IOManager.participantID = pIDs;

        //Activate Randomisation Listener
        rand.SetActive(true);

    }

    private void SubmitRandID(string rIDs, GameObject start)
    {
        GameObject rID = GameObject.Find("RandomisationID");
        GameObject pIDT = GameObject.Find("ParticipantIDText");
        rID.SetActive(false);
        pIDT.SetActive(false);

        //Set Participant ID
        IOManager.randomisationID = rIDs;

        //Activate Start Button and listener
        start.SetActive(true);

        Button startButton = GameObject.Find("Start").GetComponent<Button>();
        startButton.onClick.AddListener(StartClicked);
    }

    public static string GetItemCoordinates()
    {
        string coordinates = "";
        foreach (Item it in items)
        {
            coordinates = coordinates + "(" + it.center.x + "," + it.center.y + ")";
        }
        return coordinates;
    }


    public static void StartClicked()
    {
        Debug.Log("Start Button Clicked");
        GameManager.SetTimeStamp();
        GameManager.ChangeToNextScene(itemClicks, false);
    }

    // Function to display distance and weight in Unity
    /*
    void SetTopRowText()
    {
        CalcValue();
        ValueText.text = "Total Value: $" + GameManager.valueValue.ToString();

        CalcWeight();
        WeightText.text = "Total Weight: " + GameManager.weightValue.ToString() + "kg";
        if ((GameManager.kpinstances[GameManager.TotalTrials - 1].capacity - GameManager.weightValue) >= 0)
        {
            WeightLeft.text = "Excess Capacity: " + (GameManager.kpinstances[GameManager.TotalTrials - 1].capacity -
                GameManager.weightValue).ToString() + "kg";
        }
    }
    */

    // Add current item to previous items
    static void AddItem(Item itemToLocate)
    {
        previousitems.Add(itemToLocate.ItemNumber);
        itemsvisited = previousitems.Count();

        Click newclick;
        newclick.ItemNumber = itemToLocate.ItemNumber;
        newclick.description = "item";
        newclick.offloading = "NA";
        newclick.confidence = "NA";
        newclick.State = 1;
        newclick.time = GameManager.totalTime - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Remove current item from previous items
    static void RemoveItem(Item itemToLocate)
    {
        previousitems.Remove(itemToLocate.ItemNumber);
        itemsvisited = previousitems.Count();

        Click newclick;
        newclick.ItemNumber = itemToLocate.ItemNumber;
        newclick.description = "item";
        newclick.offloading = "NA";
        newclick.confidence = "NA";
        newclick.State = 0;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    public void ResetClicked()
    {
        if (previousitems.Count() != 0)
        {
            Lightoff();
            previousitems.Clear();
            TotalValue();
            TotalWeight();
            itemsvisited = 0;

            Click newclick;
            newclick.ItemNumber = 200;
            newclick.description = "reset";
            newclick.offloading = "NA";
            newclick.confidence = "NA";
            newclick.State = 2;
            newclick.time = GameManager.timeQuestion - GameManager.tiempo;
            itemClicks.Add(newclick);
        }
    }

    public static void CalcValue()
    {
        int[] individualvalues = new int[previousitems.Count()];
        if (previousitems.Count() > 0)
        {
            for (int i = 0; i <= (previousitems.Count() - 1); i++)
            {
                individualvalues[i] = vs[previousitems[i]];
            }

            GameManager.valueValue = individualvalues.Sum();
        }
        else
        {
            GameManager.valueValue = 0;
        }
    }

    public static void CalcWeight()
    {
        int[] individualweights = new int[previousitems.Count()];
        if (previousitems.Count() > 0)
        {
            for (int i = 0; i <= (previousitems.Count() - 1); i++)
            {
                individualweights[i] = ws[previousitems[i]]; ;
            }

            GameManager.weightValue = individualweights.Sum();
        }
        else
        {
            GameManager.weightValue = 0;
        }
    }

    static void TotalValue()
    {
        CalcValue();
        ValueText.text = "TOTAL VALUE: $" + GameManager.valueValue.ToString();
        
        Click newclick;
        newclick.ItemNumber = 300;
        newclick.description = "calculate_value";
        newclick.offloading = ValueText.text.Substring(13);
        newclick.confidence = "NA";
        newclick.State = 3;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    static void TotalWeight()
    {
        CalcWeight();
        WeightText.text = "TOTAL WEIGHT: " + GameManager.weightValue.ToString() + "kg";

        Click newclick;
        newclick.ItemNumber = 301;
        newclick.description = "calculate_weight";
        newclick.offloading = WeightText.text.Substring(14);
        newclick.confidence = "NA";
        newclick.State = 3;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    static void MemoriseValue()
    {
        string storedvalue = ValueText.text.Substring(13);
        
        if (storedvalue != "$0")
        {
            notelist.Add(storedvalue);
            notelistindex = notelistindex + 1;
            
            string notefinder = "Note" + notelistindex.ToString();
            GameObject.Find(notefinder).GetComponent<UnityEngine.UI.Text>().text = storedvalue;
        }
        
        Click newclick;
        newclick.ItemNumber = 400;
        newclick.description = "store_value";
        newclick.offloading = storedvalue;
        newclick.confidence = "NA";
        newclick.State = 3;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    static void MemoriseWeight()
    {
        string storedweight = WeightText.text.Substring(14);
        if (storedweight != "0kg")
        {
            notelist.Add(storedweight);
            notelistindex = notelistindex + 1;

            string notefinder = "Note" + notelistindex.ToString();
            GameObject.Find(notefinder).GetComponent<UnityEngine.UI.Text>().text = storedweight;
        }

        Click newclick;
        newclick.ItemNumber = 401;
        newclick.description = "store_weight";
        newclick.offloading = storedweight;
        newclick.confidence = "NA";
        newclick.State = 3;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    static void MemoriseItem()
    {
        string storeditem = "";
        string storeditem_displayonly = "";

        foreach (var i in previousitems)
        {
            if (previousitems.IndexOf(i) == previousitems.Count()-1)
            {
                storeditem = storeditem + i.ToString();
                storeditem_displayonly = storeditem_displayonly + (i+1).ToString();
            }
            else
            {
                storeditem = storeditem + i.ToString() + ",";
                storeditem_displayonly = storeditem_displayonly + (i+1).ToString() + ",";
            }
        }

        if (previousitems.Count() != 0)
        {
            notelist.Add(storeditem_displayonly);
            notelistindex = notelistindex + 1;

            string notefinder = "Note" + notelistindex.ToString();
            GameObject.Find(notefinder).GetComponent<UnityEngine.UI.Text>().text = storeditem_displayonly;
        }
        
        Click newclick;
        newclick.ItemNumber = 402;
        newclick.description = "store_order";
        newclick.offloading = storeditem;
        newclick.confidence = "NA";
        newclick.State = 3;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    public static void ConfidenceButtons()
    {
        Button button0 = GameObject.Find("Button0").GetComponent<Button>();
        Button button1 = GameObject.Find("Button1").GetComponent<Button>();
        Button button2 = GameObject.Find("Button2").GetComponent<Button>();
        Button button3 = GameObject.Find("Button3").GetComponent<Button>();
        Button button4 = GameObject.Find("Button4").GetComponent<Button>();
        Button button5 = GameObject.Find("Button5").GetComponent<Button>();
        Button button6 = GameObject.Find("Button6").GetComponent<Button>();

        button0.onClick.AddListener(delegate {ConfidenceSelect(0);});
        button1.onClick.AddListener(delegate {ConfidenceSelect(1);});
        button2.onClick.AddListener(delegate {ConfidenceSelect(2);});
        button3.onClick.AddListener(delegate {ConfidenceSelect(3);});
        button4.onClick.AddListener(delegate {ConfidenceSelect(4);});
        button5.onClick.AddListener(delegate {ConfidenceSelect(5);});
        button6.onClick.AddListener(delegate {ConfidenceSelect(6);});
    }

    public static void ConfidenceSelect(int ConfidenceLevel)
    {
        Click newclick;
        newclick.ItemNumber = 600 + ConfidenceLevel;
        newclick.description = "confidence";
        newclick.offloading = "NA";
        newclick.confidence = ConfidenceLevel.ToString();
        newclick.State = 4;
        newclick.time = GameManager.totalTime - GameManager.tiempo;
        itemClicks.Add(newclick);
        
        confidence_trialinfo = ConfidenceLevel;
        GameManager.ChangeToNextScene(itemClicks, true);
    }


    // Randomizes YES/NO button positions (left or right) and allocates corresponding script to save the correspondent answer.
    /*
    public static void RandomizeButtons()
    {
        Button btnLeft = GameObject.Find("Left").GetComponent<Button>();
        Button btnRight = GameObject.Find("Right").GetComponent<Button>();

        btnLeft.onClick.AddListener(delegate {
            AnswerSelect("left");
        });
        btnRight.onClick.AddListener(delegate {
            AnswerSelect("right");
        });

        ReverseButtons = Random.Range(0, 2);

        if (ReverseButtons == 1)
        {
            btnLeft.GetComponentInChildren<Text>().text = "No";
            btnRight.GetComponentInChildren<Text>().text = "Yes";
        }
        else
        {
            btnLeft.GetComponentInChildren<Text>().text = "Yes";
            btnRight.GetComponentInChildren<Text>().text = "No";
        }
    }
    */

    public static void FinishTrial()
    {
        Debug.Log("Skipped to answer screen");
        IOManager.SaveTimeStamp("Participant_Skip");
        GameManager.ChangeToNextScene(itemClicks, true);
    }

    /*
     public static void AnswerSelect(string LeftOrRight)
     {
         if ((LeftOrRight == "left" && ReverseButtons == 1) || (LeftOrRight == "right" && ReverseButtons == 0))
         {
             // reversed left, or unreversed right, means answer is NO.
             GameManager.answer = 0;
             Debug.Log("Trial number " + ((GameManager.block - 1) * GameManager.numberOfTrials +
                 GameManager.trial) + ", Answer chosen: NO");
             GameManager.ChangeToNextScene(itemClicks, true);
         }
         else
         {
             // reversed right, or unreversed left, means answer is YES.
             GameManager.answer = 1;
             Debug.Log("Trial number " + ((GameManager.block - 1) * GameManager.numberOfTrials +
                 GameManager.trial) + ", Answer chosen: YES");
             GameManager.ChangeToNextScene(itemClicks, true);
         }
     }
    */

    private static void Lightoff()
    {
        foreach (Item item in items)
        {
            Light myLight = item.gameItem.GetComponent<Light>();
            myLight.enabled = false;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SetKeyInput();

        if (GameManager.escena == "SetUp")
        {
            pID.ActivateInputField();
            rID.ActivateInputField();
        }

        if (GameManager.escena == "EnterNumber")
        {
            EnterNum.ActivateInputField();
        }

    }

    // remove all offloading mechanisms and prepare for submission
    public static void RemoveEverything()
    {
        IOManager.SaveTimeStamp("Submit_Answer");
        submitcounter = 1;
        
        Lightoff();
        previousitems.Clear();
        itemsvisited = 0;
        CalcValue();
        CalcWeight();

        GameObject.Find("SubmitAnswer").SetActive(false);

        if (currOffloading == 1)
        {
            GameObject.Find("ShowCalcValue").SetActive(false);
            GameObject.Find("ShowCalcWeight").SetActive(false);
            GameObject.Find("StoreValue").SetActive(false);
            GameObject.Find("StoreWeight").SetActive(false);
            GameObject.Find("StoreItem").SetActive(false);
            GameObject.Find("Reset").SetActive(false);
            GameObject.Find("ValueText").SetActive(false);
            GameObject.Find("WeightText").SetActive(false);
        }

        Click newclick;
        newclick.ItemNumber = 500;
        newclick.description = "submit";
        newclick.offloading = "NA";
        newclick.confidence = "NA";
        newclick.State = 2;
        newclick.time = GameManager.totalTime - GameManager.tiempo;
        itemClicks.Add(newclick);

        GameManager.tiempo = GameManager.timeSubmit;
        GameManager.totalTime = GameManager.timeSubmit;
    }
}