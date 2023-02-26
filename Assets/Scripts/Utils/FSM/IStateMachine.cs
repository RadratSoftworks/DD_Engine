namespace DDEngine.Utils.FSM
{
    public interface IStateMachine
    {
        public void GiveData(object data);

        public void GiveDataFrom(IStateMachine otherMachine, object data);
    }
}