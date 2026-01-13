namespace Inventory.Application.DTOs.Product
{
    public class ProductQueryParamsDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public string SortBy { get; set; } = "Id";
        public bool Ascending { get; set; } = true;
    }
}
