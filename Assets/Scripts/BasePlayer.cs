using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayer : MonoBehaviour 
{
	public int m_playerIndex;
	public bool m_isAiControlled;
	public string m_factionName;
	public int m_availableArmies;
	public Color m_factionColor;
	public int m_diceModifier;
	public int m_isDefeated;
}
