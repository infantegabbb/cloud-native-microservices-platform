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

