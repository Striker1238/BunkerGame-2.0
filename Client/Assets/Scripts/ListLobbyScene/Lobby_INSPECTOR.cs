using BunkerGame.ClassLobby;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BunkerGame.ClassHero;
public class Lobby_INSPECTOR : MonoBehaviour
{
    [Header("Lobby objects")]
    public GameObject LobbyPrefab;
    public Transform LobbyPerent;

    [Header("Lobby settings for create")]
    public TMP_InputField NameLobby_IF;
    public Slider MaxPlayers_Slider;
    public Toggle AccessLobby_Toggle;
    public TMP_InputField PasswordLobby_IF;


    public List<Lobby> AllLobby = new List<Lobby>();//[HideInInspector]
    public string IndexSelectLobby = "";

    public void Start()
    {
        GetRequestListLobby();
    }




    /// <summary>
    /// Запрос данных о всех лобби
    /// </summary>
    public void GetRequestListLobby()
    {
        AllLobby.Clear();
        FindObjectOfType<Client_INSPECTOR>().GetListLobby();
    }

    /// <summary>
    /// Получение данных с сервера
    /// </summary>
    /// <param name="data"> Данные пресланные сервером</param>
    public void AddLobbyInList(string data)
    {
        Lobby? lobby = JsonUtility.FromJson<Lobby>(data);
        //Выходим если пришло ошибочное лобби
        if (lobby == null || lobby.IsStart || lobby.IsEnd) return;

        AllLobby.Add(lobby);
        var LobbyObject = Instantiate(LobbyPrefab, LobbyPerent);
        LobbyPerent.GetComponent<RectTransform>().sizeDelta += new Vector2(0,LobbyObject.GetComponent<RectTransform>().sizeDelta.y);
        LobbyObject.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(
            "{0}\t\t{1}\\{2}\t\t{3}", 
                lobby.Settings.Name, 
                lobby.AllHero.Count,
                lobby.Settings.MaxPlayers, 
                (lobby.Settings.isPrivate) ? "Private" : "Public"
                );
        LobbyObject.GetComponent<SelectLobby>().IndexThisLobby = lobby.Index;
    }

    /// <summary>
    /// Запрос на создание лобби
    /// </summary>
    public void CreateLobbyOnServer()
    {
        SettingsLobby setting = new SettingsLobby()
        {
            Name = NameLobby_IF.text,
            MaxPlayers = (byte)MaxPlayers_Slider.value,
            isPrivate = AccessLobby_Toggle.isOn,
            Password = (AccessLobby_Toggle.isOn)? PasswordLobby_IF.text : null,
        };
        var newLobby = new Lobby() { Settings = setting, IsStart = false, IsEnd = false };

        FindObjectOfType<Client_INSPECTOR>().CreateLobby(newLobby);
    }

    /// <summary>
    /// Изменение приватности лобби
    /// </summary>
    public void OnChangeAccess() => PasswordLobby_IF.gameObject.SetActive(AccessLobby_Toggle.isOn);
    /// <summary>
    /// Подключаемся к выбранному лобби
    /// </summary>
    public void StartConnectToLobby() => FindObjectOfType<Client_INSPECTOR>().ConnectToLobby(IndexSelectLobby);

    
    public void OnNewConnectToLobby(User connectUser)
    {
        InfoAboutPlayer newPlayerInLobby = new InfoAboutPlayer()
        {
            user = connectUser,
            hero = new Hero()
        };
        AllLobby.Find(x => x.Index == IndexSelectLobby).AllHero.Add(newPlayerInLobby);    
    }
}
