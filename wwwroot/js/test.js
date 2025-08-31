// Global variables to store event data
let lastRawMessage = null;
let eventHistory = [];
let eventCounts = {
    raw: 0,
    json: 0,
    error: 0,
    total: 0
};

// Helper function to safely get DOM elements
function getElement(id) {
    const element = document.getElementById(id);
    if (!element) {
        console.error(`[ERROR] Element with id '${id}' not found in DOM`);
        return null;
    }
    return element;
}

// Tab switching functionality
function showTab(tabName) {
    // Hide all tab contents
    const tabContents = document.querySelectorAll('.tab-content');
    tabContents.forEach(content => content.classList.remove('active'));

    // Remove active class from all tab buttons
    const tabButtons = document.querySelectorAll('.tab-btn');
    tabButtons.forEach(btn => btn.classList.remove('active'));

    // Show selected tab content
    const selectedTab = document.getElementById(`${tabName}-tab`);
    if (selectedTab) {
        selectedTab.classList.add('active');
    }

    // Add active class to selected tab button
    const selectedButton = document.querySelector(`[onclick="showTab('${tabName}')"]`);
    if (selectedButton) {
        selectedButton.classList.add('active');
    }
}

// Parse and categorize events from the raw GSI data
function parseEvents(rawData) {
    if (!rawData) return [];

    const events = [];

    try {
        // Check if the data is already a JSON object or needs parsing
        let gameState;

        if (typeof rawData === 'object') {
            // Data is already an object
            gameState = rawData;
        } else if (typeof rawData === 'string') {
            // Try to parse as JSON, but handle fallback gracefully
            try {
                gameState = JSON.parse(rawData);
            } catch (parseError) {
                // If it's not valid JSON, it might be a fallback message
                console.log('[INFO] Raw data is not valid JSON, treating as fallback message:', rawData);

                // Create a basic event for the fallback message
                events.push({
                    type: 'info',
                    category: 'Fallback',
                    event: 'FallbackMessage',
                    data: {
                        message: rawData,
                        note: 'This is a fallback message when JSON serialization fails'
                    },
                    timestamp: new Date().toISOString()
                });

                return events;
            }
        } else {
            console.warn('[WARNING] Unexpected raw data type:', typeof rawData);
            return events;
        }

        // Add timestamp to the event
        const timestamp = new Date().toISOString();

        // Map events
        if (gameState.map) {
            events.push({
                type: 'map',
                category: 'Map',
                event: 'MapUpdated',
                data: gameState.map,
                timestamp: timestamp
            });
        }

        // Round events
        if (gameState.round) {
            events.push({
                type: 'round',
                category: 'Round',
                event: 'RoundUpdated',
                data: gameState.round,
                timestamp: timestamp
            });
        }

        // Player events
        if (gameState.player) {
            events.push({
                type: 'player',
                category: 'Player',
                event: 'PlayerUpdated',
                data: gameState.player,
                timestamp: timestamp
            });
        }

        // Bomb events
        if (gameState.bomb) {
            const bombState = gameState.bomb.state;
            let bombEvent = 'BombUpdated';

            // Determine specific bomb event based on state
            switch (bombState) {
                case 'planted':
                    bombEvent = 'BombPlanted';
                    break;
                case 'defused':
                    bombEvent = 'BombDefused';
                    break;
                case 'exploded':
                    bombEvent = 'BombExploded';
                    break;
                case 'carried':
                    bombEvent = 'BombCarried';
                    break;
                case 'dropped':
                    bombEvent = 'BombDropped';
                    break;
            }

            events.push({
                type: 'bomb',
                category: 'Bomb',
                event: bombEvent,
                data: gameState.bomb,
                timestamp: timestamp
            });
        }

        // Grenade events
        if (gameState.grenades) {
            Object.keys(gameState.grenades).forEach(grenadeType => {
                const grenade = gameState.grenades[grenadeType];
                if (grenade) {
                    events.push({
                        type: 'grenade',
                        category: 'Grenade',
                        event: 'GrenadeUpdated',
                        data: { type: grenadeType, ...grenade },
                        timestamp: timestamp
                    });
                }
            });
        }

        // Phase countdown events
        if (gameState.phase_countdowns) {
            events.push({
                type: 'round',
                category: 'Phase',
                event: 'PhaseCountdownUpdated',
                data: gameState.phase_countdowns,
                timestamp: timestamp
            });
        }

        // Provider events
        if (gameState.provider) {
            events.push({
                type: 'map',
                category: 'Provider',
                event: 'ProviderUpdated',
                data: gameState.provider,
                timestamp: timestamp
            });
        }

        // Auth events
        if (gameState.auth) {
            events.push({
                type: 'map',
                category: 'Auth',
                event: 'AuthUpdated',
                data: gameState.auth,
                timestamp: timestamp
            });
        }

        // All players events
        if (gameState.allplayers) {
            Object.keys(gameState.allplayers).forEach(playerId => {
                const player = gameState.allplayers[playerId];
                if (player) {
                    events.push({
                        type: 'player',
                        category: 'AllPlayers',
                        event: 'PlayerUpdated',
                        data: { id: playerId, ...player },
                        timestamp: timestamp
                    });
                }
            });
        }

        // Tournament draft events
        if (gameState.tournament_draft) {
            events.push({
                type: 'map',
                category: 'Tournament',
                event: 'TournamentDraftUpdated',
                data: gameState.tournament_draft,
                timestamp: timestamp
            });
        }

    } catch (error) {
        console.error('[ERROR] Failed to parse events:', error);
        events.push({
            type: 'error',
            category: 'Error',
            event: 'ParseError',
            data: {
                error: error.message,
                rawData: rawData,
                stack: error.stack
            },
            timestamp: new Date().toISOString()
        });
    }

    return events;
}

// Update event counts
function updateEventCounts(events) {
    eventCounts = {
        raw: 0,
        json: 0,
        error: 0,
        total: 0
    };

    events.forEach(event => {
        if (event.type === 'raw') eventCounts.raw++;
        if (event.type === 'json') eventCounts.json++;
        if (event.type === 'error') eventCounts.error++;
        eventCounts.total++;
    });

    // Update the UI
    getElement('raw-events-count').textContent = eventCounts.raw;
    getElement('json-events-count').textContent = eventCounts.json;
    getElement('error-events-count').textContent = eventCounts.error;
    getElement('total-events-count').textContent = eventCounts.total;
}

// Display events in different tabs
function displayEvents(events) {
    // Update event counts
    updateEventCounts(events);

    // Display all parsed events
    const parsedContent = getElement('parsed-events-content');
    if (parsedContent) {
        if (events.length > 0) {
            const formattedEvents = events.map(event => ({
                timestamp: event.timestamp,
                category: event.category,
                event: event.event,
                data: event.data
            }));
            parsedContent.innerHTML = `<pre><code>${JSON.stringify(formattedEvents, null, 2)}</code></pre>`;
        } else {
            parsedContent.innerHTML = '<em>No events parsed yet...</em>';
        }
    }

    // Display bomb events
    const bombContent = getElement('bomb-events-content');
    if (bombContent) {
        const bombEvents = events.filter(e => e.type === 'bomb');
        if (bombEvents.length > 0) {
            bombContent.innerHTML = `<pre><code>${JSON.stringify(bombEvents, null, 2)}</code></pre>`;
        } else {
            bombContent.innerHTML = '<em>No bomb events yet...</em>';
        }
    }

    // Display round events
    const roundContent = getElement('round-events-content');
    if (roundContent) {
        const roundEvents = events.filter(e => e.type === 'round');
        if (roundEvents.length > 0) {
            roundContent.innerHTML = `<pre><code>${JSON.stringify(roundEvents, null, 2)}</code></pre>`;
        } else {
            roundContent.innerHTML = '<em>No round events yet...</em>';
        }
    }

    // Display player events
    const playerContent = getElement('player-events-content');
    if (playerContent) {
        const playerEvents = events.filter(e => e.type === 'player');
        if (playerEvents.length > 0) {
            playerContent.innerHTML = `<pre><code>${JSON.stringify(playerEvents, null, 2)}</code></pre>`;
        } else {
            playerContent.innerHTML = '<em>No player events yet...</em>';
        }
    }

    // Display map events
    const mapContent = getElement('map-events-content');
    if (mapContent) {
        const mapEvents = events.filter(e => e.type === 'map');
        if (mapEvents.length > 0) {
            mapContent.innerHTML = `<pre><code>${JSON.stringify(mapEvents, null, 2)}</code></pre>`;
        } else {
            mapContent.innerHTML = '<em>No map events yet...</em>';
        }
    }
}

// Fetch data from the API
async function fetchData() {
    try {
        const response = await fetch('/api/last-raw-message');
        if (response.ok) {
            const data = await response.json();
            lastRawMessage = data;

            // Update status panel
            const messageCountElement = getElement('message-count');
            const lastUpdateElement = getElement('last-update');
            const currentMapElement = getElement('current-map');
            const connectionStatusElement = getElement('connection-status');

            if (messageCountElement) messageCountElement.textContent = data.messageCount || 0;
            if (lastUpdateElement) lastUpdateElement.textContent = data.lastMessageTime ? new Date(data.lastMessageTime).toLocaleTimeString() : 'Never';
            if (currentMapElement) currentMapElement.textContent = data.mapName || 'Unknown';

            if (connectionStatusElement) {
                connectionStatusElement.textContent = data.messageCount > 0 ? 'Connected' : 'Disconnected';
                connectionStatusElement.className = `status-indicator ${data.messageCount > 0 ? 'connected' : 'disconnected'}`;
            }

            // Update raw message display
            updateRawMessageDisplay(data);

            // Parse and display events
            if (data.rawMessage) {
                const events = parseEvents(data.rawMessage);
                eventHistory = events;
                displayEvents(events);
            }

            console.log('[RAW GSI DEBUG] Received raw message data:', data);
        } else {
            const rawMessageContent = getElement('raw-message-content');
            if (rawMessageContent) {
                rawMessageContent.innerHTML = '<em>Failed to fetch data: ' + response.status + '</em>';
            }
        }
    } catch (error) {
        const rawMessageContent = getElement('raw-message-content');
        if (rawMessageContent) {
            rawMessageContent.innerHTML = '<em>Error: ' + error.message + '</em>';
        }
        console.error('[ERROR] fetchData failed:', error);
    }
}

// Update raw message display
function updateRawMessageDisplay(data) {
    const rawMessageContent = getElement('raw-message-content');
    const messageTimestamp = getElement('message-timestamp');
    const messageSize = getElement('message-size');

    if (!rawMessageContent || !messageTimestamp || !messageSize) {
        console.error('[ERROR] Required DOM elements not found for updateRawMessageDisplay');
        return;
    }

    if (data.lastMessageTime) {
        messageTimestamp.textContent = new Date(data.lastMessageTime).toLocaleString();
    } else {
        messageTimestamp.textContent = 'Never';
    }

    if (data.rawMessage) {
        const size = new Blob([data.rawMessage]).size;
        messageSize.textContent = size + ' bytes';

        try {
            const jsonData = JSON.parse(data.rawMessage);
            rawMessageContent.innerHTML = '<pre><code>' + JSON.stringify(jsonData, null, 2) + '</code></pre>';
        } catch (e) {
            rawMessageContent.innerHTML = '<pre><code>' + data.rawMessage + '</code></pre>';
        }
    } else {
        messageSize.textContent = '0 bytes';
        rawMessageContent.innerHTML = '<em>No raw message available</em>';
    }
}

// Refresh data
function refreshData() {
    fetchData();
}

// Clear all data
function clearData() {
    const rawMessageContent = getElement('raw-message-content');
    const messageTimestamp = getElement('message-timestamp');
    const messageSize = getElement('message-size');
    const connectionStatusElement = getElement('connection-status');

    if (rawMessageContent) {
        rawMessageContent.innerHTML = '<em>Data cleared. Waiting for new GSI data...</em>';
    }
    if (messageTimestamp) {
        messageTimestamp.textContent = 'Never';
    }
    if (messageSize) {
        messageSize.textContent = '0 bytes';
    }
    if (connectionStatusElement) {
        connectionStatusElement.textContent = 'Disconnected';
        connectionStatusElement.className = 'status-indicator disconnected';
    }

    // Clear event counts
    eventCounts = { raw: 0, json: 0, error: 0, total: 0 };
    updateEventCounts([]);

    // Clear event history
    eventHistory = [];

    // Clear all tab contents
    const tabContents = ['parsed-events', 'bomb-events', 'round-events', 'player-events', 'map-events'];
    tabContents.forEach(tab => {
        const element = getElement(`${tab}-content`);
        if (element) {
            element.innerHTML = '<em>Data cleared...</em>';
        }
    });

    lastRawMessage = null;
}

// Export data as JSON
function exportData() {
    if (!lastRawMessage && eventHistory.length === 0) {
        alert('No data to export');
        return;
    }

    const exportData = {
        timestamp: new Date().toISOString(),
        rawMessage: lastRawMessage,
        events: eventHistory,
        eventCounts: eventCounts
    };

    const dataStr = JSON.stringify(exportData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });

    const link = document.createElement('a');
    link.href = URL.createObjectURL(dataBlob);
    link.download = `fragify-gsi-events-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.json`;
    link.click();
}

// Initialize the page
document.addEventListener('DOMContentLoaded', function() {
    console.log('[DEBUG] DOM loaded, starting Fragify CS2 GSI Event Tester');

    // Set initial tab
    showTab('raw');

    // Fetch initial data
    fetchData();

    // Set up auto-refresh every 2 seconds
    setInterval(fetchData, 2000);

    // Add debug info
    const debugContent = getElement('debug-content');
    if (debugContent) {
        debugContent.innerHTML = `
            <pre><code>Page initialized at: ${new Date().toISOString()}
Library: CounterStrike2GSI
Auto-refresh: Every 2 seconds
Tabs: Raw Data, Parsed Events, Bomb Events, Round Events, Player Events, Map Events
Features: Event parsing, categorization, counting, and export</code></pre>
        `;
    }
});
