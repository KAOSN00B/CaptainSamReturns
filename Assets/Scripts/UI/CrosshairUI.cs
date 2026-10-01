using UnityEngine;

// Shows the HUD crosshair only while a gun is out.
public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector gunSelector;
    [SerializeField] private GameObject crosshair;

    private void OnEnable()
    {
        gunSelector.OnEquipChanged += HandleEquipChanged;
        HandleEquipChanged(gunSelector.IsGunEquipped);
    }

    private void OnDisable()
    {
        gunSelector.OnEquipChanged -= HandleEquipChanged;
    }

    private void HandleEquipChanged(bool gunOut)
    {
        crosshair.SetActive(gunOut);
    }
}
