using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Keycode
{
    [HideInInspector]
    public string _name;
    public KeyCode key;
    public Keys keys;
}
public class InputReceiver : MonoBehaviour
{
    public static InputReceiver Instance { get; private set; }
    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    private KeyCode _forward;
    private KeyCode _back;
    private KeyCode _left;
    private KeyCode _right;
    private KeyCode _jump;
    private KeyCode _crouch;
    private KeyCode _pickup;
    private KeyCode _drop;

    public event Action _jumpAction;
    public event Action<bool> _crouchAction;
    public event Action _interact;
    public event Action _dropAction;

    [Header("initialize keycodes")]
    [SerializeField] private List<Keycode> _keycodes;

    private void OnValidate()
    {
        foreach (Keycode k in _keycodes)
        {
            k._name = k.keys.ToString() + " : " + k.key;
        }
    }

    private void Start()
    {
        InitKeys();
    }
    private void InitKeys()
    {
        foreach (var keycode in _keycodes)
        {
            switch (keycode.keys)
            {
                case Keys.forward:
                    _forward = keycode.key;
                    break;

                case Keys.back:
                    _back = keycode.key;
                    break;

                case Keys.left:
                    _left = keycode.key;
                    break;

                case Keys.right:
                    _right = keycode.key;
                    break;

                case Keys.jump:
                    _jump = keycode.key;
                    break;

                case Keys.crouch:
                    _crouch = keycode.key;
                    break;
                    
                case Keys.interact:
                    _pickup = keycode.key;
                    break;

                case Keys.drop:
                    _drop = keycode.key;
                    break;
            }
        }
    }

    private void Update()
    {
        float horizontal = 0;
        float vertical = 0;

        if (Input.GetKey(_right)) horizontal += 1;
        if (Input.GetKey(_left)) horizontal -= 1;
        if (Input.GetKey(_forward)) vertical += 1;
        if (Input.GetKey(_back)) vertical -= 1;

        MoveInput = new Vector2(horizontal, vertical).normalized;

        if (Input.GetKeyDown(_jump)) _jumpAction?.Invoke();
        if (Input.GetKeyDown(_crouch)) _crouchAction?.Invoke(true);
        if (Input.GetKeyUp(_crouch)) _crouchAction?.Invoke(false);
        if (Input.GetKeyDown(_pickup)) _interact?.Invoke();
        if (Input.GetKeyUp(_pickup)) _dropAction?.Invoke();
    }
}

public enum Keys
{
    forward,
    back,
    right,
    left,
    jump,
    crouch,
    interact,
    drop,
}