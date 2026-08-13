using BMW.Authoring;
using BMW.Authoring.API;
using PsdzClient.Core;
using System;
using System.Linq;
using System.Text;
using PsdzClient;
using SharpLearning.Containers.Matrices;

#pragma warning disable CS0169
namespace BMW.Authoring.API
{
    public class RandomForest : IRandomForest, IHideObjectMembers
    {
        [PreserveSource(Hint = "ClassificationForestModel", Placeholder = true)]
        private PlaceholderType model;

        private bool isModelTrained;

        public void InitializeTrainingData(double[,] trainingData, int targetColumn = -1)
        {
            if (trainingData != null)
            {
                F64Matrix observationsMatrix = GetObservationsMatrix(trainingData, targetColumn);
                double[] targetsVector = GetTargetsVector(trainingData, targetColumn);
                //[-] ClassificationRandomForestLearner classificationRandomForestLearner = new ClassificationRandomForestLearner(500);
                //[-] model = classificationRandomForestLearner.Learn(observationsMatrix, targetsVector);
                isModelTrained = true;
            }
            else
            {
                Log.Error("RandomForest.InitializeTrainingData()", "Could not initialize training data, value is null.");
            }
        }

        public bool IsClassifiableAs(double expectedValue, double[] testData)
        {
            return Predict(testData) == expectedValue;
        }

        public bool IsClassifiableAs(double minExpectedValue, double maxExpectedValue, double[] testData, bool internalRange = true)
        {
            double num = Predict(testData);
            if (internalRange)
            {
                if (num >= minExpectedValue)
                {
                    return num <= maxExpectedValue;
                }
                return false;
            }
            if (!(num < minExpectedValue))
            {
                return num > maxExpectedValue;
            }
            return true;
        }

        public bool IsClassifiableAs(double[] expectedValues, double[] testData)
        {
            double value = Predict(testData);
            return expectedValues.Contains(value);
        }

        public bool CheckTrainingState()
        {
            return isModelTrained;
        }

        private string GetCsv(double[,] trainingData)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < trainingData.GetLength(0); i++)
            {
                for (int j = 0; j < trainingData.GetLength(1) - 1; j++)
                {
                    stringBuilder.Append(trainingData[i, j]);
                    stringBuilder.Append(";");
                }
                stringBuilder.Append(trainingData[i, trainingData.GetLength(1) - 1]);
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }

        private F64Matrix GetObservationsMatrix(double[,] trainingData, int targetColumn = -1)
        {
            int num = ((targetColumn == -1) ? (trainingData.GetLength(1) - 1) : targetColumn);
            double[] array = new double[trainingData.GetLength(0) * (trainingData.GetLength(1) - 1)];
            int num2 = 0;
            for (int i = 0; i < trainingData.GetLength(0); i++)
            {
                for (int j = 0; j < trainingData.GetLength(1); j++)
                {
                    if (j != num)
                    {
                        array[num2++] = trainingData[i, j];
                    }
                }
            }
            return new F64Matrix(array, trainingData.GetLength(0), trainingData.GetLength(1) - 1);
        }

        private double[] GetTargetsVector(double[,] trainingData, int targetColumn = -1)
        {
            int targetColumnIndex = ((targetColumn == -1) ? (trainingData.GetLength(1) - 1) : targetColumn);
            return (from row in Enumerable.Range(0, trainingData.GetLength(0))
                    select trainingData[row, targetColumnIndex]).ToArray();
        }

        private double Predict(double[] testData)
        {
            //[-] if (model != null)
            //[-] {
            //[-]   return model.Predict(testData);
            //[-] }
            throw new InvalidOperationException("The random forest model has not been initialized.");
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
