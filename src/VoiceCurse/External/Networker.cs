using System.Linq;
using System.Reflection;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using VoiceCurse.Core;
using VoiceCurse.Events;

namespace VoiceCurse.Networking;

public class Networker : IOnEventCallback {
    private const byte VoiceCurseEventCode = 187; 
    private static MonoBehaviour? _connectionLog;
    private static MethodInfo? _addMessageMethod;

    public static void SendCurseEvent(string spokenWord, string matchedKeyword, string eventName, Vector3 position) {
        object[] content = [
            PhotonNetwork.LocalPlayer.ActorNumber,
            spokenWord,
            matchedKeyword,
            eventName,
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
        Vector3 position = (Vector3)data[4];

        string charName = "Unknown";
        Color playerColor = Color.white;

        Photon.Realtime.Player? photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (photonPlayer != null) {
            charName = photonPlayer.NickName;
        }

        Character? character = FindCharacterByActorNumber(actorNumber);
        if (character != null) {
            if (character.refs?.customization != null) {
                playerColor = character.refs.customization.PlayerColor;
            }
        }

        DisplayNotification(charName, playerColor, spokenWord, matchedKeyword, eventName);

        if (VoiceEventHandler.Events.TryGetValue(eventName, out IVoiceEvent evt)) {
            evt.PlayEffects(position);
        }
    }

    private Character? FindCharacterByActorNumber(int actorNumber) {
        return Character.AllCharacters.FirstOrDefault(c => c.photonView.OwnerActorNr == actorNumber);
    }

    private void DisplayNotification(string playerName, Color color, string fullWord, string keyword, string eventName) {
        if (_connectionLog == null) {
            _connectionLog = Object.FindFirstObjectByType(System.Type.GetType("PlayerConnectionLog, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")) as MonoBehaviour 
                           ?? Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(m => m.GetType().Name == "PlayerConnectionLog");
            _addMessageMethod = null;
        }

        if (_connectionLog == null) return;
        
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

        string finalMessage = $"<color={playerHex}>{playerName} said \"{displayString}\" which triggered </color><color=#FFA500>{eventName}</color>";

        try {
            _addMessageMethod.Invoke(_connectionLog, [finalMessage]);
        } catch { 
            _connectionLog = null;
        }
    }
}