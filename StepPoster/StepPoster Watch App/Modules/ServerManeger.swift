//
//  ServerManeger.swift
//  StepPoster Watch App
//
//  Created by 高林秀 on 2023/10/31.
//
import Foundation


enum HTTPMethod: String {
    case get = "GET"
    case post = "POST"
    case put = "PUT"
    case delete = "DELETE"
}

/**
 * WatchOSとWebサーバーを連携するクラス
 */
class ServerManeger {
    
    var ipAdress: String
    
    /**
     * Parameter ipAdress: サーバーのIPアドレス
     */
    init(ipAdress: String) {
        self.ipAdress = ipAdress
    }
    
    
    
    private func requestToServer(method: HTTPMethod, endpoint: String,params: [String: Any], completion: @escaping (Result<Data, Error>) -> Void) {
        
        guard let url = URL(string: "\(ipAdress)\(endpoint)") else {
            completion(.failure(NSError(domain: "Invalid URL", code: 400, userInfo: nil)))
            return
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = method.rawValue
        
        
        if method == .post || method == .put { // POSTやPUTの場合にパラメータをボディに設定
            do {
                request.httpBody = try JSONSerialization.data(withJSONObject: params, options: [])
            } catch {
                completion(.failure(error))
                return
            }
        }
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                completion(.failure(error))
                return
            }
            
            if let data = data {
                completion(.success(data))
            }
        }
        task.resume()
    }
        
    
    public func sendSteps(steps: Int) {
        
    }
    
}

