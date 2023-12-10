using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class TitleScene : MonoBehaviour {

    public float RotateSpeed = 10.0f;
    public Button startButton;
    private static Camera mainCamera;

    // ロードするシーンの名前
    public string sceneName;

    // ロードの進捗状況を表示するUIなど
    public GameObject loadingUI;

    // ロードの進捗状況を管理するための変数
    private AsyncOperation async;

    // ロードを開始するメソッド
    public void StartLoad() {
        StartCoroutine(Load());
    }

    void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        startButton.onClick.AddListener(moveLobbyScene);
        loadingUI.SetActive(false);
    }

    
    void Update() {
        //カメラを1秒間にRotateSpeed分だけ回転する
        mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, RotateSpeed * Time.deltaTime);
    }

    private void moveLobbyScene() {
        Debug.Log("MoveLobbyScene");
        StartCoroutine(Load());
    }

    // コルーチンを使用してロードを実行するメソッド
    private IEnumerator Load() {
        // ロード画面を表示する
        loadingUI.SetActive(true);

        // シーンを非同期でロードする
        async = SceneManager.LoadSceneAsync(sceneName);

        // ロードが完了するまで待機する
        while (!async.isDone) {
            yield return null;
        }

        // ロード画面を非表示にする
        loadingUI.SetActive(false);
    }
}
