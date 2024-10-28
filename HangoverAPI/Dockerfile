# 1. Java 17 JDK를 기반으로 컨테이너 빌드
FROM eclipse-temurin:17-jdk

# 2. 작업 디렉토리 설정
WORKDIR /app

# 3. Gradle Wrapper 및 설정 파일을 복사하여 의존성 캐시 활성화
COPY gradlew /app/gradlew
COPY gradle /app/gradle
COPY build.gradle /app/build.gradle
COPY settings.gradle /app/settings.gradle

# 4. Gradle Wrapper에 실행 권한 부여
RUN chmod +x ./gradlew

# 5. 의존성 설치 단계 추가 (의존성 캐시 활용)
RUN ./gradlew dependencies

# 6. 소스 코드 복사 및 프로젝트 빌드
COPY src /app/src
RUN ./gradlew build

# 7. JAR 파일을 실행하도록 설정
CMD ["java", "-jar", "/app/build/libs/hangover-1.0-SNAPSHOT.jar"]
