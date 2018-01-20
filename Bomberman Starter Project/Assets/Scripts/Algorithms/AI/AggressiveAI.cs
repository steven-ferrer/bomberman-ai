using UnityEngine;
using StateStuff;

public class AggressiveAI : MonoBehaviour 
{
    public bool switchSate = false;
    public float gameTimer;
    public int seconds = 0;

    public StateMachine<AggressiveAI> stateMachine { get; set; }

    private void Start()
    {
        stateMachine = new StateMachine<AggressiveAI>(this);
        stateMachine.ChangeState(FirstState.Instance,StateType.FIRST_STATE);
        gameTimer = Time.time;
    }

    private void Update()
    {
        if (Time.time > gameTimer + 1)
        {
            gameTimer = Time.time;
            seconds++;
            Debug.Log(seconds);
        }

        if (seconds == 5)
        {
            seconds = 0;
            switchSate = !switchSate;
        }

        stateMachine.Update();
    }
}
