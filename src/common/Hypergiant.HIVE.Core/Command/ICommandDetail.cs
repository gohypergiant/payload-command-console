namespace Hypergiant.HIVE
{
    public interface ICommandDetail : ICommand
    {
        ICommandResult Result { get; }
    }
}
