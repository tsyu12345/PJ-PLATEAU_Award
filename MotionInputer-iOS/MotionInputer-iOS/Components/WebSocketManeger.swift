import Foundation

class WebSocketManager: ObservableObject {
    private var webSocketTask: URLSessionWebSocketTask?
    private var baseURL: URL
    private let session: URLSession
    var onError: ((Error) -> Void)?

    init(baseURL: URL = URL(string: "ws://example.com")!) {
        self.baseURL = baseURL
        self.session = URLSession(configuration: .default)
    }

    private func createWebSocketTask(endpoint: String) {
        let fullURL = baseURL.appendingPathComponent(endpoint)
        webSocketTask = session.webSocketTask(with: fullURL)
        webSocketTask?.resume()
        listenForMessages()
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

    private func listenForMessages() {
        webSocketTask?.receive { [weak self] result in
            switch result {
            case .failure(let error):
                self?.onError?(error)
            case .success(let message):
                switch message {
                case .string(let text):
                    print("Received string: \(text)")
                case .data(let data):
                    print("Received data: \(data)")
                @unknown default:
                    fatalError()
                }
                self?.listenForMessages()
            }
        }
    }

    func request(endpoint: String, data: [String: Any]) {
        guard let jsonData = try? JSONSerialization.data(withJSONObject: data, options: []) else { return }
        
        do {
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                webSocketTask?.send(.string(jsonString)){ [weak self] error in
                    if let error = error {
                        self?.onError?(error)
                    }
                }
            }
        } catch {
            print("Error serializing JSON: \(error)")
        }
    }
}
