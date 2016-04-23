using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

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

public class GameController : MonoBehaviour
{
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
        if (!board.HasLegalMoves())
            GameOver();
        else
        {
            board.UpdatePieces();
            UpdateSquareDisplays();
            UpdateScore();
            if (ply == human) yield return StartCoroutine(HumanTurn());
            else yield return StartCoroutine(AITurn());
        }
    }

    IEnumerator HumanTurn()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int clickedSquare = GetClickedSquare();
            if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
            {
                MakeMove(board, ply, board.squares[clickedSquare]);
                SetPly(ai);
            }
        }
        while (ply == human) yield return null;
    }

    IEnumerator AITurn()
    {
        //Player[] boardMap = GetBoardPlayerMap(board.squares);
        //MinimaxPair<Player[], double> moveToMake = Minimax(boardMap, difficulty, true);
        //Player[] nextBoard = moveToMake.bestMove;

        //int movePos = GetNextMove(board.squares, nextBoard);
        //MakeMove(board, ply, board.squares[movePos]);
        //SetPly(human);

        //while (ply == ai) yield return null; // TODO

        if (Input.GetMouseButtonUp(0))
        {
            int clickedSquare = GetClickedSquare();
            if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
            {
                MakeMove(board, ply, board.squares[clickedSquare]);
                SetPly(human);
            }
        }
        while (ply == ai) yield return null;
    }

    public void UseMenu()
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

    void MakeMove(Board board, Player ply, Square square)
    {
        if (square.isLegalMove)
        {
            board.CaptureTile(ply, square);
            board.FlankPieces(ply, square);
        }
    }

    void SetPly(Player player)
    {
        ply = player;
        board.UpdateLegalMoves(ply);
    }

    void UpdateScore()
    {
        blackScore = 0;
        whiteScore = 0;
        foreach (Player player in board.playerMap)
        {
            if (player == Player.Black)
                blackScore++;
            else if (player == Player.White)
                whiteScore++;
        }

        blackScoreText.text = "Black: " + blackScore;
        whiteScoreText.text = "White: " + whiteScore;
    }

    //MinimaxPair<Player[], double> Minimax(Player[] boardMap, int depth, bool maximizingPlayer)
    //{
    //    List<Square> legalMoves = GetLegalMoves(boardMap);
    //    if (legalMoves.Count == 0 || depth == 0)
    //        return new MinimaxPair<Player[], double>(boardMap, Evaluate(squares));

    //    MinimaxPair<Player[], double> bestBoard = new MinimaxPair<Player[], double>();

    //    if (maximizingPlayer)
    //    {
    //        bestBoard.bestScore = Mathf.NegativeInfinity;
    //        foreach (Square legalMove in legalMoves)
    //        {
    //            Board newBoard = CopyBoard();
    //            MakeMove(newBoard, ply, legalMove, false);
    //            Player[] newBoardMap = GetBoardPlayerMap(newBoard.squares);
    //            MinimaxPair<Player[], double> nextMove = Minimax(newBoardMap, newBoard.squares, depth - 1, false);
    //            if (nextMove.bestScore > bestBoard.bestScore)
    //            {
    //                bestBoard.bestMove = nextMove.bestMove;
    //                bestBoard.bestScore = nextMove.bestScore;
    //            }
    //        }
    //        return bestBoard;
    //    }
    //    else
    //    {
    //        bestBoard.bestScore = Mathf.Infinity;
    //        foreach (Square legalMove in legalMoves)
    //        {
    //            Board newBoard = CopyBoard();
    //            MakeMove(newBoard, ply, legalMove, false);
    //            Player[] newBoardMap = GetBoardPlayerMap(newBoard.squares);
    //            MinimaxPair<Player[], double> nextMove = Minimax(newBoardMap, newBoard.squares, depth - 1, true);
    //            if (nextMove.bestScore < bestBoard.bestScore)
    //            {
    //                bestBoard.bestMove = nextMove.bestMove;
    //                bestBoard.bestScore = nextMove.bestScore;
    //            }
    //        }
    //        return bestBoard;
    //    }
    //}

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

    private int GetClickedSquare()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Square s = hit.transform.gameObject.GetComponent<Square>();
            if (s != null) return s.position;
            else return -1;
        }
        else return -1;
    }

    private double Evaluate(Player[] boardMap)
    {
        double score = 0.0f;
        score += GetCornerScore(boardMap);
        score += GetEdgeScore(boardMap);
        score += GetPieceCountScore(boardMap);

        return score;
    }

    private double GetCornerScore(Player[] boardMap)
    {
        int[] corners = new int[4] { 0, 7, 56, 63 };
        double score = 0.0f;
        foreach (int corner in corners)
        {
            if (boardMap[corner] == ai)
                score++;
            else if (boardMap[corner] == human)
                score--;
        }

        return score * 20;
    }

    private double GetEdgeScore(Player[] boardMap)
    {
        double score = 0.0f;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0 || i % 8 == 7 || i / 8 == 0 || i / 8 == 7)
            {
                if (boardMap[i] == ai)
                    score++;
                else if (boardMap[i] == human)
                    score--;
            }
        }

        return score * 10;
    }

    private double GetPieceCountScore(Player[] boardMap)
    {
        double score = 0.0f;

        for (int i = 0; i < 64; i++)
        {
            if (boardMap[i] == ai)
                score++;
            else if (boardMap[i] == human)
                score--;
        }

        return score;
    }

    private int GetNextMove(Player[] currentBoardMap, Player[] nextBoardMap)
    {
        for (int i = 0; i < 64; i++)
            if (currentBoardMap[i] == Player.Nobody && nextBoardMap[i] == ai)
                return i;

        Debug.Log("Error in Minimax. Could not find next move.");
        return -1;
    }
}
