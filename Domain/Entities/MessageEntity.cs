using System;
namespace Domain.Entities
{
    public class MessageEntity : AggregateRoot
    {
        public string ThreadId { get; set; }
        public string Body { get; set; }
        public string OwnerId { get; set; }
        public bool HasReplay { get; set; }
        public bool Seen { get; set; }
        public string ReplayMessageId { get; set; }

        public MessageEntity(string threadId, string body, string owner, bool hasReplay)
        {
            Id = Guid.NewGuid().ToString();
            ThreadId = threadId;
            Body = body;
            OwnerId = owner;
            HasReplay = hasReplay;
            CreatedAt = DateTime.Now;
        }
        public void setCreatedAt()
        {
            CreatedAt = DateTime.Now;
        }

        public MessageEntity()
        {
        }
        public void Saw()
        {
            Seen = true;
        }
        public void SetDeleted()
        {
            IsDeleted = true;
        }
        public void SetReplay()
        {
            HasReplay = true;
        }
        public void unSetReplay()
        {
            HasReplay = false;
        }
    }
}

