using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[] allfields;
    public List<GameObject> allPieces;
    private GameManager gameManager;
    private int[,] colors = new int[8,8]; // 1 = biały, 0 = czerwony, -1 = brak pionka

    private Dictionary<Vector2Int, GameObject> boardFields = new Dictionary<Vector2Int, GameObject>();

    public Piece GetPieceAtPosition(int x, int y)
    {
        foreach (GameObject piece in allPieces)
        {
            Piece pieceComponent = piece.GetComponent<Piece>();
            if (pieceComponent != null && pieceComponent.GetCurrentPos().x == x && pieceComponent.GetCurrentPos().y == y)
            {
                return pieceComponent;
            }
        }
        return null;
    }

    public void RemovePiece(Piece piece){
        allPieces.Remove(piece.gameObject);
        Destroy(piece.gameObject);
    }
    public int[,] GetColors(){
        return colors;
    }

    public void SetColorField(int x, int y, int col){
        if (x >= 0 && x < 8 && y >= 0 && y < 8 && ((col == 1) || (col == 0) || (col == -1))){
            colors[x, y] = col;
        }
    }
    public void InitializeBoard(GameObject[] allFields)
    {
        int index = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject field = allFields[index];
                Vector2Int position = new Vector2Int(x, y);
                
                boardFields[position] = field;

                if (x < 3 && ((x%2 == 0 && y%2 == 1) || (x%2 == 1 && y%2 == 0))){
                    colors[x, y] = 1;
                }
                else if (x > 4 && ((x%2 == 1 && y%2 == 0) || (x%2 == 0 && y%2 == 1))){
                    colors[x, y] = 0;
                }
                else{
                    colors[x, y] = -1;
                }

                index++;
            }
        }
    }
    public bool IsFieldOccupied(int x, int y)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            return colors[x, y] != -1;
        }
        return false;
    }
    public GameObject GetFieldObject(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (boardFields.ContainsKey(position))
        {
            return boardFields[position];
        }
        return null;
    }

    public Vector2Int GetFieldPosition(GameObject g){
        Vector2Int position = new Vector2Int();
        foreach (var pair in boardFields){
            if (pair.Value == g){
                position = pair.Key;
                break;
            }
        }
        return position;
    }

    private bool IsValidPosition(Vector2Int position){
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }

    // możliwe ruchy plus informacja czy to ruch zbijający czy nie
    public Tuple<List<Vector2Int>, bool> GetPossibleMoves(Vector2Int position, bool WhiteOrRed)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();
        List<Vector2Int> onlyJumps = new List<Vector2Int>();
        Vector2Int[] directions;
        int x = position.x;
        int y = position.y;

        Vector2Int[] JumpDirections = new Vector2Int[]{
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1)};
        
        if (WhiteOrRed){
            directions = new Vector2Int[]{
                new Vector2Int(1, -1),
                new Vector2Int(1, 1)};
        }
        else{
            directions = new Vector2Int[]{
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 1)};
        }
        
        foreach (var direction in directions){
            Vector2Int newPos = new Vector2Int(x + direction.x, y + direction.y);
            if (IsValidPosition(newPos) && !IsFieldOccupied(newPos.x, newPos.y)){
                possibleMoves.Add(newPos);
            }
        }

        foreach (var direction in JumpDirections){
            Vector2Int newPos = new Vector2Int(x + direction.x, y + direction.y);
            if (IsValidPosition(newPos) &&
                IsFieldOccupied(newPos.x, newPos.y) && 
                colors[newPos.x, newPos.y] != Convert.ToInt32(WhiteOrRed)){  // jeżeli na ukos jest pionek przeciwnika

                Vector2Int jumpPos = new Vector2Int(x + 2*direction.x, y + 2*direction.y);
                if (IsValidPosition(jumpPos) && !IsFieldOccupied(jumpPos.x, jumpPos.y)){
                    onlyJumps.Add(jumpPos);
                }
            }
        }
        
        if (onlyJumps.Count > 0){
            return new Tuple<List<Vector2Int>, bool>(onlyJumps, true);
        }
        else{
            return new Tuple<List<Vector2Int>, bool>(possibleMoves, false);
        }
    }

    public Vector2Int DirectionHelper(int i){
        if (i == 0){
            return new Vector2Int(-1, -1);
        }
        else if (i == 1){
            return new Vector2Int(-1, 1);
        }
        else if(i == 2){
            return new Vector2Int(1, -1);
        }
        else{
            return new Vector2Int(1, 1);
        }
    }

    public Tuple<List<Vector2Int>, bool> GetPossiblleMovesForQueen(Vector2Int position, bool WhiteOrRed){
        List<Vector2Int> possibleMoves = new List<Vector2Int>();
        List<Vector2Int> onlyJumps = new List<Vector2Int>();
        possibleMoves.Add(new Vector2Int(position.x, position.y));

        for (int i = 0; i < 4; i++){ // 4 kierunki w które może iść
            int counter = 0; // jezeli dojdzie do 2 to znaczy ze damka napotkala na jednej z dróg drugiego piona wiec dalej nie pojdzie
            Vector2Int direction = DirectionHelper(i);
            int x = position.x + direction.x;
            int y = position.y + direction.y;

            while (counter < 2 && 0 <= x && x < 8 && 0 <= y && y < 8){
                if (colors[x, y] == -1){ // puste pole wiec moze tam isc
                    if (counter == 1){ // jak juz byl przeskoczony to dodawaj do przeskoczonych
                        onlyJumps.Add(new Vector2Int(x, y));
                    }
                    else{
                        possibleMoves.Add(new Vector2Int(x, y));
                    }
                    x += direction.x;
                    y += direction.y;
                }
                else if(colors[x, y] == Convert.ToInt32(!WhiteOrRed)){ // jest na nim pionek przeciwnika
                    counter++;
                    if (0 <= x + direction.x && x + direction.x < 8 && 0 <= y + direction.y && y + direction.y < 8){ // kolejne miesci sie w planszy
                        if (counter < 2 && colors[x + direction.x, y + direction.y] == -1){ // to nie był drugi pionek a po nim jest wolne
                            onlyJumps.Add(new Vector2Int(x + direction.x, y + direction.y));
                            x += 2*direction.x;
                            y += 2*direction.y;
                        }
                        else{
                            break;
                        }
                    }
                    else{
                        break;
                    }
                }
                else{ // jest na nim twój pionek
                    break;
                }
            }
        }

        if (onlyJumps.Count > 0){
            return new Tuple<List<Vector2Int>, bool>(onlyJumps, true);
        }
        else{
            return new Tuple<List<Vector2Int>, bool>(possibleMoves, false);
        }
    }

    void Awake()
    {
        gameManager = GetComponent<GameManager>();
        InitializeBoard(allfields);
        allPieces = new List<GameObject>(GameObject.FindObjectsByType<Piece>(FindObjectsSortMode.None)
                                      .Select(piece => piece.gameObject));
        
    }
}
