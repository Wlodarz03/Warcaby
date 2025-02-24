using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private BoardManager boardManager;
    private bool turn = true; // 1 - tura białego, 0 - tura czerwonego
    public TextMeshProUGUI turnIndicator;
    public TextMeshProUGUI warning;
    public bool GetTurn(){
        return turn;
    }
    public void SetTurn(bool b){
        turn = b;
    }

    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warning.text = "";
    }

    public void DisplayWarning(string s){
        warning.text = s;
        StartCoroutine(ClearWarningAfterDelay(1.5f));
    }
    public void UpdateTurnIndicator()
    {
        if (turn)
        {
            turnIndicator.text = "Tura Białego";
        }
        else
        {
            turnIndicator.text = "Tura Czerwonego";
        }
    }

    // Sprawdza czy możliwe zbicie przez gracza o turze turn i jeśli tak to zwraca wszystkie pionki które mogą zbić
    public Tuple<List<Vector2Int>, bool> CheckIfJumpPossible(int[,] colors, bool turn){
        List<Vector2Int> jumps = new List<Vector2Int>();
        for (int x = 0; x < 8; x++){
            for (int y = 0; y < 8; y++){
                Vector2Int currentPos = new Vector2Int(x, y);
                if (colors[x, y] == Convert.ToInt32(turn)){
                    Piece p = boardManager.GetPieceAtPosition(x, y);
                    if (p!= null && p.GetIsQueen()){
                            for (int i = 0; i < 4; i++){
                                Vector2Int d = boardManager.DirectionHelper(i);
                                int x2 = x + d.x;
                                int y2 = y + d.y;
                                while (0 <= x2 && x2 < 8 && 0 <= y2 && y2 < 8){
                                    if (colors[x2, y2] == -1){
                                        x2 += d.x;
                                        y2 += d.y;
                                    }
                                    else if(colors[x2, y2] == Convert.ToInt32(!turn)){ // pion przeciwnika
                                        if (x2 + d.x >= 0 && x2 + d.x < 8 && y2 + d.y >= 0 && y2 + d.y < 8){
                                            if (colors[x2 + d.x, y2 + d.y] == -1){
                                                jumps.Add(currentPos); // ta damka moze zbic
                                                break;
                                            }
                                            break;
                                        }
                                        break;
                                    }
                                    else{
                                        break;
                                    }
                                }
                            }
                    }
                    else{
                        if (x+2 < 8 && y+2 < 8){
                            if (colors[x+1, y+1] == Convert.ToInt32(!turn) && colors[x+2, y+2] == -1){
                                jumps.Add(currentPos);
                            }
                        }
                        if (x+2 < 8 && y-2 >= 0){
                            if (colors[x+1, y-1] == Convert.ToInt32(!turn) && colors[x+2, y-2] == -1){
                                if (!jumps.Contains(currentPos)) jumps.Add(currentPos);
                            }
                        }
                        if (x-2 >= 0 && y+2 < 8){
                            if (colors[x-1, y+1] == Convert.ToInt32(!turn) && colors[x-2, y+2] == -1){
                                if (!jumps.Contains(currentPos)) jumps.Add(currentPos);
                            }
                        }
                        if (x-2 >= 0 && y-2 >= 0){
                            if (colors[x-1, y-1] == Convert.ToInt32(!turn) && colors[x-2, y-2] == -1){
                                if (!jumps.Contains(currentPos)) jumps.Add(currentPos);
                            }
                        }
                    }
                }
            }
        }
        bool wyn = jumps.Count > 0;
        return new Tuple<List<Vector2Int>, bool>(jumps, wyn);
    }

    private bool CheckIfSomeoneWin(int [,] colors, bool WhiteOrRed){
        for (int x = 0; x < 8; x++){
            for (int y = 0; y < 8; y++){
                if (colors[x, y] == Convert.ToInt32(WhiteOrRed)){
                    return false;
                }
            }
        }
        return true;
    }

    public void CheckForWin(){
        bool redWon = CheckIfSomeoneWin(boardManager.GetColors(), true); 
        bool whiteWon = CheckIfSomeoneWin(boardManager.GetColors(), false);


        if (whiteWon)
        {
            Debug.Log("Koniec gry! Białe wygrywają!");
            string s = "Wygrały białe!";
            PlayerPrefs.SetString("PassedData", s);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        if (redWon)
        {
            Debug.Log("Koniec gry! Czerwone wygrywają!");
            string s = "Wygrały czerwone!";
            PlayerPrefs.SetString("PassedData", s);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void Start()
    {
        boardManager = FindAnyObjectByType<BoardManager>();
        UpdateTurnIndicator();
    }

    void Update()
    {
        
    }
}
