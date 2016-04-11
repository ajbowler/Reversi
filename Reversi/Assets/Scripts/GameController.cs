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
    private Player turn;

    void Start()
    {
        turn = Player.Black;
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        if (!board.UpdateLegalMoves(turn))
            Debug.Log("Game Over");
        else
        {
            board.UpdateSquares();
            if (turn == human)
                while (turn == human) yield return null;
            else yield return StartCoroutine(AITurn());
        }
    }

    IEnumerator AITurn()
    {
        while (turn == ai) yield return null; // TODO
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
}
