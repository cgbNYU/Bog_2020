using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class TutorialPopup : MonoBehaviour
{
    public Sprite[] tutorialPages;
    public Sprite readyPage;
    private Image displayedPage;
    private int _pageIndex = 0;
    
    public int playerID;
    private Player _rewiredPlayer;
    private bool _buttonPressed;

    public bool PlayerReady;
    
    void Start()
    {
        //Initialize rewired player
        _rewiredPlayer = ReInput.players.GetPlayer(playerID);
        //Get ref to tutorial page
        displayedPage = GetComponentInChildren<Image>();
    }

    void Update()
    {
        //Move left through the tutorial
        if (_rewiredPlayer.GetAxis("L_Horz") < 0 && !_buttonPressed)
        {
            PlayerReady = false;
            _buttonPressed = true;
            _pageIndex--;
            CheckIndexWithinBounds();
            DisplayTutorialPage(_pageIndex);
        }
        
        //Move right through the tutorial
        if (_rewiredPlayer.GetAxis("L_Horz") > 0 && !_buttonPressed)
        {
            PlayerReady = false;
            _buttonPressed = true;
            _pageIndex++; 
            CheckIndexWithinBounds();
            DisplayTutorialPage(_pageIndex);
        }
        
        //Reset button pressed when the stick returns to neutral
        if (_rewiredPlayer.GetAxis("L_Horz") == 0) _buttonPressed = false;
        
        //Press down to confirm ready for match
        if (_rewiredPlayer.GetAxis("L_Vert") < 0)
        {
            PlayerReady = true;
            DisplayReadyPage();
        }
    }

    private void CheckIndexWithinBounds()
    {
        if (_pageIndex < 0) _pageIndex = tutorialPages.Length - 1;
        if (_pageIndex > tutorialPages.Length - 1) _pageIndex = 0;
    }

    private void DisplayTutorialPage(int pageIndex)
    {
        displayedPage.sprite = tutorialPages[pageIndex];
    }

    private void DisplayReadyPage()
    {
        displayedPage.sprite = readyPage;
    }
}
