namespace Application.Dtos
{
    public class GetThreadMassagesInputDto
    {
        public string threadId { get; set; }
        public DateTime? Date { get; set; }
        public string userId { get; set; }
        public bool BackWard { get; set; }

    }
}

