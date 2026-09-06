using UnityEngine;

public class Cable : MonoBehaviour
{
    [SerializeField] private Rigidbody _in;
    [SerializeField] private Rigidbody _out;
    [SerializeField] private float _maxLength;
    [SerializeField] private float _size;
    [SerializeField] private float _offset;

    private Rigidbody _pickupped;
    private EnergySafe _panelIN;
    private EnergySafe _panelOUT;
    public float _energy = 0;

    public bool _inConnected;
    public bool _outConnected;

    public Rigidbody GetRigidbody(bool b)
    {
        if (b) return _in;
        else return _out;
    }

    private Rigidbody GetPickuppedObject()
    {
        if (_in.angularDrag > 1) return _in;
        else if (_out.angularDrag > 1) return _out;
        return null;
    }

    public void Connect(EnergySafe panel, bool type, Transform connectPosition)
    {
        if (type == true)
        {
            //Debug.Log("Connected IN");
            _panelIN = panel;
            _panelIN.Connect(this, type);
            _inConnected = true;
            _in.isKinematic = true;
            _in.position = connectPosition.position;
            _in.rotation = connectPosition.rotation;
        }
        if (type == false)
        {
            //Debug.Log("Connected OUT");
            _panelOUT = panel;
            _panelOUT.Connect(this, type);
            _outConnected = true;
            _out.isKinematic = true;
            _out.position = connectPosition.position;
            _out.rotation = connectPosition.rotation;
        }
    }

    public void Disconnect(Rigidbody rb)
    {
        if (rb == _in && _inConnected)
        {
            if (_panelIN == null) return;
            //Debug.Log("Disonnected IN");
            _inConnected = false;
            _in.isKinematic = false;
            _panelIN.Disconnect(true, _panelIN);
        }
        else if (rb == _out && _outConnected)
        {
            if (_panelOUT == null) return;
            //Debug.Log("Disonnected OUT");
            _outConnected = false;
            _out.isKinematic = false;
            _panelOUT.Disconnect(false, _panelOUT);
        }
    }

    public void InputEnergy(float energy)
    {
        _energy = energy;
        if (!_outConnected) OutputEnergy();
    }

    public float OutputEnergy()
    {
        float saved = _energy;
        _energy = 0;
        return saved;
    }

    private void FixedUpdate()
    {
        Rigidbody newObj = GetPickuppedObject();
        if (newObj != null && _pickupped != newObj)
        {
            //Debug.Log("Pickup changed to: " + newObj.name);
            Disconnect(newObj);
        }
        _pickupped = newObj;
    }
}
