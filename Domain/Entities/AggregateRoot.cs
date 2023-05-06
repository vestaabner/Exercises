using System;
namespace Domain.Entities
{
	public class AggregateRoot
	{
	
		public string Id { get; set; }
		public DateTime CreatedAt{
			get;set;
		}
		public bool IsDeleted{ get; set; }
	}
}
	

