using System;
using System.Collections.Generic;

namespace core_api.Models.Pagination
{
    public class PagedList<T>
    {
        public List<T> Items { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(List<T> items, int count, PageQuery pageQery)
        {
            TotalCount = count;
            PageSize = pageQery.PageSize;
            CurrentPage = pageQery.PageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageQery.PageSize);

            Items = items;
        }
    }
}
