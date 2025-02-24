using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private static Piece currentlyLiftedPiece = null;
    private BoardManager boardManager;
    private GameManager gameManager;
    private Vector2Int currentPosition;
    private List<Vector2Int> possibleJumpers = new List<Vector2Int>();
    private List<GameObject> highlightedFields = new List<GameObject>();
    private bool IfJumpPossible;
    private bool isQueen = false;
    private bool isLifted = false;
    public GameObject currentBoardField;
    public Color color;
    public bool WhiteOrRed;
    
    public bool GetIsQueen(){
        return isQueen;
    }
    public Vector2Int GetCurrentPos(){
        return currentPosition;
    }
    public static Piece GetCurrentLiftedPiece(){
        return currentlyLiftedPiece;
    }
    public static void SetCurrentLiftedPiece(Piece p){
        currentlyLiftedPiece = p;
    }
    public GameManager GetGameManager(){
        return gameManager;
    }

    public void SetIsLifted(bool b){
        isLifted = b;
    }

    public BoardManager GetBoardManager(){
        return boardManager;
    }
    
    public List<GameObject> GetHighlightedFields(){
        return highlightedFields;
    }

    void OnMouseDown()
    {
        if (currentlyLiftedPiece == null || currentlyLiftedPiece == this){
            if (gameManager.GetTurn() == WhiteOrRed){
                Tuple<List<Vector2Int>, bool> checker = gameManager.CheckIfJumpPossible(boardManager.GetColors(), gameManager.GetTurn());
                IfJumpPossible = checker.Item2;
                possibleJumpers = checker.Item1;
                if (IfJumpPossible){
                    if (possibleJumpers.Contains(currentPosition)){
                        SelectPiece();
                    }
                    else{
                        gameManager.DisplayWarning("Musisz zbijać innym pionem");
                    }
                }
                else{
                    SelectPiece();
                }
            }
            else{
                gameManager.DisplayWarning("To nie twoja tura");
            }
        }
            
    }

    public void SelectPiece()
    {
        Debug.Log("Pionek został wybrany: " + currentPosition.x + " " + currentPosition.y);
        if (!isLifted){
            StartCoroutine(LiftPiece());
            HighlightPossibleMoves();
            isLifted = true;
            currentlyLiftedPiece = this;
        }
        else{
            StartCoroutine(DropPiece());
            UnHighlightPossibleMoves();
            isLifted = false;
            currentlyLiftedPiece = null;
        }
    }

    private IEnumerator LiftPiece(){
        Vector3 originalPosition = transform.position;
        Vector3 targetPosition = originalPosition + new Vector3(0, 0.5f, 0);

        float timeToLift = 0.3f; 
        float elapsedTime = 0;

        while (elapsedTime < timeToLift)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / timeToLift);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetPosition.y = (float)1.1;
        transform.position = targetPosition; 
    }

    private IEnumerator DropPiece(){
        Vector3 originalPosition = transform.position;
        Vector3 targetPosition = originalPosition - new Vector3(0, 0.5f, 0);

        float timeToLift = 0.3f; 
        float elapsedTime = 0;

        while (elapsedTime < timeToLift)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / timeToLift);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetPosition.y = (float)0.6;
        transform.position = targetPosition; 
    }

    private void HighlightPossibleMoves()
    {
        Vector2Int position = currentPosition;
        Tuple<List<Vector2Int>, bool> t;
        if (isQueen){
            t = boardManager.GetPossiblleMovesForQueen(position, WhiteOrRed);
        }
        else{
            t = boardManager.GetPossibleMoves(position, WhiteOrRed);
        }
        List<Vector2Int> possibleMoves= t.Item1;
        possibleMoves.Add(position);
        bool JumpOrNot = t.Item2;

        foreach (var move in possibleMoves){
            GameObject targetField = boardManager.GetFieldObject(move.x, move.y);
             if (targetField != null)
            {
                targetField.GetComponent<BoardField>().Highlight();
                highlightedFields.Add(targetField);
            }
        }
    }

    public void UnHighlightPossibleMoves()
    {
        foreach (var highlightedField in highlightedFields)
        {
            if (highlightedField != null)
            {
                highlightedField.GetComponent<BoardField>().UnHighlight();
            }
        }
        highlightedFields.Clear();
    }
    
    private Vector2Int PositionOfJumpedPiece(Vector2Int origin, Vector2Int target){
        int[,] c = boardManager.GetColors();
        int directionX = target.x > origin.x ? 1 : -1;
        int directionY = target.y > origin.y ? 1 : -1;

        int i = origin.x + directionX;
        int j = origin.y + directionY;

        while (i != target.x && j != target.y)
        {
            if (c[i, j] == Convert.ToInt32(!WhiteOrRed))
            {
                return new Vector2Int(i, j);
            }
            i += directionX;
            j += directionY;
        }
        return new Vector2Int(-1, -1);
    }

    // JumpMore zwraca pozycje pionka jeśli może on skoczyć
    private Vector2Int JumpMore(){ 
        Tuple< List<Vector2Int>, bool> canjump = gameManager.CheckIfJumpPossible(boardManager.GetColors(), gameManager.GetTurn());
        IfJumpPossible = canjump.Item2;
        possibleJumpers = canjump.Item1;
        if (IfJumpPossible){
            if (possibleJumpers.Contains(currentPosition)){
                return currentPosition;
            }
            else{
                return new Vector2Int(-1, -1);
            }
        }
        else{
            return new Vector2Int(-1, -1);
        }
    }
    public void MoveTo(Vector2Int newPosition)
    {
        if (boardManager != null)
        {
            if (IfJumpPossible && currentPosition != newPosition)
            {
                Vector2Int jumpedPiecePosition = PositionOfJumpedPiece(currentPosition, newPosition);
                Debug.Log($"Pozycja zbijanego piona to {jumpedPiecePosition.x} i {jumpedPiecePosition.y}");
                boardManager.SetColorField(jumpedPiecePosition.x, jumpedPiecePosition.y, -1);
                Piece ToRemove = boardManager.GetPieceAtPosition(jumpedPiecePosition.x, jumpedPiecePosition.y);
                if (ToRemove != null)
                {
                    boardManager.RemovePiece(ToRemove);
                }

                boardManager.SetColorField(currentPosition.x, currentPosition.y, -1);
                boardManager.SetColorField(newPosition.x, newPosition.y, Convert.ToInt32(WhiteOrRed));

                GameObject targetField = boardManager.GetFieldObject(newPosition.x, newPosition.y);
                if (targetField != null)
                {
                    Vector3 targetPosition = targetField.transform.position;
                    targetPosition.y = transform.position.y;
                    transform.position = targetPosition;
                }

                currentPosition = newPosition;

                Renderer thisRenderer = GetComponent<Renderer>();
                if (currentPosition.x == 0 && WhiteOrRed == false) 
                {
                    Debug.Log("Czerwony pionek doszedł na poziom 0 i staje się damką.");
                    isQueen = true;
                    thisRenderer.material.color = Color.magenta;
                }
                else if (currentPosition.x == 7 && WhiteOrRed == true) 
                {
                    Debug.Log("Biały pionek doszedł na poziom 7 i staje się damką.");
                    isQueen = true;
                    thisRenderer.material.color = Color.green;
                }

                Vector2Int nextJump = JumpMore();
                if (nextJump != new Vector2Int(-1, -1)) 
                {
                    MoveTo(nextJump); 
                }
                else
                {
                    
                    gameManager.SetTurn(!gameManager.GetTurn());
                }

                gameManager.UpdateTurnIndicator();
            }
            else{
                boardManager.SetColorField(currentPosition.x, currentPosition.y, -1);
                boardManager.SetColorField(newPosition.x, newPosition.y, Convert.ToInt32(WhiteOrRed));

                GameObject targetField = boardManager.GetFieldObject(newPosition.x, newPosition.y);
                if (targetField != null)
                {
                    Vector3 targetPosition = targetField.transform.position;
                    targetPosition.y = transform.position.y;
                    transform.position = targetPosition;
                }

                if (currentPosition.x != newPosition.x || currentPosition.y != newPosition.y){
                    gameManager.SetTurn(!gameManager.GetTurn());
                }

                gameManager.UpdateTurnIndicator();

                currentPosition = newPosition;

                Renderer thisRenderer = GetComponent<Renderer>();
                if (currentPosition.x == 0 && WhiteOrRed == false) 
                {
                    Debug.Log("Czerwony pionek doszedł na poziom 0 i staje się damką.");
                    isQueen = true;
                    thisRenderer.material.color = Color.magenta;
                }
                else if (currentPosition.x == 7 && WhiteOrRed == true) 
                {
                    Debug.Log("Biały pionek doszedł na poziom 7 i staje się damką.");
                    isQueen = true;
                    thisRenderer.material.color = Color.green;
                }

            }
            gameManager.CheckForWin();
        }
    }

    void Start()
    {
        boardManager = FindAnyObjectByType<BoardManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        currentPosition = boardManager.GetFieldPosition(currentBoardField);
        boardManager.SetColorField(currentPosition.x, currentPosition.y, Convert.ToInt32(WhiteOrRed));
        Tuple< List<Vector2Int>, bool> checker = gameManager.CheckIfJumpPossible(boardManager.GetColors(), gameManager.GetTurn());
        IfJumpPossible = checker.Item2;
        if (IfJumpPossible){
            possibleJumpers = checker.Item1;
        }
    }
}