using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public class LodLevel
{
    [HideInInspector] public string _name;
    [HideInInspector] public int _level;
    public GameObject _obj;
    public float _distance;      // минимальная дистанция, с которой активен уровень

    public void Upd(int level)
    {
        _level = level;
        _name = "Level " + _level.ToString();
    }
}

public class LOD : MonoBehaviour
{
    [SerializeField] private Transform _folder;           // общая папка со всеми LOD-объектами
    [SerializeField] private float _cullDistance = 200f;  // дальше — всё скрыто

    private Camera _camera;

    // Внутреннее представление: корень объекта + отсортированные уровни
    private class Entry
    {
        public Transform root;
        public List<LodLevel> levels = new List<LodLevel>(); // отсортированы по _distance ↑
    }

    private readonly List<Entry> _entries = new List<Entry>();

    private void Awake()
    {
        _camera = Camera.main;
        Init();
    }

    private void Init()
    {
        _entries.Clear();
        if (_folder == null)
        {
            Debug.LogError($"[LOD] {name}: _folder не назначена.", this);
            return;
        }

        foreach (Transform child in _folder)
        {
            var entry = new Entry { root = child };

            foreach (Transform lodChild in child)
            {
                var obj = lodChild.gameObject;
                float dist = ParseDistance(obj.name);

                var level = new LodLevel
                {
                    _obj = obj,
                    _distance = dist
                };
                level.Upd(entry.levels.Count + 1);
                entry.levels.Add(level);
            }

            // Сортируем по возрастанию дистанции: LOD0 (0), LOD1 (40), LOD2 (80)…
            entry.levels.Sort((a, b) => a._distance.CompareTo(b._distance));

            // Переназначаем нумерацию после сортировки
            for (int i = 0; i < entry.levels.Count; i++)
                entry.levels[i].Upd(i);

            _entries.Add(entry);
        }

        Debug.Log($"[LOD] Инициализировано объектов: {_entries.Count}");
    }

    // Парсит "LOD1 / 40" → 40. Если не находит число после "/" — берёт первое число в строке, иначе 0.
    private static float ParseDistance(string objName)
    {
        if (string.IsNullOrEmpty(objName)) return 0f;

        int slash = objName.LastIndexOf('/');
        string numberPart = slash >= 0 ? objName.Substring(slash + 1) : objName;

        if (float.TryParse(numberPart.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;

        // Фолбэк: вытащить все цифры/точку из строки
        string digits = "";
        foreach (char c in numberPart)
            if (char.IsDigit(c) || c == '.' || c == ',') digits += c;
        digits = digits.Replace(',', '.');

        return float.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out float v2) ? v2 : 0f;
    }

    private void FixedUpdate()
    {
        if (_camera == null) { _camera = Camera.main; if (_camera == null) return; }

        Vector3 camPos = _camera.transform.position;

        foreach (var entry in _entries)
        {
            if (entry.root == null) continue;

            float sqrDist = (camPos - entry.root.position).sqrMagnitude;
            bool culled = sqrDist > _cullDistance * _cullDistance;

            // Выбираем ровно один активный уровень: последний, чей порог пройден
            LodLevel chosen = null;
            if (!culled && entry.levels.Count > 0)
            {
                chosen = entry.levels[0];
                for (int i = 1; i < entry.levels.Count; i++)
                {
                    if (entry.levels[i]._distance * entry.levels[i]._distance <= sqrDist)
                        chosen = entry.levels[i];
                    else
                        break;
                }
            }

            foreach (var level in entry.levels)
            {
                if (level._obj == null) continue;
                bool shouldBeActive = !culled && level == chosen;
                if (level._obj.activeSelf != shouldBeActive)
                    level._obj.SetActive(shouldBeActive);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Переинициализировать LOD")]
    private void ReinitInEditor() => Init();
#endif
}