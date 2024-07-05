package main

import (
	"fmt"
	"github.com/charmbracelet/bubbles/list"
	tea "github.com/charmbracelet/bubbletea"
	"github.com/charmbracelet/lipgloss"
	"log"
	"os"
)

func getChoices() []list.Item {
	var availableDemos []list.Item

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
			availableDemos = append(availableDemos, item{title: file.Name(), desc: "A demo file"})
		}
	}

	fmt.Printf("Available demos: %v\n", availableDemos)
	return availableDemos
}

var docStyle = lipgloss.NewStyle().Margin(1, 2)

type item struct {
	title, desc string
}

func (i item) Title() string       { return i.title }
func (i item) Description() string { return i.desc }
func (i item) FilterValue() string { return i.title }

type model struct {
	list list.Model
}

func (m model) Init() tea.Cmd {
	return nil
}

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
	switch msg := msg.(type) {
	case tea.KeyMsg:
		if msg.String() == "ctrl+c" {
			return m, tea.Quit
		}
	case tea.WindowSizeMsg:
		h, v := docStyle.GetFrameSize()
		m.list.SetSize(msg.Width-h, msg.Height-v)
	}

	var cmd tea.Cmd
	m.list, cmd = m.list.Update(msg)
	return m, cmd
}

func (m model) View() string {
	return docStyle.Render(m.list.View())
}

func main() {
	// Uncomment for list
	items := []list.Item{
		item{title: "Raspberry Pi’s", desc: "I have ’em all over my house"},
		item{title: "Nutella", desc: "It's good on toast"},
		item{title: "Pineapple", desc: "I like it on pizza"},
	}

	//items = getChoices()

	m := model{list: list.New(items, list.NewDefaultDelegate(), 0, 0)}
	m.list.Title = "Demos"

	p := tea.NewProgram(m, tea.WithAltScreen())

	if _, err := p.Run(); err != nil {
		fmt.Println("Error running program:", err)
		os.Exit(1)
	}

	// Start the parser
	//Start("demos/" + "comp_anc_15_4_2024_17_38.dem")
	//Start("demos/" + "comp_anc_15_4_2024_17_38.dem")
}
