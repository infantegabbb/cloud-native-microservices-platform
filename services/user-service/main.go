package main

import (
	"encoding/json"
	"log"
	"net/http"
	"os"
	"strconv"
	"sync"
)

type User struct {
	ID    int    `json:"id"`
	Name  string `json:"name"`
	Email string `json:"email"`
}

type Store struct {
	mu     sync.RWMutex
	users  map[int]User
	nextID int
}

func NewStore() *Store {
	return &Store{
		users: map[int]User{
			1: {ID: 1, Name: "Ada Lovelace", Email: "ada@example.com"},
			2: {ID: 2, Name: "Grace Hopper", Email: "grace@example.com"},
		},
		nextID: 3,
	}
}

func NewMux(store *Store) http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("/health", health)
	mux.HandleFunc("/users", store.usersHandler)
	mux.HandleFunc("/users/", store.userByID)
	return mux
}

func writeJSON(w http.ResponseWriter, code int, value any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_ = json.NewEncoder(w).Encode(value)
}

func health(w http.ResponseWriter, _ *http.Request) {
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok", "service": "user-service"})
}

func (s *Store) usersHandler(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case http.MethodGet:
		s.mu.RLock()
		defer s.mu.RUnlock()
		out := make([]User, 0, len(s.users))
		for _, user := range s.users {
			out = append(out, user)
		}
		writeJSON(w, http.StatusOK, out)
	case http.MethodPost:
		var input struct{ Name, Email string }
		if err := json.NewDecoder(r.Body).Decode(&input); err != nil || input.Name == "" || input.Email == "" {
			writeJSON(w, http.StatusBadRequest, map[string]string{"error": "name and email are required"})
			return
		}
		s.mu.Lock()
		user := User{ID: s.nextID, Name: input.Name, Email: input.Email}
		s.users[s.nextID] = user
		s.nextID++
		s.mu.Unlock()
		writeJSON(w, http.StatusCreated, user)
	default:
		w.WriteHeader(http.StatusMethodNotAllowed)
	}
}

