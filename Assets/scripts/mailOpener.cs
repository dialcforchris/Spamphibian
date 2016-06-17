﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class mailOpener : MonoBehaviour {

    public static mailOpener instance;

    //Need to be able to open and close emails at will
    //i.e. have them be seperate events

    public SpriteRenderer emailContent;
    public Animator monitorAnimator,miniEmailAnimator;
    public Camera monitorCamera,mainCam;

    public List<mail> messages; //All the possible emails the player might have to deal with
    mail currentMail;
    
    //An email object, might need variables for score and such in the future
    [System.Serializable]
    public struct mail
    {
        public Sprite image;
        public bool isJunk;
    }

    void Awake()
    {
        instance = this;
        //Always remember to seed your random :)
        Random.seed = System.DateTime.Now.Millisecond;

        //Purly for testing reasosns, remove at a later date
    }

    public void enterView()
    {
        //should probably change game manager state to email viewing
        if (GameStateManager.instance)
            GameStateManager.instance.ChangeState(GameStates.STATE_EMAIL);

        emailPos = 0;
        //Change cameras over
        mainCam.enabled = false;
        monitorCamera.enabled = true;

        //Intro animation
        monitorAnimator.Play("mail_intro");

        //Choose a random email to display
        int chosen = Random.Range(0, messages.Count);
        currentMail = messages[chosen];

        //Remove this email from the list so we can't get it twice
        messages.Remove(currentMail);
        emailContent.sprite = currentMail.image;
        StartCoroutine(zoomInOut(7.5f));
    }

    IEnumerator zoomInOut(float newSize)
    {
        yield return new WaitForSeconds(1);
        while (Mathf.Abs(monitorCamera.orthographicSize - newSize) > 0.1f)
        {
            monitorCamera.orthographicSize= Mathf.Lerp(monitorCamera.orthographicSize, newSize, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public void exitView()
    {
        GameStateManager.instance.ChangeState(GameStates.STATE_GAMEPLAY);

        //Change cameras over
        mainCam.enabled = true;
        monitorCamera.enabled = false;
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
        }
        else if (emailPos > 0) //Email currently sits at the right of the screen
        {
            if (dir < 0)//Move left
            {
                monitorAnimator.Play("mail_right_reverse");
                emailPos = 0;//Email is back in the middle now
            }
        }
        else //Email must be at the left hand side of the monitor
        {
            if (dir > 0)//Move right
            {
                monitorAnimator.Play("mail_left_reverse");
                emailPos = 0;//Email is back in the middle now
            }
        }
    }

    void Update()
    {
        //Only allow the player to move the email if it's not moving
        if (monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_middle")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_left")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_right"))
        {

            if (Input.GetAxis("Horizontal") != 0)
            {
                moveEmail(Input.GetAxis("Horizontal") > 0 ? 1 : -1);
            }
            
            if (Input.GetAxis("Fire1") > 0)
            {
                if (emailPos > 0) //If email is in the JUNK zone
                {
                    //Do animation for email being destroyed
                    monitorAnimator.Play("mail_junk");

                    if (currentMail.isJunk)
                    {
                        Debug.Log("Junk email put in junk pile, good job");
                        //Junk email put in junk pile, good job
                        //+ points
                        StatTracker.instance.scoreToAdd += 100;
                        StatTracker.instance.junkEmailsCorrect++;
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 4);
                    }
                    else
                    {
                        Debug.Log("You put a safe email in the junk pile, YOU WALLY");
                        //You put a safe email in the junk pile
                        //oooooo
                        StatTracker.instance.scoreToAdd -= 100;
                        StatTracker.instance.safeEmailsWrong++;
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 4);
                    }
                }
                else if (emailPos < 0) //If email is in the SAFE zone
                {
                    //Do animation for email being marked as safe
                    monitorAnimator.Play("mail_safe");
                    miniEmailAnimator.Play("email_leave 0");

                    if (currentMail.isJunk)
                    {
                        Debug.Log("You put junk in the safe pile");
                        //You put junk in the safe pile
                        //- points
                        StatTracker.instance.scoreToAdd -= 100;
                        StatTracker.instance.junkEmailsWrong++;
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                    }
                    else
                    {
                        Debug.Log("Safe mail was marked as safe, woopee");
                        //Safe mail was marked as safe, woopee
                        //+ points
                        StatTracker.instance.scoreToAdd += 100;
                        StatTracker.instance.safeEmailsCorrect++;
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                    }
                }
            }
        }
    }
}

[CustomEditor(typeof(mailOpener))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (EditorApplication.isPlaying)
        {
            //mailOpener myScript = (mailOpener)target;
            if (GUILayout.Button("Enter Monitor View"))
            {
                mailOpener.instance.enterView();
            }
        }
    }
}
