using Azure.Search.Documents;
using BookSearch.Models;
using BookSearch.Global.Models;
using Azure.Search.Documents.Models;

namespace BookSearch.Services
{
	public interface IAzureSearchService
	{
		Task AddBookAsync(Book book);
		Task<List<string>> AutocompleteAsync(string term);
		Task<SearchResult> SearchBooksAsync(string query, int page);
	}

	public class AzureSearchService : IAzureSearchService
	{
		private readonly SearchClient _searchClient;
		private readonly IVectorizerApi _vectorizerApi;

		public AzureSearchService(SearchClient searchClient, IVectorizerApi vectorizerApi)
		{
			_searchClient = searchClient;
			_vectorizerApi = vectorizerApi;
		}

		public async Task<List<string>> AutocompleteAsync(string term)
		{
			if (term.Length > 50)
			{
				return [];
			}
			var options = new SuggestOptions { UseFuzzyMatching = true, OrderBy = { "search.score() desc" } };
			var response = await _searchClient.SuggestAsync<SearchResult.BookSearchResult>(term, "default_suggester", options);
			return response == null ? [] : response.Value.Results.Select(r => r.Text).ToList();
		}

		public async Task<SearchResult> SearchBooksAsync(string query, int page)
		{
			var queryVector = await _vectorizerApi.GetVectorAsync(query);

			var vectorSearchOptions = new VectorSearchOptions();
			var q1 = new VectorizedQuery(queryVector) { Weight = 2 };
			q1.Fields.Add("TitleVector");

			var q2 = new VectorizedQuery(queryVector) { Weight = 0.5f };
			q2.Fields.Add("DescriptionVector");

			vectorSearchOptions.Queries.Add(q1);
			vectorSearchOptions.Queries.Add(q2);

			var options = new SearchOptions
			{
				Filter = null,
				IncludeTotalCount = true,
				Size = SearchResult.PageSize,
				Skip = page * SearchResult.PageSize,
				VectorSearch = vectorSearchOptions,
			};

			var response = await _searchClient.SearchAsync<SearchResult.BookSearchResult>(query, options);

			var results = response.Value.GetResults().ToList();

			var books = results.Select(r => r.Document).ToList();

			var searchResult = new SearchResult()
			{
				Books = books,
				TotalCount = (int)response.Value.TotalCount,
			};

			return  searchResult;
		}

		public async Task AddBookAsync(Book book)
		{
			book.Id = Guid.NewGuid().ToString();

			book.TitleVector = await _vectorizerApi.GetVectorAsync(book.Title);

			if (!string.IsNullOrEmpty(book.Description))
			{
				book.DescriptionVector = await _vectorizerApi.GetVectorAsync(book.Description);
			}

			await _searchClient.UploadDocumentsAsync([book]);
		}
	}
}
