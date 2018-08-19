using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCreationUIPlayer : MonoBehaviour
{

	public MenuLogic m_menuLogic;
	public GameObject m_removePlayer;

	public bool m_addPlayerButton = false;

	public BasePlayer m_faction;
	public GameObject playerObject;

	void Awake()
	{
		m_faction = playerObject.GetComponent<BasePlayer>();
	}

	public void ManageRemovePlayerButton()
	{
		if(this.gameObject == m_menuLogic.m_uiPlayers[m_menuLogic.m_gameLogic.m_factions-1] && m_menuLogic.m_gameLogic.m_factions > 2)
		{
			m_removePlayer.SetActive(true);
		}
		else
		{
			m_removePlayer.SetActive(false);
		}
	}

	public void SetAiSetting()
	{
		m_faction.m_isAiControlled = !m_faction.m_isAiControlled;
	}

	public void DisablePlayer()
	{
		this.gameObject.SetActive(false);
		m_menuLogic.m_gameLogic.m_factions--;
		m_menuLogic.m_gameLogic.m_factionList.Remove(m_faction);
		m_menuLogic.SetGameCreationUIPlayers();
	}

	public void AddPlayer()
	{
		m_menuLogic.m_gameLogic.m_factions++;
		Debug.Log(m_faction);
		m_menuLogic.m_uiPlayers[m_menuLogic.m_gameLogic.m_factions-1].SetActive(true);
		
		m_menuLogic.m_gameLogic.m_factionList.Add(m_menuLogic.m_uiPlayers[m_menuLogic.m_gameLogic.m_factions-1].GetComponent<GameCreationUIPlayer>().m_faction);

		m_menuLogic.SetGameCreationUIPlayers();
		if(m_addPlayerButton && m_menuLogic.m_gameLogic.m_factions == m_menuLogic.m_uiPlayers.Count)
		{
			this.gameObject.SetActive(false);
		}
	}
}
