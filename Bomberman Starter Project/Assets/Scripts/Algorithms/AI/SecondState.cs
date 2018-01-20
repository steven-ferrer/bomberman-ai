using UnityEngine;
using StateStuff;

public class SecondState : State<AggressiveAI>
{
    private static SecondState _instance;

    private SecondState()
    {
        if (_instance != null)
        {
            return;
        }

        _instance = this;
    }

    public static SecondState Instance
    {
        get
        {
            if (_instance == null)
            {
                new SecondState();
            }

            return _instance;
        }
    }

    public override void EnterState(AggressiveAI _owner)
    {
        Debug.Log("Entering Second State");
    }

    public override void ExitState(AggressiveAI _owner)
    {
        Debug.Log("Exiting Second State");
    }

    public override void UpdateState(AggressiveAI _owner)
    {
        for (int x = 1; x <= 5; x++)
        {
            Debug.Log("StateType: " + (int)_owner.stateMachine.Type + " count: " + x);
        }

        _owner.stateMachine.ChangeState(FirstState.Instance, StateType.FIRST_STATE);
    }

}
