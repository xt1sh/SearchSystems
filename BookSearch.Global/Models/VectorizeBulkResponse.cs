namespace BookSearch.Global.Models
{
	public class VectorizeBulkResponse
	{
		public string InitialString { get; set; }

		public float[] Embeddings { get; set; }
	}
}
