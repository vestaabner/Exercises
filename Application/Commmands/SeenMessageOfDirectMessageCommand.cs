using System;
using Application.Comman;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commmands
{
    public class SeenMessageOfDirectMessageCommand : IRequest<ServiceResult>
    {
        public SeenMessageOfDirectMessageCommand(string threadId, string userId)
        {
            ThreadId = threadId;
            UserId = userId;
        }

        public string ThreadId { get; set; }
        public string UserId { get; set; }
    }
    public class SeenMessageOfDirectMessageHandler : BaseHandler,
     IRequestHandler<SeenMessageOfDirectMessageCommand, ServiceResult>
    {
        private readonly IActivityEventService _eventService;

        public SeenMessageOfDirectMessageHandler(IHttpContextAccessor httpContextAccessor,
         IUnitOfWork<DbContextChatMessage> unitOfWork, IActivityEventService eventService) :
         base(httpContextAccessor, unitOfWork)
        {
            _eventService = eventService;
        }

        private static string CheckErorItems(SeenMessageOfDirectMessageCommand item)
        {
            var response = "";
            if (item.ThreadId is null || item.UserId is null)
                response = "Error";
            return response;
        }
        public async Task<ServiceResult> Handle(SeenMessageOfDirectMessageCommand item, CancellationToken cancellationToken)
        {

            var errorResult = CheckErorItems(item);

            if (errorResult.Length > 0)
                return ServiceResult.Empty.SetError("Invalid Data");

            var messageIds = new List<string>();
            var threadRepo = unitOfWork.GetRepository<ThreadEntity>();
            var messageRepo = unitOfWork.GetRepository<MessageEntity>();
            var participantRepo = unitOfWork.GetRepository<ParticipantEntity>();

            var existThread = threadRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(item.ThreadId)
            && !x.IsDeleted);

            var foundsMassages = messageRepo.GetQueryale().Where(x => x.ThreadId.Equals(existThread.Id)
            && x.Seen.Equals(false) && !x.IsDeleted &&
            x.OwnerId != item.UserId).ToList();

            if (foundsMassages.Count != 0 && existThread != null)
            {
                messageIds = foundsMassages.Select(x => x.Id).ToList();
                var targetPerson = foundsMassages.First().OwnerId;

                foreach (var element in foundsMassages)
                    element.Saw();

                var participantElement = participantRepo.GetFirstOrDefault(predicate:
                 x => x.ThreadId.Equals(item.ThreadId)
               && x.UserId.Equals(item.UserId));

                participantElement.MessageCount = 0;
                participantElement.LastSeenAt = System.DateTime.Now;
                messageRepo.Update(foundsMassages);
                participantRepo.Update(participantElement);
                unitOfWork.SaveChanges();
                var seenEvent = new SeenEvent(messageIds, existThread.Id, targetPerson);
                _eventService.Send(seenEvent);

            }

            var res = foundsMassages.FirstOrDefault()?.Id;


            return ServiceResult.Create(res);
        }
    }
}

