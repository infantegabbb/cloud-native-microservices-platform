.PHONY: up down test test-unit logs build
up:
	docker compose up --build -d

down:
	docker compose down

build:
	docker compose build

test:
	python3 scripts/smoke_test.py

test-unit:
	cd services/user-service && go test ./...
	cd services/order-service && dotnet test --configuration Release

logs:
	docker compose logs -f
