using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiAgentStates {AiDisabled, AiIdle, AiThinking, AiActing, AiFinishTurn}

public class AIAgent : MonoBehaviour 
{

	public GameLogic m_gameLogic;
	public BasePlayer m_myFaction;

	public float m_actionTimerMax = 2.0f;
	public float m_actionTimer = 0.0f;

	public bool m_notBusy = false; 

	
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
				if (m_myFaction.m_isAiControlled)
				{
					Debug.Log("Ai Status: It's my turn!");
					m_aiAgentState = AiAgentStates.AiThinking; 
				}
				if (!m_myFaction.m_isAiControlled)
				{
					m_aiAgentState = AiAgentStates.AiDisabled;
				}
			break;
			
			case AiAgentStates.AiThinking :

				CheckIfBusy();
				if(m_notBusy)
				{
					AiIsTinking();
					SetActionTimer();
				}
			break;

			case AiAgentStates.AiActing :
				CheckIfBusy();
				if(m_notBusy)
				{
					AiIsTakingAnAction();
					SetActionTimer();
				}

			break;

			case AiAgentStates.AiFinishTurn :
				CheckIfBusy();
				if(m_notBusy)
				{
					AiIsFinishingTurn();
					SetActionTimer();
				}
			break;

			case AiAgentStates.AiDisabled :
				Debug.Log("Ai Status: Taking a break.");
			break;
		}
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

	void AiIsTinking()
	{
		//SetActionTimer();
		Debug.Log ("Ai Status: I'm thinking about my turn");
		m_aiAgentState = AiAgentStates.AiActing;
	}

	void AiIsTakingAnAction()
	{
		Debug.Log("Ai Status: I'm taking an action.");
		m_aiAgentState = AiAgentStates.AiFinishTurn;
	}

	void AiIsFinishingTurn()
	{	

		Debug.Log("Ai Status: Finishing Turn.");
		m_gameLogic.AdvanceTurnOrder();
		m_gameLogic.AdvanceTurnOrder();
		m_gameLogic.AdvanceTurnOrder();
		m_aiAgentState = AiAgentStates.AiIdle;
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