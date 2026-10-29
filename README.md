# Blink

**A First-Person Psychological Horror Puzzle Prototype Built with Unity**

In the Blink is a solo game jam prototype exploring a perception-driven horror experience where players switch between reality and an alternate dimension to uncover hidden clues and solve environmental puzzles.

Set in a hospital environment, the game combines first-person exploration, psychological horror, and a webcam-based blink detection system. Instead of relying exclusively on conventional input, the prototype uses the player's physical blinks as gameplay events, connecting real-world interaction with in-game dimension switching.

## Table of Contents

- [Overview](#overview)
- [Core Gameplay](#core-gameplay)
- [System Architecture](#system-architecture)
- [Blink Detection](#blink-detection)
- [Python-to-Unity Communication](#python-to-unity-communication)
- [Dimension Switching](#dimension-switching)
- [Puzzle Design](#puzzle-design)
- [Technical Considerations](#technical-considerations)
- [Development Constraints](#development-constraints)
- [Future Improvements](#future-improvements)

## Overview

| Property | Details |
|---|---|
| Engine | Unity |
| Language | Python for blink detection; C# for Unity gameplay |
| Genre | Psychological Horror, Puzzle, Exploration |
| Perspective | First-person |
| Core Mechanic | Blink-triggered dimension switching |
| Computer Vision | OpenCV and MediaPipe Face Mesh |
| Blink Detection | Eye Aspect Ratio (EAR) |
| Communication | UDP over localhost |
| Environment | Hospital |
| Project Type | Solo game jam prototype |

## Core Gameplay

The experience is built around a perception-based gameplay loop:

1. **Explore:** Navigate the hospital and inspect the environment.
2. **Blink:** Use physical blinking as an input to interact with the central mechanic.
3. **Shift dimensions:** Reveal an alternate version of the environment.
4. **Investigate:** Discover differences, hidden clues, symbols, warnings, and environmental changes.
5. **Solve:** Combine information across dimensions to progress through the environment.

The central design idea is that the player's perception determines what information is available. Blinking becomes more than a visual effect: it connects a physical action to the game's exploration and puzzle-solving mechanics.

## System Architecture

The prototype separates webcam-based blink detection from the Unity gameplay system. Python processes the camera feed and identifies blink events, while Unity receives those events and handles the corresponding gameplay response.

```text
                 PLAYER
                   |
                   v
             Physical Blink
                   |
                   v
       +------------------------+
       | Python Detection Layer |
       |                        |
       | OpenCV Webcam Capture  |
       | MediaPipe Face Mesh    |
       | Eye Landmark Extraction|
       | EAR Calculation        |
       | Blink Threshold Check  |
       | Consecutive-Frame Check|
       +------------------------+
                   |
                   v
          UDP: "blink"
          127.0.0.1:5052
                   |
                   v
       +------------------------+
       |     Unity Runtime      |
       |                        |
       | UDP Event Reception    |
       | Blink Input Handling   |
       | Dimension State Change |
       +------------------------+
                   |
                   v
       Reality <-> Alternate
                   |
                   v
        Environmental Puzzles
```

The diagram describes the implemented Python detection and UDP transmission path, followed by the intended gameplay integration. The exact Unity receiver implementation depends on the project's C# scripts.

### System Responsibilities

**Python detection layer**
- Captures webcam frames and processes them using computer-vision libraries.
- Extracts facial landmarks using MediaPipe Face Mesh.
- Calculates eye openness using Eye Aspect Ratio.
- Validates a blink using a configurable threshold and consecutive-frame count.
- Sends a UDP datagram when a blink is detected.

**Unity gameplay layer**
- Receives the blink event through a UDP listener.
- Interprets the event as gameplay input.
- Applies the dimension-switching logic.
- Updates the environment according to the active dimension.

Keeping these responsibilities separate allows the detection algorithm to operate independently of the game's environment and puzzle logic.

## Blink Detection

The blink-detection system is implemented in Python using **OpenCV, MediaPipe Face Mesh, and Eye Aspect Ratio (EAR)**. It does not rely on a pre-trained model directly classifying each frame as a blink. Instead, it estimates eye openness from facial landmarks and applies threshold-based logic.

### 1. Webcam Capture

OpenCV initializes the webcam using `cv2.VideoCapture(0)` and continuously reads frames.

Each successfully captured frame is converted from OpenCV's default BGR color format to RGB before being passed to MediaPipe.

```python
ret, frame = cap.read()

if not ret:
    break

rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
results = face_mesh.process(rgb_frame)
```

This provides the live image input used by the detection pipeline.

### 2. Facial Landmark Extraction

MediaPipe Face Mesh processes each frame and returns facial landmarks when a face is detected. The implementation enables refined landmarks and limits processing to one face.

```python
mp_face_mesh = mp.solutions.face_mesh

face_mesh = mp_face_mesh.FaceMesh(
    refine_landmarks=True,
    max_num_faces=1
)
```

The normalized landmark coordinates are converted into pixel coordinates using the captured frame's width and height.

The script then selects six landmarks for each eye to estimate the distances between the upper and lower eyelids and the horizontal eye width.

### 3. Eye Aspect Ratio Calculation

The implementation calculates the Eye Aspect Ratio for each eye using six landmark points.

\[
EAR = \frac{d_1+d_2}{2d_h}
\]

Where:

- \(d_1\) and \(d_2\) represent vertical distances between eyelid landmarks.
- \(d_h\) represents the horizontal distance across the eye.

The calculation is implemented using Euclidean distance:

```python
def calc_EAR(landmarks, eye_points):
    v1 = dist(
        landmarks[eye_points[1]],
        landmarks[eye_points[5]]
    )
    v2 = dist(
        landmarks[eye_points[2]],
        landmarks[eye_points[4]]
    )
    h = dist(
        landmarks[eye_points[0]],
        landmarks[eye_points[3]]
    )

    return (v1 + v2) / (2.0 * h)
```

When the eyes close, the vertical distances decrease relative to the horizontal distance, causing EAR to fall. The script calculates EAR for both eyes and averages the results to reduce dependence on a single eye's measurement.

### 4. Blink Validation

The current implementation uses two configurable parameters:

| Parameter | Value | Purpose |
|---|---:|---|
| `blink_threshold` | `0.20` | Defines the EAR value below which the eyes are considered closed |
| `consec_frames` | `2` | Requires the threshold condition to persist across consecutive frames |

The detection logic increments a frame counter while the average EAR remains below the threshold. Once the eyes reopen, it sends a blink event if the counter reached the required number of frames.

```python
if avg_ear < blink_threshold:
    frame_counter += 1
else:
    if frame_counter >= consec_frames:
        sock.sendto(
            b"blink",
            (UDP_IP, UDP_PORT)
        )

    frame_counter = 0
```

This approach uses temporal validation rather than treating every low EAR measurement as a separate blink.

The threshold and frame count are prototype parameters. Their effectiveness depends on camera quality, frame rate, lighting, and individual eye characteristics.

## Python-to-Unity Communication

The Python application and Unity communicate using **UDP sockets on the local machine**. This keeps computer-vision processing outside the Unity runtime while allowing the game to consume detected blink events.

### 1. UDP Configuration

The Python script creates a UDP socket and defines the destination endpoint:

```python
UDP_IP = "127.0.0.1"
UDP_PORT = 5052

sock = socket.socket(
    socket.AF_INET,
    socket.SOCK_DGRAM
)
```

`127.0.0.1` is the loopback address, meaning the datagram is intended for an application running on the same computer.

### 2. Event Transmission

When the blink-validation condition is satisfied, Python sends the byte payload `blink` to port `5052`.

```python
sock.sendto(b"blink", (UDP_IP, UDP_PORT))
```

This message is an event signal, not a continuous stream of webcam frames or facial landmark data. Unity only needs to receive the signal and pass it to the relevant gameplay logic.

The resulting data flow is:

```text
Detected Blink
      |
      v
Python UDP Socket
      |
      |  Payload: "blink"
      |  Destination: 127.0.0.1:5052
      v
Unity UDP Listener
      |
      v
Gameplay Input Event
      |
      v
Dimension Switch
```

The Unity listener must be configured to receive datagrams on the corresponding port and interpret the payload. UDP is connectionless, so it does not guarantee delivery or retransmission of a message.

### 3. Why Use UDP?

UDP provides a lightweight way to pass small event messages between separate applications without establishing a persistent connection.

For this prototype, the message contains only a short event identifier. The approach avoids sending unnecessary image data to Unity and keeps the detection and gameplay systems loosely coupled.

The trade-off is that UDP does not guarantee delivery, ordering, or duplicate prevention. Those limitations may be acceptable for an experimental prototype, but a more robust implementation could add event debouncing, sequence identifiers, or an alternative communication protocol if required.

## Dimension Switching

Dimension switching is the gameplay response to a blink event.

The intended state model consists of two mutually exclusive states:

- **Reality:** The baseline hospital environment.
- **Alternate dimension:** A modified version containing additional or altered environmental information.

A validated blink event is passed to the Unity gameplay layer, which requests a transition to the other state.

```text
         Received Blink Event
                  |
                  v
         Read Current State
                  |
                  v
          Toggle World State
             /         \
            v           v
         Reality     Alternate
            \           /
             v         v
       Update Environment
                  |
                  v
         Continue Gameplay
```

Depending on the scene implementation, this can be achieved by toggling dimension-specific objects, changing materials, or updating selected environment elements. Shared geometry can remain in place to avoid unnecessary duplication.

The state-management logic should remain independent of webcam processing. This makes it possible to test dimension switching with simulated input without requiring the camera or blink-detection application.

## Puzzle Design

The puzzles are designed around information that is distributed across the two dimensions.

Examples include:

- Hidden warnings or codes that appear only in the alternate dimension.
- Environmental markings that provide part of a puzzle solution.
- Differences between objects or room layouts that reveal additional information.
- Clues that require players to observe one dimension and act on information found in the other.

The intention is to make dimension switching an essential part of reasoning and exploration rather than a purely cosmetic effect.

## Technical Considerations

### Detection Reliability

A fixed EAR threshold is simple to implement, but it can behave differently under varying lighting, camera angles, and user conditions. Calibration or adaptive thresholds could improve reliability.

### Event Debouncing

The consecutive-frame check helps distinguish sustained eye closure from a single-frame measurement. Additional event gating could prevent repeated triggers caused by tracking noise or unusual camera input.

### Communication Reliability

UDP has low overhead but does not guarantee delivery. The current sender does not implement acknowledgements or retransmission.

### Runtime Integration

The Unity receiver should avoid performing Unity scene operations directly from a background networking thread. Incoming events should be transferred safely to Unity's main thread before modifying game objects or scene state.

### Separation of Concerns

Computer vision, network communication, gameplay input, and dimension-state management are separate responsibilities. Keeping them decoupled makes the system easier to test and extend.

## Development Constraints

Built as a solo game jam prototype under a limited development window, the project prioritized experimentation with a distinctive input mechanic and a focused environmental puzzle experience.

The prototype combines computer vision, inter-process communication, and Unity gameplay to explore how a player's physical actions can influence perception and interaction inside a game.

## Future Improvements

Potential areas for further development include:

- Calibrating EAR thresholds for different users and camera conditions.
- Improving blink-event debouncing and tracking-loss handling.
- Adding communication diagnostics and clearer error reporting.
- Measuring end-to-end detection-to-gameplay latency.
- Expanding the hospital environment and interconnected puzzles.
- Refining visual and audio feedback during dimension transitions.
- Adding automated tests for dimension state and puzzle progression.

## Project Status

**Experimental game jam prototype.**

Blink explores a webcam-driven input mechanic that connects real-world blinking with dimension-based exploration and psychological horror. The Python detection script implements the webcam processing, EAR-based blink detection, and UDP event transmission. The final in-game response depends on the corresponding Unity integration.

---

*Built with Unity, Python, OpenCV, and MediaPipe. Exploring computer vision, UDP communication, and perception-driven gameplay.*
