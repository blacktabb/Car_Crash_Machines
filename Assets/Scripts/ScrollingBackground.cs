using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Renderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // X ekseninde arkaplaný kaydýrma.
        float offset = Time.time * scrollSpeed;       
        meshRenderer.material.mainTextureOffset = new Vector2(offset, 0);
    }
}