namespace core_api.Models.Pagination
{
    public class PageQuery
    {
        const int maxPageSize = 20;

        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        private int _pageNumber = 1;
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                _pageNumber = (value <= 0) ? 1 : value;
            }
        }
    }
}
