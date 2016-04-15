﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square squarePrefab;
    public Piece piecePrefab;
    public Square[] squares;
    public Dictionary<int, Piece> pieces;
    public List<Square> legalMoves;

    void Start()
    {
        squares = new Square[64];
        pieces = new Dictionary<int, Piece>();
        legalMoves = new List<Square>();
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

    public void CaptureTile(Player player, Square square)
    {
        squares[square.position].isLegalMove = false;
        squares[square.position].player = player;
        Piece existingPiece;
        if (pieces.TryGetValue(square.position, out existingPiece))
            pieces[square.position].gameObject.transform.Rotate(180f, 0f, 0f);
        else
            PlacePiece(squares[square.position]);

    }

    public void FlipPiece(Player player, Square square)
    {
        CaptureTile(player, square);
    }

    public void Reset()
    {
        this.squares = new Square[64];
        this.legalMoves.Clear();
        this.pieces.Clear();
        GameObject[] pieceObjects = GameObject.FindGameObjectsWithTag("Piece");
        GameObject[] squareObjects = GameObject.FindGameObjectsWithTag("Square");
        for (int i = 0; i < pieceObjects.Length; i++)
            Destroy(pieceObjects[i]);
        for (int i = 0; i < squareObjects.Length; i++)
            Destroy(squareObjects[i]);
    }

    Vector3 DeterminePlacementCoordinates(int position)
    {
        int row = position / 8;
        int col = position % 8;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z);
    }

    void PlacePiece(Square s)
    {
        Vector3 piecePosition = s.transform.position;
        piecePosition.y = 1f;
        Piece newPiece;
        if (s.player == Player.White)
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.identity) as Piece;
        else
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.AngleAxis(180f, Vector3.right)) as Piece;
        pieces.Add(s.position, newPiece);
    }

    void InitializeTile(int position, Player player)
    {
        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(squarePrefab, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.player = player;
        s.position = position;
        s.row = position % 8;
        s.column = position / 8;
        s.isLegalMove = false;
        if (player != Player.Nobody)
            PlacePiece(s);
        squares[position] = s;
    }
}
