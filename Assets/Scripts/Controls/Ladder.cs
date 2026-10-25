using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LadderType { t1, t2, t3 }

public class Ladder : MonoBehaviour
{
    private const string VisualName = "Visual";

    [Header("Interaction")]
    [SerializeField] private string _tag = "Ladder";
    [SerializeField] private float _distance = 3f;
    [SerializeField] private LayerMask _targetLayer;

    [Header("Positions")]
    [SerializeField] private Transform _downPosition;
    [SerializeField] private Transform _upPosition;
    [SerializeField] private Transform _downCollider;
    [SerializeField] private Transform _upCollider;
    [SerializeField] private Transform _positionsFolder;

    [Header("Visual")]
    [SerializeField] private LadderType _type;
    [SerializeField] private Transform _rightPipe;
    [SerializeField] private Transform _leftPipe;
    [SerializeField] private GameObject _ladderTop;
    [SerializeField] private GameObject _ladderDown;
    [SerializeField] private GameObject _attachmentPrefab;
    [SerializeField] private GameObject _attachmentPrefab2;
    [SerializeField] private float _ladderSizeY = 2f;

    [Header("Options")]
    [SerializeField] private bool _upEnabled;
    [SerializeField] private bool _downEnabled;
    [SerializeField] private bool _drawGizmos;

    private static readonly Vector3 DrawOffsetRight = new Vector3(-0.1f, 0f, 0.4f);
    private static readonly Vector3 DrawOffsetLeft = new Vector3(-0.1f, 0f, -0.4f);

    private Camera _camera;
    private PlayerMovement _pm;
    private Transform _visual;
    private BoxCollider _col;
    private Material _sharedMat;

    private void Awake()
    {
        _visual = transform.Find(VisualName);
        if (_visual == null)
        {
            var v = new GameObject(VisualName).transform;
            v.SetParent(transform, false);
            _visual = v;
        }

        _col = GetComponent<BoxCollider>();
        if (_col == null) _col = gameObject.AddComponent<BoxCollider>();

        if (_rightPipe != null)
            _sharedMat = _rightPipe.GetComponent<Renderer>().sharedMaterial;

        _camera = Camera.main;
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _pm = player.GetComponent<PlayerMovement>();

        if (InputReceiver.Instance != null)
            InputReceiver.Instance._interact += Climb;
        else
            Debug.LogWarning($"[Ladder] {name}: InputReceiver.Instance == null в Start.");

        CreateLadder();
    }

    private void OnDestroy()
    {
        if (InputReceiver.Instance != null)
            InputReceiver.Instance._interact -= Climb;
    }

    // ---------- Creation ----------

    private void CreateLadder()
    {
        if (_rightPipe == null || _leftPipe == null || _rightPipe.parent == null)
        {
            Debug.LogError($"[Ladder] {name}: _rightPipe/_leftPipe или их parent не назначены.", this);
            enabled = false;
            return;
        }
        if (_positionsFolder == null)
        {
            Debug.LogError($"[Ladder] {name}: _positionsFolder не назначен.", this);
            enabled = false;
            return;
        }

        Transform p = _rightPipe.parent;
        Vector3 forward = p.forward;
        Vector3 right = p.right;
        Vector3 up = p.up;
        Vector3 down = -up;

        // Collider
        _col.size = new Vector3(1f, _ladderSizeY, 0.1f);
        _col.center = new Vector3(0f, _ladderSizeY * -0.5f, 0f);

        // Type setup
        bool isT1 = _type == LadderType.t1;
        if (isT1)
        {
            _rightPipe.gameObject.SetActive(false);
            _leftPipe.gameObject.SetActive(false);
            _upEnabled = false;
            _downEnabled = false;
        }
        else
        {
            _rightPipe.gameObject.SetActive(true);
            _leftPipe.gameObject.SetActive(true);
        }

        if (_ladderTop != null) _ladderTop.SetActive(_upEnabled);
        if (_ladderDown != null) _ladderDown.SetActive(_downEnabled);

        if (_upEnabled)
        {
            _col.size += Vector3.up * 0.75f;
            _col.center += Vector3.up * 0.375f;
        }
        if (_downEnabled)
        {
            _col.size += Vector3.up * 0.75f;
            _col.center += Vector3.down * 0.375f;
        }

        _rightPipe.localScale = new Vector3(0.1f, _ladderSizeY / 2f, 0.1f);
        _leftPipe.localScale = new Vector3(0.1f, _ladderSizeY / 2f, 0.1f);

        Vector3 bottomPoint = p.position - up * (_ladderSizeY * 0.5f);
        float pipeHalf = _rightPipe.localScale.y;

        _rightPipe.position = bottomPoint - up * (_ladderSizeY * 0.5f) + right * 0.4f - forward * 0.1f + up * pipeHalf;
        _leftPipe.position = bottomPoint - up * (_ladderSizeY * 0.5f) - right * 0.4f - forward * 0.1f + up * pipeHalf;

        _positionsFolder.localScale = new Vector3(1f, _ladderSizeY, 0.5f);
        _positionsFolder.position = p.position - up * (_ladderSizeY / 2f);

        int ioffset = _downEnabled ? 1 : 0;
        int i2 = _upEnabled ? 0 : 1;
        int total = Mathf.RoundToInt(_ladderSizeY * 2f);

        for (int i = i2; i < total + ioffset; i++)
        {
            Vector3 worldPos = p.position + down * (i * 0.5f) - forward * 0.1f;

            if (isT1)
            {
                if (_attachmentPrefab2 == null) continue;
                var obj = Instantiate(_attachmentPrefab2);
                obj.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 180f, 0f);
                obj.transform.position = worldPos - obj.transform.forward * 0.2f;
                var r = obj.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = _sharedMat;
                obj.transform.SetParent(_visual, true);
            }
            else
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.localScale = new Vector3(0.08f, 0.03f, 0.85f);
                obj.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 90f, 0f);
                obj.transform.position = worldPos;
                var r = obj.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = _sharedMat;
                obj.transform.SetParent(_visual, true);
            }
        }

        var leftRenderer = _leftPipe.GetComponent<Renderer>();
        if (leftRenderer != null) leftRenderer.sharedMaterial = _sharedMat;

        if (_ladderDown != null)
            _ladderDown.transform.position = p.position - Vector3.up * _ladderSizeY;

        SetCapMaterial(_ladderTop);
        SetCapMaterial(_ladderDown);

        if (isT1) CreateAttachments();
    }

    private void SetCapMaterial(GameObject cap)
    {
        if (cap == null) return;
        var r = cap.GetComponentInChildren<Renderer>(true);
        if (r != null) r.sharedMaterial = _sharedMat;
    }

    private void CreateAttachments()
    {
        if (_rightPipe == null || _rightPipe.parent == null) return;
        if (_attachmentPrefab == null) return;

        Transform p = _rightPipe.parent;
        Vector3 forward = p.forward;

        Vector3 wbl = p.TransformPoint(new Vector3(DrawOffsetLeft.z, 0f, DrawOffsetLeft.x));
        Vector3 wbr = p.TransformPoint(new Vector3(DrawOffsetRight.z, 0f, DrawOffsetRight.x));
        Vector3 wtl = p.TransformPoint(new Vector3(DrawOffsetLeft.z, -_ladderSizeY, DrawOffsetLeft.x));
        Vector3 wtr = p.TransformPoint(new Vector3(DrawOffsetRight.z, -_ladderSizeY, DrawOffsetRight.x));

        for (int i = 1; i < _ladderSizeY / 2f; i++)
        {
            float t = i * 2f / _ladderSizeY;
            Vector3 leftPoint = Vector3.Lerp(wbl, wtl, t) + transform.forward * 0.2f;
            Vector3 rightPoint = Vector3.Lerp(wbr, wtr, t) + transform.forward * 0.2f;

            NewAttachment(forward, leftPoint);
            NewAttachment(forward, rightPoint);
        }
    }

    private GameObject NewAttachment(Vector3 fwd, Vector3 pos)
    {
        const float offset = 0.2f;
        int mask = ~(1 << gameObject.layer);
        if (!Physics.Linecast(pos - fwd * offset, pos + fwd * offset, mask))
            return null;

        var attachment = Instantiate(_attachmentPrefab);
        attachment.transform.position = pos;
        attachment.transform.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(180f, 0f, 0f);
        var r = attachment.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = _sharedMat;
        attachment.transform.SetParent(_visual, true);
        return attachment;
    }

    // ---------- Climb ----------

    private void Climb()
    {
        //Debug.Log("Climb try");
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _distance, ~_targetLayer))
        {
            //Debug.Log("Raycast");
            if (hit.transform.tag == _tag && hit.transform == transform)
            {
                //Debug.Log("Tag");
                Vector3 newPos = new Vector3(_downPosition.position.x, _pm.transform.position.y, _downPosition.position.z);
                _pm.SetClimbing(true, newPos, _downPosition.position, _upPosition.position, _downCollider.position, _upCollider.position, transform.name);
            }
        }
    }

    // ---------- Gizmos ----------

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        if (_rightPipe == null || _rightPipe.parent == null) return;

        Transform p = _rightPipe.parent;

        Vector3 localBottomLeft = new Vector3(DrawOffsetLeft.z, 0f, DrawOffsetLeft.x);
        Vector3 localBottomRight = new Vector3(DrawOffsetRight.z, 0f, DrawOffsetRight.x);
        Vector3 localTopLeft = localBottomLeft - Vector3.up * _ladderSizeY;
        Vector3 localTopRight = localBottomRight - Vector3.up * _ladderSizeY;

        Vector3 worldBottomLeft = p.TransformPoint(localBottomLeft);
        Vector3 worldBottomRight = p.TransformPoint(localBottomRight);
        Vector3 worldTopLeft = p.TransformPoint(localTopLeft);
        Vector3 worldTopRight = p.TransformPoint(localTopRight);

        Gizmos.DrawLine(worldBottomLeft, worldTopLeft);
        Gizmos.DrawLine(worldBottomRight, worldTopRight);

        int start = _upEnabled ? 0 : 1;
        int total = Mathf.RoundToInt(_ladderSizeY * 2f);

        // Rungs
        for (int i = start; i < total; i++)
        {
            float t = i * 0.5f / _ladderSizeY;
            Gizmos.DrawLine(
                Vector3.Lerp(worldBottomLeft, worldTopLeft, t),
                Vector3.Lerp(worldBottomRight, worldTopRight, t));
        }

        // Depth ticks
        for (int i = start; i < total; i++)
        {
            float t = i * 2f / _ladderSizeY;
            Vector3 l = Vector3.Lerp(worldBottomLeft, worldTopLeft, t);
            Vector3 r = Vector3.Lerp(worldBottomRight, worldTopRight, t);
            Gizmos.DrawLine(l, l + transform.forward * 0.2f);
            Gizmos.DrawLine(r, r + transform.forward * 0.2f);
        }

        DrawPositionBox(_downCollider);
        DrawPositionBox(_upCollider);

#if UNITY_EDITOR
        if (_downCollider != null) Handles.Label(_downCollider.position, _downCollider.name);
        if (_upCollider != null) Handles.Label(_upCollider.position, _upCollider.name);
#endif
    }

    private void DrawPositionBox(Transform obj)
    {
        if (obj == null) return;
        Vector3 s = transform.localScale * 0.5f;
        Vector3 c = obj.position;

        Vector3 p1 = c + new Vector3(s.x, s.y, s.z);
        Vector3 p2 = c + new Vector3(-s.x, s.y, s.z);
        Vector3 p3 = c + new Vector3(s.x, -s.y, s.z);
        Vector3 p4 = c + new Vector3(-s.x, -s.y, s.z);
        Vector3 p5 = c + new Vector3(s.x, s.y, -s.z);
        Vector3 p6 = c + new Vector3(-s.x, s.y, -s.z);
        Vector3 p7 = c + new Vector3(s.x, -s.y, -s.z);
        Vector3 p8 = c + new Vector3(-s.x, -s.y, -s.z);

        Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p1, p3); Gizmos.DrawLine(p1, p5);
        Gizmos.DrawLine(p2, p4); Gizmos.DrawLine(p5, p7); Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p3, p7); Gizmos.DrawLine(p4, p8); Gizmos.DrawLine(p7, p8);
        Gizmos.DrawLine(p5, p6); Gizmos.DrawLine(p2, p6); Gizmos.DrawLine(p8, p6);
    }
}