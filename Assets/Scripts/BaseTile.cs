using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BaseTile : MonoBehaviour
{
    public GameLogic m_gameLogic;
    public string m_territoryName;
    public TerritoryTypes m_territoryType;
	public Material[] m_territoryMaterials;
    public ResourceTypes m_resource1;
    public int m_playerId;
    public bool m_selected;
    public bool m_mouseOver;
    // Unit variables
    public int m_armyCount;
    public int m_tempArmyCount;
    public Sprite[] m_counterSprites = new Sprite[10];
    public SpriteRenderer m_numberTens;
    public SpriteRenderer m_numberSingles;

    // base tile colour // adjust interactive tiles on your own turn 
    public Color m_baseTileColour;


    // counter colour
    public Color m_baseCounterColour = Color.black;
    public Color m_modifiedCounterColour = Color.blue;
    public Material m_factionMaterial;
    public GameObject m_factionToken;

    public Color m_factionColor; 
    public Color m_factionColorSelected;
    public Mesh m_landArmy;
    public Mesh m_waterArmy;

    public bool m_debug = false;
    void Awake()
    {
        //SetUnitCount();
    }

    void OnMouseEnter()
    {
        if(m_gameLogic.m_playerIndex == m_playerId && m_gameLogic.m_gamePhase != GamePhases.InMenues || m_gameLogic.m_turnPhase == TurnPhases.Attack && m_gameLogic.m_gamePhase != GamePhases.InMenues && m_gameLogic.m_attackableTerritories.Contains(gameObject) && m_gameLogic.m_selectedTeritorry != null)
        {
            SetTokenColor(m_factionColorSelected);
        }
    }

    void OnMouseOver()
    {
        if(Input.GetButtonDown("Fire1") && m_gameLogic.m_menuLogic.NotBlockedByUI())
        {
            switch(m_gameLogic.m_turnPhase)
            {
                case TurnPhases.Recruitment : 
                    if(m_gameLogic.m_playerTerritories.Contains(this.gameObject))
                    {          
                        SelectTerritory();
                    }
                break;

                case TurnPhases.Attack :
                    if(m_gameLogic.m_playerTerritories.Contains(this.gameObject))
                    {
                        SelectTerritory();
                    }
                    else if(m_gameLogic.m_attackableTerritories.Contains(this.gameObject) && m_gameLogic.m_selectedTeritorry != null)
                    {
                        //Debug.Log("Selected attackable territory: "+ this.gameObject.name);
                        SelectTargetTerritory();
                    }
                break;

                case TurnPhases.Movement :
                    if(m_gameLogic.m_playerTerritories.Contains(this.gameObject) && m_gameLogic.m_selectedTeritorry == null || m_gameLogic.m_playerTerritories.Contains(this.gameObject) && m_gameLogic.m_selectedTeritorry == this)
                    {          
                        SelectTerritory();
                    }
                    else if(m_gameLogic.m_playerTerritories.Contains(this.gameObject) && m_gameLogic.m_selectedTeritorry != this)
                    {
                        //Debug.Log("Selected movement territory: "+ this.gameObject.name);
                        SelectMovementTerritory();
                    }
                break;
            }
        }
    }
    void OnMouseExit()
    {
        if(!m_selected)
        {
            SetTokenColor(m_factionColor);
        }
    }

    public void AdjustTerritoryMaterial()
    {
        switch(m_territoryType)
        {
            case TerritoryTypes.Land :
                gameObject.GetComponent<Renderer>().material = m_territoryMaterials[0];
                m_baseTileColour = m_territoryMaterials[0].color;
            break;

            case TerritoryTypes.Water :
                gameObject.GetComponent<Renderer>().material = m_territoryMaterials[1];
                m_baseTileColour = m_territoryMaterials[1].color;
            break;
        }
    }

    public void SetFaction(Material factionMaterial)
    {
        m_factionMaterial = factionMaterial;
        m_factionToken.GetComponent<Renderer>().material = m_factionMaterial;
        m_factionColor = m_factionMaterial.color;
        m_factionColorSelected = m_factionColor + Color.grey;
        m_factionColorSelected.a = 1.0f;
        if(m_territoryType == TerritoryTypes.Land)
        {
            m_factionToken.GetComponent<MeshFilter>().mesh = m_landArmy;
        }
        else if(m_territoryType == TerritoryTypes.Water)
        {
            m_factionToken.GetComponent<MeshFilter>().mesh = m_waterArmy;
        }
    }

    public void SetTokenColor (Color color)
    {
        m_factionToken.GetComponent<Renderer>().material.color = color;
    }

    public void SetCounterColor()
    {
        if(m_tempArmyCount > 0)
        {
            m_numberSingles.color = m_modifiedCounterColour;
            m_numberTens.color = m_modifiedCounterColour;
        }
        else
        {
            m_numberSingles.color = m_baseCounterColour;
            m_numberTens.color = m_baseCounterColour;
        }
    }

    public void TeritorrySelection()
    {
        if(!m_selected)
        {
            m_selected = true;
            SetTokenColor(m_factionColorSelected);
        }
        else if(m_selected)
        {
            m_selected = false; 
            SetTokenColor(m_factionColor);
        }
    }
    public void SetUnitCount() // 3D solution
    {
        int totalArmies = m_tempArmyCount+m_armyCount;
        int a = (totalArmies%100)/10;
        int b = totalArmies%10;

        m_numberTens.sprite = m_counterSprites[a];
        m_numberSingles.sprite = m_counterSprites[b];
    }
    /*public void RandomlyAssignResource()
    {
        switch(m_tileType)
        {
            case TileTypes.Land :
                name = "LandTile";
                m_resource1 = GetRandomEnum<Resources>(2,0);
            break;

            case TileTypes.Water :
                name = "WaterTile";
                m_resource1 = GetRandomEnum<Resources>(0,2);
            break;
        }
    } */
    void SelectTerritory()
    {
        if(!m_selected)
        {
            m_gameLogic.DeselectTerritories();
        }
        TeritorrySelection();
        m_gameLogic.SetSelectedTerritory(this);
    }
    void SelectTargetTerritory()
    {
        if(!m_selected && m_gameLogic.m_attackableTerritories.Contains(this.gameObject))
        {
            m_gameLogic.DeselectAttackableTerritories();
        }
        TeritorrySelection();
        m_gameLogic.SelectTargetTerritory(this);
    }

    void SelectMovementTerritory()
    {
        if(!m_selected)
        {
            m_gameLogic.DeselectMoveableTerritories();
        }
        TeritorrySelection();
        m_gameLogic.SelectMovementTerritory(this);
    }

    public void ConquerTile( Material newFaction)
    {
        m_factionMaterial = newFaction;
        SetFaction(m_factionMaterial);
        SetUnitCount();
    }
}