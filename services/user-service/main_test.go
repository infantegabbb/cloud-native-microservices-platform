package main

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
)

func decodeJSON[T any](t *testing.T, rec *httptest.ResponseRecorder) T {
	t.Helper()
	var value T
	if err := json.Unmarshal(rec.Body.Bytes(), &value); err != nil {
		t.Fatalf("decode json: %v body=%s", err, rec.Body.String())
	}
	return value
}

func TestHealth(t *testing.T) {
	req := httptest.NewRequest(http.MethodGet, "/health", nil)
	rec := httptest.NewRecorder()
	NewMux(NewStore()).ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d, want %d", rec.Code, http.StatusOK)
	}
	body := decodeJSON[map[string]string](t, rec)
	if body["status"] != "ok" || body["service"] != "user-service" {
		t.Fatalf("unexpected body: %v", body)
	}
}

func TestListUsersReturnsSeedData(t *testing.T) {
	req := httptest.NewRequest(http.MethodGet, "/users", nil)
	rec := httptest.NewRecorder()
	NewMux(NewStore()).ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d, want %d", rec.Code, http.StatusOK)
	}
	users := decodeJSON[[]User](t, rec)
	if len(users) != 2 {
		t.Fatalf("user count = %d, want 2", len(users))
	}
	byID := map[int]User{}
	for _, user := range users {
		byID[user.ID] = user
	}
	if byID[1].Name != "Ada Lovelace" || byID[2].Email != "grace@example.com" {
		t.Fatalf("unexpected seed users: %v", users)
	}
}

