#if UNITY_EDITOR
using System.Net.Mail;
using UnityEditor;
#endif
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private string _tag;
    [SerializeField] private float _distance;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Transform _downPosition;
    [SerializeField] private Transform _upPosition;
    [SerializeField] private Transform _downCollider;
    [SerializeField] private Transform _upCollider;
    [SerializeField] private Transform _positionsFolder;
    [SerializeField] private GameObject _attachentPrefab;
    [SerializeField] private GameObject _attachentPrefab2;
    [SerializeField] private int _type;
    [SerializeField] private GameObject _ladderTop;
    [SerializeField] private GameObject _ladderDown;
    private GameObject _player;
    private PlayerMovement _pm;

    [SerializeField] private Transform _rightPipe;
    [SerializeField] private Transform _leftPipe;

    [SerializeField] private bool _upEnabled;
    [SerializeField] private bool _downEnabled;
    [SerializeField] private float _ladderSizeY;
    private Vector3 _drawOffsetRight = new Vector3(-0.1f, 0, 0.4f);
    private Vector3 _drawOffsetLeft = new Vector3(-0.1f, 0, -0.4f);
    [SerializeField] private bool _drawGizmos;

    BoxCollider col;
    void Start()
    {
        _ladderTop.SetActive(_upEnabled);

        _camera = Camera.main;
        InputReceiver.Instance._interact += Climb;
        _player = GameObject.FindGameObjectWithTag("Player");
        _pm = _player.GetComponent<PlayerMovement>();

        CreateLadder(false);
    }

    private void OnValidate()
    {
        //CreateLadder(true);
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        Transform p = _rightPipe.parent;

        Vector3 localBottomLeft = new Vector3(_drawOffsetLeft.z, 0, _drawOffsetLeft.x);
        Vector3 localBottomRight = new Vector3(_drawOffsetRight.z, 0, _drawOffsetRight.x);
        Vector3 localTopLeft = localBottomLeft - Vector3.up * _ladderSizeY;
        Vector3 localTopRight = localBottomRight - Vector3.up * _ladderSizeY;

        Vector3 worldBottomLeft = p.TransformPoint(localBottomLeft);
        Vector3 worldBottomRight = p.TransformPoint(localBottomRight);
        Vector3 worldTopLeft = p.TransformPoint(localTopLeft);
        Vector3 worldTopRight = p.TransformPoint(localTopRight);

        Gizmos.DrawLine(worldBottomLeft, worldTopLeft);
        Gizmos.DrawLine(worldBottomRight, worldTopRight);

        int i2 = 1;
        if (_upEnabled) i2 = 0;
        for (int i = i2; i < _ladderSizeY * 2; i++)
        {
            float t = i * 0.5f / _ladderSizeY;
            Vector3 leftPoint = Vector3.Lerp(worldBottomLeft, worldTopLeft, t);
            Vector3 rightPoint = Vector3.Lerp(worldBottomRight, worldTopRight, t);
            Gizmos.DrawLine(leftPoint, rightPoint);
        }
        for (int i = i2; i < _ladderSizeY * 2; i++)
        {
            float t = i * 2f / _ladderSizeY;
            Vector3 leftPoint = Vector3.Lerp(worldBottomLeft, worldTopLeft, t);
            Vector3 rightPoint = Vector3.Lerp(worldBottomRight, worldTopRight, t);
            Gizmos.DrawLine(leftPoint, leftPoint + transform.forward * 0.2f);
            Gizmos.DrawLine(rightPoint, rightPoint + transform.forward * 0.2f);
        }

        #if UNITY_EDITOR
        Handles.Label(_downCollider.position, _downCollider.name);
        Handles.Label(_upCollider.position, _upCollider.name);
        #endif
        DrawPosition(_downCollider);
        DrawPosition(_upCollider);
    }

    private void DrawPosition(Transform obj)
    {
        if (obj == null) return;
        Vector3 pos1 = obj.position + new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z / 2);
        Vector3 pos2 = obj.position + new Vector3(-transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z / 2);
        Vector3 pos3 = obj.position + new Vector3(transform.localScale.x / 2, -transform.localScale.y / 2, transform.localScale.z / 2);
        Vector3 pos4 = obj.position + new Vector3(-transform.localScale.x / 2, -transform.localScale.y / 2, transform.localScale.z / 2);
        Vector3 pos5 = obj.position + new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, -transform.localScale.z / 2);
        Vector3 pos6 = obj.position + new Vector3(-transform.localScale.x / 2, transform.localScale.y / 2, -transform.localScale.z / 2);
        Vector3 pos7 = obj.position + new Vector3(transform.localScale.x / 2, -transform.localScale.y / 2, -transform.localScale.z / 2);
        Vector3 pos8 = obj.position + new Vector3(-transform.localScale.x / 2, -transform.localScale.y / 2, -transform.localScale.z / 2);

        Gizmos.DrawLine(pos1, pos2);
        Gizmos.DrawLine(pos1, pos3);
        Gizmos.DrawLine(pos1, pos5);
        Gizmos.DrawLine(pos2, pos4);
        Gizmos.DrawLine(pos5, pos7);
        Gizmos.DrawLine(pos3, pos4);
        Gizmos.DrawLine(pos3, pos7);
        Gizmos.DrawLine(pos4, pos8);
        Gizmos.DrawLine(pos7, pos8);
        Gizmos.DrawLine(pos5, pos6);
        Gizmos.DrawLine(pos2, pos6);
        Gizmos.DrawLine(pos8, pos6);

        //Vector3 offset = Vector3.up * 0.05f + Vector3.right * 0.05f;

        //#if UNITY_EDITOR
        //Handles.Label(pos1 + offset, "Pos1: " + pos1.ToString());
        //Handles.Label(pos2 + offset, "Pos2: " + pos2.ToString());
        //Handles.Label(pos3 + offset, "Pos3: " + pos3.ToString());
        //Handles.Label(pos4 + offset, "Pos4: " + pos4.ToString());
        //Handles.Label(pos5 + offset, "Pos5: " + pos5.ToString());
        //Handles.Label(pos6 + offset, "Pos6: " + pos6.ToString());
        //Handles.Label(pos7 + offset, "Pos7: " + pos7.ToString());
        //Handles.Label(pos8 + offset, "Pos8: " + pos8.ToString());
        //#endif

    }

    private void CreateLadder(bool editor)
    {
        if (editor) return;
        Transform p = _rightPipe.parent;

        Vector3 forward = p.forward;
        Vector3 right = p.right;
        Vector3 up = p.up;
        Vector3 down = -up;

        col = GetComponent<BoxCollider>();
        col.size = new Vector3(1, _ladderSizeY, 0.1f);
        col.center = new Vector3(0, _ladderSizeY * -0.5f, 0);

        if (_type != 0)
        {
            _upEnabled = false;
            _downEnabled = false;
            _rightPipe.gameObject.SetActive(false);
            _leftPipe.gameObject.SetActive(false);
            _ladderTop.SetActive(false);
            _ladderDown.SetActive(false);
        }
        else
        {
            _rightPipe.gameObject.SetActive(true);
            _leftPipe.gameObject.SetActive(true);
            _ladderTop.SetActive(true);
            _ladderDown.SetActive(true);
        }

        if (_upEnabled)
        {
            col.size += Vector3.up * 0.75f;
            col.center += Vector3.up * 0.375f;
        }
        if (_downEnabled)
        {
            col.size += Vector3.up * 0.75f;
            col.center += Vector3.down * 0.375f;
        }
        _rightPipe.localScale = new Vector3(0.1f, _ladderSizeY / 2, 0.1f);
        _leftPipe.localScale = new Vector3(0.1f, _ladderSizeY / 2, 0.1f);

        Vector3 bottomPoint = p.position - up * (_ladderSizeY * 0.5f);

        _rightPipe.position = bottomPoint + up * (_ladderSizeY * 0.5f) + right * 0.4f - forward * 0.1f - down * -_rightPipe.localScale.y;
        _leftPipe.position = bottomPoint + up * (_ladderSizeY * 0.5f) - right * 0.4f - forward * 0.1f - down * -_rightPipe.localScale.y;

        _positionsFolder.transform.localScale = new Vector3(1, _ladderSizeY, 0.5f);
        _positionsFolder.transform.position = p.position - up * (_ladderSizeY / 2);


        int ioffset = 0;
        int i2 = 1;
        if (_upEnabled) i2 = 0;
        if (_downEnabled) ioffset = 1;
        for (int i = i2; i < _ladderSizeY * 2 + ioffset; i++)
        {
            Vector3 addPos = down * (i * 0.5f) - forward * 0.1f;

            Vector3 worldPos = p.position + addPos;

            GameObject obj = null;
            if (_type == 1)
            {
                obj = Instantiate(_attachentPrefab2);
                obj.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180, 0);
                obj.transform.position = worldPos - obj.transform.forward * 0.2f;
            }
            else
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.localScale = new Vector3(0.08f, 0.03f, 0.85f);
                obj.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 90, 0);
                obj.transform.position = worldPos;
            }
            obj.GetComponent<Renderer>().material = _rightPipe.GetComponent<Renderer>().material;
            obj.transform.parent = transform.Find("Visual");

        }
        if (_ladderDown) _ladderDown.transform.position = p.position - new Vector3(0, _ladderSizeY, 0);
        if (_type == 0) CreateAttachments();
    }

    private void CreateAttachments()
    {
        Transform p = _rightPipe.parent;

        Vector3 forward = p.forward;
        Vector3 right = p.right;
        Vector3 up = p.up;
        Vector3 down = -up;

        Vector3 localBottomLeft = new Vector3(_drawOffsetLeft.z, 0, _drawOffsetLeft.x);
        Vector3 localBottomRight = new Vector3(_drawOffsetRight.z, 0, _drawOffsetRight.x);
        Vector3 localTopLeft = localBottomLeft - Vector3.up * _ladderSizeY;
        Vector3 localTopRight = localBottomRight - Vector3.up * _ladderSizeY;

        Vector3 worldBottomLeft = p.TransformPoint(localBottomLeft);
        Vector3 worldBottomRight = p.TransformPoint(localBottomRight);
        Vector3 worldTopLeft = p.TransformPoint(localTopLeft);
        Vector3 worldTopRight = p.TransformPoint(localTopRight);

        for (int i = 1; i < _ladderSizeY / 2; i++)
        {
            float t = i * 2f / _ladderSizeY;
            Vector3 leftPoint = Vector3.Lerp(worldBottomLeft, worldTopLeft, t) + transform.forward * 0.2f;
            Vector3 rightPoint = Vector3.Lerp(worldBottomRight, worldTopRight, t) + transform.forward * 0.2f;

            GameObject att1 = NewAttachment(forward, leftPoint);
            GameObject att2 = NewAttachment(forward, rightPoint);
        }
    }

    private GameObject NewAttachment(Vector3 fwd, Vector3 pos)
    {
        float offset = 0.2f;
        //Debug.Log(gameObject.layer);
        if (Physics.Linecast(pos - fwd * offset, pos + fwd * offset, out RaycastHit hit, ~gameObject.layer))
        {
            //Debug.Log($"hit point: {hit.transform.gameObject}");
            //if (!hit.transform.CompareTag("Untagged")) return null;

            GameObject attachment = Instantiate(_attachentPrefab);
            attachment.transform.position = pos;
            attachment.GetComponent<Renderer>().material = _rightPipe.GetComponent<Renderer>().material;
            attachment.transform.parent = transform.Find("Visual");

            attachment.transform.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(180, 0, 0);

            return attachment;
        }
        else return null;
    }

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
                Vector3 newPos = new Vector3(_downPosition.position.x, _player.transform.position.y, _downPosition.position.z);
                _pm.SetClimbing(true, newPos, _downPosition.position, _upPosition.position, _downCollider.position, _upCollider.position, transform.name);
            }
        }
    }
}
