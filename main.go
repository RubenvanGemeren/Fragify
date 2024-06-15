package main

import (
	"errors"
	"fmt"
	"github.com/charmbracelet/bubbles/filepicker"
	tea "github.com/charmbracelet/bubbletea"
	"log"
	"os"
	"strings"
	"time"
)

func getChoices() []string {
	availableDemos := []string{}

	// Go through all files with the .dem extension in demos folder
	files, err := os.ReadDir("demos")
	if err != nil {
		log.Panic("failed to read directory: ", err)
	}
	for _, file := range files {
		if file.IsDir() {
			continue
		}
		if file.Name()[len(file.Name())-4:] == ".dem" {
			availableDemos = append(availableDemos, file.Name())
		}
	}

	fmt.Printf("Available demos: %v\n", availableDemos)
	return availableDemos
}

type model struct {
	filepicker   filepicker.Model
	selectedFile string
	quitting     bool
	err          error
}

type clearErrorMsg struct{}

func clearErrorAfter(t time.Duration) tea.Cmd {
	return tea.Tick(t, func(_ time.Time) tea.Msg {
		return clearErrorMsg{}
	})
}

func (m model) Init() tea.Cmd {
	return m.filepicker.Init()
}

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
	switch msg := msg.(type) {
	case tea.KeyMsg:
		switch msg.String() {
		case "ctrl+c", "q":
			m.quitting = true
			return m, tea.Quit
		}
	case clearErrorMsg:
		m.err = nil
	}

	var cmd tea.Cmd
	m.filepicker, cmd = m.filepicker.Update(msg)

	// Did the user select a file?
	if didSelect, path := m.filepicker.DidSelectFile(msg); didSelect {
		// Get the path of the selected file.
		m.selectedFile = path
	}

	// Did the user select a disabled file?
	// This is only necessary to display an error to the user.
	if didSelect, path := m.filepicker.DidSelectDisabledFile(msg); didSelect {
		// Let's clear the selectedFile and display an error.
		m.err = errors.New(path + " is not valid.")
		m.selectedFile = ""
		return m, tea.Batch(cmd, clearErrorAfter(2*time.Second))
	}

	return m, cmd
}

func (m model) View() string {
	if m.quitting {
		return ""
	}
	var s strings.Builder
	s.WriteString("\n  ")
	if m.err != nil {
		s.WriteString(m.filepicker.Styles.DisabledFile.Render(m.err.Error()))
	} else if m.selectedFile == "" {
		s.WriteString("Pick a file:")
	} else {
		s.WriteString("Selected file: " + m.filepicker.Styles.Selected.Render(m.selectedFile))
	}
	s.WriteString("\n\n" + m.filepicker.View() + "\n")
	s.WriteString("Current path " + m.filepicker.Styles.DisabledSelected.Render(m.filepicker.CurrentDirectory))
	return s.String()
}

func main() {
	// Uncomment for filepicker
	//fp := filepicker.New()
	//fp.AllowedTypes = []string{".dem"}
	//// Get demos directory
	//fp.CurrentDirectory = "demos"
	//
	//m := model{
	//	filepicker: fp,
	//}
	//tm, _ := tea.NewProgram(&m).Run()
	//mm := tm.(model)
	//fmt.Println("\n  You selected: " + m.filepicker.Styles.Selected.Render(mm.selectedFile) + "\n")
	//
	//if mm.selectedFile == "" {
	//	fmt.Println("No file selected. Exiting...")
	//	return
	//}

	// Start the parser
	//Start("demos/" + "comp_anc_15_4_2024_17_38.dem")
	Start("demos/" + "comp_anc_15_4_2024_17_38.dem")
}
