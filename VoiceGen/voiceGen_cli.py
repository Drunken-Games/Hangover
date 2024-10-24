import random
import pandas as pd
from pydub import AudioSegment
from pydub.playback import play
from jamo import h2j, j2hcj
import os
from datetime import datetime

# 한글 자모 리스트
char_list = ['ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ', ' ']

# 자모에 대한 음원 저장소
char_sounds = {}
char_sounds_high = {}

# 자모 음원 로드
for idx, item in enumerate(char_list):
    str_idx = str(idx + 1).zfill(2)  # 01, 02 형식으로 인덱스 생성
    char_sounds[item] = AudioSegment.from_mp3(f'./sources/{str_idx}.padata')
    char_sounds_high[item] = AudioSegment.from_mp3(f'./sources/high/{str_idx}.padata')

# 현재 날짜와 시간 정보로 폴더 이름 생성
current_time = datetime.now().strftime('%Y%m%d_%H%M%S')
result_folder = f'./result/{current_time}'

# 결과 폴더 생성 (존재하지 않을 경우)
os.makedirs(result_folder, exist_ok=True)

while True:
    # CSV 파일 로드
    source = input('dialogue 폴더 내 csv 파일: ') # CSV 파일 경로를 적어주세요.
    if source == 'exit':
        break
    df = pd.read_csv('./dialogue/' + source)

    # 각 행에 대해 대사 텍스트 처리
    for _, row in df.iterrows():
        id = int(row['ID'])
        day = int(row['일차'])
        npcId = int(row['인물 ID'])
        text = str(row['대사 텍스트'])

        result_sound = None
        result_sound_high = None
        print(f'생성중: {text}')
        
        # 대사 텍스트의 각 문자 처리
        for ch in text:
            jamo_ch = j2hcj(h2j(ch))  # 한글 자모로 변환
            if jamo_ch[0] not in char_list:
                print(f'지원되지 않는 문자를 건너뛰었습니다: {jamo_ch}')
                continue  # 지원되지 않는 문자는 건너뜀
            
            # 자모에 해당하는 음원 가져오기
            char_sound = char_sounds[jamo_ch[0]]
            char_sound_high = char_sounds_high[jamo_ch[0]]

            # 랜덤으로 음의 높이 조정
            octaves = 2 * random.uniform(0.96, 1.15)
            new_sample_rate = int(char_sound.frame_rate * (2.0 ** octaves))

            # 음원의 피치를 변경
            pitch_char_sound = char_sound._spawn(char_sound.raw_data, overrides={'frame_rate': new_sample_rate})
            result_sound = pitch_char_sound if result_sound is None else result_sound + pitch_char_sound

            pitch_char_sound_high = char_sound_high._spawn(char_sound_high.raw_data,
                                                        overrides={'frame_rate': new_sample_rate})
            result_sound_high = pitch_char_sound_high if result_sound_high is None else result_sound_high + pitch_char_sound_high

        # 파일 이름 생성
        filename_normal = f'{result_folder}/day{day}_{id}_{npcId}_normal.mp3'
        filename_high = f'{result_folder}/day{day}_{id}_{npcId}_high.mp3'

        # 음원 재생
        # print("재생중: " + text + "(일반)")
        # play(result_sound)

        # print("재생중: " + text + "(고음)")
        # play(result_sound_high)

        # 음원 저장
        result_sound.export(filename_normal, format='mp3')
        result_sound_high.export(filename_high, format='mp3')
        print(f"저장 완료: {filename_normal}, {filename_high}")

    print('모든 대사가 처리되었습니다.')

print("종료. Good Bye!")
