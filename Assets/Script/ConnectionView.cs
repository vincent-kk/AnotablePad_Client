using UnityEngine;

public class ConnectionView : MonoBehaviour, IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}
