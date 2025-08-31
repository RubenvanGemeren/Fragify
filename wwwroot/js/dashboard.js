

// SIMPLE THEME APPLICATION - Direct style changes
function applyTheme(theme) {
    console.log('Applying theme:', theme);

    const body = document.body;
    const headerTitle = document.getElementById('header-title');
    const headerSubtitle = document.getElementById('header-subtitle');
    const cards = document.querySelectorAll('.card');
    const cardHeaders = document.querySelectorAll('.card h2');
    const statValues = document.querySelectorAll('.stat-value');
    const statLabels = document.querySelectorAll('.stat-label');

    // Apply background
    if (theme.backgroundGradient) {
        body.style.background = theme.backgroundGradient;
        console.log('Set background to:', theme.backgroundGradient);
    }

    // Apply header colors
    if (theme.primaryColor) {
        headerTitle.style.color = theme.primaryColor;
        console.log('Set header title color to:', theme.primaryColor);
    }

    if (theme.textColor) {
        headerSubtitle.style.color = theme.textColor;
        console.log('Set header subtitle color to:', theme.textColor);
    }

    // Apply card colors
    if (theme.cardBackground) {
        cards.forEach(card => {
            card.style.background = theme.cardBackground;
        });
        console.log('Set card background to:', theme.cardBackground);
    }

    if (theme.cardBorder) {
        cards.forEach(card => {
            card.style.borderColor = theme.cardBorder;
        });
        console.log('Set card border to:', theme.cardBorder);
    }

    // Apply text colors
    if (theme.primaryColor) {
        cardHeaders.forEach(header => {
            header.style.color = theme.primaryColor;
            header.style.borderBottomColor = theme.primaryColor;
        });
        statValues.forEach(value => {
            value.style.color = theme.primaryColor;
        });
        console.log('Set primary colors to:', theme.primaryColor);
    }

    if (theme.textColor) {
        statLabels.forEach(label => {
            label.style.color = theme.textColor;
        });
        console.log('Set text colors to:', theme.textColor);
    }

    console.log('Theme application complete');
}

// Fetch and apply theme from API
async function updateTheme(mapName) {
    try {
        console.log('=== THEME UPDATE START ===');
        console.log('Fetching theme for map:', mapName);
        const response = await fetch(`/api/theme?mapName=${encodeURIComponent(mapName)}`);
        console.log('Theme response status:', response.status);

        if (response.ok) {
            const theme = await response.json();
            console.log('Theme received from API:', theme);
            console.log('Theme object keys:', Object.keys(theme));
            console.log('Theme properties:', {
                primaryColor: theme.primaryColor,
                backgroundGradient: theme.backgroundGradient,
                cardBackground: theme.cardBackground,
                textColor: theme.textColor
            });

            if (theme && typeof theme === 'object') {
                console.log('Theme is valid object, applying...');
                applyTheme(theme);
            } else {
                console.error('Theme is not a valid object:', typeof theme);
            }
        } else {
            console.error('Failed to fetch theme, status:', response.status);
            const errorText = await response.text();
            console.error('Error response:', errorText);
        }
        console.log('=== THEME UPDATE END ===');
    } catch (error) {
        console.error('Error fetching theme:', error);
    }
}

// Update map info display
async function updateMapInfo(gameMapName) {
    const currentGameMapSpan = document.getElementById('current-game-map');
    const mapNameDisplay = document.getElementById('map-name-display');
    const mapDisplay = document.getElementById('map-display');

    if (gameMapName && gameMapName !== 'Unknown') {
        currentGameMapSpan.textContent = gameMapName;
        mapNameDisplay.textContent = gameMapName;

        // Update map display to show map info
        mapDisplay.innerHTML = `
            <div class="map-active">
                <h3>🎯 ${gameMapName}</h3>
                <p>Map is currently active</p>
                <div class="map-status">
                    <span class="status-indicator active"></span>
                    <span>Connected to game</span>
                </div>
            </div>
        `;

        // Automatically apply theme based on the new map
        await updateTheme(gameMapName);
    } else {
        currentGameMapSpan.textContent = 'Unknown';
        mapNameDisplay.textContent = 'No map selected';

        // Show placeholder when no map is active
        mapDisplay.innerHTML = `
            <div class="map-placeholder">
                <p>Map display will appear here</p>
                <p class="map-placeholder-subtitle">Start a CS2 game to see map information</p>
            </div>
        `;
    }
}

async function updateDashboard(stats) {
    // Update game information
    document.getElementById('map-name').textContent = stats.mapName || 'Unknown';
    document.getElementById('game-mode').textContent = stats.gameMode || 'Unknown';
    document.getElementById('round-number').textContent = stats.roundNumber || '0';
    document.getElementById('round-phase').textContent = stats.roundPhase || 'Unknown';
    document.getElementById('score').textContent = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;

    // Update bomb state
    const bombStateElement = document.getElementById('bomb-state');
    if (bombStateElement) {
        if (stats.bombState) {
            bombStateElement.textContent = stats.bombState;
            // Add visual styling based on bomb state
            bombStateElement.className = 'stat-value bomb-state-' + stats.bombState.toLowerCase().replace(/\s+/g, '-');
        } else {
            bombStateElement.textContent = 'Unknown';
            bombStateElement.className = 'stat-value';
        }
    }

    // Update map info
    await updateMapInfo(stats.mapName);

    // Update map info details
    document.getElementById('game-mode-display').textContent = stats.gameMode || 'Unknown';
    document.getElementById('round-phase-display').textContent = stats.roundPhase || 'Unknown';

    // Update player statistics
    document.getElementById('player-kills').textContent = stats.playerKills || 0;
    document.getElementById('player-deaths').textContent = stats.playerDeaths || 0;
    document.getElementById('player-assists').textContent = stats.playerAssists || 0;
    document.getElementById('player-mvps').textContent = stats.playerMvps || 0;
    document.getElementById('player-score').textContent = stats.playerScore || 0;
    document.getElementById('player-team').textContent = stats.playerTeam || 'Unknown';

    // Update player status
    document.getElementById('player-health').textContent = stats.playerHealth || 100;
    document.getElementById('player-armor').textContent = stats.playerArmor || 100;
    document.getElementById('player-money').textContent = `$${(stats.playerMoney || 800).toLocaleString()}`;
    document.getElementById('active-weapon').textContent = stats.activeWeapon || 'Unknown';

    // Update session statistics
    document.getElementById('session-duration').textContent = stats.sessionDuration || '00:00:00';
    document.getElementById('total-rounds').textContent = stats.totalRounds || 0;
    document.getElementById('rounds-won').textContent = stats.roundsWon || 0;
    document.getElementById('rounds-lost').textContent = stats.roundsLost || 0;
    document.getElementById('win-rate').textContent = `${(stats.winRate || 0).toFixed(1)}%`;

    // Update debug information
    const isConnected = stats.isConnected || false;
    document.getElementById('connection-status').textContent = isConnected ? 'Connected' : 'Disconnected';
    document.getElementById('messages-received').textContent = stats.messagesReceived || 0;
    document.getElementById('last-message-time').textContent = stats.lastMessageTime || 'Never';
    document.getElementById('last-update').textContent = stats.lastMessageTime || 'Never';

    // Update last refresh time
    const now = new Date();
    document.getElementById('last-refresh').textContent = now.toLocaleTimeString();
}

async function fetchStats() {
    try {
        const response = await fetch('/api/stats');
        if (response.ok) {
            const stats = await response.json();
            await updateDashboard(stats);
        }
    } catch (error) {
        console.error('Failed to fetch stats:', error);
    }
}

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function() {
    fetchStats();
    setInterval(fetchStats, 2000);
});
