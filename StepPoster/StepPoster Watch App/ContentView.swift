//
//  ContentView.swift
//  StepPoster Watch App
//
//  Created by 高林秀 on 2023/10/24.
//

import SwiftUI
import HealthKit

let healthStore = HKHealthStore()
var webSocketTask: URLSessionWebSocketTask? = nil

struct ContentView: View {
    @State private var steps: Int = 0
    let timer = Timer.publish(every: 1, on: .main, in: .common).autoconnect()
    
    var body: some View {
        VStack {
            Text("Steps: \(steps)")
        }
        .onAppear {
            requestHealthKitAuthorization()
        }
        .onReceive(timer) { _ in
            fetchStepsData { s in
                steps = s
                sendDataToServer(steps: steps)
            }
        }
    }

    func requestHealthKitAuthorization() {
        let stepType = HKObjectType.quantityType(forIdentifier: .stepCount)!
        healthStore.requestAuthorization(toShare: [], read: [stepType]) { (success, error) in
            if !success {
                // Handle error
            }
        }
    }

    func fetchStepsData(completion: @escaping (Int) -> Void) {
        let stepsType = HKQuantityType.quantityType(forIdentifier: .stepCount)!
        let now = Date()
        let startOfDay = Calendar.current.startOfDay(for: now)
        let predicate = HKQuery.predicateForSamples(withStart: startOfDay, end: now, options: .strictStartDate)

        let stepsQuery = HKStatisticsQuery(quantityType: stepsType, quantitySamplePredicate: predicate, options: .cumulativeSum) { _, result, error in
            let sum = result?.sumQuantity()?.doubleValue(for: HKUnit.count()) ?? 0
            completion(Int(sum))
        }

        healthStore.execute(stepsQuery)
    }

    func sendDataToServer(steps: Int) {
        let url = URL(string: "ws://127.0.0.1:8000/ws_post_step")!
        
        // WebSocketの接続を確立
        if webSocketTask == nil {
            webSocketTask = URLSession.shared.webSocketTask(with: url)
            webSocketTask?.resume()
        }
        
        let parameters: [String: Any] = ["steps": steps]
        do {
            let jsonData = try JSONSerialization.data(withJSONObject: parameters)
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                // データをWebSocketを使用して送信
                webSocketTask?.send(.string(jsonString)) { error in
                    if let error = error {
                        print("Error sending data: \(error)")
                    }
                }
            }
        } catch {
            print("Error serializing JSON: \(error)")
        }
    }
}


struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
