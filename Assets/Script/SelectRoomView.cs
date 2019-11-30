using UnityEngine;

public class SelectRoomView : MonoBehaviour, IView
{
    [SerializeField] private ApplicationManager _applicationManager;
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        if(show)  _applicationManager.GetRoomList();
    }
}
