using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class HealthCellUI : MonoBehaviour
{
    public Transform healthContainer;
    public Sprite healthSprite;
    public Sprite emptyHealthSprite;

    public int initializeHealthCells = 20;
    public float cellSize = 40f;
    public float offset = 5f;

    private bool m_initialized = false;
    private List<GameObject> m_emptyHealthCells = new List<GameObject>();
    private List<GameObject> m_healthCells = new List<GameObject>();



    private void Awake()
    {
        if (healthContainer == null)
        {
            healthContainer = GetComponent<Transform>();
        }
    }

    private void InitializeHealthUI()
    {
        if (m_healthCells.Count > 0)
        {
            foreach (Transform child in healthContainer)
            {
                child.gameObject.SetActive(false);
            }
        }
        
        Vector2 size = emptyHealthSprite.bounds.size;
        float ratio = size.x / size.y;

        for (int i = 0; i < initializeHealthCells; i++)
        {
            GameObject emptyCell = new GameObject("EmptyCell_" + (i + 1));
            emptyCell.transform.SetParent(healthContainer);
            
            RectTransform rt = emptyCell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(cellSize * ratio, cellSize);    
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2((cellSize + offset) * i, 0);
            Image img = emptyCell.AddComponent<Image>();
            img.sprite = emptyHealthSprite;

            m_emptyHealthCells.Add(emptyCell);


            GameObject cell = new GameObject("Cell_" + (i + 1));
            cell.transform.SetParent(emptyCell.transform);
            rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(cellSize * ratio, cellSize);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            img = cell.AddComponent<Image>();
            img.sprite = healthSprite;

            m_healthCells.Add(cell);

            emptyCell.SetActive(false);
        }

        m_initialized = true;
    }

    private void SetCell(int _currentHealth, int _maxHealth)
    {
        for (int i = 0; i < _maxHealth; i++)
        {
            m_emptyHealthCells[i].SetActive(true);

            if (i < _currentHealth)
            {
                m_healthCells[i].SetActive(true);
            }
            else
            {
                m_healthCells[i].SetActive(false);
            }
        }
    }

    public void UpdateHealthUI(int _currentHealth, int _maxHealth)
    {
        if (false == m_initialized)
        {
            InitializeHealthUI();
        }

        SetCell(_currentHealth, _maxHealth);
    }
}
