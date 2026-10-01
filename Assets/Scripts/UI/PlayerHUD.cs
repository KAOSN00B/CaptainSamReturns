using UnityEngine;

// Feeds the player's health and guard into the HUD every frame.
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Poise playerGuard;
    [SerializeField] private HUDBar healthBar;
    [SerializeField] private HUDBar guardBar;     // optional: guard as a bar
    [SerializeField] private HUDDial guardDial;   // optional: guard as a ring dial (Minimal HUD)

    private void Update()
    {
        if (playerHealth != null && healthBar != null)
            healthBar.SetValue(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (playerGuard == null) return;

        bool blocking = playerHealth != null && playerHealth.IsBlocking;   // show the guard while you block

        if (guardBar != null)
        {
            guardBar.SetValue(playerGuard.CurrentPoise, playerGuard.MaxPoise);
            guardBar.SetForceVisible(blocking);
        }

        if (guardDial != null)
        {
            guardDial.SetValue(playerGuard.CurrentPoise, playerGuard.MaxPoise);
            guardDial.SetForceVisible(blocking);
        }
    }
}
