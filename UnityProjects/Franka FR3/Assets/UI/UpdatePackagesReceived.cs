using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UpdatePackagesReceived : MonoBehaviour
{
    private VisualElement root;
    private Label _packagesReceived;
    private int _packagesNum; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _packagesReceived = root.Q<Label>("PackagesReceived");
        _packagesNum = 0;
    }

    public void UpdatePackagesReceivedText()
    {
        _packagesNum += 1;
        _packagesReceived.text = _packagesNum+"";
    }
}
