/*
 * Monster Hunt Bot - Java Template
 * Usage: java BotTemplate <server_url> <game_id> <bot_name>
 * See README.md for full API documentation
 */
package com.monsterhunt.bot;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

import com.google.gson.Gson;
import com.google.gson.JsonObject;

public class BotTemplateJava {
    private final HttpClient http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(5)).build();
    private final String serverUrl, gameId, botName;
    private Integer playerId;
    private final Gson gson = new Gson();

    public BotTemplateJava(String serverUrl, String gameId, String botName) {
        this.serverUrl = serverUrl.replaceAll("/$", "");
        this.gameId = gameId;
        this.botName = botName;
    }

    public JsonObject getGameState() {
        try {
            var url = String.format("%s/game/state/%s", serverUrl, gameId);
            var request = HttpRequest.newBuilder().uri(URI.create(url)).timeout(Duration.ofSeconds(5)).GET().build();
            var response = http.send(request, HttpResponse.BodyHandlers.ofString());
            return response.statusCode() == 200 ? gson.fromJson(response.body(), JsonObject.class) : null;
        } catch (Exception e) {
            return null;
        }
    }

    public boolean findMyPlayerId(JsonObject state) {
        if (state == null) return false;
        var players = state.getAsJsonObject("Players");
        for (var entry : players.entrySet()) {
            var player = entry.getValue().getAsJsonObject();
            if (player.get("Name").getAsString().equals(botName)) {
                playerId = player.get("Id").getAsInt();
                return true;
            }
        }
        return false;
    }

    public boolean isMyTurn(JsonObject state) {
        if (state == null || playerId == null) return false;
        var myPlayer = state.getAsJsonObject("Players").getAsJsonObject(playerId.toString());
        boolean isFirst = myPlayer.get("First").getAsBoolean();
        String gameState = state.get("GameState").getAsString();
        return (isFirst && gameState.equals("Player1Turn")) || (!isFirst && gameState.equals("Player2Turn"));
    }

    public boolean isGameOver(JsonObject state) {
        if (state == null) return false;
        return state.get("GameState").getAsString().equals("Ending");
    }

    public static void main(String[] args) throws Exception {
        if (args.length < 3) {
            System.out.println("Usage: java BotTemplate <server_url> <game_id> <bot_name>");
            System.exit(1);
        }

        var bot = new BotTemplateJava(args[0], args[1], args[2]);

        while (true) {
            var state = bot.getGameState();
            if (state != null && bot.findMyPlayerId(state)) break;
            Thread.sleep(500);
        }

        System.out.println("Connected as Player " + bot.playerId + "\n");

        var state = bot.getGameState();
        while (state != null && !bot.isGameOver(state)) {
            if (bot.isMyTurn(state)) {
                System.out.println("My turn!");
                // TODO: Implement your strategy here
                // See README.md for available endpoints
                
                Thread.sleep(500);
                state = bot.getGameState();
            } else {
                Thread.sleep(500);
                state = bot.getGameState();
            }
        }
        System.out.println("Game " + bot.gameId + " has ended. Exiting...");
    }
}
