using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class TitleScene : MonoBehaviour {

    public float RotateSpeed = 10.0f;
    public Button startButton;
    private static Camera mainCamera;

    void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        startButton.onClick.AddListener(moveLobbyScene);
    }

    
    void Update() {
        //カメラを1秒間にRotateSpeed分だけ回転する
        mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, RotateSpeed * Time.deltaTime);
    }

    private void moveLobbyScene() {
        Debug.Log("MoveLobbyScene");
        //SceneManager.LoadScene("LobbyScene");
    }
}
