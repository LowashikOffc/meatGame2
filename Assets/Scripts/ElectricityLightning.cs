using UnityEngine;

public class ElectricityLightning : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private Transform _lightTexture;
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
            SetEmission(state);
        }
    }
    public void SetEmission(bool state)
    {
        var renderer = _lightTexture.GetComponent<Renderer>();
        var mat = renderer.material;

        if (state)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1, 1, 1));
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
    }
}
