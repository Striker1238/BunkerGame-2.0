using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BunkerGame.ClassUser;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class CreatingNewPlayerObject : MonoBehaviour
{
    public GameObject playerPerentObject;
    public GameObject playerObject;
    static GameController gameController;
    public void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }
    public void CreatePlayerObject(User newUser, byte MaxPlayers, byte MyNumber)
    {
        var PlayerObject = Instantiate(playerObject, playerPerentObject.transform);

        //��������� ������� ������������
        byte[] data = Convert.FromBase64String(newUser.AvatarBase64);

        var texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        texture.Apply();
        //��������� �������� ������� ������������
        PlayerObject.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
        //������� ������, ���� ���� ����� - ������ ������
        if (FindObjectOfType<Client_To_Scripts_Bridge>().ThisPlayer.UserInfo.Login != newUser.Login)
            PlayerObject.GetComponentInChildren<Button>().gameObject.SetActive(false);
        
        //������������� ��� ������
        PlayerObject.GetComponentInChildren<TextMeshProUGUI>().text = $"{newUser.UserName}";
        //��������� ������ �������
        PlayerObject.transform.localPosition = gameController.AllPosition.ToList().Find(x => x.CountPlayers == MaxPlayers).AllPosForPlayer[MyNumber];
    }
}
