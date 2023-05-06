namespace Application.Dtos
{
    public class MessageResultDto
    {
        private string _month;
        const string passphrase = "HackBehkhaan";
        public string OwnerId { get; set; }
        public bool HasReplay { get; set; }
        public bool Seen { get; set; }
        public string ReplayMessageId { get; set; }
        public string Id { get; set; }
        public DateTime CreateAt { get; set; }
        // public string Body { get; set; }
        public string Body
        {
            get => _month;
            set
            {
                _month = Application.Comman.AES.Decrypt(value, passphrase);
            }
        }
    }
}

