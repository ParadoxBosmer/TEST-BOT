/**
 * Monster Hunt Bot - JavaScript Template
 * Usage: node bot_template.js <server_url> <game_id> <bot_name>
 * See README.md for full API documentation
 */
const axios = require('axios');

class BotTemplate {
    constructor(serverUrl, gameId, botName) {
        this.serverUrl = serverUrl.replace(/\/$/, '');
        this.gameId = gameId;
        this.botName = botName;
        this.playerId = null;
    }

    async getGameState() {
        const url = `${this.serverUrl}/game/state/${this.gameId}`;
        const response = await axios.get(url, { timeout: 5000 });
        return response.status === 200 ? response.data : null;
    }

    async findMyPlayerId(gameState) {
        const players = gameState.Players || {};
        for (const [playerId, player] of Object.entries(players)) {
            if (player.Name === this.botName) {
                this.playerId = player.Id;
                return true;
            }
        }
        return false;
    }

    isMyTurn(gameState) {
        if (!gameState || !this.playerId) return false;
        const players = gameState.Players || {};
        const myPlayer = players[this.playerId.toString()] || players[this.playerId];
        if (!myPlayer) return false;
        const isFirst = myPlayer.First || false;
        const gameStateStr = gameState.GameState || '';
        return (isFirst && gameStateStr === 'Player1Turn') || (!isFirst && gameStateStr === 'Player2Turn');
    }

    isGameOver(gameState) {
        if (!gameState) return false;
        return (gameState.GameState || '') === 'Ending';
    }
}

(async () => {
    if (process.argv.length < 5) {
        console.log('Usage: node bot_template.js <server_url> <game_id> <bot_name>');
        process.exit(1);
    }

    const bot = new BotTemplate(process.argv[2], process.argv[3], process.argv[4]);

    while (true) {
        const state = await bot.getGameState();
        if (state && await bot.findMyPlayerId(state)) break;
        await new Promise(r => setTimeout(r, 500));
    }

    console.log(`Connected as Player ${bot.playerId}\n`);

    let state = await bot.getGameState();
    while (state && !bot.isGameOver(state)) {
        if (bot.isMyTurn(state)) {
            console.log('My turn!');
            // TODO: Implement your strategy here
            // See README.md for available endpoints
            
            await new Promise(r => setTimeout(r, 500));
            state = await bot.getGameState();
        } else {
            await new Promise(r => setTimeout(r, 500));
            state = await bot.getGameState();
        }
    }
    console.log(`Game ${bot.gameId} has ended. Exiting...`);
})();
