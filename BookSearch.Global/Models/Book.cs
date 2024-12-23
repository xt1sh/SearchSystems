using System.ComponentModel.DataAnnotations;

namespace BookSearch.Global.Models
{
	public class Book
	{
		[Key]
		public string Id { get; set; }

		public string Title { get; set; }

		public string Authors { get; set; }

		public string Description { get; set; }

		public string Category { get; set; }

		public int Year { get; set; }

		public float[] TitleVector { get; set; } = [];

		public float[] DescriptionVector { get; set; } = [];
	}
}
