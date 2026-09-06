using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Textbox
{
    public int index;
    public string text = "empty";
    public int state = 0;
}

public class Checklist : MonoBehaviour
{
    private PlayerPrefs _prefs;

    [SerializeField] private List<Textbox> _textboxes;
    [SerializeField] private GameObject _referenceTextbox;
    [SerializeField] private Transform _textboxFolder;
    private float _color = 0.6f;
    private void CreateTextbox(string text, int state, int index)
    {
        GameObject newT = Instantiate(_referenceTextbox);
        newT.transform.parent = _textboxFolder;
        newT.name = "T_" + index.ToString();
        if (!newT.transform.Find("Text")) return;
        newT.transform.Find("Text").GetComponent<TMP_Text>().text = text;
        newT.transform.Find("Index").GetComponent<TMP_Text>().text = index.ToString();
        if (state == 0) newT.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
        if (state == 1) newT.GetComponent<Image>().color = new Color(_color, 0, 0, 0.3f);
        if (state == 2) newT.GetComponent<Image>().color = new Color(_color, _color, 0, 0.3f);
        if (state == 3) newT.GetComponent<Image>().color = new Color(0, _color, 0, 0.3f);
    }

    private void DeleteObject(Transform obj)
    {
        Destroy(obj);
    }

    private void ChangeObject(Transform obj, string text, int state)
    {
        obj.transform.Find("Text").GetComponent<TMP_Text>().text = text;
        if (state == 0) obj.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
        if (state == 1) obj.GetComponent<Image>().color = new Color(_color, 0, 0, 0.3f);
        if (state == 2) obj.GetComponent<Image>().color = new Color(_color, _color, 0, 0.3f);
        if (state == 3) obj.GetComponent<Image>().color = new Color(0, _color, 0, 0.3f);
    }

    private void OnValidate()
    {
        int i = 0;
        var textboxesCopy = _textboxes.ToList();

        foreach (var v in textboxesCopy)
        {
            i++;

            // Сначала обновляем индекс
            v.index = i;

            bool found = false;
            string newIndex = v.index.ToString();

            foreach (Transform v2 in _textboxFolder)
            {
                var indexTransform = v2.Find("Index");
                if (indexTransform == null) continue;

                var tmpText = indexTransform.GetComponent<TMP_Text>();
                if (tmpText == null) continue;

                // Ищем по НОВОМУ индексу
                if (tmpText.text == newIndex)
                {
                    ChangeObject(v2, v.text, v.state);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                CreateTextbox(v.text, v.state, v.index);
            }
        }
    }

}
