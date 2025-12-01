using System;
using System.Collections.Generic;
using System.ComponentModel;

#pragma warning disable CS0109
namespace PsdzClient.Core.Container
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuJob : INotifyPropertyChanged
    {
        string EcuName { get; }

        DateTime ExecutionEndTime { get; }

        DateTime ExecutionStartTime { get; }

        bool FASTARelevant { get; }

        int JobErrorCode { get; }

        string JobErrorText { get; }

        string JobName { get; }

        string JobParam { get; }

        string JobResultFilter { get; }

        int JobResultSets { get; }

        bool IsDone();
        bool IsFASTARelevant(ushort set, string resultName);
        bool IsJobState(ushort set, string state);
        bool IsJobState(string state);
        bool IsNullOrEmpty(ushort set, string resultName);
        bool IsNullOrEmpty(string resultName);
        bool IsOkay();
        bool IsOkay(ushort set);
        new string ToString();
        byte[] getByteArrayResult(ushort set, string resultName, out uint len);
        byte[] getByteArrayResult(string resultName, out uint len);
        byte? getByteResult(ushort set, string resultName);
        byte? getByteResult(string resultName);
        object getISTAResult(string resultName);
        T getISTAResultAs<T>(string resultName);
        object getISTAResultAsType(string resultName, Type targetType);
        object getResult(ushort set, string resultName);
        object getResult(string resultName, bool getLast = false);
        T getResultsAs<T>(string resultName, T defaultRes = default(T), int set = -1);
        T getResultAs<T>(ushort set, string resultName, T defaultRes = default(T));
        T getResultAs<T>(string resultName, T defaultRes = default(T), bool getLast = false);
        int getResultFormat(ushort set, string resultName);
        int getResultFormat(string resultName);
        IList<IEcuResult> getResultSet(ushort set);
        string getResultString(ushort set, string resultName, string format);
        IList<string> getResultStringList(ushort startSet, ushort stopSet, string resultName, string format);
        string getStringResult(ushort set, string resultName);
        string getStringResult(string resultName);
        char? getcharResult(ushort set, string resultName);
        char? getcharResult(string resultName);
        double? getdoubleResult(ushort set, string resultName);
        double? getdoubleResult(string resultName);
        long? getlongResult(ushort set, string resultName);
        long? getlongResult(string resultName);
        int? getintResult(ushort set, string resultName);
        int? getintResult(string resultName);
        short? getshortResult(ushort set, string resultName);
        short? getshortResult(string resultName);
        uint? getuintResult(ushort set, string resultName);
        uint? getuintResult(string resultName);
        ushort? getushortResult(ushort set, string resultName);
        ushort? getushortResult(string resultName);
        void maskResultFASTARelevant(bool defRelevant);
        void maskResultFASTARelevant(ushort startSet, int stopSet, string resultName);
    }
}