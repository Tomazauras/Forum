namespace Forum.Helpers
{
    public class SearchParameters
    {
        private int _PageNumber = 1;
        private int _PageSize = 5;

        private const int MaxPageSize = 50;

        public int? PageNumber { get => _PageNumber; set => _PageNumber = value is null or <= 0 ? _PageNumber : value.Value; }

        public int? PageSize { get => _PageSize > MaxPageSize ? MaxPageSize : _PageSize; set => _PageSize = value is null or <= 0 ? _PageSize : value.Value; }
    }
}
