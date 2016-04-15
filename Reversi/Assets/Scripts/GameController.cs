﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum Player
{
    Black,
    White,
    Nobody
};

public class MinimaxPair<First, Second>
{
    public First bestMove { get; set; }
    public Second bestScore { get; set; }

    public MinimaxPair(First bestMove, Second bestScore)
    {
        this.bestMove = bestMove;
        this.bestScore = bestScore;
    }

    public MinimaxPair()
    {

    }
}

public class GameController : MonoBehaviour {
    public Board board;
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public Text blackScoreText;
    public Text whiteScoreText;
    public Text winnerText;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public Player human;
    public Player ai;

    private int difficulty;
    private int blackScore;
    private int whiteScore;
    private Player ply;

    void Start()
    {
        ply = Player.Black;
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        if (board.legalMoves.Count == 0)
            GameOver();
        else
        {
            UpdateSquareDisplays();
            UpdateScore();
            //if (ply == human)
            //{
            //    if (Input.GetMouseButtonUp(0))
            //    {
            //        int clickedSquare = GetClickedSquare();
            //        if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
            //            MakeMove(clickedSquare);
            //    }
            //    while (ply == human) yield return null;
            //}

            // TESTING PIECE PLACEMENT AND FLIPPING
            if (Input.GetMouseButtonUp(0))
            {
                int clickedSquare = GetClickedSquare();
                if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
                {
                    MakeMove(board, board.squares[clickedSquare]);
                    if (ply == human) SetPly(ai);
                    else SetPly(human);
                }
            }
            while (ply == human) yield return null;

            //else yield return StartCoroutine(AITurn());
        }
    }

    IEnumerator AITurn()
    {
        while (ply == ai) yield return null; // TODO
    }

    void UseMenu()
    {
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
            blackScoreText.gameObject.SetActive(true);
            whiteScoreText.gameObject.SetActive(true);
            SetPly(Player.Black);
        }
    }

    void GameOver()
    {
        board.gameObject.SetActive(false);
        board.Reset();

        if (blackScore > whiteScore)
            winnerText.text = "Black Wins!";
        else if (whiteScore > blackScore)
            winnerText.text = "White Wins!";
        else
            winnerText.text = "Tie!";

        gameOverMenu.gameObject.SetActive(true);
        if (Input.GetKeyUp(KeyCode.R))
        {
            mainMenu.gameObject.SetActive(true);
            gameOverMenu.gameObject.SetActive(false);
        }
    }

    void MakeMove(Board board, Square square)
    {
        if (square.isLegalMove)
        {
            board.CaptureTile(ply, square);
            FlankPieces(board, ply, square);
        }
    }

    void SetPly(Player player)
    {
        ply = player;
        UpdateLegalMoves(ply);
    }

    void UpdateScore()
    {
        blackScore = 0;
        whiteScore = 0;
        foreach (Square square in board.squares)
        {
            if (square.player == Player.Black)
                blackScore++;
            if (square.player == Player.White)
                whiteScore++;
        }

        blackScoreText.text = "Black: " + blackScore;
        whiteScoreText.text = "White: " + whiteScore;
    }

    MinimaxPair<Square[], double> Minimax(Square[] squares, Player currentPlayer, int depth, bool maximizingPlayer)
    {
        List<Square> legalMoves = GetLegalMoves(squares);
        if (legalMoves.Count == 0 || depth == 0)
            return new MinimaxPair<Square[], double>(Evaluate(squares, currentPlayer), 0);

        MinimaxPair<Square[], double> bestBoard = new MinimaxPair<Square[], double>();
        Square[] bestMove = new Square[64];
        double bestScore;

        if (maximizingPlayer)
        {
            bestScore = Mathf.NegativeInfinity;
            foreach (Square legalMove in legalMoves)
            {
                Board newBoard = CopyBoard();
                MakeMove(newBoard, legalMove);
                MinimaxPair<Square[], double> nextMove = Minimax(newBoard.squares, human, depth - 1, false);
                if (nextMove.bestScore > bestScore)
                {
                    bestMove = nextMove.bestMove;
                    bestScore = nextMove.bestScore;
                    bestBoard.bestMove = bestMove;
                    bestBoard.bestScore = bestScore;
                }
            }
            return bestBoard;
        }
        else
        {
            bestScore = Mathf.Infinity;
            foreach (Square legalMove in legalMoves)
            {
                Board newBoard = CopyBoard();
                MakeMove(newBoard, legalMove);
                MinimaxPair<Square[], double> nextMove = Minimax(newBoard.squares, ai, depth - 1, true);
                if (nextMove.bestScore < bestScore)
                {
                    bestMove = nextMove.bestMove;
                    bestScore = nextMove.bestScore;
                    bestBoard.bestMove = bestMove;
                    bestBoard.bestScore = bestScore;
                }
            }
            return bestBoard;
        }
    }

    public void FlankPieces(Board board, Player currentPlayer, Square square)
    {
        List<int> flankedPieces = new List<int>();
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -1));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 1));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -8));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 8));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -9));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 9));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -7));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 7));

        foreach (int piecePos in flankedPieces)
            board.FlipPiece(currentPlayer, board.squares[piecePos]);
    }

    public void UpdateLegalMoves(Player currentPlayer)
    {
        if (board.legalMoves.Count > 0)
            ResetLegalMoves();
        foreach (Square square in board.squares)
        {
            if (square.player == currentPlayer)
            {
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -1));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 1));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -8));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 8));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -9));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 9));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -7));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 7));
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

    private List<int> AddFlankedPieces(Player currentPlayer, Square square, int direction)
    {
        List<int> flankedPieces = new List<int>();

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (board.squares[i].player != currentPlayer && board.squares[i].player != Player.Nobody)
                flankedPieces.Add(i);
            else if (board.squares[i].player == currentPlayer)
                break; // we have flanked everything we can in this direction
            else if (board.squares[i].player == Player.Nobody || IsPastBoardEdge(square, board.squares[i], direction))
            {
                flankedPieces.Clear();
                break; // nothing here can be flanked
            }
        }

        return flankedPieces;
    }

    private Board CopyBoard()
    {
        Board newBoard = new Board();
        newBoard.gameController = this;
        newBoard.legalMoves = new List<Square>();
        newBoard.squares = new Square[64];
        newBoard.pieces = new Dictionary<int, Piece>();
        foreach (Square square in board.squares)
        {
            Square s = CopySquare(square);
            newBoard.squares[square.position] = s;
            if (s.isLegalMove)
                newBoard.legalMoves.Add(s);
            if (s.player != Player.Nobody)
                newBoard.CaptureTile(s.player, s);
        }
        return newBoard;
    }

    private List<Square> GetLegalMoves(Square[] squares)
    {
        List<Square> legalMoves = new List<Square>();
        foreach (Square square in squares)
            if (square.isLegalMove) legalMoves.Add(CopySquare(square));
        return legalMoves;
    }

    private Square GetLegalMoveOnPath(Player player, Square square, int direction)
    {
        bool flankablesExist = false;

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (IsPastBoardEdge(square, board.squares[i], direction))
                return null; // we can't go this way
            if (board.squares[i].player != player && board.squares[i].player != Player.Nobody)
                flankablesExist = true;
            else if (board.squares[i].player == Player.Nobody && flankablesExist)
                return board.squares[i];
            else return null; // we can't go this way
        }

        return null; // shouldn't be reached unless there is an indexing error
    }

    private Square CopySquare(Square s)
    {
        Square square = new Square();
        square.column = s.column;
        square.isLegalMove = s.isLegalMove;
        square.player = s.player;
        square.position = s.position;
        square.row = s.row;
        return square;
    }

    private bool IsPastBoardEdge(Square start, Square end, int direction)
    {
        if (direction > 0) return (start.row - end.row) > 0;
        else return (start.row - end.row) < 0;
    }

    private void AddLegalMove(Square square)
    {
        if (square != null)
        {
            board.legalMoves.Add(square);
            board.squares[square.position].isLegalMove = true;
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

    private Square[] Evaluate(Square[] board, Player currentPlayer)
    {
        return null; // TODO
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
