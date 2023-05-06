using System;
using Application.Comman;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Application.Commmands
{
    public class DeleteDirectMessageCommand : IRequest<ServiceResult>
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }

        public DeleteDirectMessageCommand(string messageId, string userId)
        {
            MessageId = messageId;
            UserId = userId;
        }
    }
    public class DeleteDirectMessageCommandHandler : BaseHandler, IRequestHandler<DeleteDirectMessageCommand, ServiceResult>
    {
        private readonly IMediator _mediator;

        public DeleteDirectMessageCommandHandler(IHttpContextAccessor httpContextAccessor,
         IUnitOfWork<DbContextChatMessage> unitOfWork
        , IMediator _madiator) :
         base(httpContextAccessor, unitOfWork)
        {
            _mediator = _madiator;
        }

        public async Task<ServiceResult> Handle(DeleteDirectMessageCommand item,
         CancellationToken cancellationToken)
        {
            var stateEvent = EventTypeEnum.DeleteMessage;
            if (item.MessageId is null)
                return ServiceResult.Empty.SetError("Invalid Data");

            var directMessageRepo = unitOfWork.GetRepository<MessageEntity>();
            var threadRepo = unitOfWork.GetRepository<ThreadEntity>();
            var participandRepo = unitOfWork.GetRepository<ParticipantEntity>();

            var extMessage = directMessageRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(item.MessageId)
             && x.OwnerId.Equals(item.UserId));

            if (extMessage is null)
                return ServiceResult.Empty.SetError("Not Found");

            if (extMessage.IsDeleted is true)
                return ServiceResult.Empty.SetError("Already Deleted");


            var thread = threadRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(extMessage.ThreadId));
            if (extMessage.ReplayMessageId != null)
            {
                var justThisMaggageIsRelay = directMessageRepo.GetQueryale().AsNoTracking().Where(predicate: x => x.ReplayMessageId.Equals(extMessage.ReplayMessageId)
                && !x.IsDeleted).Count();
                if (justThisMaggageIsRelay.Equals(1))
                {
                    var unSetReplay = directMessageRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(extMessage.ReplayMessageId));
                    unSetReplay.unSetReplay();
                    directMessageRepo.Update(unSetReplay);

                }
            }

            extMessage.SetDeleted();
            if (thread.MessageCount > 1)
            {
                thread.MessageCount--;
                var participands = participandRepo.GetFirstOrDefault(predicate: x => x.ThreadId.Equals(thread.Id) && !x.IsDeleted && x.UserId != item.UserId)
                ;
                participands.MinesMessageCount();
                participandRepo.Update(participands);
                if (thread.LastMessageId.Equals(item.MessageId))
                {
                    var lastMessageOnThread = directMessageRepo.GetQueryale().AsNoTracking()
                                       .Where(x => !x.IsDeleted && x.Id != item.MessageId)
                                       .OrderByDescending(x => x.CreatedAt)
                                       .FirstOrDefault();

                    thread.SetLastMessage(lastMessageOnThread.Id, lastMessageOnThread.CreatedAt);
                }

            }
            else if (thread.MessageCount == 1)
            {
                thread.SetDeleted();

                var participands = participandRepo.GetQueryale()
                .Where(x => x.ThreadId.Equals(thread.Id) && !x.IsDeleted)
                .ToList();
                if (participands.Any())
                    participands.ForEach(x => x.setDeleted());

                stateEvent = EventTypeEnum.DeleteThread;

            }

            // if (thread.LastMessageId.Equals(item.MessageId) && thread.MessageCount > 1)
            // {

            // }


            threadRepo.Update(thread);
            /// unitOfWork.SaveChanges();
            directMessageRepo.Update(extMessage);
            unitOfWork.SaveChanges();

            if (stateEvent.Equals(EventTypeEnum.DeleteMessage))
            {
                var inp = new SendMessageEventCommand(extMessage.Id, IUser.Id, EventTypeEnum.DeleteMessage);
                _mediator.Send<ServiceResult>(inp);
            }
            else
            {
                var inp = new SendMessageEventCommand(thread.Id, IUser.Id, EventTypeEnum.DeleteThread);
                _mediator.Send<ServiceResult>(inp);
            }


            return ServiceResult.Create(extMessage.Id);
        }
    }
}

