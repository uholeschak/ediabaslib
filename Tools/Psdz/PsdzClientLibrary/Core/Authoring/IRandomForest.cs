using PsdzClient.Core;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IRandomForest : IHideObjectMembers
    {
        void InitializeTrainingData(double[,] trainingData, int targetColumn = -1);

        bool IsClassifiableAs(double expectedValue, double[] testData);

        bool IsClassifiableAs(double minExpectedValue, double maxExpectedValue, double[] testData, bool internalRange = true);

        bool IsClassifiableAs(double[] expectedValues, double[] testData);

        bool CheckTrainingState();
    }
}
