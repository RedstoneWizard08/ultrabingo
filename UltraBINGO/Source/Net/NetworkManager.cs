using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UltraBINGO.Types;
using UltraBINGO.UI;
using UltraBINGO.Util;
using UnityEngine.Networking;

namespace UltraBINGO.Net;

public class NetworkManager {
    private const int MaxReconnectionAttempts = 3;

    public readonly SocketManager Socket;

    private int _currentReconnection;
    private Types.State _currentState = Types.State.Normal;
    private readonly string _serverMapPoolCatalogURL;

    public NetworkManager() {
        var host = Main.ModConfig.ServerHost.Value;
        var port = Main.ModConfig.ServerPort.Value;

        _serverMapPoolCatalogURL = $"http://{host}:{port}/bingoMapPool.toml";

        Socket = new SocketManager(host, port);
        Socket.OnConnect += ActionQueue.Process;
        Socket.OnError += HandleError;
    }

    public void SetState(Types.State newState) {
        _currentState = newState;
    }

    /// <summary>
    /// Fetch the bingo map catalog from the server.
    /// </summary>
    public async Task<string?> FetchCatalog() {
        try {
            using var client = new HttpClient();

            return await client.GetStringAsync(_serverMapPoolCatalogURL);
        } catch (Exception e) {
            Logging.Error("Something went wrong while fetching from the URL");
            Logging.Error(e.Message);
            return null;
        }
    }

    private void TryReconnect() {
        ActionQueue.PendingAction = AsyncAction.ReconnectGame;
        Socket.Connect();
    }

    private void HandleError() {
        // If player is on main menu
        switch (_currentState) {
            case Types.State.InMenu:
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to contact server.");
                BingoMainMenu.UnlockUI();
                break;

            case Types.State.InLobby:
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                    "Connection to the lobby was lost. Returning to menu."
                );

                GameManager.ClearGameVariables();
                SetState(Types.State.InMenu);

                BingoEncapsulator.BingoCardScreen?.SetActive(false);
                BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
                BingoEncapsulator.BingoEndScreen?.SetActive(false);
                BingoEncapsulator.BingoMenu?.SetActive(true);

                break;

            case Types.State.InGame:
                // Handle in-game reconnection attempts here
                if (GameManager.IsInBingoLevel) {
                    _currentReconnection++;

                    if (_currentReconnection > MaxReconnectionAttempts) {
                        _currentReconnection = 0;

                        GameManager.ClearGameVariables();
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                            "Failed to reconnect. Exitting game in 5 seconds."
                        );
                        SetState(Types.State.Normal);

                        Thread.Sleep(5000);
                        SceneHelper.LoadScene("Main Menu");
                    } else {
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                            $"Connection to the game was lost.\nAttempting reconnection... <color=orange>({_currentReconnection}/{MaxReconnectionAttempts})</color>"
                        );
                        Thread.Sleep(2000);

                        TryReconnect();
                    }
                }

                break;

            case Types.State.InBrowser:
                if (!BingoBrowser.fetchDone)
                    BingoBrowser.DisplayError();
                else
                    BingoBrowser.UnlockUI();
                break;

            case Types.State.Normal:
            default: break;
        }
    }
}