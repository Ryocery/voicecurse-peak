using System.Linq;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Handlers;

public class NetworkHandler : IOnEventCallback {
    private const byte VoiceCurseEventCode = 187; 
    private static MonoBehaviour? _connectionLog;
    private static MethodInfo? _addMessageMethod;
    public static string? CurrentEventDetail { get; private set; }

    public static void SendCurseEvent(string spokenWord, string matchedKeyword, string eventName, string? detail, Vector3 position) {
        object[] content = [
            PhotonNetwork.LocalPlayer.ActorNumber,
            spokenWord,
            matchedKeyword,
            eventName,
            detail ?? string.Empty,
            position
        ];

        RaiseEventOptions raiseEventOptions = new() { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new() { Reliability = true };

        PhotonNetwork.RaiseEvent(VoiceCurseEventCode, content, raiseEventOptions, sendOptions);
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code != VoiceCurseEventCode) return;

        object[] data = (object[])photonEvent.CustomData;
        int actorNumber = (int)data[0];
        string spokenWord = (string)data[1];
        string matchedKeyword = (string)data[2];
        string eventName = (string)data[3];
        string detail = (string)data[4];
        Vector3 position = (Vector3)data[5];

        string charName = "Unknown";
        Color playerColor = Color.white;

        Photon.Realtime.Player? photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (photonPlayer != null) {
            charName = photonPlayer.NickName;
        }

        Character? character = FindCharacterByActorNumber(actorNumber);
        if (character) {
            if (character.refs?.customization) {
                playerColor = character.refs.customization.PlayerColor;
            }
        }
        
        string displayDetail = detail;
        if (!string.IsNullOrEmpty(detail) && detail.Contains('|')) {
            displayDetail = detail.Split('|')[0];
        }

        DisplayNotification(charName, playerColor, spokenWord, matchedKeyword, eventName, displayDetail);

        if (!EventHandler.Events.TryGetValue(eventName, out IVoiceEvent evt)) return;

        CurrentEventDetail = detail;
        try {
            if (character) {
                evt.PlayEffects(character, position);
            } else {
                evt.PlayEffects(position);
            }
        } finally {
            CurrentEventDetail = null;
        }
    }

    private static Character? FindCharacterByActorNumber(int actorNumber) {
        return Character.AllCharacters.FirstOrDefault(c => c.photonView.OwnerActorNr == actorNumber);
    }

    private static void DisplayNotification(string playerName, Color color, string fullWord, string keyword, string eventName, string detail) {
        if (!_connectionLog) {
            _connectionLog = Object.FindFirstObjectByType(System.Type.GetType("PlayerConnectionLog, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")) as MonoBehaviour 
                           ?? Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(m => m.GetType().Name == "PlayerConnectionLog");
            _addMessageMethod = null;
        }

        if (!_connectionLog) return;
        
        if (_addMessageMethod == null) { 
            _addMessageMethod = _connectionLog.GetType().GetMethod("AddMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        if (_addMessageMethod == null) return;

        string playerHex = "#" + ColorUtility.ToHtmlStringRGB(color);
        string displayString = fullWord;
        int index = fullWord.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase);

        if (index >= 0) {
            string prefix = fullWord[..index];
            string match = fullWord.Substring(index, keyword.Length);
            string suffix = fullWord[(index + keyword.Length)..];
            displayString = $"{prefix}<color=#8B0000>{match}</color>{suffix}";
        }

        string finalMessage = $"<color={playerHex}>{playerName} said \"{displayString}\" which triggered </color><color=#FFA500>{eventName}</color><color=#FAFA33>{(string.IsNullOrEmpty(detail) ? "" : $" ({detail})")}</color>";

        try {
            _addMessageMethod.Invoke(_connectionLog, [finalMessage]);
        } catch { 
            _connectionLog = null;
        }
    }
}