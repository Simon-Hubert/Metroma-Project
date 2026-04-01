using Metroma.Inputs;

namespace Metroma
{
    public interface IControllable
    {
        public void SendInputs(GameplayInputsData inputs);
    }
}
