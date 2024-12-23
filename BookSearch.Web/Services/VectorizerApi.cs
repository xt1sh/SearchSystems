using BookSearch.Global.Models;

namespace BookSearch.Services
{
	public interface IVectorizerApi
	{
		Task<float[]> GetVectorAsync(string str);
	}

	public class VectorizerApi : IVectorizerApi
	{
		private readonly HttpClient _httpClient;

		public VectorizerApi(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory.CreateClient();
		}

		public async Task<float[]> GetVectorAsync(string str)
		{
			var response = await _httpClient.GetFromJsonAsync<VectorizeResponse>($"http://localhost:1057/vectorize?query={str}");

			return response.Embeddings;
		}
	}
}
