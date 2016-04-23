using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum Player
{
    Black,
    White,
    Nobody
};

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square squarePrefab;
    public Piece piecePrefab;
    public Square[] squares;
    public Player[] playerMap;
    public Dictionary<int, Piece> pieces;

    public void Start()
    {
        squares = new Square[64];
        pieces = new Dictionary<int, Piece>();
        playerMap = new Player[64];
    }

    public void SetInitialBoard()
    {
        this.gameObject.SetActive(true);
        for (int i = 0; i < 64; i++)
        {
            if (i == 27 || i == 36) InitializeTile(i, Player.Black);
            else if (i == 28 || i == 35) InitializeTile(i, Player.White);
            else InitializeTile(i, Player.Nobody);
        }
    }

    public void UpdatePieces()
    {
        for (int i = 0; i < 64; i++)
        {
            if (playerMap[i] != Player.Nobody)
            {
                Piece existingPiece;
                if (!pieces.TryGetValue(i, out existingPiece))
                    PlacePiece(squares[i]);
                else
                {
                    if (pieces[i].player != playerMap[i])
                        FlipPiece(playerMap[i], pieces[i]);
                }
            }
        }
    }

    public void CaptureTile(Player[] playerMap, Player player, int position)
    {
        squares[position].isLegalMove = false;
        playerMap[position] = player;
    }
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

        for (int i = 0; i < flankedPieces.Count; i++)
            CaptureTile(currentPlayer, squares[flankedPieces[i]]);
    }

    public void FlipPiece(Player player, Piece piece)
    {
        piece.gameObject.transform.Rotate(180f, 0f, 0f);
    }

    public void Reset()
    {
        this.squares = new Square[64];
        ResetLegalMoves();
        this.pieces.Clear();
        GameObject[] pieceObjects = GameObject.FindGameObjectsWithTag("Piece");
        GameObject[] squareObjects = GameObject.FindGameObjectsWithTag("Square");
        for (int i = 0; i < pieceObjects.Length; i++)
            Destroy(pieceObjects[i]);
        for (int i = 0; i < squareObjects.Length; i++)
            Destroy(squareObjects[i]);
    }

    private List<int> AddFlankedPieces(Player currentPlayer, Square square, int direction)
    {
        List<int> flankedPieces = new List<int>();

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (playerMap[i] != currentPlayer && playerMap[i] != Player.Nobody)
                flankedPieces.Add(i);
            else if (playerMap[i] == currentPlayer)
                break; // we have flanked everything we can in this direction
            else if (playerMap[i] == Player.Nobody || IsPastBoardEdge(square, squares[i], direction))
            {
                flankedPieces.Clear();
                break; // nothing here can be flanked
            }
        }

        return flankedPieces;
    }

    private Vector3 DeterminePlacementCoordinates(int position)
    {
        int row = position / 8;
        int col = position % 8;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z);
    }

    public void PlacePiece(Square s)
    {
        Vector3 piecePosition = s.transform.position;
        piecePosition.y = 1f;
        Piece newPiece;
        if (playerMap[s.position] == Player.White)
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.identity) as Piece;
        else
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.AngleAxis(180f, Vector3.right)) as Piece;
        pieces.Add(s.position, newPiece);
    }

    public void InitializeTile(int position, Player player)
    {
        playerMap[position] = player;

        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(squarePrefab, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.position = position;
        s.row = position % 8;
        s.column = position / 8;
        s.isLegalMove = false;

        if (player != Player.Nobody)
            PlacePiece(s);
        squares[position] = s;
    }

    public void UpdateLegalMoves(Player currentPlayer)
    {
        ResetLegalMoves();
        for (int i = 0; i < 64; i++)
        {
            if (playerMap[i] == currentPlayer)
            {
                AddLegalMove(currentPlayer, i, -1);
                AddLegalMove(currentPlayer, i, 1);
                AddLegalMove(currentPlayer, i, -8);
                AddLegalMove(currentPlayer, i, 8);
                AddLegalMove(currentPlayer, i, -9);
                AddLegalMove(currentPlayer, i, 9);
                AddLegalMove(currentPlayer, i, -7);
                AddLegalMove(currentPlayer, i, 7);
            }
        }
    }

    private void AddLegalMove(Player player, int position, int direction)
    {
        bool flankablesExist = false;

        for (int i = position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (IsPastBoardEdge(squares[position], squares[i], direction))
                return; // we can't go this way
            if (playerMap[i] != player && playerMap[i] != Player.Nobody)
                flankablesExist = true;
            else if (playerMap[i] == Player.Nobody && flankablesExist)
            {
                squares[i].isLegalMove = true;
                return;
            }
            else return; // we can't go this way
        }
    }

    public bool IsPastBoardEdge(Square start, Square end, int direction)
    {
        if (direction > 0) return (start.row - end.row) > 0;
        else return (start.row - end.row) < 0;
    }

    public void ResetLegalMoves()
    {
        foreach (Square s in squares)
            if (s.isLegalMove)
                s.isLegalMove = false;
    }

    public bool HasLegalMoves()
    {
        for (int i = 0; i < 64; i++)
            if (squares[i].isLegalMove)
                return true;
        return false;
    }
}
