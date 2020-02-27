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
    private TextMeshProUGUI _endGameTextBox;
    
    void Start()
    {
        //Singleton
        if (UM == null)
        {
            DontDestroyOnLoad(gameObject);
            UM = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        FindUIReferences();
        ClearAllUIElements();
        
    }
    
    //Searches for the references to the UI elements
    private void FindUIReferences()
    {
        _endGameTextBox = GameObject.Find("EndGameTextbox").GetComponent<TextMeshProUGUI>();
    }

    //Clears all the UI elements 
    private void ClearAllUIElements()
    {
        _endGameTextBox.text = "";
    }

    //Displays end game UI
    public void DisplayEndGameUI(int losingTeamId)
    {
        if (losingTeamId == 0) _endGameTextBox.text = "Blue team wins!";
        if (losingTeamId == 1) _endGameTextBox.text = "Red team wins!";
    }
}
