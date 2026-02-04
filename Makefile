-include ../../compose/.env
-include .env
export

API_DIR = Api
HUB_DIR = Hub
API_CS = $(API_DIR)/Api.csproj
HUB_CS = $(HUB_DIR)/Trading.Hub.csproj
MIGRATION_PATH = Infra/Migrations

# 3. 색상 정의 (로그 가독성용)
BLUE  = \033[1;34m
GREEN = \033[1;32m
RESET = \033[0m

# ───────────────────────────────
# 🚀 Development (Watch Mode)
# ───────────────────────────────

# API 서버 실행: .env의 PORT_API_TARGET 변수를 연동합니다.
dev-api:
	@echo "$(BLUE)Starting API Server on port $(PORT_API_TARGET)...$(RESET)"
	dotnet watch run --project $(API_CS) --urls "http://0.0.0.0:$(PORT_API_TARGET)"

# Hub 서버 실행: .env의 PORT_HUB_TARGET 변수를 연동합니다.
dev-hub:
	@echo "$(BLUE)Starting Hub Server on port $(PORT_HUB_TARGET)...$(RESET)"
	dotnet watch run --project $(HUB_CS) --urls "http://0.0.0.0:$(PORT_HUB_TARGET)"

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
