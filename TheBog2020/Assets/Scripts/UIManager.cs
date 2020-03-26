using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Receives data relevant to the UI and displays it
/// Lives remaining
/// Time?
/// Score?
/// </summary>
public class UIManager : MonoBehaviour
{
    //Singleton 
    public static UIManager UM;
    
    //References
    [SerializeField]private TMP_Text _fullScreenTextBox;
    private TMP_Text[] _eggsTextBox;
    
    //Tutorial images
    public Sprite[] tutorialPages;

    void Start()
    {
        //Singleton
        if (UM == null)
        {
            UM = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        //Init UI Manager
        InitializeUIManager();
    }

    //Finds all the UI object references. To be called from the GameManager at the start of the Match 
    //and after reloading the Game scene on a new match.
    public void InitializeUIManager()
    {
        FindUIReferences();
        ClearAllUIElements();
    }

    //Searches for the references to the UI elements
    private void FindUIReferences()
    {
        _fullScreenTextBox = GameObject.Find("EndGameTextbox").GetComponent<TMP_Text>();
        _eggsTextBox = new TMP_Text[2];
        _eggsTextBox[0]= GameObject.Find("RedEggsTextbox").GetComponent<TMP_Text>();
        _eggsTextBox[1]= GameObject.Find("BlueEggsTextbox").GetComponent<TMP_Text>();
    }

    //Clears all the UI elements 
    public void ClearAllUIElements()
    {
        _fullScreenTextBox.color = Color.white;
        _fullScreenTextBox.text = "";
        _eggsTextBox[0].text = "";
        _eggsTextBox[1].text = "";
    }

    //Displays end game UI
    public void DisplayEndGameUI(int losingTeamId)
    {
        if (losingTeamId == 0)
        {
            _fullScreenTextBox.text = "Blue team wins!\n\nPress the 'Backspace' key to restart.";
        }

        if (losingTeamId == 1)
        {
            _fullScreenTextBox.text = "Red team wins!\n\nPress the 'Backspace' key to restart.";
        }
    }

    public void DisplayStartGameUI()
    {
        _fullScreenTextBox.text = "Press any key to start.";
    }

    public void UpdateEggsRemainingUI(int teamID, int eggsRemaining)
    {
        _eggsTextBox[teamID].text = "Eggs left: " + eggsRemaining;
    }
}
