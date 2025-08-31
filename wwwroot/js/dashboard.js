

// SIMPLE THEME APPLICATION - Direct style changes
function applyTheme(theme) {
    console.log('🎨 Applying theme:', theme);

    const body = document.body;
    const headerTitle = document.getElementById('header-title');
    const headerSubtitle = document.getElementById('header-subtitle');
    const cards = document.querySelectorAll('.card');
    const cardHeaders = document.querySelectorAll('.card h2');
    const statValues = document.querySelectorAll('.stat-value');
    const statLabels = document.querySelectorAll('.stat-label');

    console.log('🔍 Found elements:', {
        headerTitle: !!headerTitle,
        headerSubtitle: !!headerSubtitle,
        cards: cards.length,
        cardHeaders: cardHeaders.length,
        statValues: statValues.length,
        statLabels: statLabels.length
    });

    // Apply background
    if (theme.backgroundGradient) {
        body.style.background = theme.backgroundGradient;
        console.log('✅ Set background to:', theme.backgroundGradient);
    }

    // Apply header colors
    if (theme.primaryColor) {
        headerTitle.style.color = theme.primaryColor;
        console.log('✅ Set header title color to:', theme.primaryColor);
    }

    if (theme.textColor) {
        headerSubtitle.style.color = theme.textColor;
        console.log('✅ Set header subtitle color to:', theme.textColor);
    }

    // Apply card colors
    if (theme.cardBackground) {
        cards.forEach(card => {
            card.style.background = theme.cardBackground;
        });
        console.log('✅ Set card background to:', theme.cardBackground);
    }

    if (theme.cardBorder) {
        cards.forEach(card => {
            card.style.borderColor = theme.cardBorder;
        });
        console.log('✅ Set card border to:', theme.cardBorder);
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
        console.log('✅ Set primary colors to:', theme.primaryColor);
    }

    if (theme.textColor) {
        statLabels.forEach(label => {
            label.style.color = theme.textColor;
        });
        console.log('✅ Set text colors to:', theme.textColor);
    }

    console.log('🎨 Theme application complete');
}

// Fetch and apply theme from API
async function updateTheme(mapName) {
    try {
        console.log('=== 🎨 THEME UPDATE START ===');
        console.log('🔍 Fetching theme for map:', mapName);
        const response = await fetch(`/api/theme?mapName=${encodeURIComponent(mapName)}`);
        console.log('📡 Theme response status:', response.status);

        if (response.ok) {
            const theme = await response.json();
            console.log('📦 Theme received from API:', theme);
            console.log('🔑 Theme object keys:', Object.keys(theme));
            console.log('🎨 Theme properties:', {
                primaryColor: theme.primaryColor,
                backgroundGradient: theme.backgroundGradient,
                cardBackground: theme.cardBackground,
                textColor: theme.textColor
            });

            if (theme && typeof theme === 'object') {
                console.log('✅ Theme is valid object, applying...');
                applyTheme(theme);
            } else {
                console.error('❌ Theme is not a valid object:', typeof theme);
            }
        } else {
            console.error('❌ Failed to fetch theme, status:', response.status);
            const errorText = await response.text();
            console.error('❌ Error response:', errorText);
        }
        console.log('=== 🎨 THEME UPDATE END ===');
    } catch (error) {
        console.error('❌ Error fetching theme:', error);
    }
}

// Update map info display
async function updateMapInfo(gameMapName) {
    console.log('🗺️ Updating map info for:', gameMapName);

    const currentGameMapSpan = document.getElementById('current-game-map');
    const mapDisplay = document.getElementById('map-display');

    console.log('🔍 Map elements found:', {
        currentGameMapSpan: !!currentGameMapSpan,
        mapDisplay: !!mapDisplay
    });

    if (gameMapName && gameMapName !== 'Unknown') {
        currentGameMapSpan.textContent = gameMapName;
        console.log('✅ Updated current game map span');

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
        console.log('✅ Updated map display HTML');

        // Automatically apply theme based on the new map
        await updateTheme(gameMapName);
    } else {
        currentGameMapSpan.textContent = 'Unknown';
        console.log('✅ Set map to Unknown');

        // Show placeholder when no map is active
        mapDisplay.innerHTML = `
            <div class="map-placeholder">
                <p>Map display will appear here</p>
                <p class="map-placeholder-subtitle">Start a CS2 game to see map information</p>
            </div>
        `;
        console.log('✅ Updated map display to placeholder');
    }
}

async function updateDashboard(stats) {
    console.log('📊 Updating dashboard with stats:', stats);

    try {
        // Update game information
        console.log('🎮 Updating game information...');
        const mapNameElement = document.getElementById('map-name');
        const gameModeElement = document.getElementById('game-mode');
        const roundNumberElement = document.getElementById('round-number');
        const roundPhaseElement = document.getElementById('round-phase');
        const scoreElement = document.getElementById('score');

        console.log('🔍 Game info elements found:', {
            mapName: !!mapNameElement,
            gameMode: !!gameModeElement,
            roundNumber: !!roundNumberElement,
            roundPhase: !!roundPhaseElement,
            score: !!scoreElement
        });

        if (mapNameElement) mapNameElement.textContent = stats.mapName || 'Unknown';
        if (gameModeElement) gameModeElement.textContent = stats.gameMode || 'Unknown';
        if (roundNumberElement) roundNumberElement.textContent = stats.roundNumber || '0';
        if (roundPhaseElement) roundPhaseElement.textContent = stats.roundPhase || 'Unknown';
        if (scoreElement) {
            const scoreText = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;
            scoreElement.textContent = scoreText;
            console.log('🎯 Score updated:', { scoreT: stats.scoreT, scoreCT: stats.scoreCT, displayText: scoreText });
        }
        console.log('✅ Game information updated');

        // Update map info
        console.log('🗺️ Updating map info...');
        await updateMapInfo(stats.mapName);

        // Update player statistics
        console.log('👤 Updating player statistics...');
        const playerKillsElement = document.getElementById('player-kills');
        const playerDeathsElement = document.getElementById('player-deaths');
        const playerAssistsElement = document.getElementById('player-assists');
        const playerMvpsElement = document.getElementById('player-mvps');
        const playerScoreElement = document.getElementById('player-score');
        const playerTeamElement = document.getElementById('player-team');

        console.log('🔍 Player stat elements found:', {
            kills: !!playerKillsElement,
            deaths: !!playerDeathsElement,
            assists: !!playerAssistsElement,
            mvps: !!playerMvpsElement,
            score: !!playerScoreElement,
            team: !!playerTeamElement
        });

        if (playerKillsElement) playerKillsElement.textContent = stats.playerKills || 0;
        if (playerDeathsElement) playerDeathsElement.textContent = stats.playerDeaths || 0;
        if (playerAssistsElement) playerAssistsElement.textContent = stats.playerAssists || 0;
        if (playerMvpsElement) playerMvpsElement.textContent = stats.playerMVPs || 0;
        if (playerScoreElement) playerScoreElement.textContent = stats.playerScore || 0;
        if (playerTeamElement) playerTeamElement.textContent = stats.playerTeam || 'Unknown';
        console.log('✅ Player statistics updated');

        // Update player status
        console.log('📊 Updating player status...');
        const playerHealthElement = document.getElementById('player-health');
        const playerArmorElement = document.getElementById('player-armor');
        const playerMoneyElement = document.getElementById('player-money');
        const activeWeaponElement = document.getElementById('active-weapon');

        console.log('🔍 Player status elements found:', {
            health: !!playerHealthElement,
            armor: !!playerArmorElement,
            money: !!playerMoneyElement,
            weapon: !!activeWeaponElement
        });

        if (playerHealthElement) playerHealthElement.textContent = stats.playerHealth || 100;
        if (playerArmorElement) playerArmorElement.textContent = stats.playerArmor || 100;
        if (playerMoneyElement) playerMoneyElement.textContent = `$${(stats.playerMoney || 800).toLocaleString()}`;
        if (activeWeaponElement) activeWeaponElement.textContent = stats.activeWeapon || 'Unknown';
        console.log('✅ Player status updated');

        // Update session statistics
        console.log('📈 Updating session statistics...');
        const sessionDurationElement = document.getElementById('session-duration');
        const totalRoundsElement = document.getElementById('total-rounds');
        const roundsWonElement = document.getElementById('rounds-won');
        const roundsLostElement = document.getElementById('rounds-lost');
        const winRateElement = document.getElementById('win-rate');

        console.log('🔍 Session stat elements found:', {
            duration: !!sessionDurationElement,
            totalRounds: !!totalRoundsElement,
            roundsWon: !!roundsWonElement,
            roundsLost: !!roundsLostElement,
            winRate: !!winRateElement
        });

        if (sessionDurationElement) sessionDurationElement.textContent = stats.sessionDuration || '00:00:00';
        if (totalRoundsElement) totalRoundsElement.textContent = stats.totalRounds || 0;
        if (roundsWonElement) roundsWonElement.textContent = stats.roundsWon || 0;
        if (roundsLostElement) roundsLostElement.textContent = stats.roundsLost || 0;
        if (winRateElement) winRateElement.textContent = `${(stats.winRate || 0).toFixed(1)}%`;
        console.log('✅ Session statistics updated');

        // Update debug information
        console.log('🔍 Updating debug information...');
        const connectionStatusElement = document.getElementById('connection-status');
        const messagesReceivedElement = document.getElementById('messages-received');
        const lastMessageTimeElement = document.getElementById('last-message-time');
        const lastUpdateElement = document.getElementById('last-update');

        console.log('🔍 Debug elements found:', {
            connectionStatus: !!connectionStatusElement,
            messagesReceived: !!messagesReceivedElement,
            lastMessageTime: !!lastMessageTimeElement,
            lastUpdate: !!lastUpdateElement
        });

        const isConnected = stats.isConnected || false;
        if (connectionStatusElement) connectionStatusElement.textContent = isConnected ? 'Connected' : 'Disconnected';
        if (messagesReceivedElement) messagesReceivedElement.textContent = stats.messagesReceived || 0;
        if (lastMessageTimeElement) lastMessageTimeElement.textContent = stats.lastMessageTime || 'Never';
        if (lastUpdateElement) lastUpdateElement.textContent = stats.lastMessageTime || 'Never';
        console.log('✅ Debug information updated');

        // Update last refresh time
        console.log('⏰ Updating refresh time...');
        const lastRefreshElement = document.getElementById('last-refresh');
        console.log('🔍 Last refresh element found:', !!lastRefreshElement);

        if (lastRefreshElement) {
            const now = new Date();
            lastRefreshElement.textContent = now.toLocaleTimeString();
            console.log('✅ Refresh time updated to:', now.toLocaleTimeString());
        }

        console.log('🎉 Dashboard update complete!');
    } catch (error) {
        console.error('❌ Error updating dashboard:', error);
        console.error('❌ Error details:', {
            message: error.message,
            stack: error.stack
        });
    }
}

async function fetchStats() {
    try {
        console.log('📡 Fetching stats from API...');
        const response = await fetch('/api/stats');
        console.log('📡 API response status:', response.status);

        if (response.ok) {
            const stats = await response.json();
            console.log('📦 Stats received:', stats);
            console.log('🎯 Score values from API:', { scoreT: stats.scoreT, scoreCT: stats.scoreCT });
            await updateDashboard(stats);
        } else {
            console.error('❌ API request failed with status:', response.status);
            const errorText = await response.text();
            console.error('❌ Error response:', errorText);
        }
    } catch (error) {
        console.error('❌ Failed to fetch stats:', error);
        console.error('❌ Error details:', {
            message: error.message,
            stack: error.stack
        });
    }
}

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 DOM loaded, initializing dashboard...');

    // Check if all required elements exist
    const requiredElements = [
        'header-title', 'header-subtitle', 'current-game-map', 'map-display',
        'map-name', 'game-mode', 'round-number', 'round-phase', 'score',
        'player-kills', 'player-deaths', 'player-assists', 'player-mvps', 'player-score', 'player-team',
        'player-health', 'player-armor', 'player-money', 'active-weapon',
        'session-duration', 'total-rounds', 'rounds-won', 'rounds-lost', 'win-rate',
        'connection-status', 'messages-received', 'last-message-time', 'last-update', 'last-refresh'
    ];

    console.log('🔍 Checking required elements...');
    const missingElements = [];
    requiredElements.forEach(id => {
        const element = document.getElementById(id);
        if (!element) {
            missingElements.push(id);
        }
    });

    if (missingElements.length > 0) {
        console.error('❌ Missing elements:', missingElements);
    } else {
        console.log('✅ All required elements found');
    }

    console.log('🚀 Starting dashboard...');
    fetchStats();
    setInterval(fetchStats, 2000);
    console.log('✅ Dashboard initialized with 2-second refresh interval');
});
