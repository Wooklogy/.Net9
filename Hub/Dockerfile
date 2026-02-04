# 개발용이므로 SDK 이미지를 베이스로 사용합니다.
FROM mcr.microsoft.com/dotnet/sdk:9.0

# 컨테이너 내 작업 디렉토리 설정
WORKDIR /workspace

# ✅ 여기에 필요한 도구들을 모두 추가합니다.
RUN apt-get update && apt-get install -y \
    git \
    curl \
    vim \
    make \
    procps \
    openssh-client \
    && rm -rf /var/lib/apt/lists/*

# EF Core 도구 설치
RUN dotnet tool install --global dotnet-ef --version 9.0.1
ENV PATH="${PATH}:/root/.dotnet/tools"

# 확실한 무한 대기 명령어로 변경
CMD ["sleep", "infinity"]
