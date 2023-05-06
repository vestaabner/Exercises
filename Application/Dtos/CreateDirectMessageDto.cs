using System;
namespace Application.Dtos
{
    public class CreateDirectMessageDto
    {
        public string FirstUserId { get; set; }
        public string SecoundUserId { get; set; }
        public string Body { get; set; }
        public bool isReplay { get; set; }
        public string MessageId { get; set; }
    }
    public class GetThreadMassagesInputDto
    {
        public string threadId { get; set; }
        public DateTime? Date { get; set; }
        public string userId { get; set; }
        public bool BackWard { get; set; }

    }
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

