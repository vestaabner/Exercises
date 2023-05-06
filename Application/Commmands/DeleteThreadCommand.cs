using System;
using Application.Comman;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Application.Commmands
{
    public class DeleteThreadCommand : IRequest<ServiceResult>
    {
        public string ThreadId { get; set; }
        public string UserId { get; set; }

        public DeleteThreadCommand(string threadId, string userId)
        {
            ThreadId = threadId;
            UserId = userId;
        }
    }
    public class DeleteThreadCommandHandler : BaseHandler, IRequestHandler<DeleteThreadCommand, ServiceResult>
    {
        private readonly IMediator _mediator;

        public DeleteThreadCommandHandler(
           IHttpContextAccessor httpContextAccessor,
           IUnitOfWork<DbContextChatMessage> unitOfWork,
           IMediator _madiator
           ) : base(httpContextAccessor, unitOfWork)
        {
            this._mediator = _madiator;
        }

        public async Task<ServiceResult> Handle(DeleteThreadCommand request, CancellationToken cancellationToken)
        {
            if (request?.ThreadId is null || request?.UserId is null)
                return ServiceResult.Empty.SetError("Invalid data ");


            var repo = unitOfWork.GetRepository<ParticipantEntity>();

            var data = repo.GetFirstOrDefault(predicate: x => x.UserId.Equals(request.UserId) &&
            x.ThreadId.Equals(request.ThreadId));


            if (data is null)
                return ServiceResult.Empty.SetError("this thread is not exist");


            data.setDeleted();
            repo.Update(data);
            unitOfWork.SaveChanges();



            var inp = new SendMessageEventCommand(data.ThreadId, IUser.Id, EventTypeEnum.DeleteThread);
            _mediator.Send<ServiceResult>(inp);

            return ServiceResult.Create(data.Id);
        }

    }
}

