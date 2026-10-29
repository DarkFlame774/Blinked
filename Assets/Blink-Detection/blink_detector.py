import cv2
import mediapipe as mp
import socket
from math import dist

# UDP setup
UDP_IP = "127.0.0.1"
UDP_PORT = 5052
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Mediapipe setup
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(refine_landmarks=True, max_num_faces=1)

cap = cv2.VideoCapture(0)
blink_threshold = 0.20
frame_counter = 0
consec_frames = 2

def calc_EAR(landmarks, eye_points):
    v1 = dist(landmarks[eye_points[1]], landmarks[eye_points[5]])
    v2 = dist(landmarks[eye_points[2]], landmarks[eye_points[4]])
    h = dist(landmarks[eye_points[0]], landmarks[eye_points[3]])
    return (v1 + v2) / (2.0 * h)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)
    h, w, _ = frame.shape

    if results.multi_face_landmarks:
        landmarks = [(lm.x * w, lm.y * h) for lm in results.multi_face_landmarks[0].landmark]
        left_eye = [33, 160, 158, 133, 153, 144]
        right_eye = [362, 385, 387, 263, 373, 380]

        left_ear = calc_EAR(landmarks, left_eye)
        right_ear = calc_EAR(landmarks, right_eye)
        avg_ear = (left_ear + right_ear) / 2.0

        if avg_ear < blink_threshold:
            frame_counter += 1
        else:
            if frame_counter >= consec_frames:
                sock.sendto(b"blink", (UDP_IP, UDP_PORT))
                print("Blink event sent to Unity!")
            frame_counter = 0

    cv2.imshow("Blink Detection", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
