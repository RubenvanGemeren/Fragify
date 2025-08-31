let comprehensiveMapData = {};

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

// Test themes
function testManualTheme() {
    const theme = {
        backgroundGradient: 'linear-gradient(135deg, #1A0F0F 0%, #8B0000 50%, #FF4500 100%)',
        primaryColor: '#FF4500',
        textColor: '#FFE4E1',
        cardBackground: 'rgba(26, 15, 15, 0.2)',
        cardBorder: 'rgba(255, 69, 0, 0.3)'
    };
    applyTheme(theme);
}

function testDust2Theme() {
    const theme = {
        backgroundGradient: 'linear-gradient(135deg, #8B4513 0%, #D4AF37 50%, #F4A460 100%)',
        primaryColor: '#d4af37',
        textColor: '#F5DEB3',
        cardBackground: 'rgba(139, 69, 19, 0.2)',
        cardBorder: 'rgba(212, 175, 55, 0.3)'
    };
    applyTheme(theme);
}

function testMirageTheme() {
    const theme = {
        backgroundGradient: 'linear-gradient(135deg, #2F1B14 0%, #8B4513 50%, #DAA520 100%)',
        primaryColor: '#8B4513',
        textColor: '#F5DEB3',
        cardBackground: 'rgba(47, 27, 20, 0.2)',
        cardBorder: 'rgba(139, 69, 19, 0.3)'
    };
    applyTheme(theme);
}

function resetTheme() {
    const theme = {
        backgroundGradient: 'linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)',
        primaryColor: '#4ade80',
        textColor: 'white',
        cardBackground: 'rgba(255, 255, 255, 0.1)',
        cardBorder: 'rgba(255, 255, 255, 0.2)'
    };
    applyTheme(theme);
}

function loadSelectedMap() {
    const mapSelect = document.getElementById('map-select');
    const selectedMap = mapSelect.value;
    const mapDisplay = document.getElementById('map-display');

    if (!selectedMap) {
        mapDisplay.innerHTML = '<div class="map-placeholder"><p>Select a map to view</p></div>';
        return;
    }

    const mapData = comprehensiveMapData[selectedMap];
    if (mapData && mapData.imageUrl) {
        mapDisplay.innerHTML = '<img src="' + mapData.imageUrl + '" alt="Map minimap" style="width: 100%; height: 100%; object-fit: contain;">';
    } else {
        mapDisplay.innerHTML = '<div class="map-placeholder"><p>Map not available: ' + selectedMap + '</p></div>';
    }
}

async function updateMapSelector(gameMapName) {
    const mapSelect = document.getElementById('map-select');
    const currentGameMapSpan = document.getElementById('current-game-map');

    if (gameMapName && gameMapName !== 'Unknown') {
        currentGameMapSpan.textContent = gameMapName;
        mapSelect.value = gameMapName;
        loadSelectedMap();

        // Automatically apply theme based on the new map
        await updateTheme(gameMapName);
    } else {
        currentGameMapSpan.textContent = 'Unknown';
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

    // Update map selector
    await updateMapSelector(stats.mapName);

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

async function loadComprehensiveMapData() {
    try {
        const response = await fetch('/api/maps');
        if (response.ok) {
            comprehensiveMapData = await response.json();
            console.log('Loaded map data:', Object.keys(comprehensiveMapData).length, 'maps');
        }
    } catch (error) {
        console.error('Error loading map data:', error);
    }
}

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function() {
    loadComprehensiveMapData();
    fetchStats();
    setInterval(fetchStats, 2000);
});
