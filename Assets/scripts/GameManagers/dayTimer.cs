﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class dayTimer : MonoBehaviour
{
    [SerializeField]
    private GameObject continueText;
    public static dayTimer instance;
    [SerializeField]
    private int secondsPerDay;

    [SerializeField]
    private TextMesh monitorClock;

    public int maxDays = 5;
    public float secondsDay { get { return secondsPerDay; } }
    private float currentTime;
    public float time { get { return currentTime; } }
    [SerializeField]
    private GameObject progressUI;
    [SerializeField]
    private Image bigHand, smallHand;
    [SerializeField]
    private GameObject emailIconPrefab;
    [SerializeField]
    private Animator dayFinishedText;
    [SerializeField]
    private Sprite junkMailSprite;
    public  List<completedEmail> todaysEmails = new List<completedEmail>();
    [SerializeField]
    private List<GameObject> emailObjects = null;
    [SerializeField]
    private AudioClip stampSound;
    [SerializeField]
    private Slider timeSlider;
    [SerializeField]
    private Text emailTargetText;
    [SerializeField]
    AudioClip tick;
    [Header("Ending stats")]
    [SerializeField]
    private Text StatsTitle;
    [SerializeField]
    private Text StatsDeath,StatsEmail,StatsProf,StatsBossAnger,StatsBossDeath,ContinuePrompt;
    [SerializeField]
    private Image endingScreen,StatsBox;
    [SerializeField]
    private Sprite PromotionScreen, FiredScreen, BossDeathScreen, DeathScreen, Unproffessional, Average, Friends;
    [SerializeField]
    private GameObject continueTexteEOD;
    bool once = true;

    [System.Serializable]
    public struct completedEmail
    {
        public bool junk;
        public bool correctAnswer;
    }

    enum weekDays
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday
    }

    [Header("Day Over UI")]
    public Text dayCompletedHeader;
    public Text filedText, performanceText, performanceResult;

    [Header("Day transition UI")]
    public Text DayText;
    public Text TimeText,Player1Text,Player2Text;
    public Image background;
    private bool finishedDisplay = false;

    public void NewDay()
    {
        if (StatTracker.instance.numOfDaysCompleted[0] == 0)//Making sure this only happens for 1 player, no need to play the intro email again for P2
        {
            GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);
            introMonitor.instance.BeginGame();
        }
        else
        {
            transitioning = false;
            SoundManager.instance.music.Play();
            GameStateManager.instance.ChangeState(GameStates.STATE_GAMEPLAY);
        }
    }

    public void NewDayTransition()
    {
        StartCoroutine(NextDayTransition());
    }

    void Awake()
    {
           instance = this;
    }
        
    void Update()
    {
        //Clock and timer based things
        if (GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY && currentTime < secondsPerDay)
        {
            currentTime += Time.deltaTime;
            if (once)
            {
                if (secondsPerDay-currentTime<8)
                {
                    SoundManager.instance.playSound(tick);
                    once = false;
                }
            }
            if (currentTime > secondsPerDay)
            {
                finishedDisplay = false;
                StartCoroutine(endOfDay());
            }

            //Go from 90 to -150
            smallHand.rectTransform.rotation = Quaternion.Euler(0, 0, 90+(currentTime/secondsPerDay * -240));

            //go from 0 to 2880
            bigHand.rectTransform.rotation = Quaternion.Euler(0, 0, (currentTime / secondsPerDay * -2880));

            timeSlider.value = currentTime / secondsPerDay;


            //9 to 5 means 8 hours, 8*60 = 480

            float hours = 9 + ((currentTime / secondsPerDay) * 8);
            hours = Mathf.FloorToInt(hours);
            float mins = (9 + ((currentTime / secondsPerDay) * 8))-hours;
            mins *= 60;
            mins = Mathf.FloorToInt(mins);
            monitorClock.text =  (mins < 10) ? hours + ":0" + mins : hours + ":" + mins;
            SoundManager.instance.PauseSound(tick, false);
        }
        if (GameStateManager.instance.GetState() != GameStates.STATE_GAMEPLAY)
        {
            if (SoundManager.instance.IsSoundPlaying(tick))
            {
                SoundManager.instance.PauseSound(tick, true);
            }
        }

        if (GameFinished)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                continueText.SetActive(false);
                StatsBox.gameObject.SetActive(false);
            }
            if (Input.GetButtonUp("Fire1"))
            {
                continueText.SetActive(true);
                StatsBox.gameObject.SetActive(true);
            }

            if (Input.GetButton("Fire1") && Input.GetAxis("Vertical") < 0)
            {
                GameFinished = false;
                continueText.SetActive(false);
                LeaderBoard.instance.SetScore(StatTracker.instance.GetScore());
                StatsBox.gameObject.SetActive(false);
            }
        }

        if (GameStateManager.instance.GetState() == GameStates.STATE_DAYOVER && finishedDisplay)
        {
            if(Input.GetButtonDown("Fire1") && !transitioning)
            {
                continueTexteEOD.SetActive(false);
                StatTracker.instance.CalculateProfessionalism();
                if (StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer] < maxDays)
                {
                    transitioning = true;
                    StartCoroutine(NextDayTransition());
                }
                else
                {
                    transitioning = true;
                    StartCoroutine(FinishGame());
                }
            }
        }
    }

    bool transitioning,GameFinished;

    IEnumerator FinishGame()
    {
        //Fade to black
        while (background.color.a < 1)
        {
            Color col = background.color;
            col.a += Time.deltaTime;
            background.color = col;
            yield return new WaitForEndOfFrame();
        }


        //Display stats window
        endingScreen.gameObject.SetActive(true);

        float performance = StatTracker.instance.getAveragePerformance();

        if (performance > 6)
        {
            endingScreen.sprite = PromotionScreen;
            StatsTitle.text = "You were promoted! \n <size=32>Congrulations, sir!</size>";
        }
        else if (StatTracker.instance.bossAngered[multiplayerManager.instance.currentActivePlayer] == 0 && performance > 4 && ((StatTracker.instance.totalProfessionalism[multiplayerManager.instance.currentActivePlayer] / maxDays) > 80))
        {
            endingScreen.sprite = Friends;
            StatsTitle.text = "You got a raise! \n <size=32>Your boss is not an easy man to please</size>";
        }
        else if (((StatTracker.instance.totalProfessionalism[multiplayerManager.instance.currentActivePlayer] / maxDays) < 70) && performance > 3)
        {
            endingScreen.sprite = Unproffessional;
            StatsTitle.text = "You kept your job \n <size=32>Somehow...</size>";
        }
        else if (StatTracker.instance.bossDeaths[multiplayerManager.instance.currentActivePlayer] > maxDays - 1 && performance > 3)
        {
            endingScreen.sprite = BossDeathScreen;
            StatsTitle.text = "You kept your job \n <size=32>But your boss did not...</size>";
        }
        else if (performance > 3)
        {
            endingScreen.sprite = Average;
            StatsTitle.text = "You kept your job \n <size=32>You were unremarkably average</size>";
        }
        else
        {
            endingScreen.sprite = FiredScreen;
            StatsTitle.color = Color.red;
            StatsTitle.text = "You were fired! \n <size=32>Better luck next time</size>";
        }
        
        yield return new WaitForSeconds(1);

        //Fade the black out
        while (background.color.a > 0)
        {
            Color col = background.color;
            col.a -= Time.deltaTime;
            background.color = col;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(2);
        SoundManager.instance.playSound(0, .95f);

        StatsBox.enabled = true;
        StatsTitle.enabled = true;

        yield return new WaitForSeconds(.8f);
        SoundManager.instance.playSound(0, .95f);

        StatsDeath.text = "You died a total of <color=red>"+ StatTracker.instance.totalDeaths[multiplayerManager.instance.currentActivePlayer] + "</color> times";
        StatsDeath.enabled = true;

        yield return new WaitForSeconds(.8f);
        SoundManager.instance.playSound(0, .95f);

        StatsBossDeath.text = "Your boss killed you <color=red>" + StatTracker.instance.bossDeaths[multiplayerManager.instance.currentActivePlayer] + "</color> times";
        StatsBossDeath.enabled = true;

        yield return new WaitForSeconds(.8f);
        SoundManager.instance.playSound(0, .95f);

        float correct = StatTracker.instance.safeEmailsCorrect[multiplayerManager.instance.currentActivePlayer] + StatTracker.instance.junkEmailsCorrect[multiplayerManager.instance.currentActivePlayer];
        float total = StatTracker.instance.junkEmailsWrong[multiplayerManager.instance.currentActivePlayer] + StatTracker.instance.safeEmailsWrong[multiplayerManager.instance.currentActivePlayer] + StatTracker.instance.safeEmailsCorrect[multiplayerManager.instance.currentActivePlayer] + StatTracker.instance.junkEmailsCorrect[multiplayerManager.instance.currentActivePlayer];

        if (correct > 0)
            StatsEmail.text = "You processed <color=red>" + total + "</color> emails and sorted <color=red>" + (int)(correct / total * 100) + "%</color> of them correctly";
        else
            StatsEmail.text = "You processed <color=red>" + total + "</color> emails and sorted <color=red>0%</color> of them correctly";
        StatsEmail.enabled = true;

        yield return new WaitForSeconds(.8f);
        SoundManager.instance.playSound(0, .95f);

        StatsProf.text = "Your overall professionalism is <color=red>" + ((StatTracker.instance.totalProfessionalism[multiplayerManager.instance.currentActivePlayer] / maxDays)) + "%</color>"; //REDO THIS M9
        StatsProf.enabled = true;

        yield return new WaitForSeconds(.8f);
        SoundManager.instance.playSound(0, .95f);

        StatsBossAnger.text = "You angered your boss <color=red>"+StatTracker.instance.bossAngered[multiplayerManager.instance.currentActivePlayer] + "</color> times";
        StatsBossAnger.enabled = true;

        yield return new WaitForSeconds(1);
        GameFinished = true;
        continueText.SetActive(true);
    }

    IEnumerator NextDayTransition() //Fades the screen to black, display the day # text and fades back in
    {
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);
        //Fade to black
        while (background.color.a < 1)
        {
            Color col = background.color;
            col.a += Time.deltaTime;
            background.color = col;
            yield return new WaitForEndOfFrame();
        }
        
        DayText.text = (weekDays)StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer] + "\n <size=64>" + (StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer] + 4)+ "th May 1991</size> \n";

        emailTargetText.text = "Target: " + (4 + StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer]) +"x";

        SoundManager.instance.officeAmbience.DOFade(SoundManager.instance.volumeMultiplayer * 0.3f, 2);

        //Fade in day text
        #region fade text
        while (DayText.color.a < 1)
        {
            Color col = DayText.color;
            col.a += Time.deltaTime;
            DayText.color = col;
            yield return new WaitForEndOfFrame();
        }

        //I really need to stop copy and pasting code and make this a formalized function
        //Then again, fuck it.
        if (multiplayerManager.instance.numberOfPlayers > 1)
        {
            //Do an if based on who's turn it is
            if (multiplayerManager.instance.currentActivePlayer == 0)
            {
                while (Player1Text.color.a < 1)
                {
                    Color col = Player1Text.color;
                    col.a += Time.deltaTime;
                    Player1Text.color = col;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (Player2Text.color.a < 1)
                {
                    Color col = Player2Text.color;
                    col.a += Time.deltaTime;
                    Player2Text.color = col;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        while (TimeText.color.a < 1)
        {
            Color col = TimeText.color;
            col.a += Time.deltaTime;
            TimeText.color = col;
            yield return new WaitForEndOfFrame();
        }
        #endregion

        yield return new WaitForSeconds(1);

        Debug.Log("the bit that kills computers");
        if(!introMonitor.instance.gameIntro)
        {
            introMonitor.instance.gameObject.SetActive(true);
            introMonitor.instance.InitialiseMonitor();
        }
        foreach (WorldObject _wo in FindObjectsOfType<WorldObject>())
        {
            _wo.Reset();
        }
        Debug.Log("stuff");
        TileManager.instance.UpgradeSpawners(0.855f, 0.855f, 1.0875f); //Do I spy magic numbers? Shaun, you naughty boy.
        Boss.instance.ModifyBoss(1.0875f); //holy shit thats a specific number
        BossFace.instance.Reset();
        Debug.Log("end stuff");
        currentTime = 0;
        progressUI.SetActive(false);
        dayFinishedText.Stop();
        progressUI.GetComponent<Image>().enabled = false;
        dayCompletedHeader.text = string.Empty;
        dayCompletedHeader.enabled = false;
        filedText.enabled = false;
        for (int i = 0; i < todaysEmails.Count; i++)
        {
            Destroy(emailObjects[i]);
        }
        todaysEmails.Clear();
        emailObjects.Clear();

        performanceText.enabled = false;
        performanceResult.text = string.Empty;
        finishedDisplay = false;

        //Also reset the clock
        smallHand.rectTransform.rotation = Quaternion.Euler(0, 0, 90);
        bigHand.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
        
        WorkerManager.instance.SetupDefaultPositions();
        //Fade out both
        while (DayText.color.a > 0)
        {
            Color colA = DayText.color;
            colA.a -= Time.deltaTime;
            DayText.color = colA;
            TimeText.color = colA;
            Color colB = background.color;
            colB.a -= Time.deltaTime;
            background.color = colB;

            //If this is a 2 player thing, fade out player text too
            if (multiplayerManager.instance.numberOfPlayers > 1)
            {
                while (Player1Text.color.a < 1)
                {
                    Color col = Player1Text.color;
                    col.a -= Time.deltaTime;
                    Player1Text.color = col;
                }
                while (Player2Text.color.a < 1)
                {
                    Color col = Player2Text.color;
                    col.a -= Time.deltaTime;
                    Player2Text.color = col;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        NewDay();
    }

    IEnumerator endOfDay()
    {
        SoundManager.instance.officeAmbience.DOFade(0, 5);
        SoundManager.instance.music.DOFade(0, 5);

        //play some sort of sound
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);
        progressUI.SetActive(true);
        dayFinishedText.Play("dayfinished");

        yield return new WaitForSeconds(2.5f);
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER); //All these state changes, you made me do this Craig, coroutine problems
        progressUI.GetComponent<Image>().enabled = true;
        
        StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer]++;

        dayCompletedHeader.text = "Day "+StatTracker.instance.numOfDaysCompleted + " completed";
        dayCompletedHeader.enabled = true;
        yield return new WaitForSeconds(1.5f);
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);

        filedText.enabled = true;
        yield return new WaitForSeconds(1f);
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);

        int top = 0, bot = 0;
        float correct = 0;

        #region make email icons
        emailObjects = new List<GameObject>();
        for (int i = 0; i < todaysEmails.Count; i++)
        {
            //For each email, do a prefab.
            GameObject email = Instantiate(emailIconPrefab) as GameObject;
            emailObjects.Add(email);

            if (todaysEmails[i].junk)
                email.GetComponent<Image>().sprite = junkMailSprite;

            email.GetComponent<RectTransform>().SetParent(filedText.transform.parent);
            if (todaysEmails[i].correctAnswer)
            {
                SoundManager.instance.playSound(0, .95f);
                email.GetComponent<RectTransform>().localPosition = new Vector2(-25 + (top * 35), 75);
                if (email.GetComponent<RectTransform>().localPosition.x > 650)
                    email.GetComponent<RectTransform>().localPosition = new Vector2(-25 + (top * 35) - 675, 50);

                top++;
                correct++;
            }
            else
            {
                //if xpos is greater than 625, go to a new line
                //New line should be y = 25
                SoundManager.instance.playSound(0, .25f);
                email.GetComponent<RectTransform>().localPosition = new Vector2(-25 + (bot * 35), -75);
                if (email.GetComponent<RectTransform>().localPosition.x > 650)
                    email.GetComponent<RectTransform>().localPosition = new Vector2(-25 + (bot * 35) - 675, -100);

                bot++;
            }
            yield return new WaitForSeconds(.35f);
            GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);
        }
        #endregion

        performanceText.enabled = true;
        yield return new WaitForSeconds(2f);
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);

        #region performance handling
        int livesToAdd = 0;
        //8 ranks
        int performanceRank;
        if (todaysEmails.Count == 0)
        {
            performanceRank = 0;
            performanceResult.text = "USELESS";
            performanceResult.color = Color.red;
        }
        else
        {
            if (correct / todaysEmails.Count < .15f)
                performanceRank = 1;
            else if (correct / todaysEmails.Count < .30f)
                performanceRank = 2;
            else if (correct / todaysEmails.Count < .45f)
                performanceRank = 3;
            else if (correct / todaysEmails.Count < .60f)
                performanceRank = 4;
            else if (correct / todaysEmails.Count < .75f)
                performanceRank = 5;
            else if (correct / todaysEmails.Count < .90f)
                performanceRank = 6;
            else
                performanceRank = 7;

            int min = 4 + StatTracker.instance.numOfDaysCompleted[multiplayerManager.instance.currentActivePlayer] - 1;

            if (todaysEmails.Count < min)
            {
                performanceRank -= min - (todaysEmails.Count);
            }

            if (performanceRank < 0)
                performanceRank = 0;

            if (performanceRank > 0)
                livesToAdd = Mathf.RoundToInt((performanceRank / 3.9f));

            StatTracker.instance.addDayPerformance(performanceRank);

            switch (performanceRank)
            {
                case 0:
                    performanceResult.text = "USELESS";
                    break;
                case 1:
                    performanceResult.text = "HOW DO YOU STILL HAVE A JOB??";
                    break;
                case 2:
                    performanceResult.text = "SHAME";
                    break;
                case 3:
                    performanceResult.text = "NEEDS IMPROVEMENT";
                    break;
                case 4:
                    performanceResult.text = "MEDIOCRE";
                    break;
                case 5:
                    performanceResult.text = "SUPRISINGLY GOOD";
                    break;
                case 6:
                    performanceResult.text = "RIBBITING!";
                    break;
                case 7:
                    performanceResult.text = "EMPLOYEE OF THE DAY!";
                    break;
            }

            performanceResult.color = Color.Lerp(Color.red, Color.green, (performanceRank +1.0f) / 8.0f);
        }
        SoundManager.instance.playSound(stampSound);
        performanceResult.enabled = true;
        #endregion

        yield return new WaitForSeconds(1.5f);
        GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);

        #region regen lives
        if (Player.instance.strikes[multiplayerManager.instance.currentActivePlayer] < 4)
        {
            for (int i = 0; i < livesToAdd; i++)
            {
                Player.instance.strikes[multiplayerManager.instance.currentActivePlayer]++;
                if (Player.instance.strikes[multiplayerManager.instance.currentActivePlayer] > 3)
                {
                    Player.instance.strikes[multiplayerManager.instance.currentActivePlayer] = 3;
                    break;
                }
                StatTracker.instance.changeLifeCount(Player.instance.strikes[multiplayerManager.instance.currentActivePlayer] - 1,true);
                yield return new WaitForSeconds(.5f);
                SoundManager.instance.playSound(0, 1.2f);
                yield return new WaitForSeconds(.25f);
                GameStateManager.instance.ChangeState(GameStates.STATE_DAYOVER);
            }
        }
        #endregion

        finishedDisplay = true;

        yield return new WaitForSeconds(0.5f);
        continueTexteEOD.SetActive(true);
    }
}
