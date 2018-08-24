using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiAgentStates {AiDisabled, AiIdle, AiSettingUp, AiRecruiting, AiAttacking, AiMoving, AiFinishTurn}

public class AIAgent : MonoBehaviour 
{

	public GameLogic m_gameLogic;
	public BasePlayer m_myFaction;

	public float m_actionTimerMax = 2.0f;
	public float m_actionTimer = 0.0f;

	public bool m_notBusy = true; 

	
	public AiAgentStates m_aiAgentState;


	void Awake()
	{
		m_myFaction = gameObject.GetComponent<BasePlayer>();
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
			case AiAgentStates.AiIdle :
				if (m_myFaction.m_isAiControlled && m_myFaction.m_isDefeated != 1)
				{
					Debug.Log("Ai Status: It's my turn!");

					switch(m_gameLogic.m_turnOrder)
					{
						case 1 :
						// Recruitment and setup
						if(m_gameLogic.m_gamePhase == GamePhases.SetupPhase)
						{
							// Setup the game board
							m_aiAgentState = AiAgentStates.AiSettingUp;
						}
						else if(m_gameLogic.m_gamePhase == GamePhases.GamePhase)
						{
							// Distribute recruted troops
							m_aiAgentState = AiAgentStates.AiRecruiting;
						}
						
						break;
						case 2 :
						// Decide on an order attacks
							m_aiAgentState = AiAgentStates.AiAttacking;
						break;
						case 3 :
						// Move troops 
							m_aiAgentState = AiAgentStates.AiMoving;
						break;
						
						case 4 :
							m_aiAgentState = AiAgentStates.AiFinishTurn;
						break;
					}
				}
				if (!m_myFaction.m_isAiControlled)
				{
					m_aiAgentState = AiAgentStates.AiDisabled;
				}
			break;
			
			case AiAgentStates.AiSettingUp :
				if(m_notBusy)
				{
					Debug.Log("Setup phase");
					if(m_myFaction.m_availableArmies > 0)
					{
					m_notBusy = false;
					StartCoroutine("AiIsSettingUp");
					}
					else
					{
						m_gameLogic.AdvanceTurnOrder();
						m_aiAgentState = AiAgentStates.AiIdle;
					}
				}

				// if no more troops can be set up, switch to the next round phase
			break;

			case AiAgentStates.AiRecruiting :
				if(m_notBusy)
				{
					Debug.Log("Recruitment phase");
					m_notBusy = false;
					StartCoroutine("AiIsRecruitingArmies");
				}
				// if no more troops can be recruited, switch to the next round phase
			break;

			case AiAgentStates.AiAttacking :
				if(m_notBusy)
				{
					Debug.Log("Attack phase");
					m_notBusy = false;
					StartCoroutine("AiIsAttacking");
				}

				// if the ai I is not attacking anymore, switch to the next phase
			break;

			case AiAgentStates.AiMoving :
				if(m_notBusy)
				{
					Debug.Log("Movement phase");
					m_notBusy = false;
					StartCoroutine("AiIsMoving");
				}

				// if the ai is not moving / can't move, switch to next phase
			break;

			case AiAgentStates.AiFinishTurn :
				if(m_notBusy)
				{
					Debug.Log("Finish turn");
					m_notBusy = false;
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
		yield return new  WaitForSeconds(2.0f);
		Debug.Log("Ai is setting up armies");
		// pick a random territory I own 
		BaseTile rndTile = m_gameLogic.m_playerTerritories[Random.Range(1,m_gameLogic.m_playerTerritories.Count)].GetComponent<BaseTile>();
		rndTile.m_armyCount++;
		m_myFaction.m_availableArmies--;
		rndTile.SetUnitCount();
		m_aiAgentState = AiAgentStates.AiIdle;
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsRecruitingArmies()
	{
		yield return new  WaitForSeconds(2.0f);
		Debug.Log("Ai is recruiting armies...");
		m_aiAgentState = AiAgentStates.AiIdle;;
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsAttacking()
	{
		yield return new  WaitForSeconds(2.0f);
		Debug.Log("Ai is attacking...");
		m_aiAgentState = AiAgentStates.AiIdle;;
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsMoving()
	{
		yield return new  WaitForSeconds(2.0f);
		Debug.Log("Ai is moving armies");
		m_aiAgentState = AiAgentStates.AiIdle;;
		m_gameLogic.AdvanceTurnOrder();
		m_notBusy = true;
	}

	IEnumerator AiIsFinishingTurn()
	{	
		yield return new  WaitForSeconds(2.0f);
		Debug.Log("Ai Status: Finishing Turn.");
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
			Debug.Log("Ai is free to perform next action...");
		}
	}

	void SetActionTimer()
	{
		m_notBusy = false;
		m_actionTimer = m_actionTimerMax;
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