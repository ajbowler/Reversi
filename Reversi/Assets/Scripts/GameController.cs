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
    private HashSet<int> flippedPieces;

    void Start()
    {
        turn = Player.Black;
        flippedPieces = new HashSet<int>();
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        if (board.legalMoves.Count == 0)
            Debug.Log("Game Over");
        else
        {
            UpdateSquareDisplays();
            if (turn == human)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    int clickedSquare = GetClickedSquare();
                    if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
                    {
                        MakeMove(clickedSquare);
                        // TODO make move
                    }
                }
                while (turn == human) yield return null;
            }

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
        board.gameObject.SetActive(false);
        if (Input.GetKeyUp(KeyCode.Return))
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
            SetTurn(Player.Black);
        }
    }

    void MakeMove(int position)
    {
        if (board.squares[position].isLegalMove)
        {
            Debug.Log("Legal"); // TODO
        }
        if (turn == human) SetTurn(ai);
        else SetTurn(human);
    }

    void SetTurn(Player player)
    {
        turn = player;
        UpdateLegalMoves(turn);
    }

    public void UpdateLegalMoves(Player currentPlayer)
    {
        if (board.legalMoves.Count > 0)
            ResetLegalMoves();
        List<int> indicesToFlip = new List<int>();
        foreach (Square s in board.squares)
        {
            if (s.player == currentPlayer)
            {
                CheckPath(indicesToFlip, currentPlayer, s.position, -1);
                CheckPath(indicesToFlip, currentPlayer, s.position, 1);
                CheckPath(indicesToFlip, currentPlayer, s.position, -8);
                CheckPath(indicesToFlip, currentPlayer, s.position, 8);
                CheckPath(indicesToFlip, currentPlayer, s.position, -9);
                CheckPath(indicesToFlip, currentPlayer, s.position, 9);
                CheckPath(indicesToFlip, currentPlayer, s.position, -7);
                CheckPath(indicesToFlip, currentPlayer, s.position, 7);
            }
        }
    }

    public void UpdateSquareDisplays()
    {
        foreach (Square s in board.squares)
        {
            if (s.isLegalMove)
                s.gameObject.GetComponent<MeshRenderer>().enabled = true;
            else
                s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void CheckPath(List<int> indicesToFlip, Player player, int startingPosition, int difference)
    {
        bool flankablesExist = false;

        for (int i = startingPosition + difference; i >= 0 && i <= 63; i += difference)
        {
            if (board.squares[i].player != player && board.squares[i].player != Player.Nobody)
            {
                flankablesExist = true;
                flippedPieces.Add(i);
            }
            else if (board.squares[i].player == Player.Nobody && flankablesExist)
            {
                flippedPieces.Add(i);
                board.legalMoves.Add(i);
                board.squares[i].isLegalMove = true;
                break;
            }
            else break; // we can't go this way
        }
    }

    private int GetClickedSquare()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Square s = hit.transform.gameObject.GetComponent<Square>();
            return s.position;
        }
        else return -1;
    }

    private void ResetLegalMoves()
    {
        foreach (Square s in board.squares)
        {
            if (s.isLegalMove)
                s.isLegalMove = false;
        }

        board.legalMoves.Clear();
    }
}
