using UnityEngine;

/// <summary>
/// 그림을 그리는 화면을 전환하는 클래스
/// </summary>

public class DrawingView : MonoBehaviour, IView
{
    [SerializeField] private Drawable _drawable;
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        _drawable.SetNewDrawing(show);
    }
}
