namespace BookSearch.Configs
{
	public class AzureSearchConfig
	{
		public string ServiceName { get; set; }

		public Uri ServiceUrl => new($"https://{ServiceName}.search.windows.net");

		public string IndexName { get; set; }

		public string ApiKey { get; set; }
	}
}
