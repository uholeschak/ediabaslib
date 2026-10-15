
namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingState
    {
        bool IsErrorState { get; }

        bool IsFinal { get; }

        string LocalizedDescription { get; }

        string ErrorText { get; }

        IProgrammingStateData Data { get; }

        string Label { get; }
    }
}
