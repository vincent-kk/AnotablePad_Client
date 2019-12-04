

using UnityEngine;

public class DrawingView : MonoBehaviour, IView
{
    [SerializeField] private Drawable _drawable;
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        _drawable.SetNewDrawing(show);
    }
}
