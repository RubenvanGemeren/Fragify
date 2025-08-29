package main

import (
	"encoding/json"
	"log"
	"os"
)

type displayScoreboard struct {
	Player        string
	TotalDamage   float64
	UtilityDamage float64
	Kills         float64
	Deaths        float64
	Assists       float64
	KDR           float64
	ADR           float64
}

func Display(file string) ([]displayScoreboard, error) {
	f, err := os.Open(file)
	if err != nil {
		log.Panic("failed to open json file: ", err)
	}
	defer f.Close()

	var displayScoreboard []displayScoreboard

	decoder := json.NewDecoder(f)
	err = decoder.Decode(&displayScoreboard)
	if err != nil {
		log.Panic("failed to decode json: ", err)
	}

	return displayScoreboard, nil
}
