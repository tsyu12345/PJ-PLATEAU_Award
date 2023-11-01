using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


enum HTTPMethod {
    GET,
    POST,
    PUT,
    DELETE
}

/***
* UnityWebRequestを使用し、サーバーから色々な情報を取得するクラス（Sample）
*/
public class Client : MonoBehaviour {

    [Header("Server Info")]
    public string baseURL = "http://0.0.0.0:8000";
    public string step_endpoint = "/step_data";
    public string userid = "TESTUSER1";

    public int stepCount = 0;

    public delegate void OnStepsReceivedDelegate(int steps);

    [System.Serializable]
    public class APIResponse {
        public int steps;
        public string userid;
        public string error;
    }


    private float timeSinceLastRequest = 0f;
    private float requestInterval = 0.5f;
    

    void Start() {
        //Nope
    }

    void Update() {
        timeSinceLastRequest += Time.deltaTime;

        if (timeSinceLastRequest >= requestInterval) {
            StartCoroutine(GetSteps(OnStepsReceived));
            timeSinceLastRequest = 0f;
        }
    }


    public IEnumerator GetSteps(OnStepsReceivedDelegate callback) {
        string url = baseURL + step_endpoint;
        string query = "?userid=" + userid;
        url += query;
        Debug.Log("API Request URL: " + url);
        using(var request = UnityWebRequest.Get(url)) {

            yield return request.SendWebRequest();
            
            if(request.isHttpError || request.isNetworkError) {
                Debug.Log(request.error);
            } else {
                Debug.Log(request.downloadHandler.text);
                // JSONのデシリアライズ
                APIResponse response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                callback?.Invoke(response.steps); //stepsを返す
            }
        }         
    }

    // コールバック関数
    void OnStepsReceived(int steps) {
        Debug.Log("Received steps: " + steps);
        //オブジェクトを歩数分だけZ軸方向に移動させる
        //現在の位置＋歩数分だけ移動させる
        Vector3 pos = transform.position;
        pos.z += steps;
        transform.position = pos;
    }



}
