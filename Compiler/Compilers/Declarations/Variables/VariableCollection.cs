namespace General.Shaders
{
    interface IVariableCollection
    {
        Variable? GetVariable(string name);
        void PushVariable(Variable variable);
    }
}
