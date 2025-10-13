using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class HealthCellUI : MonoBehaviour
{
    public Transform healthContainer;
    public RectTransform fullHPImage;
    public Sprite healthSprite;

    public int initializeHealthCells = 20;
    public float cellSize = 40f;
    public float offset = 5f;

    private bool m_initialized = false;
    private List<RectTransform> m_healthCells = new List<RectTransform>();



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
        
        Vector2 size = fullHPImage.sizeDelta;
        float cellSizeX = size.x / initializeHealthCells;

        for (int i = 0; i < initializeHealthCells; i++)
        {
            GameObject cell = new GameObject("Cell_" + (i + 1));
            RectTransform rt = cell.AddComponent<RectTransform>();
            cell.transform.SetParent(healthContainer.transform);
            rt.sizeDelta = new Vector2(size.x, size.y);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            Image img = cell.AddComponent<Image>();
            img.sprite = healthSprite;

            m_healthCells.Add(rt);
            cell.SetActive(false);
        }

        m_initialized = true;
    }

    private void SetCell(int _currentHealth, int _maxHealth)
    {
        Vector2 size = fullHPImage.sizeDelta;
        float cellSizeX = size.x / _maxHealth;
        size.x = cellSizeX;

        for (int i = 0; i < _maxHealth; i++)
        {
            if (i < _currentHealth)
            {
                m_healthCells[i].sizeDelta = size;

                Vector2 position = new Vector2(cellSizeX * i, 0);
                m_healthCells[i].localPosition = position;
                m_healthCells[i].gameObject.SetActive(true);
            }
            else
            {
                m_healthCells[i].gameObject.SetActive(false);
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
