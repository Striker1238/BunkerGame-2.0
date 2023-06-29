using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLobby : MonoBehaviour
{
    [HideInInspector]public string IndexThisLobby;
    public void OnSelectLobby() => FindObjectOfType<Lobby_INSPECTOR>().SelectLobbyForConnect(IndexThisLobby);
}
