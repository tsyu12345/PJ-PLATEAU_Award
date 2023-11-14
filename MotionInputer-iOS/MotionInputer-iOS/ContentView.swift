// ContentView.swift
// MotionInputer-iOS
//
// Created by 高林秀 on 2023/11/14.

import SwiftUI
import CoreMotion // CoreMotionをインポートする必要があります

struct ContentView: View {
    @State private var motionStrength = 0.0
    private var motionDetector = MotionDetector()

    var body: some View {
        VStack {
            Text("Motion Strength: \(motionStrength)")
            Button("Start Monitoring") {
                startMonitoring()
            }
            Button("Stop Monitoring") {
                stopMonitoring()
            }
        }
        .padding()
    }

    private func startMonitoring() {
        motionDetector.motionDetected = { strength in
            DispatchQueue.main.async {
                self.motionStrength = strength
            }
        }
        motionDetector.startMonitoring()
    }

    private func stopMonitoring() {
        motionDetector.stopMonitoring()
    }
    
    private func pushDataForServer(String userId, Float strength) {
        
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
