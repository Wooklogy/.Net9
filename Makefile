# ───────────────────────────────
# ⚙️ Environment Configuration
# ───────────────────────────────

API_PORT ?= $(PORT_API)
ifeq ($(API_PORT),)
    API_PORT := 5000
endif

HUB_PORT ?= $(PORT_HUB)
ifeq ($(HUB_PORT),)
    HUB_PORT := 5001
endif

API_DIR = Api
HUB_DIR = Hub
API_CS = $(API_DIR)/$(API_DIR).csproj
HUB_CS = $(HUB_DIR)/$(HUB_DIR).csproj
MIGRATION_PATH = Infra/Migrations

# 🎨 색상 정의 (로그 가독성용)
BLUE   = \033[1;34m
GREEN  = \033[1;32m
YELLOW = \033[1;33m
RESET  = \033[0m

# ───────────────────────────────
# 🚀 Development (Watch Mode)
# ───────────────────────────────

dev-api:
	@echo "$(BLUE)[DEV] Starting API Server using Container Env (Port: $(API_PORT))...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Development \
	ASPNETCORE_URLS="http://0.0.0.0:$(API_PORT)" \
	dotnet watch run --project $(API_CS) --no-launch-profile

dev-hub:
	@echo "$(BLUE)[DEV] Starting Hub Server using Container Env (Port: $(HUB_PORT))...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Development \
	ASPNETCORE_URLS="http://0.0.0.0:$(HUB_PORT)" \
	dotnet watch run --project $(HUB_CS) --no-launch-profile

# ───────────────────────────────
# 🌐 Production (Production Mode)
# ───────────────────────────────

prod-api:
	@echo "$(YELLOW)[PROD] Starting API Server using Container Env (Port: $(API_PORT))...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS="http://0.0.0.0:$(API_PORT)" \
	dotnet run --project $(API_CS) --configuration Release --no-launch-profile

prod-hub:
	@echo "$(YELLOW)[PROD] Starting Hub Server using Container Env (Port: $(HUB_PORT))...$(RESET)"
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS="http://0.0.0.0:$(HUB_PORT)" \
	dotnet run --project $(HUB_CS) --configuration Release --no-launch-profile

# ───────────────────────────────
# 🛠 DB Migrations (EF Core)
# ───────────────────────────────

migrate:
	@echo "$(GREEN)Adding migration: $(name)...$(RESET)"
	dotnet ef migrations add $(name) \
		--project $(API_CS) \
		--startup-project $(API_CS) \
		--output-dir $(MIGRATION_PATH)

db-update:
	@echo "$(GREEN)Updating database...$(RESET)"
	dotnet ef database update \
		--project $(API_CS) \
		--startup-project $(API_CS)

migrate-remove:
	@echo "$(GREEN)Removing last migration...$(RESET)"
	dotnet ef migrations remove \
		--project $(API_CS) \
		--startup-project $(API_CS)

# ───────────────────────────────
# 🧹 Maintenance & Cleanup
# ───────────────────────────────

reload:
	@echo "$(BLUE)Reloading projects...$(RESET)"
	dotnet clean $(API_CS)
	dotnet clean $(HUB_CS)
	dotnet nuget locals all --clear
	dotnet restore
	dotnet build

kill:
	@echo "$(GREEN)Killing all dotnet processes...$(RESET)"
	pkill -f dotnet || true

# ───────────────────────────────
# ✨ Code Quality
# ───────────────────────────────

format:
	dotnet format .

check:
	dotnet build /warnaserror