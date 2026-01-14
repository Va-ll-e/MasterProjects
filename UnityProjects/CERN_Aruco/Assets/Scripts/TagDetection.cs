using UnityEngine;

public class TagDetection : MonoBehaviour
{

    [Header("Setup")] 
    public GameObject box;
    
    private WebCamTexture webcamTexture;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        webcamTexture = new WebCamTexture(1920, 1080, 30);
        webcamTexture.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
            return;
        
        box.GetComponent<Renderer>().material.color = Color.forestGreen;
        
    }
}
