using UnityEngine;

public class EnergySafe : MonoBehaviour
{
    [SerializeField] private float _energy;
    [SerializeField] private float _energyOutput;

    private Cable _INcable;
    private Cable _OUTcable;

    private Rigidbody _INrigidbody;
    private Rigidbody _OUTrigidbody;

    //[SerializeField] private bool _typeCanConnect;

    private bool _connectedIN;
    private bool _connectedOUT;
    //public bool _connectedType;

    public float OutputEnergy()
    {
        if (_energy <= 0) return 0;
        _energy -= _energyOutput;
        return _energyOutput;
    }

    public bool GetBoolIN()
    {
        return _connectedIN;
    }
    public bool GetBoolOUT()
    {
        return _connectedOUT;
    }
    public Rigidbody GetRigidbodyIN()
    {
        return _INrigidbody;
    }
    public Rigidbody GetRigidbodyOUT()
    {
        return _OUTrigidbody;
    }

    public Cable GetCableIN()
    {
        return _INcable;
    }
    public Cable GetCableOUT()
    {
        return _OUTcable;
    }
    public string GetParameter(string name)
    {
        if (name == "_energy") return _energy.ToString();
        else if (name == "_energyOutput") return _energyOutput.ToString();
        //else if (name == "_cable") return _cable.ToString();
        //else if (name == "_typeCanConnect")
        //{
        //    if (_typeCanConnect) return "IN";
        //    else return "OUT";
        //}
        //else if (name == "_connected") return _connected.ToString();
        //else if (name == "_connectedType") return _connectedType.ToString();
        return "";
    }

    //public bool GetTypeCanConnect()
    //{
    //    return _typeCanConnect;
    //}

    public void Connect(Cable cable, bool type)
    {
        if (_energy > 0)
        {
            int rand = Random.Range(1, 4);
            if (rand == 1) SoundService.Instance.PlaySound(SoundBase.ElectricConnect1, transform.position, 0.3f);
            if (rand == 2) SoundService.Instance.PlaySound(SoundBase.ElectricConnect2, transform.position, 0.5f);
            if (rand == 3) SoundService.Instance.PlaySound(SoundBase.ElectricConnect3, transform.position, 0.5f);
            cable.GetRigidbody(type).transform.Find("Particles").GetComponent<ParticleSystem>().Play();
        }
        if (_connectedIN == false && type)
        {
            _connectedIN = true;
            _INcable = cable;
            _INrigidbody = cable.GetRigidbody(type);
        }
        if (_connectedOUT == false && !type)
        {
            _connectedOUT = true;
            _OUTcable = cable;
            _OUTrigidbody = cable.GetRigidbody(type);
        }
        //Debug.Log($"ConnectedType: {_connectedType}");
    }

    public void Disconnect(bool type, EnergySafe safe)
    {
        //Debug.Log(safe == this);
        if (safe != this) return;
        if (_connectedIN == true && type)
        {
            _connectedIN = false;
            _INcable = null;
            _INrigidbody = null;
        }
        if (_connectedOUT == true && !type)
        {
            _connectedOUT = false;
            _OUTcable = null;
            _OUTrigidbody = null;
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log($"Connected: {_connected}, ConnectType: {_connectedType}, Cable: {_cable}");
        if (_INcable)
        {
            if (_connectedIN) _INcable.InputEnergy(OutputEnergy());
        }
        if (_OUTcable)
        {
            if (_connectedOUT) _energy += _OUTcable.OutputEnergy();
        }
    }
}
