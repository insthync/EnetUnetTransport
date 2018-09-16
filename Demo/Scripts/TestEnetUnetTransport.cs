using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TestEnetUnetTransport : MonoBehaviour
{
    public static TestEnetUnetTransport Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            Debug.Log("Setup transport to custom transport");
            //NetworkManager.activeTransport = new TestLLAPITransport();
            NetworkManager.activeTransport = new EnetUnetTransport();
            SceneManager.LoadScene("DemoLiteNetLibUnetTransport");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
