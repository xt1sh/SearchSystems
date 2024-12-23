using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BookSearch.Global.Models;
using CsvHelper;
using System.Globalization;
using System.Net.Http.Json;

const string endpoint = "https://mp41-search-systems.search.windows.net";
const string indexName = "books";
const string apiKey = "";

using var reader = new StreamReader(@"C:\Users\azyla\Downloads\BooksDatasetClean.csv\BooksDatasetClean.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = new List<Book>();
csv.Read();
csv.ReadHeader();

var id = 1;

while (csv.Read() && id <= 2000)
{
	var record = new Book
	{
		Id = $"{id++}",
		Title = csv.GetField(0),
		Authors = csv.GetField(1),
		Description = csv.GetField(2),
		Category = csv.GetField(3),
		Year = csv.GetField<int>(7)
	};

	if (string.IsNullOrEmpty(record.Description))
	{
		id--;
		continue;
	}

	if (record.Authors != null && record.Authors.StartsWith("By "))
	{
		record.Authors = record.Authors[3..];
	}

	records.Add(record);
}

var httpClient = new HttpClient();

httpClient.Timeout = TimeSpan.FromHours(2);

var response = await httpClient.PostAsJsonAsync("http://localhost:1057/vectorizeBulk", records.Select(r => r.Title));

var titleVectors = await response.Content.ReadFromJsonAsync<List<VectorizeBulkResponse>>(new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

foreach (var record in records)
{
	record.TitleVector = titleVectors.First(t => t.InitialString == record.Title).Embeddings;
}

var recordsDescriptions = records.Where(r => r.Description != null).Select(r => r.Description);

response = await httpClient.PostAsJsonAsync("http://localhost:1057/vectorizeBulk", recordsDescriptions);

var descriptionVectors = await response.Content.ReadFromJsonAsync<List<VectorizeBulkResponse>>();

foreach (var record in records.Where(r => r.Description != null))
{
	record.DescriptionVector = descriptionVectors.First(t => t.InitialString == record.Description).Embeddings;
}


var serviceClient = new SearchClient(
			new Uri(endpoint),
			indexName,
			new AzureKeyCredential(apiKey)
		);

var bulk = new List<Book>();

for (int i = 0; i < records.Count; i++)
{
	bulk.Add(records[i]);

	if (i != 0 && i % 1000 == 0)
	{
		var batch = IndexDocumentsBatch.Upload(bulk);
		var result = await serviceClient.IndexDocumentsAsync(batch);

		bulk.Clear();
	}
}

if (bulk.Count > 0)
{
	var batch = IndexDocumentsBatch.Upload(bulk);
	var result = await serviceClient.IndexDocumentsAsync(batch);
}

Console.ReadLine();
