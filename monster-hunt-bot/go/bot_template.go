/*
 * Monster Hunt Bot - Go Template
 * Usage: go run bot_template.go <server_url> <game_id> <bot_name>
 * See README.md for full API documentation
 */
package main

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"
	"strings"
	"time"
)

type BotTemplate struct {
	http      *http.Client
	serverURL string
	gameID    string
	botName   string
	playerID  *int
}

func NewBot(serverURL, gameID, botName string) *BotTemplate {
	return &BotTemplate{
		http:      &http.Client{Timeout: 5 * time.Second},
		serverURL: strings.TrimSuffix(serverURL, "/"),
		gameID:    gameID,
		botName:   botName,
	}
}

func (b *BotTemplate) GetGameState() (map[string]interface{}, error) {
	url := fmt.Sprintf("%s/game/state/%s", b.serverURL, b.gameID)
	resp, err := b.http.Get(url)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()
	body, _ := io.ReadAll(resp.Body)
	var state map[string]interface{}
	json.Unmarshal(body, &state)
	return state, nil
}

func (b *BotTemplate) FindMyPlayerID(state map[string]interface{}) bool {
	players := state["Players"].(map[string]interface{})
	for _, p := range players {
		player := p.(map[string]interface{})
		if player["Name"].(string) == b.botName {
			id := int(player["Id"].(float64))
			b.playerID = &id
			return true
		}
	}
	return false
}

func (b *BotTemplate) IsMyTurn(state map[string]interface{}) bool {
	if b.playerID == nil {
		return false
	}
	players := state["Players"].(map[string]interface{})
	myPlayer := players[fmt.Sprintf("%d", *b.playerID)].(map[string]interface{})
	isFirst := myPlayer["First"].(bool)
	gameState := state["GameState"].(string)
	return (isFirst && gameState == "Player1Turn") || (!isFirst && gameState == "Player2Turn")
}

func (b *BotTemplate) IsGameOver(state map[string]interface{}) bool {
	if state == nil {
		return false
	}
	gameState := state["GameState"].(string)
	return gameState == "Ending"
}

func main() {
	if len(os.Args) < 4 {
		fmt.Println("Usage: go run bot_template.go <server_url> <game_id> <bot_name>")
		os.Exit(1)
	}

	bot := NewBot(os.Args[1], os.Args[2], os.Args[3])

	for {
		state, _ := bot.GetGameState()
		if state != nil && bot.FindMyPlayerID(state) {
			break
		}
		time.Sleep(500 * time.Millisecond)
	}

	fmt.Printf("Connected as Player %d\n\n", *bot.playerID)

	state, _ := bot.GetGameState()
	for state != nil && !bot.IsGameOver(state) {
		if bot.IsMyTurn(state) {
			fmt.Println("My turn!")
			// TODO: Implement your strategy here
			// See README.md for available endpoints

			time.Sleep(500 * time.Millisecond)
			state, _ = bot.GetGameState()
		} else {
			time.Sleep(500 * time.Millisecond)
			state, _ = bot.GetGameState()
		}
	}

	fmt.Printf("Game %s has ended. Exiting...\n", os.Args[2])
}
