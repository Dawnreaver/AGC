using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiAgentStates {AiDisabled, AiWaitingForResources, AiIdle, AiSettingUp, AiRecruiting, AiAttacking, AiMoving, AiFinishTurn}

public class AIAgent : MonoBehaviour 
{
	public bool m_aiDebug;

	public bool m_aiVisualization = false;
	private List<int> m_aiPriorities = new List<int>();
	public List<Color> m_aiVisualizationColours = new List<Color>();


	public GameLogic m_gameLogic;
	public BasePlayer m_myFaction;

	public float m_actionTimerMax = 2.0f;
	public float m_actionTimer = 0.0f;

	public bool m_notBusy = true; 

	
	public AiAgentStates m_aiAgentState;

	void Awake()
	{
		m_myFaction = gameObject.GetComponent<BasePlayer>();
		/*if(m_aiVisualization)
		{
			InitialzeVisualisation();
		}*/
	}

	void FixedUpdate()
	{
		if(CheckForMyTurn())
		{
			AIAgentManageStates();
		}
	}

	void AIAgentManageStates()
	{
		switch(m_aiAgentState)
		{
			case AiAgentStates.AiWaitingForResources :
				if (m_myFaction.m_isAiControlled && m_myFaction.m_isDefeated != 1)
				{
					if(m_myFaction.m_availableArmies == 0)
					{
						Debug.Log("Still waiting for armies");
					}
					else
					{
						if(m_aiDebug)
						{
							Debug.Log("Ai Status: Starting my turn!");
						}
						m_aiAgentState = AiAgentStates.AiRecruiting;
					}
				}
				if (!m_myFaction.m_isAiControlled)
				{
					m_aiAgentState = AiAgentStates.AiDisabled;
				}
			break;

			case AiAgentStates.AiRecruiting :
				if(m_notBusy)
				{
					// Is the recruitment in the Setup phase
					if(m_gameLogic.m_gamePhase == GamePhases.SetupPhase)
					{
						if(m_myFaction.m_availableArmies > 0)
						{
							m_notBusy = false;
							StartCoroutine("AiIsSettingUp");
						}
						else
						{
							m_gameLogic.AdvanceTurnOrder();
							m_aiAgentState = AiAgentStates.AiWaitingForResources;
						}
					}
					// Is the recrutemnt in the game phase
					else if(m_gameLogic.m_gamePhase == GamePhases.GamePhase)
					{
						if(m_myFaction.m_availableArmies > 0)
						{
							m_notBusy = false;
							StartCoroutine("AiIsRecruitingArmies");
						}
						else
						{
							m_gameLogic.AdvanceTurnOrder();
							m_aiAgentState = AiAgentStates.AiAttacking;
						}
					}
				}
			break;

			case AiAgentStates.AiAttacking :
				if(m_notBusy)
				{
					m_notBusy = false;
					StartCoroutine("AiIsAttacking");
				}

				// if the ai I is not attacking anymore, switch to the next phase
			break;

			case AiAgentStates.AiMoving :
				if(m_notBusy)
				{
					m_notBusy = false;
					StartCoroutine("AiIsMoving");
				}

				// if the ai is not moving / can't move, switch to next phase
			break;

			case AiAgentStates.AiFinishTurn :
				if(m_notBusy)
				{
					m_notBusy = false;
					Debug.Log("Getting here");
					StartCoroutine("AiIsFinishingTurn");
				}

				// wrapping up turn
			break;

			case AiAgentStates.AiDisabled :
				Debug.Log("Ai Status: Taking a break.");
			break;
		}
	}

// Todo: Create coroutines to have the ai act
// Co-Routines should control if the ai is still busy -> m_notBusy

	IEnumerator AiIsSettingUp()
	{
		yield return new  WaitForSeconds(1.0f); // 2.0f
		if(m_aiDebug)
		{
			Debug.Log("Ai is setting up armies");
		}
		// pick a random territory I own 
		AddArmyToRandomTerritory();
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsRecruitingArmies()
	{
		yield return new  WaitForSeconds(2.0f);
		if(m_aiDebug)
		{
			Debug.Log("Ai is recruiting armies...");
		}
		AddArmyToRandomTerritory();
		//m_aiAgentState = AiAgentStates.AiRecruiting;;
		//m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	void AddArmyToRandomTerritory()
	{
		int randomTile;

		if(m_gameLogic.m_playerTerritories.Count == 1)
		{
			randomTile = 0;
		}
		else
		{
			randomTile = Random.Range(0,m_gameLogic.m_playerTerritories.Count);
		}
		//Debug.Log("Random Tile Int: "+randomTile);
		//Debug.Log("Random Tile: "+m_gameLogic.m_playerTerritories[randomTile]);
		BaseTile rndTile = m_gameLogic.m_playerTerritories[randomTile].GetComponent<BaseTile>();
		//Debug.Log(rndTile.gameObject.name);
		rndTile.m_armyCount++;
		m_myFaction.m_availableArmies--;
		rndTile.SetUnitCount();
	}

	IEnumerator AiIsAttacking()
	{
		yield return new  WaitForSeconds(2.0f);
		if(m_aiDebug)
		{
			Debug.Log("Ai is attacking...");
		}
		m_aiAgentState = AiAgentStates.AiMoving;;
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsMoving()
	{
		yield return new  WaitForSeconds(2.0f);
		if(m_aiDebug)
		{
			Debug.Log("Ai is moving armies");
		}
		m_aiAgentState = AiAgentStates.AiFinishTurn;
		m_notBusy = true;
	}

	IEnumerator AiIsFinishingTurn()
	{	
		yield return new  WaitForSeconds(2.0f);
		if(m_aiDebug)
		{
			Debug.Log("Ai Status: Finishing Turn.");
		}
		m_aiAgentState = AiAgentStates.AiWaitingForResources;
		m_gameLogic.AdvanceTurnOrder();	
		m_notBusy = true;
	}

	bool CheckForMyTurn()
	{
		bool isMyTurn = false;
		if(m_gameLogic.m_playerIndex == m_myFaction.m_playerIndex)
		{
			isMyTurn = true;
		}
		return isMyTurn;
	}

	void CheckIfBusy()
	{
		if(!m_notBusy && m_actionTimer > 0.0f)
		{
			m_actionTimer -= 1*Time.deltaTime;
		}
		else if( !m_notBusy && m_actionTimer <= 0.0f)
		{
			m_actionTimer = 0.0f;
			m_notBusy = true;

			if(m_aiDebug)
			{
				Debug.Log("Ai is free to perform next action...");
			}
		}
	}

	void SetActionTimer()
	{
		m_notBusy = false;
		m_actionTimer = m_actionTimerMax;
	}

	// Ai vizualisation 

	public void InitialzeVisualisation()
	{
		if(m_aiVisualization)
		{
			for( int a = 0; a < m_gameLogic.m_assignableTerritories.Count; a++)
			{
				// fill the priorities list with 0, so they can be assigned prioities from 1 - 6
				m_aiPriorities.Add(0);
			}
			
			SetVisualizationColour();
		}
	}
	
	public void SetVisualizationColour()
	{
		for( int b = 0; b < m_gameLogic.m_assignableTerritories.Count; b++)
		{
			GameObject square = m_gameLogic.m_aiVizualisationSquares[b];
			square.SetActive(true);
			m_gameLogic.m_aiVizualisationSquares[b].GetComponent<Renderer>().material.color = m_aiVisualizationColours[m_aiPriorities[b]];
		}
	}
	/*

	Ai Actions 

		Move camera -> Wait for CAMERA TO MOVE
		Reinforce territory 
		Attack territory -> Wait for attack to end 
		Move troops if nessesarry 
		Go to next turn
		End Turn 


	Waiting for my turn
	
	My turn stared
	
	Recruitment phase
	while I still have armies
	check for every army what the best re-enforcement should be:
		- has the territory a low number of troups
			-> check all player owned territories
		- is the territory bordring my other territories
			-> Check number of adjacent enemie and friendly territories
		- has the territory a resource I need
			-> check currentl owned resources 
			-> check if conquring this territory would take away the enemies advantage
		- Is the territory bordered by a territory with resources ( preparation for attack)
			- check adjacent terretories of the target
	Reinforce the territory 
		- Add army/ies to the territory
	Enter next turn phase

	Attack phase
	While I still can attack / didn't use up my attacks
	choose the player to attack
		- weakest player > esay way to get more territories
		- stop the strongest player from snowballing 
		- player that has a resource I need to get the resource bonus, that I can attack this turn
		- Attacks: never, a couple of times, agressively (until I don't have any armies left)
	Enter next turn phase

	Reinforcement Phase
	Move troops from dtrong teritory to a teerritory that is sorrunded by a lot of enemy troops, that holds a resource
	Auto Pass turn phase after placing the troups

	Wating for my turn

	*/
}