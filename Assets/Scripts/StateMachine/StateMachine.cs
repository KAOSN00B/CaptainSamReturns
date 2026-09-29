using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private State currentState;

    void Start()
    {
        
    }


    void Update()
    {
        currentState?.Tick(Time.deltaTime);

    }

    public void SwitchState(State newState)
    {

        currentState?.Exit();

        currentState = newState;
        currentState?.Enter();

    }
}
