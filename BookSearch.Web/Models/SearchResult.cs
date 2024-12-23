namespace BookSearch.Models
{
	public class SearchResult
	{
		public const int PageSize = 20;

		public List<BookSearchResult> Books { get; set; }

		public int TotalCount { get; set; }

		public int CurrentPage { get; set; }

		public string Query { get; set; }

		public int Count => Books.Count;

		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
		public bool HasPreviousPage => CurrentPage > 1;
		public bool HasNextPage => CurrentPage < TotalPages;

		public class BookSearchResult
		{
			public string Title { get; set; }
			public string Authors { get; set; }
			public string Description { get; set; }
			public string Category { get; set; }
			public int? Year { get; set; }
		}
	}
}
