using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    public void LocalGameBtn()
    {

    }

    public void OnlineGameBtn()
    {

    }

}
