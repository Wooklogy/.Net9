-include ../../compose/.env
-include .env
export

API_DIR = Api
HUB_DIR = Hub
API_CS = $(API_DIR)/Api.csproj
HUB_CS = $(HUB_DIR)/Hub.csproj
MIGRATION_PATH = Infra/Migrations

# 3. 색상 정의 (로그 가독성용)
BLUE  = \033[1;34m
GREEN = \033[1;32m
RESET = \033[0m

# ───────────────────────────────
# 🚀 Development (Watch Mode)
# ───────────────────────────────

# API 서버 실행: .env의 PORT_API_TARGET 변수를 연동합니다.
# API 서버 실행: PORT_API(5200) 변수를 우선 사용합니다.
# 개발 환경 실행: Hot Reload 활성화 및 상세 로그 출력
dev-api:
	@echo "$(BLUE)[DEV] Starting API Server in Development mode...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Development \
	ASPNETCORE_URLS="http://0.0.0.0:5000" \
	dotnet watch run --project $(API_CS) --no-launch-profile

dev-hub:
	@echo "$(BLUE)[DEV] Starting Hub Server on port 5001...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Development \
	ASPNETCORE_URLS="http://0.0.0.0:5001" \
	dotnet watch run --project $(HUB_CS) --no-launch-profile

# ───────────────────────────────
# 🌐 Production (Production Mode)
# ───────────────────────────────

# 운영 환경 실행: 최적화 빌드 후 실행 (Hot Reload 비활성화)
prod-api:
	@echo "$(YELLOW)[PROD] Starting API Server in Production mode...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS="http://0.0.0.0:5000" \
	dotnet run --project $(API_CS) --configuration Release --no-launch-profile

prod-hub:
	@echo "$(YELLOW)[PROD] Starting Hub Server on port 5001...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS="http://0.0.0.0:5001" \
	dotnet run --project $(HUB_CS) --configuration Release --no-launch-profile

# ───────────────────────────────
# 🛠 DB Migrations (EF Core)
# ───────────────────────────────

# 마이그레이션 추가 (사용법: make migrate name=InitDB)
migrate:
	@echo "$(GREEN)Adding migration: $(name)...$(RESET)"
	dotnet ef migrations add $(name) \
		--project $(API_CS) \
		--startup-project $(API_CS) \
		--output-dir $(MIGRATION_PATH)

# 데이터베이스 업데이트
db-update:
	@echo "$(GREEN)Updating database...$(RESET)"
	dotnet ef database update \
		--project $(API_CS) \
		--startup-project $(API_CS)

# 마지막 마이그레이션 제거
migrate-remove:
	@echo "$(GREEN)Removing last migration...$(RESET)"
	dotnet ef migrations remove \
		--project $(API_CS) \
		--startup-project $(API_CS)

# ───────────────────────────────
# 🧹 Maintenance & Cleanup
# ───────────────────────────────

# 캐시 삭제, 패키지 복원, 다시 빌드
reload:
	@echo "$(BLUE)Reloading projects...$(RESET)"
	dotnet clean $(API_CS)
	dotnet clean $(HUB_CS)
	dotnet nuget locals all --clear
	dotnet restore
	dotnet build

# 포트 점유 중인 좀비 dotnet 프로세스 처단
kill:
	@echo "$(GREEN)Killing all dotnet processes...$(RESET)"
	pkill -f dotnet || true

# ───────────────────────────────
# ✨ Code Quality
# ───────────────────────────────

# 코드 스타일 자동 정리
format:
	dotnet format .

# 경고를 에러로 취급하여 빌드 체크
check:
	dotnet build /warnaserror
