﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum TerritoryTypes {Land, Water}
public enum ResourceTypes {Fish, Empty, Grain, Wine, Armor, Helmet, Weapon, Gold}
public enum GamePhases { InMenues, SetupPhase, GamePhase, EndGamePhase};
public enum TurnPhases { Idle, Recruitment, Attack, Movement};
public enum PlayerTurn {Human, Ai};
public class GameLogic : MonoBehaviour 
{
	public bool m_debug = false;
	public Camera m_camera;
	public MenuLogic m_menuLogic;

	// Game options
	[Range(1, 6)]
	public int m_factions = 2;
	[Range(1,40)]
	public int m_startArmySize = 20; // make start armies dependent on the number of players, see risk rule book
	public bool m_randomlyPlaceArmies = false;
	public List<BasePlayer> m_players = new List<BasePlayer>();
	public List<BasePlayer> m_factionList = new List<BasePlayer>();
	public List<Sprite> m_factionIcons = new List<Sprite>();
	public bool m_randomBoard = false;

	// Faction variables
	public Material[] m_factionMaterials = new Material[6];

	// Territory variables
	public string[] m_territoryTypes = new string[]{ "Land", "Water"};

	// Resource variables
	public string[] m_resourceTypes = new string[]{ "Empty", "Fish", "Grain", "Wine", "Armor", "Helmet", "Weapon", "Gold"};

	[Range(0,9)]
	public int m_numberOfRandomResources = 0;

	public List<GameObject> m_territories = new List<GameObject>();
	public List<BaseTile> m_resourceTerritories = new List<BaseTile>();
	public List<GameObject> m_assignableTerritories = new List<GameObject>();
	public List<GameObject> m_attackableTerritories = new List<GameObject>();
	public float m_attackReach = 1.5f;
	public BaseTile m_selectedTeritorry;

	public BaseTile m_targetTerritory; // teeritory to attack in the attack phase, territory to enforce in the movment phase

	public GamePhases m_gamePhase;
	int m_gamePhaseIndex = 0;

	// player variables
	public int m_playerIndex = 1;
	int m_myPlayerID;
	public List<GameObject> m_playerTerritories = new List<GameObject>();

	// turnorder variables 
	public int m_turnOrder = 1;
	public TurnPhases m_turnPhase;

	// Army recruitment variables
	int m_minimumArmyTerritories = 9;
	int m_minimumArmiesGenerated = 3;
	int m_armieRecruitmentFactor = 3;
	public int m_availableArmies;

	// player variables
	public GameObject m_playerPrefab;

	public PlayerTurn m_playerTurn; 

	// Ai visualization
	public GameObject m_aiVisualizationPrefab;
	public List<GameObject> m_aiVizualisationSquares = new List<GameObject>();
	public Hashtable m_aiVisualizationHt = new Hashtable();

	// game options
	private void Awake()
	{
		SetBoard();
	}

	void SetBoard()
	{
		GameObject[] tempTerritories = CollectTerritories();
		foreach( GameObject territory in tempTerritories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			tile.AdjustTerritoryMaterial();
			tile.m_factionToken.SetActive(false);
			tile.m_resourceToken.SetActive(false);
			tile.m_gameLogic = this;
			//territories.Add(tile.gameObject);
		}
	}
	public void GenerateBoard()
	{
		GameObject[] tempTerritories = CollectTerritories();
		List<GameObject> territories = new List<GameObject>();
		foreach( GameObject territory in tempTerritories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			if(m_randomBoard)
			{
				tile.m_territoryType = GetRandomEnum<TerritoryTypes>(0,0);
			}
			tile.AdjustTerritoryMaterial();
			tile.m_factionToken.SetActive(false);
			tile.m_resourceToken.SetActive(false);
			tile.m_gameLogic = this;
			territories.Add(tile.gameObject);
		}
		m_assignableTerritories = territories;
		m_assignableTerritories = m_assignableTerritories.OrderBy(terrtile => terrtile.name).ToList();
	
		PrepareVisualization(m_assignableTerritories);
		
		for (int r = 0; r < m_numberOfRandomResources; r++)
		{
			BaseTile tile = territories[Random.Range(1,territories.Count)].GetComponent<BaseTile>();
			if(tile.m_territoryType == TerritoryTypes.Water && tile.m_resource1 == ResourceTypes.Empty)
			{
				tile.m_resource1 = ResourceTypes.Fish;
			}
			else if(tile.m_territoryType == TerritoryTypes.Land && tile.m_resource1 == ResourceTypes.Empty)
			{
				tile.m_resource1 = GetRandomEnum<ResourceTypes>(2,7);
			}
			else
			{
				r--;
			}
		}
		SetDiceBonus();
	}
	private void DistributeTerritories()
	{
		GameObject randomTerritory;
		int iterations = m_assignableTerritories.Count;
		for ( int b = 0; b < iterations; b++)
		{
			randomTerritory = m_assignableTerritories[Random.Range(0,m_assignableTerritories.Count)];
			BaseTile territory = randomTerritory.GetComponent<BaseTile>();
			territory.m_playerId = m_playerIndex;
			territory.m_factionToken.SetActive(true);
			if( territory.m_resource1 != ResourceTypes.Empty)
			{
				territory.m_resourceToken.SetActive(true);
				m_resourceTerritories.Add(territory);
			}
			m_factionList[m_playerIndex-1].m_availableArmies --;
			territory.SetFaction(m_factionMaterials[m_playerIndex-1]);
			territory.m_armyCount++;
			territory.SetUnitCount();
			SetActivePlayer();
			m_assignableTerritories.Remove(randomTerritory);
			m_territories.Add(randomTerritory);
		}
		m_playerIndex = 1; // Select random Player for start of the game? 
	}

// Helper functions
	public void SetSelectedTerritory(BaseTile tile)
	{
		if (m_selectedTeritorry == null || m_selectedTeritorry != tile)
		{
			m_selectedTeritorry = tile;
			SetAttackableTerritories();
		}
		else if (m_selectedTeritorry == tile)
		{
			m_selectedTeritorry = null;
			DeselectAttackableTerritories();
			DeselectMoveableTerritories();
		}		
	}

	public void SelectTargetTerritory(BaseTile tile)
	{
		if (m_targetTerritory == null || m_targetTerritory != tile)
		{
			m_targetTerritory = tile;
		}
		else if (m_targetTerritory == tile)
		{
			if(m_debug)
			Debug.Log("SelectTargetTerritory called");
			m_targetTerritory = null;
		}
	}

	public void SelectMovementTerritory(BaseTile tile)
	{
		if (m_targetTerritory == null || m_targetTerritory != tile)
		{	
			m_targetTerritory = tile;
		}
		else if (m_targetTerritory == tile)
		{
			if(m_debug)
			Debug.Log("SelectMovementTerritory called");
			m_targetTerritory = null;
		}
	}

	public void DeselectTerritories()
	{
		foreach (GameObject territory in m_territories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			tile.m_selected = false;
			//if(tile != m_targetTerritory)
			//{
				tile.SetTokenColor(tile.m_factionColor);
			//}
		}
		m_selectedTeritorry = null;
		if(m_debug)
		Debug.Log("DeselectTerritories called");
		m_targetTerritory = null;
	}
	public void DeselectAttackableTerritories()
	{
		foreach (GameObject territory in m_attackableTerritories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			tile.m_selected = false;
			tile.SetTokenColor(tile.m_factionColor);
		}
		if(m_debug)
		Debug.Log("DeselectAttackableTerritory called");
		m_targetTerritory = null;
	}
	public void DeselectMoveableTerritories()
	{
		foreach (GameObject territory in m_playerTerritories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			if(tile != m_selectedTeritorry)
			{
			tile.m_selected = false;
				tile.SetTokenColor(tile.m_factionColor);
			}
		}
		if(m_debug)
		Debug.Log("DeselectMoveableTerritory called");
		m_targetTerritory = null;
	}
	static T GetRandomEnum<T>(int startEnum, int maxLength)
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V;
		if(maxLength == 0)
		{
			V = (T)A.GetValue(UnityEngine.Random.Range(startEnum,A.Length));
		}
		else 
		{
			V = (T)A.GetValue(UnityEngine.Random.Range(startEnum,maxLength));
		}
		return V;
	}
	// Game Phase Logic
	public void SetGamePhaseIndex (int index)
	{
		m_gamePhaseIndex = index;
		SetGamePhase();
	}
	void SetGamePhase()
	{
		switch(m_gamePhaseIndex)
		{
			case 0 :
				m_gamePhase = GamePhases.InMenues;
			break;
			case 1 :
				m_gamePhase = GamePhases.SetupPhase;
			break;

			case 2 :
				m_gamePhase = GamePhases.GamePhase;
			break;

			case 3 :
				m_gamePhase = GamePhases.EndGamePhase;
			break;
		}
	}
	// Player Turnorder Logic
	public void AdvanceTurnOrder()
	{
		if( m_gamePhase == GamePhases.SetupPhase)
		{
			switch(m_turnOrder)
			{
				case 1 :
					SelectPlayerTerritories();
					m_menuLogic.SetNotification("Setup Phase");
					m_turnOrder = 3;
				break;

				case 4 :
				
					DeselectTerritories();
					SetArmies();
					SetActivePlayer();
					m_menuLogic.SetNotification("Player "+m_playerIndex+" turn.");
					if(GameSetupCheck())
					{
						m_gamePhase = GamePhases.GamePhase;
					}
					StartCoroutine("AutoAdvanceTurn",0.5f);
				break;
			}
			m_turnOrder = IncreaseValue(m_turnOrder, 4);
		}
		else if(m_gamePhase == GamePhases.GamePhase)
		{
			switch(m_turnOrder)
			{
				case 1 :
					SelectPlayerTerritories();
					m_factionList[m_playerIndex-1].m_availableArmies = RecruitArmies();
					m_menuLogic.SetNotification("Recruitment Phase");
					m_turnPhase = TurnPhases.Recruitment;
				break;

				case 2 : 
					m_menuLogic.SetNotification("Attack Phase");
					SetArmies();
					DeselectTerritories();
					m_turnPhase = TurnPhases.Attack;
				break;

				case 3 :
					m_menuLogic.SetNotification("Movement Phase");
					DeselectTerritories();
					m_turnPhase = TurnPhases.Movement;
				break;

				case 4 :
					DeselectTerritories();
					SetActivePlayer();
					if(m_factionList[m_playerIndex-1].m_isDefeated ==1) // if the new player is defeated, loop back 
					{
						m_turnPhase = TurnPhases.Movement;
						AdvanceTurnOrder();
						return; // without the return, the else appears to be performed too, leading to skipping a phase e.g. recruitment
					}
					else
					{
						m_menuLogic.SetNotification("Player "+m_playerIndex+" turn.");
						StartCoroutine("AutoAdvanceTurn",0.5f);
						m_turnPhase = TurnPhases.Idle;
					}
				break;
			}
			m_turnOrder = IncreaseValue(m_turnOrder, 4);
		}
		
		else if( m_gamePhase == GamePhases.EndGamePhase)
		{
			if(m_debug)
			Debug.Log("End Game");
		}
	}
	
	// Playerfunctions
	public void InitialisePlayers()
	{
		for (int p = 0; p< m_factions; p++)
		{
			m_factionList[p].m_availableArmies = m_startArmySize;
			if(m_factionList[p].m_isAiControlled)
			{
				m_factionList[p].gameObject.GetComponent<AIAgent>().InitialzeVisualisation();
			}
		}
	}

	public void SetActivePlayer()
	{
		m_playerIndex = IncreaseValue(m_playerIndex, m_factions);
		// check if the next player is controlled by a human or by Ai
		if(m_factionList[m_playerIndex-1].m_isAiControlled)
		{
			m_playerTurn = PlayerTurn.Ai;
		}
		else
		{
			m_playerTurn = PlayerTurn.Human;
		}
	}

	void SelectPlayerTerritories()
	{
		m_playerTerritories.Clear();
		GameObject[] tempTerritories = CollectTerritories();

		for( int a = 0; a < tempTerritories.Length; a++)
		{
			if(tempTerritories[a].GetComponent<BaseTile>().m_playerId == m_playerIndex)
			{
				m_playerTerritories.Add(tempTerritories[a]);
			}
		}
	}

	// Territory functions
	GameObject[] CollectTerritories()
	{
		GameObject[] tempTeritories = GameObject.FindGameObjectsWithTag("Territory");
		return tempTeritories;
	}

	void SetAttackableTerritories()
	{
		m_attackableTerritories.Clear();
		for( int a = 0; a < m_territories.Count; a++)
		{
			if(m_territories[a].GetComponent<BaseTile>().m_playerId != m_playerIndex)
			{
				if(Vector3.Distance(m_selectedTeritorry.transform.position, m_territories[a].transform.position) <= m_attackReach)
				{
					m_attackableTerritories.Add(m_territories[a]);
				}
			}
		}
	}

	// Army functions
	public void AddArmies()
    {
		int unit = 1;
		if(m_factionList[m_playerIndex-1].m_availableArmies >=1)
		{
			m_factionList[m_playerIndex-1].m_availableArmies--;
			m_selectedTeritorry.m_tempArmyCount  += unit;
			m_selectedTeritorry.SetUnitCount();
			m_selectedTeritorry.SetCounterColor();
		}
		else 
		{
			if(m_debug)
			Debug.Log("No available armies remaining");
		}
    }
	public int RecruitArmies() // other bonuses still need to be applied e.g. for owned continents 
	{
		Debug.Log("Recruiting Armies called");
		int recruitedArmies = 0;
		if(m_playerTerritories.Count < m_minimumArmyTerritories)
		{
			recruitedArmies = m_minimumArmiesGenerated;
		}
		else
		{
			recruitedArmies = m_playerTerritories.Count / m_armieRecruitmentFactor;
		}
		Debug.Log("Armies: "+recruitedArmies);
		return recruitedArmies;
	}
    public void RemoveArmies()
    {   
		int unit = 1;
        if(m_selectedTeritorry.m_tempArmyCount >= 1)
        {
            m_selectedTeritorry.m_tempArmyCount  -= unit;
			m_factionList[m_playerIndex-1].m_availableArmies++;
			m_selectedTeritorry.SetUnitCount();
			m_selectedTeritorry.SetCounterColor();
        }
        else
        {
			if(m_debug)
            Debug.Log("can't deduct more units"); // visual queue
        }
    }
	void RandomlyPlaceArmies()
	{
		for(int c = 0; c < m_factionList.Count; c++)
		{
			m_playerIndex = c+1;
			SelectPlayerTerritories();
			while(m_factionList[c].m_availableArmies > 0)
			{
				int random = Random.Range(0,m_playerTerritories.Count);
				BaseTile baseTile = m_playerTerritories[random].GetComponent<BaseTile>();
				baseTile.m_armyCount++;
				baseTile.SetUnitCount();
				m_factionList[c].m_availableArmies--;
			}
		}
	}
	void SetArmies()
	{
		foreach(GameObject territory in m_playerTerritories)
		{
			BaseTile tile = territory.GetComponent<BaseTile>();
			int totalArmies = tile.m_tempArmyCount+tile.m_armyCount;
			tile.m_armyCount = totalArmies;
			tile.m_tempArmyCount = 0;
			tile.SetCounterColor();
		}
	}

	public void SetUpGame()
	{
		if(m_debug)
		Debug.Log("Setting Up Game");
		InitialisePlayers();
		DistributeTerritories();
		if(m_randomlyPlaceArmies)
		{
			RandomlyPlaceArmies();
			m_turnPhase = TurnPhases.Recruitment;
			m_gamePhase = GamePhases.GamePhase;
		}
		else
		{
			m_turnPhase = TurnPhases.Recruitment;
			m_gamePhase = GamePhases.SetupPhase;
		}
		m_menuLogic.SetNotification("Player "+m_playerIndex+" turn.");
		m_playerIndex = Random.Range(1, m_factions+1);
	}

	bool GameSetupCheck()
	{
		bool setupComplete = true;
		
		for( int a = 0; a < m_factionList.Count; a++)
		{
			if(m_factionList[a].m_availableArmies > 0)
			{
				setupComplete = false;
			}
		}

		return setupComplete;
	}
	IEnumerator AutoAdvanceTurn(float time)
	{
		yield return new WaitForSeconds(time);
		AdvanceTurnOrder();
	}

	// this function only works if the dfault value is 1
	int IncreaseValue(int value, int max)
	{
		value++;
		if(value > max)
		{
			value = 1;
		}
		return value;
	}

	public void PressAttackButton()
	{
		AttackTerritory((int)m_menuLogic.m_armySliderAmmount, m_targetTerritory.GetComponent<BaseTile>().m_armyCount);
	}

	public void AttackTerritory (int attackingArmy, int defendingArmy)
	{	
		if(m_debug)
		Debug.Log("clicked attack");
		int attacker = attackingArmy;
		int defender = defendingArmy;
		BaseTile attackingTerritory = m_selectedTeritorry.GetComponent<BaseTile>();
		BaseTile defendedTerritory = m_targetTerritory.GetComponent<BaseTile>();

		if(m_debug)
		Debug.Log("Deducted units");
		attackingTerritory.m_armyCount = attackingTerritory.m_armyCount - attacker;
		attackingTerritory.SetUnitCount();

		while(attacker > 0)
		{
			int attackRoll = Random.Range(1,7);
			Debug.Log("Attacker roll: "+attackRoll);
			int defendRoll = Random.Range(1,7);
			// TODO add bonuses for both sides (maybe as a helper function?)
			if( attackRoll < 5)
			{
				attackRoll += m_factionList[m_selectedTeritorry.m_playerId-1].m_diceModifier;
				if( attackRoll > 5)
				{
					attackRoll = 5;
				}
			}
			if( defendRoll < 5)
			{
				defendRoll += m_factionList[m_targetTerritory.m_playerId-1].m_diceModifier;
				if( defendRoll > 5)
				{
					defendRoll = 5;
				}
			}
			Debug.Log("Attacker roll with bonus: "+attackRoll);

			if(m_debug)
			Debug.Log("Defender: "+defendRoll+" Attacker: "+attackRoll);
			if(attackRoll > defendRoll)
			{
				defender--;
				defendedTerritory.m_armyCount--;
				defendedTerritory.SetUnitCount();
				if(defender == 0)
				{
					break;
				}
			}
			else if(attackRoll <= defendRoll)
			{
				attacker--;
				if(attacker == 0)
				{
					break;
				}
			}
			attackRoll = 0;
			defendRoll = 0;
		}

		if ( defender == 0 )
		{	
			int defenderPlayerID = defendedTerritory.m_playerId; 
			if(m_debug)
			Debug.Log(attacker);
			if(m_debug)
			Debug.Log("Conquered territory!");
			defendedTerritory.m_armyCount = attacker;
			// broadcast message if that the player was defeated
			defendedTerritory.m_playerId = attackingTerritory.m_playerId;
			if(IsPlayerDefeated(defenderPlayerID))
			{
				m_menuLogic.m_uiNotification.text = "Player "+defenderPlayerID+" was vanquished.";
				m_factionList[defenderPlayerID-1].m_isDefeated = 1;//Set the player to defeated
				
				//######
				// if the defeated faction was my own player 
				// show the lose screen and pause the game 
				// offer to continue ( and let the ais duke it out ) or quit

				// check if we won the game
				if(DidPlayerWin(attackingTerritory.m_playerId))
				{
					if(m_debug)
					Debug.Log("Player "+attackingTerritory.m_playerId+" won the game!");
					m_gamePhase = GamePhases.InMenues;
					m_menuLogic.SetWinLoosePanel();
					DeselectTerritories();
					if(m_debug)
					Debug.Log("Break out of the funktion...");
				}
			}
			defendedTerritory.m_playerId = attackingTerritory.m_playerId;
			defendedTerritory.ConquerTile(attackingTerritory.m_factionMaterial);
			SelectPlayerTerritories();
			PrepareNextAttack();
			SetDiceBonus();
		}
		else if(attacker == 0)
		{
			if(m_debug)
			Debug.Log("Attack failed!");
			if(m_selectedTeritorry.m_armyCount == 1)
			{
				//m_menuLogic.DisableArmySlider();
				//m_menuLogic.DisableTargetTerritoryPanel();
			}
		}
	}

	public void ReinforceTerritory()
	{
		int reinforcement = (int)m_menuLogic.m_armySlider.value;
		if(m_debug)
		Debug.Log("Clicked reinforcement");
		m_selectedTeritorry.m_armyCount -= reinforcement;
		m_targetTerritory.m_armyCount += reinforcement;
		
		m_targetTerritory.SetUnitCount();
		m_selectedTeritorry.SetUnitCount();

		// Advance turn after reinforcing territory
		AdvanceTurnOrder();
	}

	public void PrepareNextAttack()
	{
		DeselectTerritories();
		DeselectAttackableTerritories();
	}
	// check if the player was defeated
	bool IsPlayerDefeated(int playerIndex)
	{
		bool playerLost = true;
		for ( int a = 0; a < m_territories.Count; a++)
		{
			if(m_territories[a].GetComponent<BaseTile>().m_playerId == playerIndex)
			{
				playerLost = false;
			}
		}
		return playerLost;
	}
	// check if the player won the game
	bool DidPlayerWin(int playerIndex)
	{
		bool playerWin = true;
		for ( int b = 0; b < m_territories.Count; b++)
		{
			if(m_territories[b].GetComponent<BaseTile>().m_playerId != playerIndex)
			{
				playerWin = false;
			}
		}
		return playerWin;
	}

	public void WinLooseGame()
	{
		// check if the game was won or lost
	}

	public void ResetGame()
	{
		foreach(GameObject gameTile in m_territories)
		{
			BaseTile tile = gameTile.GetComponent<BaseTile>();
			tile.m_armyCount = 0;
			tile.m_playerId = 0;
			tile.m_resource1 = ResourceTypes.Empty;
			tile.m_resourceToken.SetActive(false);
			tile.m_factionToken.SetActive(false);
			// reset tile values as appropriate ....
		}

		m_factions = 2;
		m_factionList.Clear();
		m_territories.Clear();
		m_resourceTerritories.Clear();
		m_attackableTerritories.Clear();
		m_playerTerritories.Clear();
		m_selectedTeritorry = null;
		m_targetTerritory = null;
		m_turnPhase = TurnPhases.Idle;
		m_turnOrder = 4;
	}

	void SetDiceBonus()
	{
		foreach(BasePlayer player in m_factionList)
		{
			bool armement1 = false;
			bool armement2 = false;
			bool armement3 = false;
			bool gold = false;
			bool food1 = false;
			bool food2 = false;
			bool food3 = false;

			player.m_diceModifier = 0;

			for(int a = 0; a < m_territories.Count;a++)
			{
				if(m_territories[a].GetComponent<BaseTile>().m_playerId == player.m_playerIndex)
				{
					switch(m_territories[a].GetComponent<BaseTile>().m_resource1)
					{
						case ResourceTypes.Armor :
							armement1 = true;
						break;
						case ResourceTypes.Helmet :
							armement2 = true;
						break;
						case ResourceTypes.Weapon :
							armement3 = true;
						break;

						case ResourceTypes.Fish :
							food1 = true;
						break;
						case ResourceTypes.Grain :
							food2 = true;
						break;
						case ResourceTypes.Wine :
							food3 = true;
						break;
						case ResourceTypes.Gold :
							gold = true;
						break;
					}
				}
			}

			if(food1 && food2 && food3 || armement1 && armement2 && armement3 || gold)
			{
				player.m_diceModifier = 1;
			}
			else if( food1 && food2 && food3 && gold || armement1 && armement2 && armement3 && gold || food1 && food2 && food3 && armement1 && armement2 && armement3)
			{
				player.m_diceModifier = 2;
			}
			else if(food1 && food2 && food3 && armement1 && armement2 && armement3 && gold)
			{
				player.m_diceModifier = 3;
			}
		}
	}

	void PrepareVisualization(List<GameObject> territories)
	{
			for( int a = 0; a < territories.Count; a++)
			{
				GameObject vizSquare = Instantiate(m_aiVisualizationPrefab, transform.position, Quaternion.identity) as GameObject;
				vizSquare.transform.SetParent(this.gameObject.transform);
				vizSquare.transform.position = new Vector3(territories[a].transform.localPosition.x,territories[a].transform.localPosition.y+0.75f,territories[a].transform.localPosition.z);
				m_aiVizualisationSquares.Add(vizSquare);
				vizSquare.SetActive(false);
				vizSquare.name = "vizSquare - "+territories[a].name;
				m_aiVisualizationHt.Add(territories[a], vizSquare);
			}
	}
}