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

func TestCreateUserSuccess(t *testing.T) {
	req := httptest.NewRequest(http.MethodPost, "/users", strings.NewReader(`{"name":"Jeff","email":"jeff@example.com"}`))
	rec := httptest.NewRecorder()
	NewMux(NewStore()).ServeHTTP(rec, req)

	if rec.Code != http.StatusCreated {
		t.Fatalf("status = %d, want %d body=%s", rec.Code, http.StatusCreated, rec.Body.String())
	}
	user := decodeJSON[User](t, rec)
	if user.ID != 3 || user.Name != "Jeff" || user.Email != "jeff@example.com" {
		t.Fatalf("unexpected user: %+v", user)
	}
}

func TestCreateUserMissingFields(t *testing.T) {
	cases := []string{`{}`, `{"name":"Jeff"}`, `{"email":"jeff@example.com"}`, `not-json`}
	for _, body := range cases {
		req := httptest.NewRequest(http.MethodPost, "/users", strings.NewReader(body))
		rec := httptest.NewRecorder()
		NewMux(NewStore()).ServeHTTP(rec, req)
		if rec.Code != http.StatusBadRequest {
			t.Fatalf("body %q status = %d, want %d", body, rec.Code, http.StatusBadRequest)
		}
	}
}

func TestGetUserByID(t *testing.T) {
	mux := NewMux(NewStore())

	t.Run("found", func(t *testing.T) {
		req := httptest.NewRequest(http.MethodGet, "/users/1", nil)
		rec := httptest.NewRecorder()
		mux.ServeHTTP(rec, req)
		if rec.Code != http.StatusOK {
			t.Fatalf("status = %d, want %d", rec.Code, http.StatusOK)
		}
		user := decodeJSON[User](t, rec)
		if user.Name != "Ada Lovelace" {
			t.Fatalf("unexpected user: %+v", user)
		}
	})

	t.Run("not found", func(t *testing.T) {
		req := httptest.NewRequest(http.MethodGet, "/users/99", nil)
		rec := httptest.NewRecorder()
		mux.ServeHTTP(rec, req)
		if rec.Code != http.StatusNotFound {
			t.Fatalf("status = %d, want %d", rec.Code, http.StatusNotFound)
		}
	})

	t.Run("invalid id", func(t *testing.T) {
		req := httptest.NewRequest(http.MethodGet, "/users/abc", nil)
		rec := httptest.NewRecorder()
		mux.ServeHTTP(rec, req)
		if rec.Code != http.StatusBadRequest {
			t.Fatalf("status = %d, want %d", rec.Code, http.StatusBadRequest)
		}
	})
}

func TestStoresAreIsolated(t *testing.T) {
	first := httptest.NewRecorder()
	NewMux(NewStore()).ServeHTTP(first, httptest.NewRequest(http.MethodPost, "/users", strings.NewReader(`{"name":"A","email":"a@example.com"}`)))
	second := httptest.NewRecorder()
	NewMux(NewStore()).ServeHTTP(second, httptest.NewRequest(http.MethodGet, "/users", nil))

	users := decodeJSON[[]User](t, second)
	if len(users) != 2 {
		t.Fatalf("second store leaked first store writes: %v", users)
	}
}
