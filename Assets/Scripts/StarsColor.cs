using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsColor : MonoBehaviour
{
    private Stars starsManager;
    private Renderer starRenderer;
    // Start is called before the first frame update
    void Start()
    {
        starsManager = GameObject.Find("Stars").GetComponent<Stars>();
        starRenderer = GetComponent<Renderer>();
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (starsManager.isExoplanetColorScheme)
        {
            starRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            starRenderer.material.color = Color.white;
        }
    }
}
