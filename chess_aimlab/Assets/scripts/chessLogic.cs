/*using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

public class chessLogic : MonoBehaviour { // this is the file that has the state of the board

    string sideTurn = "white";

    private bool leftWhiteRookMoved = false;
    private bool rightWhiteRookMoved = false;
    private bool leftBlackRookMoved = false;
    private bool rightBlackRookMoved = false;

    private bool endOfGame = false;

    public String[,] board = new String[8, 8];
    // syntax 
    /*
    "" = empty cell
    BP = black pawn
    BQ = black queen
    BR = black rook
    BK = black king
    Bb = black bishop
    BK = black knight

    WP = white pawn
    WQ = white queen
    WR = white rook
    WK = white king
    Wb = white bishop
    WK = white knight
    
    *//*

    public void initialializeEmptyBoard() {
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                board[i, j] = "";
            }
        }
    }
    public void initializeStartingBoard() {
        initialializeEmptyBoard();
    }

    List<(int, int)> getPossibleDiagonalPawnMoves(int i, int j) {
        // i and j are the coordinates of the pawn we want to check

        List<(int, int)> retourner = new List<(int, int)>();

        string pawn = board[i, j];
        int[] pawnCoords = [i, j];
        int side = -1; // side = -1 for white
        if (pawn.Contains('B')) side = 1;
        // if not out of bounds, and not empty square, and same color with the piece
        if (!getOOB(i + side, j + 1) && board[i + side, j + 1] != "" && !isSameColor(pawnCoords, [i + side, j + 1])) retourner.Add((i + side, j + 1));

        if (!getOOB(i + side, j - 1) && board[i + side, j - 1] != "" && !isSameColor(pawnCoords, [i + side, j - 1])) retourner.Add((i + side, j - 1));

        return retourner;
    }
    List<(int, int)> getPossibleKnightMovesArray(int[] horse) { //DONE

        var tupleList = new List<(int, int)>();

        int i = horse[0];
        int j = horse[1];

        int[] bothDirections = { -1, 1 };
        foreach (int d in bothDirections) {
            if (!getOOB(i + d, j - 2) && (board[i + d, j - 2] == "" || !isSameColor(horse, [i + d, j - 2]))) tupleList.Add((i + d, j - 2));

            if (!getOOB(i + d, j + 2) && (board[i + d, j + 2] == "" || !isSameColor(horse, [i + d, j + 2]))) tupleList.Add((i + d, j + 2));

            if (!getOOB(i + 2, j + d) && (board[i + 2, j + d] == "" || !isSameColor(horse, [i + 2, j + d]))) tupleList.Add((i + 2, j + d));

            if (!getOOB(i - 2, j + d) && (board[i - 2, j + d] == "" || !isSameColor(horse, [i - 2, j + d]))) tupleList.Add((i - 2, j + d));
        }
        return tupleList;
    }


    List<(int, int)> getTilesBetweenKingAndAttackerArray(int[] Attacker) { //DONE
        // the tile of the attacker is included
        // get tile of king attacked, get tile of attacker. then, sub their coordinates together. 
        // normalize coordinate so that >1 becomes 1. then, start at the attacker until reach king
        String pieceName = board[attackerRow, attackColumn];
        int attackerRow = Attacker[0];
        int attackColumn = Attacker[1];
        int[] coordKing = GetKing(getOppositeColor(pieceName));

        if (pieceName.Contains('K')) return new List<(int, int)> { (attackerRow, attackColumn) };

        var tupleList = new List<(int, int)>();

        int changeI = (coordKing[0] - attackerRow) == 0 ? 0 : (coordKing[0] - attackerRow) / (Math.Abs(coordKing[0] - coordAttacker[0]));
        int changeJ = (coordKing[1] - attackColumn) == 0 ? 0 : (coordKing[1] - attackColumn) / (Math.Abs(coordKing[1] - coordAttacker[1]));


        int i = attackerRow;
        int j = attackColumn;

        while (i != coordKing[0] || j != coordKing[1]) {

            tupleList.Add((i, j));
            i += changeI;
            j += changeJ;

        }
        return tupleList;
    }


    List<(int, int)> isPinnedArray(int[] pieceCoords) { // returns a list of coordinates the pinned piece can go to while still be pinned or eat the piece
                                                        // need to differentiate when its pinned and there is no place to go to still be pinned.

        // first element in list indicate if the current piece is pinned or not.
        // 100,100 : piece is not pinned ||||| 200,200 : piece is pinned
        string pieceName = board[pieceCoords[0], pieceCoords[1]];
        List<(int, int)> retourner = new List<(int, int)>();
        bool passed = false;

        int[] kingCoords = GetKing(getColor(pieceCoords));

        int xDirection = pieceCoords[0] - kingCoords[0];
        int yDirection = pieceCoords[1] - kingCoords[1];

        // return not pinned if y/x is higher than 1. abs
        if (xDirection != 0 && yDirection != 0 && Math.Floor((double)xDirection / yDirection) != xDirection / yDirection) return new List<(int, int)> { (100, 100) };

        // normalize the directions
        if (yDirection != 0) yDirection = yDirection / Math.Abs(yDirection);
        if (xDirection != 0) xDirection = xDirection / Math.Abs(xDirection);

        // determines which piece to check depending on the direction between piece and king
        string pieceToCheck = "b"; // for bishop
        if (xDirection == 0 || yDirection == 0) pieceToCheck = "R"; // for rook

        int i = kingCoords[0];
        int j = kingCoords[1];
        i += xDirection;
        j += yDirection;

        while (!getOOB(i, j)) {

            if (board[i, j] == "") {
                i += xDirection;
                j += yDirection;
                continue;
            }
            //Debug.Log($"Are they the same object? {board[i, j] == piece}");
            if (i == pieceCoords[0] && j == pieceCoords[1]) {
                passed = true;
                i += xDirection;
                j += yDirection;
                continue;
            }
            // if encounter a piece from the same color. it is not pinned. since already checked if its piece
            if (isSameColor(pieceCoords, [i, j])) return new List<(int, int)> { (100, 100) };

            else {// encounter a opposite piece
                if (passed && board[i, j].Contains(pieceToCheck)) {// it is pinned
                    if (pieceName.Contains('K')) return new List<(int, int)> { (200, 200) };

                    List<(int, int)> result = [(200, 200), .. getTilesBetweenKingAndAttackerArray(board[i, j]), (i, j)];
                    return result;
                }
                else return new List<(int, int)> { (100, 100) };
            }
        }
        //Debug.Log("should reach here");
        return new List<(int, int)> { (100, 100) };
    }



    string getColor(int[] pieceCoords) {
        pieceName = board[pieceCoords[0], pieceCoords[1]];
        return pieceName[0];
    }


    bool getOOB(int i, int j) { // check if coordinates are out of bounds
        return i < 0 || i > 7 || j < 0 || j > 7;
    }


    bool isWhite(int[] piece) {
        pieceName = board[piece[0], piece[1]];
        return !isEmptyCell(piece) && pieceName.Contains('W');
    }

    string getOppositeColor(string color) {
        if (color.Contains('B')) return "W";
        return "W";
    }

    int[] GetKing(string side) { // get the coordinates of the king

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (board[i, j] == (side + "K")) return [i, j];
            }
        }
        return null;
    }
    bool hasKingMoved(string side) {
        return side.Contains('W') ? hasWhiteKingMoved : hasBlackKingMoved;
    }
    bool hasLeftRookMoved(string side) {
        return side.Contains('W') ? leftWhiteRookMoved : leftBlackRookMoved;

    }
    bool hasRightRookMoved(string side) {
        return side.Contains('W') ? rightWhiteRookMoved : rightBlackRookMoved;
    }

    string getOppositeColor(string color) {
        if (color.Contains('W')) return "B";
        return "W";
    }

    bool isSameColor(int[] firstPiece, int[] secondPiece) {
        if (isEmptyCell(firstPiece) || isEmptyCell(secondPiece)) return false;
        return board[firstPiece[0], firstPiece[1]][0] == board[secondPiece[0], secondPiece[1]][0];
    }

    bool isEmptyCell(int[] cell) {
        return board[cell[0], cell[1]] == "";
    }

    void setKingMoved(string side) {
        if (side.Contains('W')) hasWhiteKingMoved = true;
        else hasBlackKingMoved = true;
    }





}*/