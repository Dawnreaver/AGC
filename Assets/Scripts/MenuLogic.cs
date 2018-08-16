using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuLogic : MonoBehaviour 
{
	public GameLogic m_gameLogic;
	// variables fpr graphicscasting
	GraphicRaycaster m_raycaster;
	PointerEventData m_pointerEventData;
	EventSystem m_eventSystem;
	public GameObject m_mainMenuScreen;
	public GameObject m_createNewGameScreen;

	// Loading Screen Variables
	public GameObject m_loadingScreen;
	public Slider m_loadingBar;
	public float m_maxLoadingTime = 4.0f;
	float m_loadingTime = 0.0f;
	public bool m_loadingGame = false;

	// In game menu variables
	public GameObject m_inGameUi; 
	public Text m_uiNotification;
	public GameObject m_selectedTerritoryPanel;
	public GameObject m_targetTerritoryPanel;
	public Button m_removeArmiesButton;
	public Button m_addArmiesButton;

	// information displayed
	public Text m_selectedTerritoryNameText;
	public Text m_selectedTeritorryResourceText;
	public Text m_selectedTeritorryNumberOfArmiesText;
	
	// turn button
	public GameObject m_turnButton;
	public List<Sprite> m_turnPhaseSprites = new List<Sprite>();
	public Image m_turnPhaseIndicator;

	// Use this for initialization
	public List<GameObject> m_uiPlayers = new List<GameObject>();
	public GameObject m_addUIPlayerButton;

	// PlayerTurn Indicator
	public Image m_playerTurnIndicatorBackground;
	public Image m_playerTurnIndicatorIcon;

	// Armyslider 
	public GameObject m_armySliderUIElement;
	public Slider m_armySlider;
	public Text m_armySliderText;
	public int m_armySliderAmmount;

	// Target Panel
	public Text m_targetPanelNameText;
	public Text m_targetTerritoryNameText;
	public Text m_targetTerritoryResourceText;
	public Text m_targetTerritoryArmyCountText;
	public Button m_attackButton;
	public Button m_reinforcementButton;

	// Win Lose Panel
	public GameObject m_winLoosePanel;

	// resources
	public Image m_resourceIconSelected;
	public Image m_resourceIconTarget;
	public List<Sprite> m_resourceIcons = new List<Sprite>();
	void Awake() 
	{
		m_raycaster = GetComponent<GraphicRaycaster>();
        m_eventSystem = GetComponent<EventSystem>();

		InitializeMenu();	
	}
	
	public bool NotBlockedByUI()
	{
		bool notBlocked = true;
		
		m_pointerEventData = new PointerEventData(m_eventSystem);
		//Set the Pointer Event Position to that of the mouse position
		m_pointerEventData.position = Input.mousePosition;
		//Create a list of Raycast Results
		List<RaycastResult> results = new List<RaycastResult>();
		//Raycast using the Graphics Raycaster and mouse click position
		m_raycaster.Raycast(m_pointerEventData, results);

		//For every result returned, output the name of the GameObject on the Canvas hit by the Ray
		foreach (RaycastResult result in results)
		{
			if(result.gameObject.layer == 5)
			{
				notBlocked = false;
			}
		}
		return notBlocked;
	}
	void FixedUpdate() 
	{
		if(m_loadingGame)
		{
			m_loadingTime += 1.0f*Time.deltaTime;
			m_loadingBar.value = m_loadingTime;

			if(m_loadingTime >= m_maxLoadingTime)
			{
				StartSession();
				m_loadingGame = false;
				m_loadingTime = 0.0f;
				m_loadingBar.value = 0.0f;
				
				m_gameLogic.SetGamePhaseIndex(1);
				m_gameLogic.StartCoroutine("AutoAdvanceTurn",0.5f);
				m_gameLogic.GenerateBoard();
				m_gameLogic.SetUpGame();
			}
		}
		if(m_gameLogic.m_selectedTeritorry != null) // probably needs adjustment if other objects where selectable 
		{
			UpdateStatusPanel();
		}
		UpdateArmySliderText();
		UpdateTurnButton();
	}

	public void InitializeMenu()
	{
		m_mainMenuScreen.SetActive(true);
		m_createNewGameScreen.SetActive(false);
		m_loadingScreen.SetActive(false);
		m_inGameUi.SetActive(false);
	}

	public void StartNewGame()
	{
		m_createNewGameScreen.SetActive(true);
		m_mainMenuScreen.SetActive(false);

		// Adjust the UI Player Placeholders
		SetGameCreationUIPlayers();
	}

	public void ContinueGame()
	{
		m_loadingScreen.SetActive(true);
		m_createNewGameScreen.SetActive(false);
		m_mainMenuScreen.SetActive(false);
		m_loadingGame = true;
	}

	public void StartSession()
	{
		m_inGameUi.SetActive(true);
		m_loadingScreen.SetActive(false);
		m_loadingGame = true;
	}

	public void SetNotification(string notification)
	{
		m_uiNotification.text = notification;
	}

	public void SetPhaseIcon()
	{
		switch(m_gameLogic.m_turnPhase)
		{
			case TurnPhases.Idle:
				m_turnPhaseIndicator.sprite = null;
			break;
			case TurnPhases.Recruitment :
				m_turnPhaseIndicator.sprite = m_turnPhaseSprites[0];
			break;

			case TurnPhases.Attack :
				m_turnPhaseIndicator.sprite = m_turnPhaseSprites[1];
			break;

			case TurnPhases.Movement :
				m_turnPhaseIndicator.sprite = m_turnPhaseSprites[2];
			break;
		}
		SetPlayerTurnIndicator();
	}

	public void EnableSelectedTerritoryPanel()
	{
		// ToDo: Animate the panel as well 
		m_selectedTerritoryPanel.SetActive(true);
	}

	public void DisableSelectedTerritoryPanel()
	{
		// ToDo: Animate the panel as well 
		m_selectedTerritoryPanel.SetActive(false);
	}
	public void EnableTargetTerritoryPanel()
	{
		m_targetTerritoryPanel.SetActive(true);
	}
	public void DisableTargetTerritoryPanel()
	{
		m_targetTerritoryPanel.SetActive(false);
		DisableArmySlider();
	}

	public void UpdateStatusPanel()
	{
		m_selectedTeritorryNumberOfArmiesText.text = ""+(m_gameLogic.m_selectedTeritorry.m_armyCount+m_gameLogic.m_selectedTeritorry.m_tempArmyCount);
		m_selectedTerritoryNameText.text = m_gameLogic.m_selectedTeritorry.m_territoryName;
		// update resource icon
		SetResourceIcon(m_gameLogic.m_selectedTeritorry,m_resourceIconSelected);

		if(m_gameLogic.m_selectedTeritorry.m_resource1 != ResourceTypes.Empty)
		{
			m_selectedTeritorryResourceText.text = ""+m_gameLogic.m_selectedTeritorry.m_resource1.ToString();
		}
		else
		{
			m_selectedTeritorryResourceText.text = "";
		}

		if(m_gameLogic.m_turnPhase == TurnPhases.Recruitment)
		{
			m_removeArmiesButton.gameObject.SetActive(true);
			m_addArmiesButton.gameObject.SetActive(true);
		}
		if(m_gameLogic.m_turnPhase == TurnPhases.Attack || m_gameLogic.m_turnPhase == TurnPhases.Movement) // this might lead to the panel remainng open when switchen the turn 
		{
			m_removeArmiesButton.gameObject.SetActive(false);
			m_addArmiesButton.gameObject.SetActive(false);
			UpdateTargetTerritoryPanel();
		}
		
		if(m_gameLogic.m_gamePhase == GamePhases.SetupPhase)
		{
			if(m_gameLogic.m_factionList[m_gameLogic.m_playerIndex-1].m_availableArmies > 0 && m_gameLogic.m_selectedTeritorry.m_tempArmyCount == 0)
			{
				m_addArmiesButton.interactable = true;
			}
			else
			{
				m_addArmiesButton.interactable = false;
			}

			if(m_gameLogic.m_selectedTeritorry.m_tempArmyCount == 1)
			{
				m_removeArmiesButton.interactable = true; 
			}
			else
			{
				m_removeArmiesButton.interactable = false; 
			}
		}
		else if(m_gameLogic.m_gamePhase == GamePhases.GamePhase)
		{
			if(m_gameLogic.m_factionList[m_gameLogic.m_playerIndex-1].m_availableArmies > 0)
			{
				m_addArmiesButton.interactable = true;
			}
			else
			{
				m_addArmiesButton.interactable = false;
			}

			if(m_gameLogic.m_selectedTeritorry.m_tempArmyCount > 0)
			{
				m_removeArmiesButton.interactable = true; 
			}
			else
			{
				m_removeArmiesButton.interactable = false; 
			}
		}
	}

	void UpdateTurnButton()
	{
		if(m_gameLogic.m_turnOrder == 1 || m_gameLogic.m_gamePhase == GamePhases.InMenues)
		{
			m_turnButton.SetActive(false);
		}
		else
		{
			m_turnButton.SetActive(true);
		}
	}

	void UpdateTargetTerritoryPanel()
	{
		if(m_gameLogic.m_targetTerritory != null)
		{
			if(!m_targetTerritoryPanel.activeSelf)
			{
				m_targetTerritoryPanel.SetActive(true);

				m_targetTerritoryNameText.text = m_gameLogic.m_targetTerritory.m_territoryName;
				m_targetTerritoryArmyCountText.text = ""+m_gameLogic.m_targetTerritory.m_armyCount;
				if( m_gameLogic.m_targetTerritory.m_resource1 != ResourceTypes.Empty)
				{
					m_targetTerritoryResourceText.text = ""+m_gameLogic.m_targetTerritory.m_resource1.ToString();
				}
				else
				{
					m_targetTerritoryResourceText.text = "";
				}
				// update resource icon
				SetResourceIcon(m_gameLogic.m_targetTerritory,m_resourceIconTarget);

				switch(m_gameLogic.m_turnPhase)
				{
					case TurnPhases.Attack :

						m_targetPanelNameText.text = "Attack Territory";
						if(m_gameLogic.m_selectedTeritorry.m_armyCount >1)
						{
							m_attackButton.gameObject.SetActive(true);
						}
						else
						{
							m_attackButton.gameObject.SetActive(false);
						}
						m_reinforcementButton.gameObject.SetActive(false);
					break;

					case TurnPhases.Movement :
						m_targetPanelNameText.text = "Move To Territory";

						m_attackButton.gameObject.SetActive(false);
						if(m_gameLogic.m_selectedTeritorry.m_armyCount >1)
						{
							m_reinforcementButton.gameObject.SetActive(true);
						}
						else
						{
							m_reinforcementButton.gameObject.SetActive(false);
						}
					break;
				}
			}
		}
		else
		{
			m_targetTerritoryPanel.SetActive(false);
		}
	}

	public void SetGameCreationUIPlayers()
	{
		foreach (GameObject player in m_uiPlayers)
		{
			player.GetComponent<GameCreationUIPlayer>().ManageRemovePlayerButton();
		}
		if(m_gameLogic.m_playerIndex < m_uiPlayers.Count)
		{
			 m_addUIPlayerButton.SetActive(true);
		}
	}
	
	public void SetPlayerTurnIndicator()
	{
		m_playerTurnIndicatorBackground.color = m_gameLogic.m_factionList[m_gameLogic.m_playerIndex-1].m_factionColor;
		m_playerTurnIndicatorIcon.sprite = m_gameLogic.m_factionIcons[m_gameLogic.m_playerIndex-1];
	}

	/// ARMY SLIDER

	public void EnableArmySlider()
	{
		if(m_gameLogic.m_selectedTeritorry.m_armyCount > 1)
		{
			m_armySliderUIElement.SetActive(true);
			SetArmySlider();
		}
	}

	public void DisableArmySlider()
	{
		m_armySliderUIElement.SetActive(false);
	}

	public void SetArmySlider()
	{
		if(m_gameLogic.m_targetTerritory != null)
		{
			m_armySlider.maxValue = m_gameLogic.m_selectedTeritorry.GetComponent<BaseTile>().m_armyCount-1;
			m_armySlider.minValue = 1;
			if(m_armySlider.value > m_gameLogic.m_selectedTeritorry.GetComponent<BaseTile>().m_armyCount-1)
			{
				m_armySlider.value = m_gameLogic.m_selectedTeritorry.GetComponent<BaseTile>().m_armyCount-1;
			}
		}
	}

	void UpdateArmySliderText()
	{
		m_armySliderText.text = ""+m_armySlider.value;
		m_armySliderAmmount = (int)m_armySlider.value;
	}

	public void SetWinLoosePanel()
	{
		if(!m_winLoosePanel.activeSelf)
		{
			m_winLoosePanel.SetActive(true);
			DisableTurnButton();
		}
	}

	public void BackToMainMenu()
	{
		m_winLoosePanel.SetActive(false);
		m_inGameUi.SetActive(false);
		m_mainMenuScreen.SetActive(true);
		m_gameLogic.ResetGame();
	}
	public void EnableTurnButton()
	{
		m_turnButton.SetActive(true);
	}
	public void DisableTurnButton()
	{
		m_turnButton.SetActive(false);
	}
	void SetResourceIcon(BaseTile tile, Image targetImage)
	{
		targetImage.gameObject.SetActive(true);
		
		switch(tile.m_resource1)
		{
			case ResourceTypes.Fish :
				targetImage.sprite = m_resourceIcons[0];
			break;
			case ResourceTypes.Grain :
				targetImage.sprite = m_resourceIcons[1];
			break;
			case ResourceTypes.Wine :
				targetImage.sprite = m_resourceIcons[2];
			break;
			case ResourceTypes.Armor :
				targetImage.sprite = m_resourceIcons[3];
			break;
			case ResourceTypes.Helmet :
				targetImage.sprite = m_resourceIcons[4];
			break;
			case ResourceTypes.Weapon :
				targetImage.sprite = m_resourceIcons[5];
			break;
			case ResourceTypes.Gold :
				targetImage.sprite = m_resourceIcons[6];
			break;

			default :
				targetImage.gameObject.SetActive(false);
			break;
		}
	}
}