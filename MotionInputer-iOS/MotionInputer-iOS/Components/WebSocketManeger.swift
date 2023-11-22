import Foundation

class WebSocketManager: ObservableObject {
    private var webSocketTask: URLSessionWebSocketTask?
    private var baseURL: URL
    private let session: URLSession

    init(baseURL: URL = URL(string: "ws://example.com")!) {
        self.baseURL = baseURL
        self.session = URLSession(configuration: .default)
    }

    private func createWebSocketTask(endpoint: String) {
        let fullURL = baseURL.appendingPathComponent(endpoint)
        webSocketTask = session.webSocketTask(with: fullURL)
        webSocketTask?.resume()
    }
    
    func setBaseURL(_ url: URL) {
            self.baseURL = url
    }

    func connect(endpoint: String) {
        createWebSocketTask(endpoint: endpoint)
    }

    func disconnect() {
        webSocketTask?.cancel(with: .goingAway, reason: nil)
    }

    func request(endpoint: String, data: [String: Any]) {
        //connect(endpoint: endpoint)

        guard let jsonData = try? JSONSerialization.data(withJSONObject: data, options: []) else { return }
        
        do {
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                webSocketTask?.send(.string(jsonString)){ error in
                    if let error = error {
                        print("WebSocket sending error: \(error)")
                    }
                }
            }
        } catch {
            print("Error serializing JSON: \(error)")
        }
        
    }
}
