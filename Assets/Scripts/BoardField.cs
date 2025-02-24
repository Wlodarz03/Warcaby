using Unity.VisualScripting;
using UnityEngine;

public class BoardField : MonoBehaviour
{   
    private Renderer fieldRenderer;
    private Color originalColor;

    void Start()
    {
        fieldRenderer = GetComponent<Renderer>();
        originalColor = fieldRenderer.material.color;
    }

    void OnMouseDown()
    {
        Piece currentlyLiftedPiece = Piece.GetCurrentLiftedPiece();
        
        if (currentlyLiftedPiece != null)
        {
            Vector2Int fieldPosition = currentlyLiftedPiece.GetBoardManager().GetFieldPosition(gameObject);

            if (currentlyLiftedPiece.GetHighlightedFields().Contains(gameObject))
            {
                currentlyLiftedPiece.MoveTo(fieldPosition);
                currentlyLiftedPiece.SelectPiece();
            }
        }
    }

    public void Highlight()
    {
        fieldRenderer.material.color = Color.blue;
    }

    public void UnHighlight()
    {
        fieldRenderer.material.color = originalColor;
    }
}
