using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square square;
    public Piece piece;
    private List<Square> squares = new List<Square>();
    private List<Piece> pieces = new List<Piece>();

    public void SetInitialBoard()
    {
        InitializeTile(27, Player.Black);
        InitializeTile(28, Player.White);
        InitializeTile(35, Player.White);
        InitializeTile(36, Player.Black);
    }

    Vector3 DeterminePlacementCoordinates(Square square)
    {
        int row = square.index / 8;
        int col = square.index % 8;
        square.row = row;
        square.column = col;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z); // TODO
    }

    void PlacePiece(Square square)
    {
        if (square.player == Player.White)
            Instantiate(piece, DeterminePlacementCoordinates(square), Quaternion.identity);
        else
            Instantiate(piece, DeterminePlacementCoordinates(square), Quaternion.AngleAxis(180f, Vector3.right));
        pieces.Add(piece);
    }

    void InitializeTile(int position, Player player)
    {
        Square s = gameController.CreateSquare(position);
        squares.Add(s);
        s.player = player;
        PlacePiece(s);
    }
}
