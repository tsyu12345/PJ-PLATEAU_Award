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
* UnityWebRequestを使用し、サーバーから色々な情報を取得するクラス
*/
public class Client : MonoBehaviour {

    [Header("Server Info")]
    public string baseURL = "http://localhost:8080/";
    public string step_endpoint = "/step_data";
    public string userid = "TESTUSER1";
    

    void Start() {
        //Nope
    }

    void Update() {
        //Nope
    }


    public int GetSteps() {
        string url = baseURL + step_endpoint;
        
        using(var request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            if(request.isHttpError || request.isNetworkError) {
                Debug.Log(request.error);
            } else {
                Debug.Log(request.downloadHandler.text);
                //todo:レスポンスからステップ数を取得
            }
        }         
    }

}
