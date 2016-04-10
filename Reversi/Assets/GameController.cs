using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Player
{
    Black,
    White,
    Nobody
};

public class GameController : MonoBehaviour {
    public Board board;
    public GameObject mainMenu;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public EventSystem eventSystem;
    public Player human;
    public Player ai;

    private int difficulty;
    private List<Square> squares;

    void Start()
    {
        // TODO
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else PlayGame();
    }

    void PlayGame()
    {

    }

    void UseMenu()
    {
        GameObject currentSelection = eventSystem.currentSelectedGameObject;
        if (currentSelection == GameObject.Find("Play Button"))
        {
            mainMenu.SetActive(false);
            difficulty = difficultySelector.value + 1;
            if (colorSelector.value == 0)
            {
                human = Player.Black;
                ai = Player.White;
            }
            else
            {
                human = Player.White;
                ai = Player.Black;
            }
            board.SetInitialBoard();
        }
    }

    public Square CreateSquare(int index)
    {
        Square s = this.gameObject.AddComponent<Square>();
        s.index = index;
        return s;
    }
}
