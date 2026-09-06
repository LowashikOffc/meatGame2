using System.Reflection;
using TMPro;
using UnityEngine;

public class UIFlyingPanel : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _connectScript;
    [SerializeField] private Transform _panel;
    private TMP_Text _parameterText;
    private TMP_Text _parameterValue;
    [SerializeField] private string _parameter;
    private System.Reflection.MethodInfo _cachedMethod;

    private Camera _camera;
    private string FormatParameterName(string parameter)
    {
        string result = parameter.TrimStart('_');

        // Вставляем пробелы перед заглавными буквами
        result = System.Text.RegularExpressions.Regex.Replace(result, "([A-Z])", " $1").Trim();

        // Делаем первую букву заглавной
        if (!string.IsNullOrEmpty(result))
        {
            result = char.ToUpper(result[0]) + result.Substring(1);
        }

        return result;
    }
    private void Start()
    {
        _parameterText = _panel.Find("Text").GetComponent<TMP_Text>();
        _parameterValue = _panel.Find("Value").GetComponent<TMP_Text>();
        _parameterText.text = FormatParameterName(_parameter);
        //_parameterText.enableAutoSizing = true;
        //_parameterValue.enableAutoSizing = true;


        _camera = Camera.main;

        if (_connectScript != null)
        {
            _cachedMethod = _connectScript.GetType().GetMethod("GetParameter");
            if (_cachedMethod == null)
            {
                Debug.LogError($"Method GetParameter not found in {_connectScript.GetType().Name}");
            }
        }
    }

    private void Update()
    {
        transform.rotation = _camera.transform.rotation;
    }

    private void FixedUpdate()
    {
        if (_cachedMethod != null && _connectScript != null)
        {
            try
            {
                // Передаем параметр _parameter
                var result = _cachedMethod.Invoke(_connectScript, new object[] { _parameter });
                _parameterValue.text = result?.ToString() ?? "0";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error calling GetParameter: {e.Message}");
            }
        }
    }
}

//System.Type type = _connectScript.GetType();

//MethodInfo method = type.GetMethod("GetParameter");
//if (method != null)
//{
//    _parameterValue.text = method.Invoke(_connectScript, null).ToString();
//}