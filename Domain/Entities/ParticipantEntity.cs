using System;
namespace Domain.Entities
{
    public class ParticipantEntity : AggregateRoot
    {

        public DateTime? LastSeenAt { get; set; }
        public string ThreadId { get; set; }
        public string UserId { get; set; }
        public ParticipantRoleEnum Role { get; set; }
        public int MessageCount { get; set; }

        public ParticipantEntity(
            string threadId,
            string userId,
            ParticipantRoleEnum role) : this(threadId, userId)
        {
            Role = role;
        }

        public ParticipantEntity(string threadId, string userId)
        {
            Id = Guid.NewGuid().ToString();
            ThreadId = threadId;
            UserId = userId;
            Role = ParticipantRoleEnum.Creator;
            MessageCount = 1;
            LastSeenAt = DateTime.Now;
        }

        public void setDeleted()
        {
            IsDeleted = true;
        }

        public void PulseMessageCount()
        {
            MessageCount += 1;
        }


        public void MinesMessageCount()
        {
            if (MessageCount != 0)
                MessageCount -= 1;
        }

        public void UpdateLastSeen()
        {
            LastSeenAt = DateTime.Now;
        }
    }
}

