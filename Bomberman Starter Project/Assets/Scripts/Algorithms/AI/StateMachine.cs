namespace StateStuff{

    public enum StateType:int
    {
        FIRST_STATE = 0,
        SECOND_STATE = 1
    }

	public class StateMachine<T>{
        public State<T> currentState { get; private set; }
        public StateType Type { get; private set; }
		public T Owner;

		public StateMachine(T _owner){
			Owner = _owner;
			currentState = null;
		}

		public void ChangeState(State<T> _newState,StateType _type){
			if(currentState != null)
				currentState.ExitState (Owner);
			currentState = _newState;
            Type = _type;
			currentState.EnterState (Owner);
		}

		public void Update(){
			if (currentState != null)
				currentState.UpdateState (Owner);
		}
	}

	public abstract class State<T>{
		public abstract void EnterState (T _owner);
		public abstract void ExitState(T _owner);
		public abstract void UpdateState(T _owner);
	}
}