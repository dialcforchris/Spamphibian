﻿using UnityEngine;
//using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class mailOpener : MonoBehaviour 
{
    public static mailOpener instance;
    
    public SpriteRenderer emailContent;
    public Animator monitorAnimator,miniEmailAnimator;
    public Camera monitorCamera,mainCam;
    public SpriteRenderer mainCamTransition, monCamTransition,tickCross;
    public Sprite Tick, Cross;
    public Texture[] gradients;
    public AudioClip[] keypressSounds;
    public AudioSource computerSounds;
    public Slider angerMeter;
    public bool activeCountdown;
    Vector2 angerMeterOrigin;
    public ParticleSystem angryParticles;
    public List<mailColection> messages; //All the possible emails the player might have to deal with

    public float multi;
    [SerializeField]
    private mailColection selectedList;
    [SerializeField]
    private mail currentMail;
    [SerializeField]
    AudioClip good, bad;

    [System.Serializable]
    public class mailColection
    {
        public string name;
        public List<mail> messages;
        public bool randomSelection;
        public int[] index;
        public int score;
    }

    //An email object, might need variables for score and such in the future
    [System.Serializable]
    public struct mail
    {
        public Sprite image;
        public bool isJunk,repeating;
    }
    void Awake()
    {
        angerMeterOrigin = angerMeter.GetComponent<RectTransform>().anchoredPosition;
        instance = this;
        //Always remember to seed your random :)
        Random.seed = System.DateTime.Now.Millisecond;
        nonFrogMail = new int[2] { 0, 0 };
        frogStory = new bool[2] { true, true };
    }

    public void enterView()
    {
        //should probably change game manager state to email viewing
        GameStateManager.instance.ChangeState(GameStates.STATE_EMAIL);
        emailPos = 0;

        //Change cameras over
        StartCoroutine(camTransition(true));
    }

    IEnumerator camTransition(bool InOut) //True for entering email mode, false for exiting it
    {
        if (InOut)
        {
            angryParticles.Stop();
            pop = false;
            angerMeter.value = 0;
        }
        SoundManager.instance.officeAmbience.DOFade((InOut) ? SoundManager.instance.volumeMultiplayer * 0.3f : SoundManager.instance.volumeMultiplayer, 2);
        computerSounds.DOFade((InOut) ? SoundManager.instance.volumeMultiplayer : 0, 2);
        SoundManager.instance.managedAudioSources[0].volumeLimit = (InOut) ? 1 : 0;
        SoundManager.instance.music.DOFade((InOut) ? SoundManager.instance.musicVolume*0.15f : SoundManager.instance.musicVolume, 2);
        SoundManager.instance.bossMusic.DOFade((InOut) ? SoundManager.instance.musicVolume * 0.15f : SoundManager.instance.musicVolume, 2);

        Random.seed = System.DateTime.Now.Millisecond;
        mainCamTransition.material.SetTexture("_SliceGuide", gradients[Random.Range(0, gradients.Length)]);
        monCamTransition.material.SetTexture("_SliceGuide", gradients[Random.Range(0, gradients.Length)]);

        float lerpy = 0;

        #region camera effect transition, IN
        //TEMPOARY SHIT
        lerpy = 1;
        while (lerpy > 0)
        {
            lerpy -= Time.deltaTime * 1.5f;

            if (!InOut)
                monCamTransition.material.SetFloat("_SliceAmount", lerpy);
            else
            {
                mainCamTransition.material.SetFloat("_SliceAmount", lerpy);
            }
            yield return new WaitForEndOfFrame();
        }
        #endregion

        //Zoom in
        if (InOut)
        {
        #region zoomin in
            while (lerpy < 1)
            {
                lerpy += Time.deltaTime*2.5f;
                if (lerpy > 1)
                    lerpy = 1;

                //Black bars yo
                CameraZoom.instance.overlayBot.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(960, -410), new Vector2(960, -540), 1 - lerpy);
                CameraZoom.instance.overlayTop.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(960, 540), new Vector2(960, 670), 1 - lerpy);
                //yield return new WaitForEndOfFrame();
            }
            lerpy = 0;
            while (Camera.main.orthographicSize > 2)
            {
                lerpy += Time.deltaTime*1.25f;
                if (lerpy > 1)
                    lerpy = 1;
                
                Camera.main.transform.position = Vector3.Lerp(new Vector3(0, 9.5f, -10), Player.instance.transform.position, lerpy);
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
                
                Camera.main.orthographicSize = Mathf.Lerp(9, 2, lerpy);
                mainCamTransition.transform.localScale = Vector3.Lerp(new Vector3(32, 18, 1), new Vector3(7.15f, 4, 1), lerpy);
            }
        #endregion
        }

        #region change over camera
        angryParticles.Stop();
        yield return new WaitForSeconds(0.1f);
        mainCam.enabled = !InOut;
        monitorCamera.enabled = InOut;
        #endregion

        #region camera effect transition, OUT
        lerpy = 0;
        while (lerpy < 1)
        {
            lerpy += Time.deltaTime*1.5f;

            if (InOut)
                monCamTransition.material.SetFloat("_SliceAmount", lerpy);
            else
            {
                mainCamTransition.material.SetFloat("_SliceAmount", lerpy);
            }

            yield return new WaitForEndOfFrame();
        }
        #endregion

        if (InOut)
        {
            activeCountdown = true;
            //Intro animation
            monitorAnimator.Play("mail_intro");
            pickEmail();

            StopCoroutine("zoomInOut");
            StartCoroutine(zoomInOut(7.5f));
        }
        else
        {
            angryParticles.Stop();
            angryParticles.Clear();    
            angerMeter.value = 0;
            while (Camera.main.orthographicSize < 9)
            {
                lerpy -= Time.deltaTime*1.5f;
                if (lerpy < 0)
                    lerpy = 0;
                Camera.main.transform.position = Vector3.Lerp(new Vector3(0, 9.5f, -10), Player.instance.transform.position, lerpy);
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
                Camera.main.orthographicSize = Mathf.Lerp(9, 2, lerpy);
                mainCamTransition.transform.localScale = Vector3.Lerp(new Vector3(32, 18, 1), new Vector3(7.15f, 4, 1), lerpy);

                yield return new WaitForEndOfFrame();
            }
            
            while (lerpy < 1)
            {
                lerpy += Time.deltaTime*2.5f;
                if (lerpy > 1)
                    lerpy = 1;

                //Black bars yo
                CameraZoom.instance.overlayBot.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(960, -410), new Vector2(960, -540), lerpy);
                CameraZoom.instance.overlayTop.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(960, 540), new Vector2(960, 670),lerpy);
                yield return new WaitForEndOfFrame();
            }
            GameStateManager.instance.ChangeState(GameStates.STATE_GAMEPLAY);
        }
    }

    int[] nonFrogMail;
    bool[] frogStory;

    void pickEmail()
    {
        #region Select a list   
        if (nonFrogMail[multiplayerManager.instance.currentActivePlayer] > 5 && frogStory[multiplayerManager.instance.currentActivePlayer])
        {
            nonFrogMail[multiplayerManager.instance.currentActivePlayer] = 0;
            selectedList = messages[0];
        }
        else
        {
            //Pick a random list of emails to display messages from
            selectedList = messages[Random.Range(1, messages.Count)];
            nonFrogMail[multiplayerManager.instance.currentActivePlayer]++;
        }
        #endregion

        if (selectedList.randomSelection)
        {
            //Pick a random message form the selected list
            currentMail = selectedList.messages[Random.Range(0, selectedList.messages.Count)];
            //Remove this email from the list so we can't get it twice
            if (!currentMail.repeating)
                selectedList.messages.Remove(currentMail);

            if (selectedList.messages.Count == 0)
            {
                messages.Remove(selectedList);
            }
        }
        else //Only list of random messages is frog mail
        {
            currentMail = selectedList.messages[selectedList.index[multiplayerManager.instance.currentActivePlayer]];
            selectedList.index[multiplayerManager.instance.currentActivePlayer]++;

            //When we're done with the frog mail, for a player, don't remove the messages from the list, just make them inaccessable for that player.
            if (selectedList.index[multiplayerManager.instance.currentActivePlayer] > selectedList.messages.Count-1)
            {
                if (selectedList.name == "Frog Mail") 
                {
                    frogStory[multiplayerManager.instance.currentActivePlayer] = false;
                }
                else if (selectedList.name == "TypeFighter Cheats")
                {
                    messages.Remove(selectedList);
                }
                else if (selectedList.name == "Drunk mail")
                {
                    messages.Remove(selectedList);
                }
            }
        }

        emailContent.sprite = currentMail.image;
    }

    IEnumerator zoomInOut(float newSize) //Used for slight zoom in monitor view
    {
        yield return new WaitForSeconds(1);
        while (Mathf.Abs(monitorCamera.orthographicSize - newSize) > 0.1f)
        {
            monitorCamera.orthographicSize = Mathf.Lerp(monitorCamera.orthographicSize, newSize, Time.deltaTime*2);
            if (GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY)
            {
                monitorCamera.orthographicSize = 10.9f;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void exitView()
    {
        //Change cameras over
        StartCoroutine(camTransition(false));
    }

    int emailPos = 0;
    public void moveEmail(int dir)
    {
        if (emailPos == 0) //Email is in the middle of the screen
        {
            if (dir > 0)
                monitorAnimator.Play("mail_right");
            else
                monitorAnimator.Play("mail_left");

            emailPos += dir;

            if (!soundPlaying)
            {
                SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                soundPlaying = true;
                Invoke("allowSounds", .5f);
            }
        }
        else if (emailPos > 0) //Email currently sits at the right of the screen
        {
            if (dir < 0)//Move left
            {
                monitorAnimator.Play("mail_right_reverse");
                emailPos = 0;//Email is back in the middle now
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
            }
        }
        else //Email must be at the left hand side of the monitor
        {
            if (dir > 0)//Move right
            {
                monitorAnimator.Play("mail_left_reverse");
                emailPos = 0;//Email is back in the middle now
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
            }
        }
    }

    bool pop;
    void Update()
    {
        //Shake the bar more as the value gets higher
        float shift = ((angerMeter.value* angerMeter.value* angerMeter.value) / angerMeter.maxValue)*multi;
        angerMeter.GetComponent<RectTransform>().anchoredPosition = angerMeterOrigin + new Vector2(Mathf.Sin(Random.value) * shift, Mathf.Sin(Random.value) * shift);

        //If the value is REALLY high, makw the bar explode with particles
        if (angerMeter.value ==angerMeter.maxValue && !pop)
        {
            StartCoroutine(AngerMeterPop());
        }
        InputRelated();
    }

    IEnumerator AngerMeterPop()
    {
        pop = true;
        while (multi < 0.1f)
        {
            multi += Time.deltaTime * 0.1f;
            yield return new WaitForEndOfFrame();
        }
        angryParticles.Emit(150);
        angryParticles.Play();
        while (multi >0 )
        {
            multi -= Time.deltaTime*0.2f;
            yield return new WaitForEndOfFrame();
        }
    }

    void allowSounds()
    {
        soundPlaying = false;
    }

    bool soundPlaying;

    void InputRelated()
    {
        //Only allow the player to move the email if it's not moving
        if (monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_middle")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_left")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_right"))
        {

            if (Input.GetAxis("HorizontalStick" + multiplayerManager.instance.currentActivePlayer.ToString()) != 0)
            {
                moveEmail(Input.GetAxis("HorizontalStick" + multiplayerManager.instance.currentActivePlayer.ToString()) > 0 ? 1 : -1);
            }

            if (Input.GetButtonDown("Fire" + multiplayerManager.instance.currentActivePlayer.ToString()) && emailPos != 0)
            {
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length-1)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
                #region if junk
                if (emailPos > 0) //If email is in the JUNK zone
                {
                    activeCountdown = false;
                    BossFace.instance.addEmailAngerXP();

                    //Do animation for email being destroyed
                    monitorAnimator.Play("mail_junk");

                    if (selectedList.name == "Drunk mail")
                    {
                        messages.Remove(selectedList);
                    }
                    if (selectedList == messages[0])
                        frogStory[multiplayerManager.instance.currentActivePlayer] = false;

                    #region if it's junk
                    if (currentMail.isJunk)
                    {
                        tickCross.sprite = Tick;
                        //Junk email put in junk pile, good job
                        //+ points
                        StatTracker.instance.scoreToAdd[multiplayerManager.instance.currentActivePlayer] += selectedList.score;
                        StatTracker.instance.junkEmailsCorrect[multiplayerManager.instance.currentActivePlayer]++;
                        BossFace.instance.CheckEmails(true);
                        dayTimer.completedEmail newMail;
                        newMail.junk = true;
                        newMail.correctAnswer = true;
                        dayTimer.instance.todaysEmails.Add(newMail);

                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));
                        Invoke("exitView", 2.5f);
                        SoundManager.instance.playSound(good);
                    }
                    #endregion
                    #region if not...
                    else
                    {
                        tickCross.sprite = Cross;
                        //You put a safe email in the junk pile
                        //oooooo
                        StatTracker.instance.scoreToAdd[multiplayerManager.instance.currentActivePlayer] -= (int)(.8f * selectedList.score);
                        StatTracker.instance.safeEmailsWrong[multiplayerManager.instance.currentActivePlayer]++;
                        BossFace.instance.CheckEmails(false);
                        dayTimer.completedEmail newMail;
                        newMail.junk = false;
                        newMail.correctAnswer = false;
                        dayTimer.instance.todaysEmails.Add(newMail);

                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                        SoundManager.instance.playSound(bad);
                    }
                    #endregion
                }
                #endregion
                #region if safe
                else if (emailPos < 0) //If email is in the SAFE zone
                {
                    activeCountdown = false;
                    BossFace.instance.addEmailAngerXP();
                    //Do animation for email being marked as safe
                    monitorAnimator.Play("mail_safe");
                    miniEmailAnimator.Play("email_leave 0");

                    if (currentMail.isJunk)
                    {
                        tickCross.sprite = Cross;
                        //You put junk in the safe pile
                        //- points
                        StatTracker.instance.scoreToAdd[multiplayerManager.instance.currentActivePlayer] -= (int)(.8f * selectedList.score);
                        StatTracker.instance.junkEmailsWrong[multiplayerManager.instance.currentActivePlayer]++;
                        BossFace.instance.CheckEmails(false);
                        dayTimer.completedEmail newMail;
                        newMail.junk = true;
                        newMail.correctAnswer = false;
                        dayTimer.instance.todaysEmails.Add(newMail);

                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                        SoundManager.instance.playSound(bad);
                    }
                    else
                    {
                        tickCross.sprite = Tick;
                        //Safe mail was marked as safe, woopee
                        //+ points
                        StatTracker.instance.scoreToAdd[multiplayerManager.instance.currentActivePlayer] += selectedList.score;
                        StatTracker.instance.safeEmailsCorrect[multiplayerManager.instance.currentActivePlayer]++;
                        BossFace.instance.CheckEmails(true);
                        dayTimer.completedEmail newMail;
                        newMail.junk = false;
                        newMail.correctAnswer = true;
                        dayTimer.instance.todaysEmails.Add(newMail);

                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                        SoundManager.instance.playSound(good);
                    }
                }
                #endregion
            }
        }
    }
}
