using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

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

public class GameController : MonoBehaviour
{
    public Board board;
    public Square[] squares;
    public Dictionary<int, Piece> pieces;
    public Piece piecePrefab;
    public Square squarePrefab;
    public Canvas mainMenuCanvas;
    public Canvas gameOverCanvas;
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public Text blackScoreText;
    public Text whiteScoreText;
    public Text winnerText;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;

    private Player human;
    private Player ai;
    private Player[] playerMap;
    private int difficulty;
    private int blackScore;
    private int whiteScore;
    private Player ply;
    private string STATE = "MAIN MENU";

    void Start()
    {
        ply = Player.Black;
        squares = new Square[64];
        playerMap = new Player[64];
        pieces = new Dictionary<int, Piece>();
    }

    void Update()
    {
        switch (STATE)
        {
            case "MAIN MENU":
                MainMenu();
                break;
            case "PLAY GAME":
                StartCoroutine(PlayGame());
                break;
            case "GAME OVER":
                GameOver();
                break;
        }
    }

    void UpdateScore()
    {
        blackScore = 0;
        whiteScore = 0;
        foreach (Player player in playerMap)
        {
            if (player == Player.Black)
                blackScore++;
            else if (player == Player.White)
                whiteScore++;
        }

        blackScoreText.text = "Black: " + blackScore;
        whiteScoreText.text = "White: " + whiteScore;
    }

    void UpdatePieces()
    {
        for (int i = 0; i < 64; i++)
        {
            if (playerMap[i] != Player.Nobody)
            {
                Piece existingPiece;
                if (!pieces.TryGetValue(i, out existingPiece))
                    PlacePiece(i);
                else
                {
                    if (pieces[i].player != playerMap[i])
                        pieces[i].player = playerMap[i];
                }
            }
        }
    }

    void UpdateSquareDisplays()
    {
        if (ply == human)
        {
            List<int> legalMoves = GetLegalMoves(playerMap, ply);

            for (int i = 0; i < 64; i++)
            {
                if (legalMoves.Contains(i))
                    squares[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
                else
                    squares[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        } else
            foreach (Square square in squares)
                square.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    void MainMenu()
    {
        board.gameObject.SetActive(false);
        if (Input.GetKeyUp(KeyCode.Return))
        {
            mainMenuCanvas.renderMode = RenderMode.WorldSpace;
            mainMenuCanvas.transform.position = new Vector3(1000f, 0f, 0f);
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
            SetInitialBoard();
            blackScoreText.gameObject.SetActive(true);
            whiteScoreText.gameObject.SetActive(true);
            SetPly(Player.Black);
            STATE = "PLAY GAME";
        }
    }

    IEnumerator PlayGame()
    {
        if (GetLegalMoves(playerMap, ply).Count == 0)
        {
            Player currentPlayer = ply;
            List<int> nextLegalMoves;
            if (currentPlayer == Player.White) nextLegalMoves = GetLegalMoves(playerMap, Player.Black);
            else nextLegalMoves = GetLegalMoves(playerMap, Player.White);

            if (nextLegalMoves.Count == 0) STATE = "GAME OVER";
            else
            {
                if (currentPlayer == Player.Black) SetPly(Player.White);
                else SetPly(Player.Black);
            }
        }
        else
        {
            UpdatePieces();
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
            List<int> legalMoves = GetLegalMoves(playerMap, human);
            if (clickedSquare > -1 && legalMoves.Contains(clickedSquare))
            {
                MakeMove(playerMap, ply, clickedSquare);
                SetPly(ai);
            }
        }
        while (ply == human) yield return null;
    }

    IEnumerator AITurn()
    {
        MinimaxPair<Player[], double> moveToMake = Minimax(playerMap, difficulty, true);
        Player[] nextBoard = moveToMake.bestMove;

        int movePos = GetNextMove(playerMap, nextBoard);
        MakeMove(playerMap, ai, movePos);
        SetPly(human);

        while (ply == ai) yield return null;

        // COMMENT EVERYTHING ABOVE FOR A HUMAN VS. HUMAN GAME
        // COMMENT EVERYTHING BELOW FOR A HUMAN VS. AI GAME

        //if (Input.GetMouseButtonUp(0))
        //{
        //    int clickedSquare = GetClickedSquare();
        //    List<int> legalMoves = GetLegalMoves(playerMap, ai);
        //    if (clickedSquare > -1 && legalMoves.Contains(clickedSquare))
        //    {
        //        MakeMove(playerMap, ply, clickedSquare);
        //        SetPly(human);
        //    }
        //}
        //while (ply == ai) yield return null;
    }

    void SetInitialBoard()
    {
        board.gameObject.SetActive(true);
        for (int i = 0; i < 64; i++)
        {
            if (i == 27 || i == 36) InitializeTile(i, Player.Black);
            else if (i == 28 || i == 35) InitializeTile(i, Player.White);
            else InitializeTile(i, Player.Nobody);
        }
    }


    void GameOver()
    {
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Reset();
        board.gameObject.SetActive(false);
        if (blackScore > whiteScore)
            winnerText.text = "Black Wins!";
        else if (whiteScore > blackScore)
            winnerText.text = "White Wins!";
        else
            winnerText.text = "Tie!";

        if (Input.GetKeyUp(KeyCode.R))
        {
            STATE = "MAIN MENU";
            gameOverCanvas.renderMode = RenderMode.WorldSpace;
            gameOverCanvas.transform.position = new Vector3(1000, 0);
            mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    void SetPly(Player player)
    {
        ply = player;

        // TODO add logic for skipping turns
    }

    MinimaxPair<Player[], double> Minimax(Player[] boardMap, int depth, bool maximizingPlayer)
    {
        Player[] gameTreeMap = CopyPlayerMap(boardMap);
        List<int> legalMoves;
        if (maximizingPlayer)
            legalMoves = GetLegalMoves(gameTreeMap, ai);
        else
            legalMoves = GetLegalMoves(gameTreeMap, human);

        if (legalMoves.Count == 0 || depth == 0)
            return new MinimaxPair<Player[], double>(gameTreeMap, Evaluate(gameTreeMap));

        MinimaxPair<Player[], double> bestBoard = new MinimaxPair<Player[], double>();

        if (maximizingPlayer)
        {
            bestBoard.bestScore = Mathf.NegativeInfinity;
            foreach (int legalMove in legalMoves)
            {
                MakeMove(gameTreeMap, ply, legalMove);
                Player[] newGameTreeMap = CopyPlayerMap(gameTreeMap);
                MinimaxPair<Player[], double> nextMove = Minimax(newGameTreeMap, depth - 1, false);
                if (nextMove.bestScore > bestBoard.bestScore)
                {
                    bestBoard.bestMove = nextMove.bestMove;
                    bestBoard.bestScore = nextMove.bestScore;
                }
            }
            return bestBoard;
        }
        else
        {
            bestBoard.bestScore = Mathf.Infinity;
            foreach (int legalMove in legalMoves)
            {
                MakeMove(gameTreeMap, ply, legalMove);
                Player[] newGameTreeMap = CopyPlayerMap(gameTreeMap);
                MinimaxPair<Player[], double> nextMove = Minimax(newGameTreeMap, depth - 1, true);
                if (nextMove.bestScore < bestBoard.bestScore)
                {
                    bestBoard.bestMove = nextMove.bestMove;
                    bestBoard.bestScore = nextMove.bestScore;
                }
            }
            return bestBoard;
        }
    }

    void MakeMove(Player[] map, Player currentPlayer, int position)
    {
        map[position] = currentPlayer;
        FlankPieces(map, currentPlayer, position);
    }

    void FlankPieces(Player[] map, Player currentPlayer, int startingPosition)
    {
        List<int> flankedPieces = new List<int>();
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, -1));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, 1));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, -8));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, 8));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, -9));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, 9));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, -7));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, startingPosition, 7));

        for (int i = 0; i < flankedPieces.Count; i++)
            map[flankedPieces[i]] = currentPlayer;
    }

    List<int> AddFlankedPieces(Player[] playerMap, Player currentPlayer, int position, int direction)
    {
        List<int> flankedPieces = new List<int>();

        for (int i = position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (HasHitWall(i, direction) || playerMap[i] == Player.Nobody)
            {
                flankedPieces.Clear();
                break;
            }
            else if (playerMap[i] != currentPlayer && playerMap[i] != Player.Nobody) flankedPieces.Add(i); // flank the other player
            else if (playerMap[i] == currentPlayer) break; // we have flanked everything we can in this direction
        }
        return flankedPieces;
    }

    bool HasHitWall(int position, int direction)
    {
        switch (direction)
        {
            case -1:
                return position / 8 == 0;
            case 1:
                return position / 8 == 7;
            case -7:
                return position % 8 == 0 || position / 8 == 7;
            case 7:
                return position % 8 == 7 || position / 8 == 0;
            case -8:
                return position % 8 == 0;
            case 8:
                return position % 8 == 7;
            case -9:
                return position % 8 == 0 || position / 8 == 0;
            case 9:
                return position % 8 == 7 || position / 8 == 7;
            default:
                return false;
        }
    }

    void PlacePiece(int position)
    {
        Vector3 piecePosition = squares[position].transform.position;
        piecePosition.y = 1f;
        Piece newPiece;
        if (playerMap[squares[position].position] == Player.White)
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.identity) as Piece;
        else
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.AngleAxis(180f, Vector3.right)) as Piece;
        newPiece.player = playerMap[squares[position].position];
        pieces.Add(squares[position].position, newPiece);
    }

    void InitializeTile(int position, Player player)
    {
        playerMap[position] = player;
        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(squarePrefab, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.position = position;
        squares[position] = s;

        if (player != Player.Nobody)
            PlacePiece(position);
    }

    void Reset()
    {
        squares = new Square[64];
        playerMap = new Player[64];
        pieces.Clear();
        GameObject[] pieceObjects = GameObject.FindGameObjectsWithTag("Piece");
        GameObject[] squareObjects = GameObject.FindGameObjectsWithTag("Square");
        for (int i = 0; i < pieceObjects.Length; i++)
            Destroy(pieceObjects[i]);
        for (int i = 0; i < squareObjects.Length; i++)
            Destroy(squareObjects[i]);
    }

    List<int> GetLegalMoves(Player[] map, Player currentPlayer)
    {
        List<int> legalMoves = new List<int>();
        int[] directions = new int[8] { -1, 1, -8, 8, -9, 9, -7, 7 };

        for (int start = 0; start < 64; start++)
        {
            if (map[start] == currentPlayer)
            {
                foreach (int direction in directions)
                {
                    int legalMove = AddLegalMove(map, currentPlayer, start, direction);
                    if (legalMove != -1)
                        legalMoves.Add(legalMove);
                }
            }
        }
        return legalMoves;
    }

    int AddLegalMove(Player[] map, Player player, int position, int direction)
    {
        bool flankablesExist = false;

        for (int i = position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (HasHitWall(i, direction))
                return -1; // we can't go this way
            if (map[i] != player && map[i] != Player.Nobody)
                flankablesExist = true;
            else if (map[i] == Player.Nobody && flankablesExist)
                return i; // found a legal move
            else return -1; // we can't go this way
        }

        return -1;
    }

    Vector3 DeterminePlacementCoordinates(int position)
    {
        int row = position / 8;
        int col = position % 8;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z);
    }

    Player[] CopyPlayerMap(Player[] map)
    {
        Player[] newMap = new Player[64];
        for (int i = 0; i < 64; i++)
        {
            // I am very paranoid and C# dumb
            if (map[i] == Player.Black)
                newMap[i] = Player.Black;
            else if (map[i] == Player.White)
                newMap[i] = Player.White;
            else
                newMap[i] = Player.Nobody;
        }
        return newMap;
    }

    int GetClickedSquare()
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

    double Evaluate(Player[] boardMap)
    {
        double score = 0.0f;
        score += GetCornerScore(boardMap);
        score += GetEdgeScore(boardMap);
        score += GetPieceCountScore(boardMap);
        return score;
    }

    double GetCornerScore(Player[] boardMap)
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

    double GetEdgeScore(Player[] boardMap)
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

    double GetPieceCountScore(Player[] boardMap)
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

    int GetNextMove(Player[] currentBoardMap, Player[] nextBoardMap)
    {
        for (int i = 0; i < 64; i++)
            if (currentBoardMap[i] == Player.Nobody && nextBoardMap[i] == ai)
                return i;

        Debug.Log("Error in Minimax. Could not find next move.");
        return -1;
    }
}
