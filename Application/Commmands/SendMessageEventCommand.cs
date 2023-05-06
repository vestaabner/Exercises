using System;
using Application.Comman;
using Application.Dtos;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Commmands
{
    public class SendMessageEventCommand : IRequest<ServiceResult>
    {
        public SendMessageEventCommand(string messageId, string userId, EventTypeEnum type)
        {
            MessageId = messageId;
            Type = type;
            UserId = userId;
        }
        public string UserId { get; set; }
        public string MessageId { get; set; }
        public EventTypeEnum Type { get; set; }
    }
    public class SendMessageEventCommandHandler : BaseHandler,
     IRequestHandler<SendMessageEventCommand, ServiceResult>
    {
        private readonly IActivityEventService _eventService;
        private readonly ICustomerService _customerService;
        private readonly IAES AES;
        private readonly ILogger<BlockContactEventHandler> logger;
        private readonly IServiceProvider serviceProvider;
        protected IUnitOfWork<DbContextChatMessage> unitOfWork;



        public SendMessageEventCommandHandler(IHttpContextAccessor httpContextAccessor, ICustomerService customerService,
         IUnitOfWork<DbContextChatMessage> unitOfWork, ILogger<BlockContactEventHandler> logger, IActivityEventService eventService, IAES aES,
         IServiceProvider serviceProvider) :
         base(httpContextAccessor, unitOfWork)
        {
            _eventService = eventService;
            _customerService = customerService;
            AES = aES;
            this.logger = logger;
            this.serviceProvider = serviceProvider;

        }

        public async Task<ServiceResult> Handle(SendMessageEventCommand item, CancellationToken cancellationToken)
        {
            using (var scop = serviceProvider.CreateScope())
            {
                unitOfWork = scop.ServiceProvider
                                     .GetService<IUnitOfWork<DbContextChatMessage>>();

                var istanceCreateMessage = new MessageResultDto();

                var threadRepo = unitOfWork.GetRepository<ThreadEntity>();
                var participantRepo = unitOfWork.GetRepository<ParticipantEntity>();
                var messageRepo = unitOfWork.GetRepository<MessageEntity>();

                if (item.Type.Equals(EventTypeEnum.DeleteThread))
                {
                    logger.LogInformation("DeleteThread was stated ");

                    var thread = threadRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(item.MessageId));
                    var chatSignalR = thread.Adapt<MyDirectsMessageWhiteCustomerInfoResultDto>();


                    var participantEffect = participantRepo.GetQueryale()
                    .Where(x => x.ThreadId.Equals(thread.Id) &&
                        x.UserId != item.UserId).FirstOrDefault();



                    var userIds = new List<string>();
                    userIds.Add(participantEffect.UserId);
                    var customerInfo = await _customerService.GetUsersInfo(userIds);
                    var cusInfo = customerInfo?.Result?.Items.FirstOrDefault();

                    var chatEvent = new ChatDeletedEvent(chatSignalR);

                    if (!(customerInfo.HasError is true || cusInfo is null))
                        chatSignalR.SetCustomerInfo(cusInfo?.UniqeName, cusInfo?.FirstName, cusInfo.HasPhoto,
                         cusInfo.HasWallpaper, participantEffect.UserId, "", cusInfo.Id, 0, item.MessageId);

                    logger.LogInformation("ChatDeletedEvent was published ");
                    _eventService.Send(chatEvent);
                }
                else
                {

                    var message = messageRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(item.MessageId));
                    MessageEntity lastMessageOnThread = null;
                    RepalyDto reply = null;


                    if (message.IsDeleted is true)
                    {
                        var messageId = threadRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(message.ThreadId))?.LastMessageId;
                        lastMessageOnThread = messageRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(messageId) && !x.IsDeleted);
                    }

                    if (message.ReplayMessageId != null)
                    {
                        var repalyMessage = messageRepo.
                        GetFirstOrDefault(predicate: x => x.Id.Equals(message.ReplayMessageId)
                        );


                        if (repalyMessage != null)
                        {
                            reply = repalyMessage.Adapt<RepalyDto>();
                            reply.Body = reply.IsDeleted is true ? null : AES.Decrypt(repalyMessage.Body);
                        }


                    }

                    var signalRMessage = message.Adapt<CreaetMessageSignalRDto>();
                    signalRMessage.Replay = reply;

                    var body = message.IsDeleted ? AES.Decrypt(lastMessageOnThread.Body) : AES.Decrypt(message.Body);


                    /// logger.LogInformation($"{item.UserId} is send the message !");

                    var participantEffect = participantRepo.GetQueryale()
                    .Where(x => x.ThreadId.Equals(message.ThreadId) &&
                     x.UserId != item.UserId
                      && !x.IsDeleted).FirstOrDefault();

                    ///     logger.LogInformation($"{participantEffect} recived the  message !");

                    signalRMessage.SetMessageSignalRّItem(participantEffect.UserId, message.ThreadId, body);
                    signalRMessage.TragetPesronId = participantEffect.UserId;


                    if (item.Type.Equals(EventTypeEnum.Insert))
                    {
                        var messageEvent = new MessageCreatedEvent(signalRMessage);
                        _eventService.Send(messageEvent);



                        var targetUserIds = new List<string>();
                        targetUserIds.Add(item.UserId);



                        var thread = threadRepo.GetFirstOrDefault(predicate: x => x.Id.Equals(message.ThreadId));
                        var chatSignalR = thread.Adapt<MyDirectsMessageWhiteCustomerInfoResultDto>();
                        var customerInfo = await _customerService.GetUsersInfo(targetUserIds);
                        var cusInfo = customerInfo?.Result?.Items.FirstOrDefault();

                        var chatEvent = new ChatCreatedEvent(chatSignalR);


                        var messageCount = messageRepo.GetQueryale()
                        .Where(x => x.ThreadId.Equals(thread.Id) && !x.Seen && !x.IsDeleted)
                        .ToList()
                        .Count();


                        if (!(customerInfo.HasError is true || cusInfo is null))
                            chatSignalR.SetCustomerInfo(cusInfo?.UniqeName, cusInfo?.FirstName, cusInfo.HasPhoto,
                             cusInfo.HasWallpaper, participantEffect.UserId, body, cusInfo.Id, messageCount, item.MessageId);

                        _eventService.Send(chatEvent);

                    }
                    else if (item.Type.Equals(EventTypeEnum.DeleteMessage))
                    {
                        logger.LogInformation("MessageDeletedEvent was published ");
                        var messageEvent = new MessageDeletedEvent(signalRMessage);
                        _eventService.Send(messageEvent);
                    }

                }


                return ServiceResult.Empty;
            }
        }
    }
}

