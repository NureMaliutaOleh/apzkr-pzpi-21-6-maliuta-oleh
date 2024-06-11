using SmartInlet.Server.Requests;

namespace SmartInlet.Server.Responses
{
    public class PageResponse<T> : BaseResponse
        where T : class
    {
        public PageResponse(ICollection<T> items, int pageIndex, int pageSize, int pageCount) : base(true, null, null)
        {
            Data = new View<T>(items, pageIndex, pageSize, pageCount);
        }

        public PageResponse(ICollection<T> items, PageRequest request, int pageCount) : base(true, null, null)
        {
            Data = new View<T>(items, request.Page, request.PageSize, pageCount);
        }

        public class View<O>
            where O : class
        {
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public int PageCount { get; set; }

            public ICollection<O> Items { get; set; }

            public View(ICollection<O> items, int pageIndex, int pageSize, int pageCount)
            {
                Items = items;
                PageCount = pageCount;
                PageSize = pageSize;
                PageIndex = pageIndex;
            }
        }
    }
}
