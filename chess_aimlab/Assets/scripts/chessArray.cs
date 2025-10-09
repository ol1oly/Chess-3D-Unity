using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
public class chessArray : MonoBehaviour {
    public Camera mainCamera;
    public GameObject chessBoard;
    public GameObject Ui;
    public GameObject pieceAtPromotion;
    public GameObject whitePieces; // will be used to initiate the board of the chess game with the corresponding assets
    public GameObject blackPieces; // will be used to initiate the board of the chess game with the corresponding assets
    public GameObject tilesParent; // to initiate the 2d array of tiles
    Dictionary<GameObject, List<(int, int)>> moveCache = new Dictionary<GameObject, List<(int, int)>>();


    int numberOfChessPiecesAttackingKing = 0;
    int[] currentPiece;
    string sideTurn = "white";

    public static bool endOfGame = false;

    public GameObject textEndGame;
    public TextMeshProUGUI whoWon;

    private bool hasWhiteKingMoved = false;

    private bool hasBlackKingMoved = false;

    private GameObject leftWhiteRook;
    private GameObject rightWhiteRook;
    private GameObject leftBlackRook;
    private GameObject rightBlackRook;

    private GameObject King;
    private GameObject bishop;
    private GameObject knight;
    private GameObject queen;

    private bool leftWhiteRookMoved = false;
    private bool rightWhiteRookMoved = false;
    private bool leftBlackRookMoved = false;
    private bool rightBlackRookMoved = false;

    // client click on piece, it show the red pieces. then, when click, the server handles the move piece method

    public GameObject[,] board = new GameObject[8, 8];
    GameObject[,] boardTiles = new GameObject[8, 8];

    // the board is a 2d array containing the pieces. bottom is white pieces
    // need to check for pawn promotion if pawn is on 7th row or 0 row
    // the methods that compute the possible moves will return a list of 2d coordinates. an array of tuples
    void Start() {


        PopulateGrid(blackPieces, 0);
        PopulateGrid(whitePieces, 6);
        PopulateTiles(tilesParent);

        leftBlackRook = board[0, 0];
        rightBlackRook = board[0, 7];

        leftWhiteRook = board[7, 0];

        rightWhiteRook = board[7, 7];
        queen = board[0, 3];
        King = board[0, 4];
        bishop = board[0, 2];
        knight = board[0, 1];

        Ui.SetActive(false);

        textEndGame.SetActive(false);

    }
    void PopulateGrid(GameObject parent, int start) {
        if (parent == null) return; // will return if the pieces parent are not assigned in the editor


        Transform[] children = parent.GetComponentsInChildren<Transform>();
        children = System.Array.FindAll(children, child => child.parent == parent.transform);

        int index = 0;

        for (int row = start; row < start + 2; row++) {
            for (int col = 0; col < 8; col++) {
                if (index < children.Length) {
                    board[row, col] = children[index].gameObject;
                    index++;
                }
                else {
                    board[row, col] = null; // Fill with null if no more children.
                }
            }
        }
    }
    void PopulateTiles(GameObject parent) {
        if (parent == null) return;

        Transform[] children = parent.GetComponentsInChildren<Transform>();

        int index = 1;

        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                if (index < children.Length) {
                    boardTiles[row, col] = children[index].gameObject;

                    index++;
                }
            }
        }
    }

    void Update() {
        if (!endOfGame && !PauseMenu.isPaused) {

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit[] hits;
                Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                hits = Physics.RaycastAll(ray);
                Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
                foreach (RaycastHit hit in hits) {

                    int[] coordsTile = GetCaseFromCollider(hit.collider.gameObject);

                    int[] coordsPiece = GetTile(hit.collider.gameObject);

                    if (coordsTile == null && coordsPiece == null) break;

                    else if (coordsTile == null && coordsPiece != null) {// it is a piece

                        if (hit.collider.tag.Contains(sideTurn)) { // same color
                            currentPiece = coordsPiece;
                            setColorsTiles(showPossibleMovesArray(hit.collider.gameObject));
                        }
                        else if (boardTiles[coordsPiece[0], coordsPiece[1]].GetComponent<Renderer>().material.color == Color.red && currentPiece != null) { // not same color
                            int[] oldPieces = currentPiece; // Save the current state of the array
                            currentPiece = null; // Set currentPiece to null
                            StartCoroutine(movePieceArray(oldPieces, coordsPiece)); // Start coroutine with the saved array
                            return;
                        }
                        break;
                    }
                    else if (coordsTile != null && coordsPiece == null) {// it is a tile

                        if (boardTiles[coordsTile[0], coordsTile[1]].GetComponent<Renderer>().material.color == Color.red && currentPiece != null) {

                            int[] oldPieces = currentPiece; // Save the current state of the array
                            currentPiece = null; // Set currentPiece to null
                            StartCoroutine(movePieceArray(oldPieces, coordsTile)); // Start coroutine with the saved array
                            return;
                        }
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Return) && endOfGame) {
            endOfGame = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
    int[] GetCaseFromCollider(GameObject tile) {
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (tile == boardTiles[i, j]) return new[] { i, j };
            }
        }
        return null;
    }

    void setColorsTiles(List<(int, int)> coups) {
        for (int i = 0; i < coups.Count; i++) {
            setColor(boardTiles[coups[i].Item1, coups[i].Item2]);
        }
    }
    int[] GetTile(GameObject piece) { // get the coordinates of a specific piece
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (piece == board[i, j]) return new[] { i, j };
            }
        }
        return null;
    }
    int[] GetKing(string side) { // get the coordinates of the king

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (board[i, j] != null && board[i, j].name == "king" && board[i, j].tag.Contains(side)) return new[] { i, j };
            }
        }
        return null;
    }
    bool hasKingMoved(string side) {
        if (side.Contains("white")) return hasWhiteKingMoved;
        return hasBlackKingMoved;
    }
    bool hasLeftRookMoved(string side) {
        if (side.Contains("white")) return leftWhiteRookMoved;
        return leftBlackRookMoved;
    }
    bool hasRightRookMoved(string side) {
        if (side.Contains("white")) return rightWhiteRookMoved;
        return rightBlackRookMoved;
    }
    bool isKing(string side, GameObject piece) {
        return piece.name == "king" && piece.tag.Contains(side);
    }
    bool isRook(string side, GameObject piece) {
        return piece.name == "rook" && piece.tag.Contains(side);
    }
    void setRookMoved(string side, GameObject piece) {
        if (side == "white") {
            if (piece == leftWhiteRook) leftWhiteRookMoved = true;
            else if (piece == rightWhiteRook) rightWhiteRookMoved = true;
            else {
                Debug.Log("in rook method but no rook found");
            }
        }
        else {
            if (piece == leftBlackRook) leftBlackRookMoved = true;
            else if (piece == rightBlackRook) rightBlackRookMoved = true;
            else {
                Debug.Log("in rook method but no rook found");
            }
        }
    }
    void setKingMoved(string side) {
        if (side == "white") hasWhiteKingMoved = true;
        else { hasBlackKingMoved = true; }
    }
    bool isSameColor(GameObject source, GameObject target) {
        return (source.tag == target.tag);
    }
    bool isWhite(GameObject piece) {
        return piece.tag.Contains("white");
    }
    void setColor(GameObject Case) {
        Case.GetComponent<Renderer>().material.color = Color.red;
    }
    void resetBoard() {
        chessBoard.GetComponent<setBoard>().setNormalColors();
    }

    string getOppositeColor(string color) {
        if (color.Contains("black")) return "white";
        else return "black";
    }

    bool isKingCheckedArray(string wichKing) {
        int[] kingCoords = GetKing(wichKing);
        return isBeingProtectedArray(wichKing, kingCoords[0], kingCoords[1]);
    }
    List<(int, int)> canCastleArray(string side) {

        if (isKingCheckedArray(side)) return new List<(int, int)>();

        if (hasKingMoved(side)) return new List<(int, int)>();

        // start from king position, check both sides. if a rook has been moved, go to next loop. otherwise, check
        // each tile to see if it is being protected, or there is a piece. if you manage to go to the rook, you can castle
        // still need to figure out a way to check if it has been eliminated
        int[] kingCoords = GetKing(side);


        List<(int, int)> validTiles = new List<(int, int)>();


        for (int w = -1; w < 2; w += 2) {
            if (w == -1 && hasLeftRookMoved(side)) {
                Debug.Log("the left rook has moved");
                continue;
            }
            if (w == 1 && hasRightRookMoved(side)) {
                Debug.Log("the right rook has moved");
                continue;
            }

            // if we reach this point, that means both rook and king didnt moved
            int j = kingCoords[1];
            j += w;
            while (!getOOB(kingCoords[0], j)) { // while i,j are not out of bounds
                if (board[kingCoords[0], j] == null) {
                    j += w;
                    continue;
                }
                if (isBeingProtectedArray(side, kingCoords[0], j) && j != 6 && j != 0 && j != 7 && j != 1) {
                    Debug.Log("a tile is being protected");
                    break;
                }
                if (board[kingCoords[0], j].tag.Contains(getOppositeColor(side))) {
                    Debug.Log("a opposite piece is being protected");
                    break;
                }
                // at this point we encounter our own piece
                if ((j == 7 || j == 0) && board[kingCoords[0], j].name == "rook") {// it can castle
                    Debug.Log("the king is able to castle");
                    j = (j == 0) ? 2 : 6;
                    validTiles.Add((kingCoords[0], j));
                }
                break;

            }
            //Debug.Log("are these coordinates out of bounds: " + getOOB(kingCoords[0], j)+ " row is " + kingCoords[0] + " column is " + j);

        }
        return validTiles;
    }

    // move piece will take as input this: starting and end position, 
    // it will determines itself or in another function if this happens to be a castle.
    // will be in charge of setting has king moved and the other rooks moved.
    IEnumerator movePieceArray(int[] start, int[] finish) {

        bool castle = false;

        if (board[start[0], start[1]].name == "rook") setRookMoved(sideTurn, board[start[0], start[1]]);


        if (isKing(sideTurn, board[start[0], start[1]])) {
            setKingMoved(sideTurn);
            if (Math.Abs(finish[1] - start[1]) > 1) castle = true;
        }


        else if (board[start[0], start[1]].name == "pawn" && (finish[0] == 0 || finish[0] == 7)) {

            // need to handle promotion here. need to reflect the change on the board
            Ui.SetActive(true); // Activating UI
            Cursor.visible = true; // Showing cursor
            Cursor.lockState = CursorLockMode.None; // Unlocking cursor
            resetBoard();
            yield return new WaitUntil(() => !Ui.activeSelf); // Waiting until UI is inac
            Debug.Log(pieceAtPromotion);


            board[start[0], start[1]].GetComponent<MeshFilter>().mesh = pieceAtPromotion.GetComponent<MeshFilter>().mesh;
            board[start[0], start[1]].GetComponent<BoxCollider>().center = pieceAtPromotion.GetComponent<BoxCollider>().center;
            board[start[0], start[1]].GetComponent<BoxCollider>().size = pieceAtPromotion.GetComponent<BoxCollider>().size;
            board[start[0], start[1]].transform.localScale = pieceAtPromotion.transform.localScale;
            board[start[0], start[1]].name = pieceAtPromotion.name;

            Cursor.visible = false; // Hiding cursor
            Cursor.lockState = CursorLockMode.Locked; // Locking cursor
        }

        // then handle the normal things. 
        // deactivate the tiles that have been eaten
        board[start[0], start[1]].transform.position = boardTiles[finish[0], finish[1]].transform.position;
        if (board[start[0], start[1]].name == "pawn") {
            Vector3 position = board[start[0], start[1]].transform.position;
            position.y += 1;
            board[start[0], start[1]].transform.position = position;

        }

        if (board[finish[0], finish[1]] != null) {
            if (board[finish[0], finish[1]].name == "rook") setRookMoved(getOppositeColor(sideTurn), board[finish[0], finish[1]]);
            board[finish[0], finish[1]].SetActive(false);
        }

        board[finish[0], finish[1]] = board[start[0], start[1]];
        // probably wrong syntax here
        board[start[0], start[1]] = null;

        if (castle) {
            int side = -(finish[1] - start[1]) / Math.Abs(finish[1] - start[1]); //
            int rook = (finish[1] == 2) ? 0 : 7; // states which rook to use

            board[start[0], finish[1] + side] = board[start[0], rook];
            board[start[0], finish[1] + side].transform.position = boardTiles[start[0], finish[1] + side].transform.position;

            board[start[0], rook] = null;
        }
        currentPiece = null;
        resetBoard();
        moveCache.Clear();
        sideTurn = sideTurn == "white" ? "black" : "white";
        checkForMate(sideTurn);
        Debug.Log("its time for " + sideTurn + " to play");

    }

    List<(int, int)> showPossibleMovesArray(GameObject piece) {
        numberOfChessPiecesAttackingKing = 0;
        getCoordPieceAttackingKingArray(sideTurn);

        List<(int, int)> coups = new List<(int, int)>();
        resetBoard();
        if (moveCache.ContainsKey(piece)) return moveCache[piece];

        //Debug.Log("number of pieces attacking king: " +numberOfChessPiecesAttackingKing);
        if (numberOfChessPiecesAttackingKing == 2) {
            if (piece.name == "king") coups = getPossibleKingMovesArray(getColor(piece));
            //Debug.Log("the king is checked by 2 different kings");
        }
        else {
            if (piece.name.Contains("pawn")) coups = handleMovementPawnArray(piece);
            else if (piece.name.Contains("knight")) coups = handleMovementKnightArray(piece);
            else if (piece.name.Contains("king")) coups = getPossibleKingMovesArray(getColor(piece));
            else coups = handleMovementNormalArray(piece);
        }
        moveCache[piece] = coups;


        return coups;
    }

    List<(int, int)> handleMovementPawnArray(GameObject piece) {

        List<(int, int)> retourner = new List<(int, int)>();

        // if the piece is not pinned and the king is in check, check normal moves and diaonal moves if you can make it not in check
        // if the piece is pinned and the king not in check, check normal moves and diagonal moves if some moves still make the piece pinned
        // if piece is not pinned and king not in check, check normal moves and diagonal moves as usual

        List<(int, int)> pinned = isPinnedArray(piece);
        bool kingCheck = isKingCheckedArray(getColor(piece));

        int[] pawnCoords = GetTile(piece);
        int i = pawnCoords[0];
        int j = pawnCoords[1];

        List<(int, int)> listeSpots = new List<(int, int)>();
        if (kingCheck) {
            int[] attackCoords = getCoordPieceAttackingKingArray(getColor(piece));
            listeSpots = getTilesBetweenKingAndAttackerArray(board[attackCoords[0], attackCoords[1]]);
        }
        int side = 1;
        if (isWhite(piece)) side = -1;

        int tilesToCheck = 1;
        if (i == (isWhite(piece) ? 6 : 1)) tilesToCheck = 2;

        // now do 2 tiles of front movement 
        for (int w = side; Math.Abs(w) <= tilesToCheck; w += side) {
            if (getOOB(i + w, j) || board[i + w, j] != null) break; // check if the tile is out of bounds or already a piece on it

            if (!kingCheck) {
                if (pinned[0] == (200, 200)) { // if the pawn is pinned
                    foreach (var tuple in pinned) {
                        if (tuple.Item1 == (i + w) && tuple.Item2 == j) {
                            retourner.Add(tuple); // Add the matching tuple to retourner
                            break; // Stop after finding the first match
                        }
                    }
                }
                else { // normal movement
                    retourner.Add((i + w, j));
                }
            }
            else { // the king is in check
                if (pinned[0] == (100, 100)) { // the pawn is not pinned
                    foreach (var tuple in listeSpots) {
                        if (tuple.Item1 == (i + w) && tuple.Item2 == j) {
                            retourner.Add(tuple); // Add the matching tuple to retourner
                            break; // Stop after finding the first match
                        }
                    }
                }
                else { // king in check and pawn is pinned
                    return new List<(int, int)>();
                }
            }
        }

        // if reach this point, king is not checked AND piece is not pinned     
        //second: handle diagonal movement
        int[] coord = GetTile(piece);
        List<(int, int)> diagonal = getPossibleDiagonalPawnMovesArray(coord[0], coord[1]);

        if (kingCheck) { // we know for sure the piece is not pinned when king is in check

            int[] attackCoords = getCoordPieceAttackingKingArray(getColor(piece));
            listeSpots = getTilesBetweenKingAndAttackerArray(board[attackCoords[0], attackCoords[1]]);
            retourner.AddRange(diagonal.Intersect(listeSpots).ToList());
        }
        else if (pinned[0] == (100, 100)) { // pawn not pinned
            retourner.AddRange(diagonal);
        }
        else { // pawn is pinned
            retourner.AddRange(pinned.Intersect(diagonal).ToList());
        }

        return retourner;
    }

    List<(int, int)> handleMovementNormalArray(GameObject piece) {

        List<(int, int)> retourner = new List<(int, int)>();

        List<(int, int)> possibleMoves = getPossibleNormalMovesArray(piece);
        List<(int, int)> pinnedArray = isPinnedArray(piece);

        if (isKingCheckedArray(getColor(piece))) {
            if (pinnedArray[0] == (100, 100)) { // the piece is not pinned
                return getTilesToDefendKingArray(possibleMoves, piece);
            }
            return new List<(int, int)>(); // the piece is pinned and the king is checked. that piece can do nothing
        }
        else {
            if (pinnedArray[0] == (100, 100)) return possibleMoves;
            // if reach this point, the piece is pinned
            return pinnedArray.Intersect(possibleMoves).ToList();
        }
    }

    List<(int, int)> handleMovementKnightArray(GameObject piece) {

        List<(int, int)> possibleMoves = getPossibleKnightMovesArray(piece);
        List<(int, int)> pinnedArray = isPinnedArray(piece);

        if (pinnedArray[0] == (200, 200)) { // if the knight is pinned
            Debug.Log("the knight should be pinned");
            return new List<(int, int)>();
        }
        // if reach this point, the knight is not pinned

        if (isKingCheckedArray(getColor(piece))) {
            List<(int, int)> DefendKing = getTilesToDefendKingArray(possibleMoves, piece);
            return DefendKing;
        }
        else {
            return possibleMoves;
        }
    }
    List<(int, int)> getPossibleKingMovesArray(string side) {

        List<(int, int)> retourner = new List<(int, int)>();
        // check all 8 directions around the king. Then, check if that case is empty and it is being protected
        int[] kingCoords = GetKing(side);
        int i = kingCoords[0];
        int j = kingCoords[1];
        for (int w = -1; w < 2; w += 1) {
            if (!getOOB(i + 1, j + w) && !isBeingProtectedArray(side, i + 1, j + w) && (board[i + 1, j + w] == null || !isSameColor(board[i, j], board[i + 1, j + w]))) {
                retourner.Add((i + 1, j + w));
            }
            if (!getOOB(i - 1, j - w) && !isBeingProtectedArray(side, i - 1, j - w) && (board[i - 1, j - w] == null || !isSameColor(board[i, j], board[i - 1, j - w]))) {
                retourner.Add((i - 1, j - w));
            }
            // the middle tile
            if (!getOOB(i, j + w) && w != 0 && !isBeingProtectedArray(side, i, j + w) && (board[i, j + w] == null || !isSameColor(board[i, j], board[i, j + w]))) {
                retourner.Add((i, j + w));
            }

            if (isKingCheckedArray(side)) {
                int[] coordsAttacker = getCoordPieceAttackingKingArray(side);
                int x = (i - coordsAttacker[0]) != 0 ? (i - coordsAttacker[0]) / Math.Abs(i - coordsAttacker[0]) : 0;
                int y = (j - coordsAttacker[1]) != 0 ? (j - coordsAttacker[1]) / Math.Abs(j - coordsAttacker[1]) : 0;


                if (!getOOB(i + x, j + y) && (board[i + x, j + y] == null || board[i + x, j + y].tag.Contains(getOppositeColor(side)))) {
                    GameObject og = board[i + x, j + y];
                    board[i + x, j + y] = board[i, j];
                    board[i, j] = null;
                    if (isKingCheckedArray(side)) retourner.Remove((i + x, j + y));

                    board[i, j] = board[i + x, j + y];
                    board[i + x, j + y] = og;
                }
            }
        }
        retourner.AddRange(canCastleArray(side));
        return retourner;
    }

    int[] checkForMate(string sideToCkeck) {

        List<(int, int)> coupsPossibles = new List<(int, int)>();

        // go through the board, if its the right color, compute its moves and add it to a list. 
        // then check for stalemate or checkmate. The function will return a tuple.
        // if end of game, which type. both are int
        // 0 means game continue, 1 means game is over
        // 0 is checkmate, 1 is stalemate, 2 is draw by insufficicent material
        bool notEnoughMaterialCurrentTeam = true;
        bool notEnoughMaterialOtherTeam = true;

        int countTeam = 0;
        int numOfKnights = 0;
        int numOfBishops = 0;
        int numOfKnightsOther = 0;
        int numOfBishopsOther = 0;

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {

                if (board[i, j] == null) continue;

                countTeam++;

                if (board[i, j].tag.Contains(sideToCkeck)) { // if its on the side to check

                    coupsPossibles.AddRange(showPossibleMovesArray(board[i, j]));

                    if (board[i, j].name == "pawn" || board[i, j].name.Contains("rook")) notEnoughMaterialCurrentTeam = false;
                    else if (board[i, j].name == "knight") numOfKnights++;
                    if (board[i, j].name.Contains("bishop")) numOfBishops++;
                }
                else { // on the other team
                    if (board[i, j].name == "pawn" || board[i, j].name.Contains("rook")) notEnoughMaterialOtherTeam = false;
                    else if (board[i, j].name == "knight") numOfKnightsOther++;
                    if (board[i, j].name.Contains("bishop")) numOfBishopsOther++;
                }
            }
        }
        // check if you can mate for that team
        if (numOfBishops == 2 && numOfKnights == 0) notEnoughMaterialCurrentTeam = false;
        else if (numOfKnights == 1 && numOfBishops == 1) notEnoughMaterialCurrentTeam = false;
        else if (numOfBishops + numOfKnights > 2) notEnoughMaterialCurrentTeam = false;

        // check if you can mate for the opposite team
        if (numOfBishopsOther == 2 && numOfKnightsOther == 0) notEnoughMaterialOtherTeam = false;
        else if (numOfKnightsOther == 1 && numOfBishopsOther == 1) notEnoughMaterialOtherTeam = false;
        else if (numOfBishopsOther + numOfKnightsOther > 2) notEnoughMaterialOtherTeam = false;


        if ((notEnoughMaterialOtherTeam && notEnoughMaterialCurrentTeam) || countTeam == 2) {
            Debug.Log("draw by insufficient material");
            Debug.Log(numOfKnights + numOfBishops);
            endOfGame = true;
            textEndGame.SetActive(true);
            whoWon.text = "draw by insufficient material";
            return new int[] { 1, 2 };
        }

        if (coupsPossibles.Count() == 0) {
            textEndGame.SetActive(true);
            Debug.Log("il n'y a auncun coups possible");
            if (isKingCheckedArray(sideToCkeck)) {
                Debug.Log("checkmate");
                whoWon.text = getOppositeColor(sideTurn) + " wins!";
                endOfGame = true;
                return new int[] { 1, 0 };
            }
            else {
                Debug.Log("stalemate");
                whoWon.text = "it's a stalemate";
                endOfGame = true;
                return new int[] { 1, 1 };
            }
        }
        return new int[] { 0, 0 };
    }
    int[] getCoordPieceAttackingKingArray(string kingside) {

        int[] retourner = new int[2];
        int[] kingCoord = GetKing(kingside);
        int i = kingCoord[0];
        int j = kingCoord[1];

        for (int w = -1; w < 2; w += 2) {
            int k = i;// while in bounds, and not encounter a piece from same color
            k += w;
            int counter = 0;
            while (!getOOB(k, j)) {
                if (counter > 20) {

                    break;
                }
                counter++;
                if (board[k, j] == null) {
                    k += w;
                    continue; // problem here.
                }
                if (board[k, j].tag.Contains(kingside)) break;

                // at this point we should have encountered a opposite piece 

                if (board[k, j].name.Contains("rook")) {
                    retourner = new int[] { k, j };
                    numberOfChessPiecesAttackingKing++;
                    k += w;
                }
                else break;
            }
            k = j;
            k += w;

            while (!getOOB(i, k)) {
                if (board[i, k] == null) {
                    k += w;
                    continue; // problem here.
                }
                if (board[i, k].tag.Contains(kingside)) break;
                // check to see if there is a opposite pawn
                // at this point we should have encountered a opposite piece 

                if (board[i, k].name.Contains("rook")) {
                    retourner = new int[] { i, k };
                    numberOfChessPiecesAttackingKing++;
                    k += w;
                }
                else break;
            }
            // -1 go towards the black piece. when -1,
            k = i;
            int p = j;
            p++;
            k += w;
            counter = 0;
            while (!getOOB(k, p)) {
                if (counter > 20) {
                    Debug.Log("infinite on 793");
                    Debug.Log("k and p are " + k + " " + p);
                    break;
                }
                counter++;
                if (board[k, p] == null) {
                    k += w;
                    p++;
                    continue; // problem here.
                }
                if (board[k, p].tag.Contains(kingside)) break;

                // at this point we should have encountered a opposite piece 
                // check to see if there is a opposite pawn
                if (p == j + 1 && !getOOB(k, j + 1) && board[k, j + 1] != null && (board[k, j + 1].name.Contains("pawn"))) {
                    retourner = new int[] { k, j + 1 };
                    numberOfChessPiecesAttackingKing++;
                }

                if (board[k, p].name.Contains("bishop")) {
                    retourner = new int[] { k, p };
                    numberOfChessPiecesAttackingKing++;
                    k += w;
                    p++;
                }
                else break;
            }
            k = i;
            p = j;
            k += w;
            p--;
            counter = 0;
            while (!getOOB(k, p)) {
                if (counter > 20) {
                    Debug.Log("infinite on 743");
                    break;
                }
                counter++;
                if (board[k, p] == null) {
                    k += w;
                    p--;
                    continue; // problem here.
                }
                if (board[k, p].tag.Contains(kingside)) break;

                // at this point we should have encountered a opposite piece 

                // check to see if there is a opposite pawn
                if (p == j - 1 && !getOOB(k, j - 1) && board[k, j - 1] != null && (board[k, j - 1].name.Contains("pawn"))) {
                    retourner = new int[] { k, j - 1 };
                    numberOfChessPiecesAttackingKing++;
                }
                if (board[k, p].name.Contains("bishop")) {
                    retourner = new int[] { k, p };
                    numberOfChessPiecesAttackingKing++;
                    k += w;
                    p--;
                }
                else break;
            }
        }
        //next: knight movement:

        for (int w = -2; w < 3; w += 4) {
            string opposite = getOppositeColor(kingside);
            if (!getOOB(i + 1, j + w) && board[i + 1, j + w] != null && board[i + 1, j + w].name.Contains("knight") && board[i + 1, j + w].tag.Contains(opposite)) {
                retourner = new int[] { i + 1, j + w };
                numberOfChessPiecesAttackingKing++;
            }
            if (!getOOB(i - 1, j + w) && board[i - 1, j + w] != null && board[i - 1, j + w].name.Contains("knight") && board[i - 1, j + w].tag.Contains(opposite)) {
                retourner = new int[] { i - 1, j + w };
                numberOfChessPiecesAttackingKing++;
            }
            if (!getOOB(i + w, j + 1) && board[i + w, j + 1] != null && board[i + w, j + 1].name.Contains("knight") && board[i + w, j + 1].tag.Contains(opposite)) {
                retourner = new int[] { i + w, j + 1 };
                numberOfChessPiecesAttackingKing++;
            }
            if (!getOOB(i + w, j - 1) && board[i + w, j - 1] != null && board[i + w, j - 1].name.Contains("knight") && board[i + w, j - 1].tag.Contains(opposite)) {
                retourner = new int[] { i + w, j - 1 };
                numberOfChessPiecesAttackingKing++;
            }
        }
        return retourner;
    }
    bool isBeingProtectedArray(string sideAttacked, int i, int j) {
        // this function checks if the tile at i j is being attacked by the other side than the one in the parameter
        for (int w = -1; w < 2; w += 2) {
            int k = i;
            k += w;
            while (!getOOB(k, j)) {

                if (board[k, j] == null) {
                    k += w;
                    continue;
                }
                if (board[k, j].tag.Contains(sideAttacked)) break;

                // check to see if there is a opposite king
                if (k == i + w && board[i + w, j].name.Contains("king")) return true;

                if (board[k, j].name.Contains("rook")) return true;
                else break;
            }
            k = j;
            k += w;

            while (!getOOB(i, k)) {

                if (board[i, k] == null) {
                    k += w;
                    continue; // problem here.
                }
                if (board[i, k].tag.Contains(sideAttacked)) break;
                // at this point it is an opposite piece
                // check to see if there is a opposite pawn
                if (k == j + w && board[i, j + w].name.Contains("king")) return true;

                if (board[i, k].name.Contains("rook")) return true;
                else break;
            }

            // next bishop movement
            k = i;
            int p = j;
            p++;
            k += w;

            int side = sideAttacked == "white" ? -1 : 1;

            GameObject next = !getOOB(i + side, j + w) ? board[i + side, j + w] : null;
            if (next != null && !next.tag.Contains(sideAttacked) && (next.name.Contains("pawn") || next.name.Contains("king"))) {
                return true;
            }

            while (!getOOB(k, p)) {

                if (board[k, p] == null) {
                    k += w;
                    p++;
                    continue;
                }
                if (board[k, p].tag.Contains(sideAttacked)) break;

                if (board[k, p].name.Contains("bishop")) return true;
                else break;
            }
            k = i;
            p = j;
            k += w;
            p--;
            while (!getOOB(k, p)) {

                if (board[k, p] == null) {
                    k += w;
                    p--;
                    continue;
                }
                if (board[k, p].tag.Contains(sideAttacked)) break;

                if (board[k, p].name.Contains("bishop")) return true;
                else break;
            }
        }
        //next: knight movement:
        for (int w = -2; w < 3; w += 4) {
            string otherSide = getOppositeColor(sideAttacked);
            if (!getOOB(i + 1, j + w) && board[i + 1, j + w] != null && board[i + 1, j + w].name.Contains("knight") && board[i + 1, j + w].tag.Contains(otherSide)) return true;
            if (!getOOB(i - 1, j + w) && board[i - 1, j + w] != null && board[i - 1, j + w].name.Contains("knight") && board[i - 1, j + w].tag.Contains(otherSide)) return true;
            if (!getOOB(i + w, j + 1) && board[i + w, j + 1] != null && board[i + w, j + 1].name.Contains("knight") && board[i + w, j + 1].tag.Contains(otherSide)) return true;
            if (!getOOB(i + w, j - 1) && board[i + w, j - 1] != null && board[i + w, j - 1].name.Contains("knight") && board[i + w, j - 1].tag.Contains(otherSide)) return true;
        }
        return false;
    }
    List<(int, int)> getPossibleKnightMovesArray(GameObject horse) //Done
    {

        var tupleList = new List<(int, int)>();
        int[] coordinate = GetTile(horse);
        int i = coordinate[0];
        int j = coordinate[1];

        int[] bothDirections = { -1, 1 };
        foreach (int d in bothDirections) {
            if (!getOOB(i + d, j - 2) && (board[i + d, j - 2] == null || !isSameColor(horse, board[i + d, j - 2]))) tupleList.Add((i + d, j - 2));

            if (!getOOB(i + d, j + 2) && (board[i + d, j + 2] == null || !isSameColor(horse, board[i + d, j + 2]))) tupleList.Add((i + d, j + 2));

            if (!getOOB(i + 2, j + d) && (board[i + 2, j + d] == null || !isSameColor(horse, board[i + 2, j + d]))) tupleList.Add((i + 2, j + d));

            if (!getOOB(i - 2, j + d) && (board[i - 2, j + d] == null || !isSameColor(horse, board[i - 2, j + d]))) tupleList.Add((i - 2, j + d));
        }
        return tupleList;
    }

    bool getOOB(int i, int j) { // check if coordinates are out of bounds
        return i < 0 || i > 7 || j < 0 || j > 7;
    }

    List<(int, int)> getPossibleNormalMovesArray(GameObject normalPiece) {

        List<(int, int)> liste = new List<(int, int)>(); // we will return this
        int[] pieceCoords = GetTile(normalPiece);
        int i = pieceCoords[0];
        int j = pieceCoords[1];

        if (normalPiece.name.Contains("rook")) {
            for (int w = -1; w < 2; w += 2) {
                int v = i;
                v += w;

                while (!getOOB(v, j)) {

                    if (board[v, j] == null) {
                        liste.Add((v, j));
                        v += w;
                        continue;
                    }
                    // if reach this point, that means there is a piece there
                    if (!isSameColor(normalPiece, board[v, j])) liste.Add((v, j));
                    break;
                }

                v = j;
                v += w;
                while (!getOOB(i, v)) {

                    if (board[i, v] == null) {
                        liste.Add((i, v));
                        v += w;
                        continue;
                    }
                    // if reach this point, that means there is a piece there
                    if (!isSameColor(normalPiece, board[i, v])) liste.Add((i, v));
                    break;
                }
            }
        }
        if (normalPiece.name.Contains("bishop")) {

            for (int w = -1; w < 2; w += 2) {
                int v = i;
                int z = j;
                v += w;
                z += 1;

                while (!getOOB(v, z)) {

                    if (board[v, z] == null) {
                        liste.Add((v, z));
                        v += w;
                        z++;
                        continue;
                    }
                    // if reach this point, that means there is a piece there
                    if (!isSameColor(normalPiece, board[v, z])) liste.Add((v, z));
                    break;
                }
                v = i;
                z = j;
                z -= 1;
                v += w;

                while (!getOOB(v, z)) {

                    if (board[v, z] == null) {
                        liste.Add((v, z));
                        v += w;
                        z--;
                        continue;
                    }
                    // if reach this point, that means there is a piece there
                    if (!isSameColor(normalPiece, board[v, z])) liste.Add((v, z));
                    break;
                }
            }
        }
        return liste;
    }

    List<(int, int)> getPossibleDiagonalPawnMovesArray(int i, int j) // done
    {
        // i and j are the coordinates of the pawn we want to check

        List<(int, int)> retourner = new List<(int, int)>();

        GameObject pawn = board[i, j];
        int side = -1;
        if (pawn.tag.Contains("black")) side = 1;
        // if not out of bounds, and not empty square, and same color with the piece
        if (!getOOB(i + side, j + 1) && board[i + side, j + 1] != null && !isSameColor(pawn, board[i + side, j + 1])) retourner.Add((i + side, j + 1));

        if (!getOOB(i + side, j - 1) && board[i + side, j - 1] != null && !isSameColor(pawn, board[i + side, j - 1])) retourner.Add((i + side, j - 1));

        return retourner;
    }

    string getColor(GameObject collider) {
        if (collider.tag.Contains("white")) return "white";
        return "black";
    }
    List<(int, int)> isPinnedArray(GameObject piece) // done
    { // returns a list of coordinates the pinned piece can go to while still be pinned or eat the piece
      // need to differentiate when its pinned and there is no place to go to still be pinned.

        // first element in list indicate if the current piece is pinned or not.
        // 100,100 : piece is not pinned ||||| 200,200 : piece is pinned

        List<(int, int)> retourner = new List<(int, int)>();
        bool passed = false;

        int[] kingCoords = GetKing(getColor(piece));
        int[] pieceCoords = GetTile(piece);

        int x = pieceCoords[0] - kingCoords[0];
        int y = pieceCoords[1] - kingCoords[1];

        // return not pinned if y/x is higher than 1. abs
        if (x != 0 && y != 0 && Math.Floor((double)x / y) != x / y) return new List<(int, int)> { (100, 100) };

        // normalize the directions
        if (y != 0) y = y / Math.Abs(y);
        if (x != 0) x = x / Math.Abs(x);

        // determines which piece to check depending on the direction between piece and king
        string pieceToCheck = "bishop";
        if (x == 0 || y == 0) pieceToCheck = "rook";

        int i = kingCoords[0];
        int j = kingCoords[1];
        i += x;
        j += y;

        while (!getOOB(i, j)) {

            if (board[i, j] == null) {
                i += x;
                j += y;
                continue;
            }
            //Debug.Log($"Are they the same object? {board[i, j] == piece}");
            if (board[i, j] == piece) {
                passed = true;
                i += x;
                j += y;
                continue;
            }
            // if encounter a piece from the same color. it is not pinned. since already checked if its piece
            if (isSameColor(piece, board[i, j])) {

                return new List<(int, int)> { (100, 100) };
            }
            else {// encounter a opposite piece
                if (passed && board[i, j].name.Contains(pieceToCheck)) {// it is pinned
                    if (piece.name == "knight") return new List<(int, int)> { (200, 200) };

                    List<(int, int)> result = new List<(int, int)> { (200, 200) };
                    result.AddRange(getTilesBetweenKingAndAttackerArray(board[i, j]));

                    result.Add((i, j));
                    return result;
                }
                else return new List<(int, int)> { (100, 100) };
            }
        }
        //Debug.Log("should reach here");
        return new List<(int, int)> { (100, 100) };
    }
    List<(int, int)> getTilesBetweenKingAndAttackerArray(GameObject Attacker) // Done 
    {
        // the tile of the attacker is included
        // get tile of king attacked, get tile of attacker. then, sub their coordinates together. 
        // normalize coordinate so that >1 becomes 1. then, start at the attacker until reach king
        int[] coordAttacker = GetTile(Attacker);
        int[] coordKing = GetKing(getOppositeColor(Attacker.tag));

        if (Attacker.name == "knight") return new List<(int, int)> { (coordAttacker[0], coordAttacker[1]) };

        var tupleList = new List<(int, int)>();

        int changeI = (coordKing[0] - coordAttacker[0]) == 0 ? 0 : (coordKing[0] - coordAttacker[0]) / (Math.Abs(coordKing[0] - coordAttacker[0]));
        int changeJ = (coordKing[1] - coordAttacker[1]) == 0 ? 0 : (coordKing[1] - coordAttacker[1]) / (Math.Abs(coordKing[1] - coordAttacker[1]));


        int i = coordAttacker[0];
        int j = coordAttacker[1];

        while ((i != coordKing[0] || j != coordKing[1])) {

            tupleList.Add((i, j));
            i += changeI;
            j += changeJ;

        }
        return tupleList;
    }

    List<(int, int)> getTilesToDefendKingArray(List<(int, int)> possibleMoves, GameObject piece) {

        int[] coordsAttacker = getCoordPieceAttackingKingArray(piece.tag);
        GameObject pieceAttacking = board[coordsAttacker[0], coordsAttacker[1]];


        List<(int, int)> liste = getTilesBetweenKingAndAttackerArray(pieceAttacking);

        if ((piece.name.Contains("rook")) || (piece.name.Contains("bishop")) || (piece.name.Contains("knight"))) {
            liste.Add((coordsAttacker[0], coordsAttacker[1]));
        }
        if (liste.Count > 15 || possibleMoves.Count > 15) {
            return new List<(int, int)>();
        }
        List<(int, int)> retourner = liste.Intersect(possibleMoves).ToList();
        return retourner;
    }
    public void promotionQueen() {
        pieceAtPromotion = queen;
        Ui.SetActive(false);
    }
    public void promotionRook() {

        pieceAtPromotion = leftBlackRook;
        Ui.SetActive(false);
    }
    public void promotionBishop() {
        pieceAtPromotion = bishop;
        Ui.SetActive(false);
    }
    public void promotionKnight() {
        pieceAtPromotion = knight;
        Ui.SetActive(false);
    }
}