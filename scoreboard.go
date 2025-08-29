package main

import (
	"encoding/json"
	"fmt"
	dem "github.com/markus-wa/demoinfocs-golang/v4/pkg/demoinfocs"
	common "github.com/markus-wa/demoinfocs-golang/v4/pkg/demoinfocs/common"
	"github.com/markus-wa/demoinfocs-golang/v4/pkg/demoinfocs/events"
	dp "github.com/markus-wa/godispatch"
	"log"
	"os"
)

func Players(p dem.Parser, err error) map[int]*common.Player {
	// Define var(s)
	var (
		// Map all players by SteamID
		players = make(map[int]*common.Player)
		_       dp.HandlerIdentifier
	)

	// Register handler on player connect events
	_ = p.RegisterEventHandler(func(e events.PlayerConnect) {
		//fmt.Printf("%s connected\n", e.Player.Name)
		players[e.Player.UserID] = e.Player
	})

	return players
}
func ScoreBoard(p dem.Parser, err error, players map[int]*common.Player) map[string]map[string]float64 {
	// Define var(s)
	var (
		playerMatrix = make(map[string]map[string]float64)
		debug        = true
		roundNr      int
	)

	// Collect info from players
	for _, v := range players {
		//fmt.Printf("Player: %s, Total damage: %d Total UT damage %d\n", v.Name, v.TotalDamage(), v.UtilityDamage())
		playerMatrix[v.Name] = make(map[string]float64)
		playerMatrix[v.Name]["Total Damage"] = float64(v.TotalDamage())
		playerMatrix[v.Name]["Utility Damage"] = float64(v.UtilityDamage())
		playerMatrix[v.Name]["Kills"] = float64(v.Kills())
		playerMatrix[v.Name]["Deaths"] = float64(v.Deaths())
		playerMatrix[v.Name]["Assists"] = float64(v.Assists())
		if playerMatrix[v.Name]["Deaths"] == 0 {
			playerMatrix[v.Name]["KDR"] = playerMatrix[v.Name]["Kills"]
		} else {
			playerMatrix[v.Name]["KDR"] = playerMatrix[v.Name]["Kills"] / playerMatrix[v.Name]["Deaths"]
		}
		if debug {
			fmt.Printf("[Player info] Player: %s, Kills: %d, Deaths: %d, Assists: %d, KDR: %f \n", v.Name, playerMatrix[v.Name]["Kills"], playerMatrix[v.Name]["Deaths"], playerMatrix[v.Name]["Assists"], playerMatrix[v.Name]["KDR"])
		}
	}

	roundNr = 1
	p.RegisterEventHandler(func(e events.RoundEndOfficial) {
		roundNr++
		// Print all players total damage (weapon + utility
		for _, v := range players {
			playerMatrix[v.Name]["ADR"] = float64(v.TotalDamage() / roundNr)
			if debug {
				fmt.Printf("[%d] ADR: %f\n", roundNr, playerMatrix[v.Name]["ADR"])
			}
		}
		if debug {
			fmt.Printf("Round %d\n", roundNr)
		}
	})

	// Get kill events
	// Register handler on kill events
	count := 0
	p.RegisterEventHandler(func(e events.Kill) {
		if e.Victim != nil && e.Killer != nil {
			if e.IsHeadshot {
				playerMatrix[e.Killer.Name]["Headshots"]++
				playerMatrix[e.Killer.Name]["Headshot %"] = playerMatrix[e.Killer.Name]["Headshots"] / playerMatrix[e.Killer.Name]["Kills"] // Calculate headshot percentage
			}
		} else {;
			if debug {
				fmt.Println("No complete kill event")
			}
		}
		count++
	})

	// Get enemy flash events
	p.RegisterEventHandler(func(e events.PlayerFlashed) {
		if e.Attacker != nil && e.Player != nil {
			if e.Attacker.Team != e.Player.Team {
				playerMatrix[e.Attacker.Name]["Flash Assists"]++
			}
		}
	})

	return playerMatrix
}

func StartProgram(file string) string {
	f, err := os.Open("demos/" + file + ".dem")
	if err != nil {
		log.Panic("failed to open demo file: ", err)
	}
	defer f.Close()

	playerParser := dem.NewParser(f)

	defer playerParser.Close()

	allPlayers := Players(playerParser, err)

	//Parse to end
	err = playerParser.ParseToEnd()
	if err != nil {
		log.Panic("failed to parse demo: ", err)
	}

	// fmt.Printf("Players: %v\n", allPlayers)

	// Reset file pointer
	_, err = f.Seek(0, 0)
	if err != nil {
		log.Panic("failed to reset file pointer: ", err)
	}

	scoreBoardParser := dem.NewParser(f)

	defer scoreBoardParser.Close()

	scoreboard := ScoreBoard(scoreBoardParser, err, allPlayers)

	//Parse to end
	err = scoreBoardParser.ParseToEnd()
	if err != nil {
		log.Panic("failed to parse demo: ", err)
	}

	// Save results to JSON
	jsonScoreboard, err := json.Marshal(scoreboard)
	if err != nil {
		log.Panic("failed to marshal json: ", err)
	}
	// fmt.Println(string(jsonScoreboard))

	// Save results to JSON file
	err = os.WriteFile("demos/Results/"+file+".json", jsonScoreboard, 0644)
	if err != nil {
		log.Panic("failed to write json file: ", err)
	}

	//var result string
	//for k, v := range scoreboard {
	//	result += fmt.Sprintf("Player: %s\n", k)
	//	for k2, v2 := range v {
	//		result += fmt.Sprintf("%s: %f", k2, v2)
	//		result += " || "
	//	}
	//	result += "\n"
	//}
	return fmt.Sprintf("%s", "test")
}
