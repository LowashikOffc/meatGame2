using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
class MenuButton
{
    [HideInInspector] public string _name;
    [HideInInspector] public TMP_Text _text = null;
    public Button _button;
    public int _id = -1;

}

public class MenuController : MonoBehaviour
{
    [SerializeField] private MenuButton[] _buttons;
    
    private int _selectedID;

    private void OnValidate()
    {
        int nextID = -1;
        foreach (var button in _buttons)
        {
            button._id = -1;
        }
        foreach (var button in _buttons)
        {
            nextID++;
            button._name = button._button.name;
            button._text = button._button.GetComponent<TMP_Text>();
            if (button._id == -1)
            {
                button._id = nextID;
            }
        }
    }

    public void StateChange(bool st, int id)
    {
        foreach (var button in _buttons)
        {
            if (id == button._id)
            {
                if (st == true) button._text.text = button._text.text.Replace("[ ]", "[>]");
                else button._text.text = button._text.text.Replace("[>]", "[ ]");
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
