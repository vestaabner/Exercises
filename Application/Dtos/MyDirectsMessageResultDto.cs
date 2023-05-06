namespace Application.Dtos
{
    public class MyDirectsMessageResultDto
    {
        public string ThreadId { get; set; }
        public string TargetPerson { get; set; }
        public string LasMassageBody { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsClosed { get; set; }
        public int MessageCount { get; set; }


    }
}

