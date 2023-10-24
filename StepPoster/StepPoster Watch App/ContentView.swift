//
//  ContentView.swift
//  StepPoster Watch App
//
//  Created by 高林秀 on 2023/10/24.
//

import SwiftUI
import HealthKit

let healthStore = HKHealthStore()

struct ContentView: View {
    @State private var steps: Int = 0
    let timer = Timer.publish(every: 10, on: .main, in: .common).autoconnect() // 10秒ごとにデータを取得・送信

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
        let url = URL(string: "http://0.0.0.0:8000/receive_data")!
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        let parameters: [String: Any] = ["steps": steps]
        request.httpBody = try? JSONSerialization.data(withJSONObject: parameters)
        print("Conecting your seaver \(url)")
        print("send data is :\(parameters)" )
        URLSession.shared.dataTask(with: request) {
            (data, response, error) in
            if let error = error {
                print("Error sending data: \(error)")
            }
        }.resume()
    }
}


struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
