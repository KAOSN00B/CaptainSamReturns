using System.Collections.Generic;
using TMPro;
using UnityEngine;

// The HUD notification feed ("Guard Broken!", "Enemy Defeated"...), built on the Synty event-log item.
// Anything can post to it with EventLogUI.Show("message").
public class EventLogUI : MonoBehaviour
{
    [SerializeField] private Transform container;        // the Synty event log (has the vertical layout)
    [SerializeField] private GameObject itemTemplate;    // one Synty event-log item, kept switched off
    [SerializeField] private float messageLifetime = 3f;
    [SerializeField] private int maxMessages = 4;

    private static EventLogUI instance;
    private readonly Queue<GameObject> shown = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;
        if (itemTemplate != null) itemTemplate.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void Show(string message)
    {
        if (instance != null) instance.Push(message);
    }

    private void Push(string message)
    {
        if (itemTemplate == null || container == null) return;

        GameObject item = Instantiate(itemTemplate, container);
        TMP_Text label = item.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = message;
        item.SetActive(true);   // Synty's item animator plays its flash-in when it switches on

        shown.Enqueue(item);
        while (shown.Count > maxMessages)
        {
            GameObject oldest = shown.Dequeue();
            if (oldest != null) Destroy(oldest);
        }

        Destroy(item, messageLifetime);
    }
}
