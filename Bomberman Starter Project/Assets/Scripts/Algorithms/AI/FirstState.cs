using UnityEngine;
using StateStuff;

public class FirstState : State<AggressiveAI> 
{
    private static FirstState _instance;

    private FirstState()
    {
        if(_instance != null){
            return;
        }

        _instance = this;
    }

    public static FirstState Instance
    {
        get
        {
            if (_instance == null)
            {
                new FirstState();
            }

            return _instance;
        }
    }

    public override void EnterState(AggressiveAI _owner)
    {
        Debug.Log("Entering First State");
    }

    public override void ExitState(AggressiveAI _owner)
    {
        Debug.Log("Exiting First State");
    }

    public override void UpdateState(AggressiveAI _owner)
    {
        for (int x = 1; x <= 5; x++)
        {
            Debug.Log("StateType: " + (int)_owner.stateMachine.Type + " count: " + x);
        }

        _owner.stateMachine.ChangeState(SecondState.Instance, StateType.SECOND_STATE);
    }

}
