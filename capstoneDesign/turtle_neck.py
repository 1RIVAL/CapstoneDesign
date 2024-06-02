import cv2
import modules.HolisticModule as hm
import math
import socket

###################################################
sensitivity = 8
###################################################

# Parameters
width, height = 1280, 720

# IP WebCam
    # cap = cv2.VideoCapture("http://Your IP Address/video")

# 일반 WebCam 을 사용할 경우
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

cap.set(3, width) # Cam 가로 프레임 설정
cap.set(4, height) # Cam 세로 프레임 설정

# Holistic 객체(어떠한 행위를 하는 친구) 생성
detector = hm.HolisticDetector()

# 네트워크
sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# turtle_neck_count 변수 초기 세팅
turtle_neck_count = 0


while True:
# 웹켐에서 프레임 가져오기
    success, img = cap.read()

# mediapipe를 거친 이미지 생성 -> img
    img = detector.findHolistic(img, draw=True)

    pose_lmList = detector.findPoseLandmark(img, draw=True)
    face_lmList = detector.findFaceLandmark(img, draw=True)

    turtleneck_score = 0

    data = [] # socket으로 보내는 데이터

    # pose_lmList에서의 랜드마크:

    # ===== [id] 부분 제거하고 소켓으로 넘기기!! =====

    # for lm in pose_lmList: # face_lmList
    #     data.extend([lm[0], height - lm[1], lm[2]])
    #     print(data)
    #     sock.sendto(str.encode(str(data)), serverAddressPort)

    # 측면에서 봤을 때 - 귀와 어깨의 X좌표 값의 차이 크게
    # 정면에서 봤을 때 - 목과 코 사이의 거리 크게


    # 인체가 감지가 되었는지 확인하는 구문
    if len(pose_lmList) != 0 and len(face_lmList) != 0:
        #print("pose[11]", pose_lmList[11]) # left_shoulder
        #print("pose[12]", pose_lmList[12]) # right_shoulder
        #print("face[152]", face_lmList[152]) # 턱

        #print("face[5]", face_lmList[5]) # 코 중앙

        #print("pose[7]", pose_lmList[7]) # left_ear
        #print("pose[8]", pose_lmList[8]) # right_ear



        # 양 어깨 좌표 11번과 12번의 중심 좌표를 찾아 낸다.
        center_shoulder = detector.findCenter(11, 12)

        # 목 길이 center_shoulder 좌표와 얼굴 152번(턱) 좌표를 사용하여 길이 구하는 부분
        # 목 길이가 표시된 이미지로 변경
        length, img = detector.findDistance(152, center_shoulder, img, draw=True)

        # x, y, z좌표 예측 (노트북 웹캠과의 거리를 대강 예측) - 노트북과의 거리
        pose_depth = abs(500 - detector.findDepth(11, 12))


        # if pose_depth < 200:
        #     turtleneck_detect_threshold = 55
        # else:
        #     turtleneck_detect_threshold = 70

        # turtleneck_detect_threshold = pose_depth / 4
        # 노트북과의 거리는 0보다 커야한다.
        if pose_depth > 0:
            # 거북목 감지 임계치
            turtleneck_detect_threshold = abs(math.log2(pose_depth)) * sensitivity

        # 노트북과의 거리가 아주 가까운 상태
        else:
            turtleneck_detect_threshold = 50

        # 목길이, 임계치, 노트북과의 거리
        print("Length : {:.3f},   Threshold : {:.3f},   Pose_depth : {}".format(length, turtleneck_detect_threshold,
                                                                                pose_depth))

        # 핵심 로직: 목 길이가 임계치보다 작을 때, 거북목으로 생각한다.
        #if length < turtleneck_detect_threshold:
        if length - turtleneck_detect_threshold < 100:
            # 얼마나 거북목인지 계산해주는 부분 (0~ 100 점)
            turtleneck_score = int((turtleneck_detect_threshold - int(length)) / turtleneck_detect_threshold * 100)
            print("WARNING - Keep your posture straigh`t.")
            print("TurtleNeck Score = ", turtleneck_score)

            cv2.putText(img, "WARNING!", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2, cv2.LINE_AA)

# img를 우리에게 보여주는 부분
    cv2.imshow("Image", img)


    # ESC 키를 눌렀을 때 창을 모두 종료하는 부분
    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()