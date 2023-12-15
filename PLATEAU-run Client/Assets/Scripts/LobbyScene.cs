using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;


public class LobbyScene : MonoBehaviour {
    [Header("Buttons")]
    public Button connectButton;
    public Button onlineButton;
    public Button practiceButton;
    [Header("UI Windows")]
    public GameObject connectUI;
    public GameObject courseSelectUI;
    public GameObject loadingsUI;
    public static bool isPractice = false;
    public static AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        connectButton.onClick.AddListener(openConnectUI);
        onlineButton.onClick.AddListener(()=>{
            isPractice = false;
            DontDestroyOnLoad(this.gameObject);
            openCourseSelectUI();

        });
        practiceButton.onClick.AddListener(()=>{
            isPractice = true;
            DontDestroyOnLoad(this.gameObject);
            openCourseSelectUI();
        });

        connectUI.SetActive(false);
        courseSelectUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        loadingsUI.SetActive(false);
    }




    /// <summary>
    /// デバイス接続の方法を表示するモーダルUIを表示する 
    /// </summary>
    private void openConnectUI() {
        connectUI.SetActive(true);
    }

    /// <summary>
    /// コース選択UIを表示する
    /// </summary>
    private void openCourseSelectUI() {
        courseSelectUI.SetActive(true);
    }
}
