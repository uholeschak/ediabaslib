namespace BMW.Rheingold.CoreFramework
{
    public class Result : IResult
    {
        private CollectiveResultSet _CollectiveResult;

        public CollectiveResultSet CollectiveResult
        {
            get
            {
                return _CollectiveResult;
            }
            set
            {
                _CollectiveResult = value;
            }
        }

        public Result()
        {
            _CollectiveResult = CollectiveResultSet.Ok;
        }

        public override string ToString()
        {
            return _CollectiveResult.ToString();
        }
    }
}
