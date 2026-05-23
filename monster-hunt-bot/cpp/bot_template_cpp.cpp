#include <iostream>
#include <string>
#include <thread>
#include <chrono>
#include <curl/curl.h>
#include "json.hpp"

using json = nlohmann::json;

size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

class BotTemplate {
    std::string serverUrl, gameId, botName;
    int playerId = -1;
    CURL* curl;

    bool httpGet(const std::string& url, std::string& response) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1L);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);
        return curl_easy_perform(curl) == CURLE_OK;
    }

public:
    BotTemplate(const std::string& url, const std::string& gid, const std::string& name)
        : serverUrl(url), gameId(gid), botName(name) {
        if (!serverUrl.empty() && serverUrl.back() == '/') serverUrl.pop_back();
        curl = curl_easy_init();
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 5L);
    }

    ~BotTemplate() { if (curl) curl_easy_cleanup(curl); }

    json getGameState() {
        std::string url = serverUrl + "/game/state/" + gameId;
        std::string response;
        return httpGet(url, response) ? json::parse(response) : nullptr;
    }

    bool findMyPlayerId(const json& state) {
        if (state.is_null()) return false;
        for (auto& [key, player] : state["Players"].items()) {
            if (player["Name"] == botName) {
                playerId = player["Id"];
                return true;
            }
        }
        return false;
    }

    bool isMyTurn(const json& state) {
        if (state.is_null() || playerId == -1) return false;
        auto myPlayer = state["Players"][std::to_string(playerId)];
        bool isFirst = myPlayer["First"];
        std::string gameState = state["GameState"];
        return (isFirst && gameState == "Player1Turn") || (!isFirst && gameState == "Player2Turn");
    }

    bool isGameOver(const json& state) {
        if (state.is_null()) return false;
        std::string gameState = state["GameState"];
        return gameState == "Ending";
    }
};

int main(int argc, char* argv[]) {
    if (argc < 4) {
        std::cout << "Usage: bot_template <server_url> <game_id> <bot_name>" << std::endl;
        return 1;
    }

    BotTemplate bot(argv[1], argv[2], argv[3]);

    while (true) {
        auto state = bot.getGameState();
        if (!state.is_null() && bot.findMyPlayerId(state)) break;
        std::this_thread::sleep_for(std::chrono::milliseconds(500));
    }

    std::cout << "Connected\n" << std::endl;

    auto state = bot.getGameState();
    while (!state.is_null() && !bot.isGameOver(state)) {
        if (bot.isMyTurn(state)) {
            std::cout << "My turn!" << std::endl;
            // TODO: Implement your strategy here
            // See README.md for available endpoints
            
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
            state = bot.getGameState();
        } else {
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
            state = bot.getGameState();
        }
    }
    
    std::cout << "Game " << argv[2] << " has ended. Exiting..." << std::endl;
    return 0;
}
