using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Forum.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public string? GetPreviousPageLink(LinkGenerator linkGenerator, HttpContext httpContext, string endpointName, object? routeValues = null)
        {
            if (HasPrevious)
            {
                if (routeValues != null)
                {
                    var combinedRouteValues = new RouteValueDictionary(routeValues)
                    {
                        ["pageNumber"] = CurrentPage - 1,
                        ["pageSize"] = PageSize
                    };

                    return linkGenerator.GetUriByName(httpContext, endpointName, combinedRouteValues);
                }

                return linkGenerator.GetUriByName(httpContext, endpointName, new
                {
                    pageNumber = CurrentPage - 1,
                    pageSize = PageSize
                });
            }
            else
            {
                return null;
            }
        }

        public string? GetNextPageLink(LinkGenerator linkGenerator, HttpContext httpContext, string endpointName, object? routeValues = null)
        {
            if (HasNext)
            {
                if (routeValues != null)
                {
                    var combinedRouteValues = new RouteValueDictionary(routeValues)
                    {
                        ["pageNumber"] = CurrentPage + 1,
                        ["pageSize"] = PageSize
                    };

                    return linkGenerator.GetUriByName(httpContext, endpointName, combinedRouteValues);
                }

                return linkGenerator.GetUriByName(httpContext, endpointName, new 
                { 
                    pageNumber = CurrentPage + 1,
                    pageSize = PageSize
                });
            }
            else
            {
                return null;
            }
        }

        public PaginationMetadata CreatePaginationMetadata(LinkGenerator linkGenerator, HttpContext httpContext, string endpointName)
        {
            return new PaginationMetadata(TotalCount, PageSize, CurrentPage, TotalPages, 
                GetPreviousPageLink(linkGenerator, httpContext, endpointName),
                GetNextPageLink(linkGenerator, httpContext, endpointName));
        }

        public PagedList(List<T> items, int currentPage, int pageSize, int totalCount)
        {
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            PageSize = pageSize;
            TotalCount = totalCount;

            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, pageNumber, pageSize, count);
        }
    }
}
