using BunkerGame.ClassHero;
using BunkerGame.ClassLobby;
using BunkerGame.ClassPlayer;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct PlayerPosition
{
    public byte CountPlayers;
    public Vector3[] AllPosForPlayer;
}
public class GameController : MonoBehaviour
{
    /// <summary>
    /// ������ ������ � ����������� �� ���� ������
    /// </summary>
    public TextMeshProUGUI playerInfo_TMP;
    /// <summary>
    /// ��� ��������� ������� ��� ������� � �����
    /// </summary>
    public PlayerPosition[] AllPosition;
    public Lobby thisLobby;

    static CreatingNewPlayerObject creatingPlayer;
    static TimerController timerController;

    private IEnumerator timer;
    void Start()
    {
        thisLobby = FindObjectOfType<Client_To_Scripts_Bridge>().ThisPlayer.ActiveLobby;
        creatingPlayer = GetComponent<CreatingNewPlayerObject>();
        timerController = FindObjectOfType<TimerController>();
        timer = timerController.TimerControll();

        for (int i = 0; i < thisLobby.AllHero.Count; i++)
        {
            var player = thisLobby.AllHero[i];
            creatingPlayer.CreatePlayerObject(player.user, thisLobby.Settings.MaxPlayers, (byte)i);
        }
        
    }
    /// <summary>
    /// ��� ����������� ������ ������������
    /// </summary>
    /// <param name="connectUser">������ ������ ������������</param>
    public void OnNewConnectToLobby(User connectUser)
    {
        InfoAboutPlayer newPlayerInLobby = new InfoAboutPlayer()
        {
            user = connectUser,
            hero = new Hero()
        };
        thisLobby.AllHero.Add(newPlayerInLobby);
        creatingPlayer.CreatePlayerObject(connectUser, thisLobby.Settings.MaxPlayers, (byte)(thisLobby.AllHero.Count-1));
    }

    public void isChangedReadiness(bool state) => FindObjectOfType<Client_To_Scripts_Bridge>().ChangeReadiness(thisLobby.Index, state);
    
    public async Task StartGame()
    {
        //������������� ���������� � ������� ������
        SetHeroInformationInTMP();

        //������������� ���������� � ������� � �������

        //������������� ������ ���
        ///�������� ������� ������ ������������ ������ ���

        //���������� ������
        StartCoroutine(timer);
        ///������� ������� ������������� � �������� �� �������

        //������ ������� �� ������������ �� ���� �����������

        //������ ������� �� ��� ��� ������

        //�������� ��� ������ ������

        //����������� ������ � ���������������� ������ ����� ���� ������� ������

        //�������� � ��������� ������ ������ ������� ����, ����� � ������ ���������� ����� ���� ���������
    }
    private void SetHeroInformationInTMP()
    {
        var _player_hero = FindObjectOfType<Client_To_Scripts_Bridge>().ThisPlayer.ActiveLobby.AllHero.Find(x => x.user.UserName == FindObjectOfType<Client_To_Scripts_Bridge>().ThisPlayer.UserInfo.UserName).hero;
        playerInfo_TMP.text = "���: " +(_player_hero.Sex_Hero ? "������� " : "������� ") + $"\t\t�������: {_player_hero.Age_Hero}\n";
        playerInfo_TMP.text += $"���������: {_player_hero.Profession_Hero} (����: "
        + (((_player_hero.ExperienceProfession_Hero / 12) != 0)
            ? $"{(_player_hero.ExperienceProfession_Hero / 12)} "+(((_player_hero.ExperienceProfession_Hero / 12)%10 <= 4) && ((_player_hero.ExperienceProfession_Hero / 12)/10 != 1)
                ?"����":"���")
            :"")
            +(((_player_hero.ExperienceProfession_Hero % 12) != 0)
            ? $" {(_player_hero.ExperienceProfession_Hero % 12)} "+(((_player_hero.ExperienceProfession_Hero % 12) <= 4)
                ?"������":"�������")
            :"") + ")\n";
        
        playerInfo_TMP.text += $"�����: {_player_hero.Hobbies_Hero} (����: "
        + (((_player_hero.ExperienceHobbies_Hero / 12) != 0)
            ? $"{(_player_hero.ExperienceHobbies_Hero / 12)} " + (((_player_hero.ExperienceHobbies_Hero / 12) % 10 <= 4) && ((_player_hero.ExperienceHobbies_Hero / 12) / 10 != 1)
                ? "����" : "���")
            : "")
            + (((_player_hero.ExperienceHobbies_Hero % 12) != 0)
            ? $" {(_player_hero.ExperienceHobbies_Hero % 12)} " + (((_player_hero.ExperienceHobbies_Hero % 12) <= 4)
                ? "������" : "�������")
            : "") + ")\n";
        playerInfo_TMP.text += $"��������: {_player_hero.HealthCondition_Hero} (�������: {_player_hero.HealthPoint_Hero}%)\n";
        playerInfo_TMP.text += $"�����: {_player_hero.Phobia_Hero} (�������: {_player_hero.PhobiaPercentage_Hero}%)\n";
        playerInfo_TMP.text += $"������� ���������: {_player_hero.BodyType_Hero} (�������: {_player_hero.BodyPrecentage_Hero}%)\n";
        playerInfo_TMP.text += $"�����: {_player_hero.Luggage_Hero}\n";
        playerInfo_TMP.text += $"������������ �����: {_player_hero.HumanTrait_Hero}\n";
        playerInfo_TMP.text += $"�������������� ����������: {_player_hero.FurtherInformation_Hero}\n";
    }
    public void ShowCharacteristic(int index)
    {
        FindObjectOfType<Client_To_Scripts_Bridge>().ShowCharacteristic(thisLobby.Index, index.ToString());
    }
    public void AnotherPlayerShowCharacteristic(string playerLogin,string index, string data)
    {
        //thisLobby.AllHero.Find(x => x.user.Login == playerLogin).hero;
        Debug.Log($"Player [{playerLogin}] show info:");
        Debug.Log($"{index}");
        Debug.Log($"{data}");

    }
    public void NewPlayerTurn(string playerLogin)
    {
        //if(FindObjectOfType<Client_To_Scripts_Bridge>().ThisPlayer.UserInfo.Login == playerLogin)
        //���������� ������ � ��������� �������������
        StopCoroutine(timer);
        StartCoroutine(timer);
        Debug.Log($"Player - {playerLogin} start step");
    }
}
