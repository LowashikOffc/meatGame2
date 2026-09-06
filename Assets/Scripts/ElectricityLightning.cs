using UnityEngine;

public class ElectricityLightning : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private Transform _lightMaterial;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private float _energyUsing;
    [SerializeField] private EnergySafe _safe;
    private void FixedUpdate()
    {
        if (_safe != null)
        {
            bool state = _safe.OutputEnergy() >= _energyUsing;
            _audio.enabled = state;
            _light.enabled = state;
            _lightMaterial.gameObject.SetActive(state);
        }
    }
}
