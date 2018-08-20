using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour 
{

	public GameLogic m_gameLogic;
	public BasePlayer myfaction;

	/*
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
