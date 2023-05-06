using System;
namespace Application.Commmands
{
    public class CreateDirectMessageCommand : IRequest<ServiceResult<CreateDirectsMessageResultDto>>
    {
        public string FirstUserId { get; set; }
        public string SecundUserId { get; set; }
        public string Body { get; set; }
        public bool isReplay { get; set; }
        public string MessageId { get; set; }
    }
    public class CreateDirectMessageCommandHandler : BaseHandler, IRequestHandler<CreateDirectMessageCommand, ServiceResult<CreateDirectsMessageResultDto>>
    {
        private readonly IAES AES;
        private readonly IMediator _mediator;

        public CreateDirectMessageCommandHandler(
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork<DbContextChatMessage> unitOfWork,
            IAES AES, IMediator _madiator) : base(httpContextAccessor, unitOfWork)
        {
            this.AES = AES;
            _mediator = _madiator;
        }

        private static string CheckErrorItems(CreateDirectMessageCommand item)
        {
            var response = "";
            if (item.FirstUserId is null || item.Body is null || item.SecundUserId is null)
                response = "Error";
            if (item.isReplay is true && item.MessageId is null)
                response = "Error";
            return response;
        }

        private static TypeDirectMessageThreadEnum MessageState(string threadId, bool isReplay)
        {
            if (threadId is null)
                return TypeDirectMessageThreadEnum.CreateThread;
            else if (threadId != null && isReplay.Equals(false))
                return TypeDirectMessageThreadEnum.InsertMessageOnThread;
            else if (isReplay is true)
                return TypeDirectMessageThreadEnum.IsReplay;
            else
                return 0;
        }

        private static List<ParticipantEntity> MakeParticipant(
            string firstUserId, string secoundUserId, string threadId)
        {
            var res = new List<ParticipantEntity>();
            var fisrtMember = new ParticipantEntity(threadId, firstUserId);
            fisrtMember.MessageCount = 0;
            res.Add(fisrtMember);
            var member = new ParticipantEntity(threadId, secoundUserId);
            member.Role = ParticipantRoleEnum.Member;
            res.Add(member);
            return res;
        }

        private List<string> Possebels(string inputA, string inputB)
        {
            var res = new List<string>();
            res.Add($"{inputA}:{inputB}");
            res.Add($"{inputB}:{inputA}");
            return res;
        }

        public async Task<ServiceResult<CreateDirectsMessageResultDto>> Handle(
            CreateDirectMessageCommand item, CancellationToken cancellationToken)
        {
            var errorResult = CheckErrorItems(item);
            var res = new CreateDirectsMessageResultDto();
            if (errorResult.Length > 0)
                return ServiceResult.Empty.SetError("Invalid Data").To<CreateDirectsMessageResultDto>();

            var threadRepo = unitOfWork.GetRepository<ThreadEntity>();
            var messageRepo = unitOfWork.GetRepository<MessageEntity>();
            var participantRepo = unitOfWork.GetRepository<ParticipantEntity>();

            var threadId = "";

            var chanse = Possebels(item.FirstUserId, item.SecundUserId);
            var existThread = threadRepo.GetFirstOrDefault(predicate: x => chanse.Contains(x.Key) && !x.IsDeleted);


            if (existThread != null)
                threadId = existThread?.Id;

            if (existThread?.IsClosed is true)
                return ServiceResult.Empty.SetError("this thread is Closed !").To<CreateDirectsMessageResultDto>();

            var type = MessageState(existThread?.Id, item.isReplay);
            var message = item.Adapt<MessageEntity>();
            res.MesssageId = message.Id;
            var hashMessage = AES.Encrypt(message.Body);
            message.Body = hashMessage;
            switch (type)
            {
                case (0):
                    return ServiceResult.Empty.SetError("Invalid Data").To<CreateDirectsMessageResultDto>();
                case TypeDirectMessageThreadEnum.CreateThread:
                    {
                        var thread = message.Adapt<ThreadEntity>();
                        thread.Key = $"{item.FirstUserId}:{item.SecundUserId}";
                        var participants = MakeParticipant(item.FirstUserId, item.SecundUserId, thread.Id);

                        threadRepo.Insert(thread);
                        res.ThreadId = thread.Id;

                        if (item.FirstUserId.Equals(item.SecundUserId))
                            participantRepo.Insert(participants[0]);
                        else
                        {
                            participantRepo.Insert(participants);
                        }
                        message.setCreatedAt();
                        messageRepo.Insert(message);
                        break;
                    }
                case TypeDirectMessageThreadEnum.InsertMessageOnThread:
                    {
                        message.ThreadId = existThread.Id;

                        var updateThread = threadRepo.GetFirstOrDefault(
                            predicate: x => x.Id.Equals(threadId) && !x.IsDeleted);
                        updateThread.PluseMessageCount();
                        updateThread.SetLastMessage(message.Id, DateTime.Now);


                        ParticipantEntity participantActor = null;
                        ParticipantEntity participantEffects = null;
                        if (item.FirstUserId != item.SecundUserId)
                        {
                            participantActor = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.FirstUserId) && !x.IsDeleted);
                            participantActor.UpdateLastSeen();

                            participantEffects = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.SecundUserId) && !x.IsDeleted);
                            participantEffects.PulseMessageCount();
                            participantRepo.Update(participantEffects);
                        }
                        else
                        {
                            participantActor = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.FirstUserId) && !x.IsDeleted);
                            participantActor.UpdateLastSeen();
                            participantActor.PulseMessageCount();
                        }

                        threadRepo.Update(updateThread);
                        res.ThreadId = updateThread.Id;
                        participantRepo.Update(participantActor);
                        message.setCreatedAt();
                        messageRepo.Insert(message);
                        break;
                    }
                case TypeDirectMessageThreadEnum.IsReplay:
                    {
                        var mustSetReplay = messageRepo.GetFirstOrDefault(
                            predicate: x => x.Id.Equals(item.MessageId) && !x.IsDeleted);

                        if (mustSetReplay is null)
                            return ServiceResult.Empty.SetError("Invalid Data").To<CreateDirectsMessageResultDto>();

                        mustSetReplay.SetReplay();
                        var threadMassege = mustSetReplay?.ThreadId;
                        var threadFound = threadRepo.GetFirstOrDefault(
                            predicate: x => x.Id.Equals(threadMassege) && !x.IsDeleted);


                        ParticipantEntity participantActor = null;
                        ParticipantEntity participantEffects = null;
                        if (item.FirstUserId != item.SecundUserId)
                        {
                            participantActor = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.FirstUserId) && !x.IsDeleted);
                            participantActor.UpdateLastSeen();

                            participantEffects = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.SecundUserId) && !x.IsDeleted);
                            participantEffects.PulseMessageCount();
                            participantRepo.Update(participantEffects);
                        }
                        else
                        {
                            participantActor = participantRepo.GetFirstOrDefault(
                                predicate: x => x.ThreadId.Equals(threadId) &&
                                    x.UserId.Equals(item.FirstUserId) && !x.IsDeleted);
                            participantActor.UpdateLastSeen();
                            participantActor.PulseMessageCount();
                        }

                        participantRepo.Update(participantActor);
                        message.ThreadId = threadMassege;
                        message.ReplayMessageId = mustSetReplay.Id;
                        threadFound.SetLastMessage(message.Id, DateTime.Now);
                        threadFound.PluseMessageCount();
                        res.ThreadId = threadFound.Id;

                        threadRepo.Update(threadFound);
                        messageRepo.Update(mustSetReplay);
                        message.setCreatedAt();
                        messageRepo.Insert(message);
                        break;
                    }

            }
            unitOfWork.SaveChanges();
            var inp = new SendMessageEventCommand(message.Id, IUser.Id, EventTypeEnum.Insert);
            _mediator.Send<ServiceResult>(inp);
            return ServiceResult.Create<CreateDirectsMessageResultDto>(res);
        }
    }
}

