using System;
using Domain.Entities;
using System;
using Mapster;
using Application.Dtos;

namespace Application.MapsterConfiguration
{
    public class ApplicationDtosConfigurations
    {
        static ApplicationDtosConfigurations _instance;

        public static ApplicationDtosConfigurations Instance => _instance ?? (_instance = new ApplicationDtosConfigurations());
        public void Initialize()
        {
            ConfigFor_CreateMessageCommand_CreateMessageCommand();
            ConfigFor_DirectMessageEntity_ThreadEntity();
            ConfigFor_CreateDirectMessageDto_CreateDirectMessageCommand();
            ConfigFor_MessageEntity_MessageResultDto();
            ConfigFor_BlockContactEvent_ThreadEntity();
            ConfigFor_ThreadEntity_MyDirectsMessageResultDto();
            ConfigFor_ThreadEntity_MyDirectsMessageWhiteCustomerInfoResultDto();
            ConfigFor_MessageEntity_RepalyDto();
        }



        private ApplicationDtosConfigurations ConfigFor_CreateMessageCommand_CreateMessageCommand()
        {
            TypeAdapterConfig<CreateDirectMessageCommand, MessageEntity>
                .NewConfig()
                .Map(x => x.Id, x => IDentifiable.New)
                .Map(x => x.ThreadId, x => IDentifiable.New)
                .Map(x => x.OwnerId, x => x.FirstUserId)
                .Map(x => x.HasReplay, x => false)
                .Map(x => x.CreatedAt, x => DateTime.Now)
                .Map(x => x.Body, x => x.Body);
            return this;
        }


        private ApplicationDtosConfigurations ConfigFor_MessageEntity_RepalyDto()
        {
            TypeAdapterConfig<MessageEntity, RepalyDto>
                .NewConfig()
                .Map(x => x.Id, x => x.Id)
                .Map(x => x.Body, x => x.Body)
                .Map(x => x.IsDeleted, x => x.IsDeleted)
                .Map(x => x.OwnerId, x => x.OwnerId)
                .Map(x => x.CreatedAt, x => x.CreatedAt);
            return this;
        }


        private ApplicationDtosConfigurations ConfigFor_DirectMessageEntity_ThreadEntity()
        {
            TypeAdapterConfig<MessageEntity, ThreadEntity>
                .NewConfig()
                .Map(x => x.Id, x => x.ThreadId)
                .Map(x => x.LastMessageId, x => x.Id)
                .Map(x => x.LastMessageCreateAt, x => DateTime.Now)
                .Map(x => x.IsClosed, x => false)
                .Map(x => x.FirstMessageId, x => x.Id)
                .Map(x => x.MessageCount, x => 1);
            return this;
        }


        private ApplicationDtosConfigurations ConfigFor_ThreadEntity_MyDirectsMessageResultDto()
        {
            TypeAdapterConfig<ThreadEntity, MyDirectsMessageResultDto>
                .NewConfig()
                .Map(x => x.ThreadId, x => x.Id)
                .Map(x => x.LastMessageAt, x => x.LastMessageCreateAt)
                .Map(x => x.IsClosed, x => x.IsClosed)
                .Map(x => x.MessageCount, x => x.MessageCount)
                .Ignore(x => x.LastMessageBody)
                .Ignore(x => x.TargetPerson);
            return this;
        }



        private ApplicationDtosConfigurations ConfigFor_ThreadEntity_MyDirectsMessageWhiteCustomerInfoResultDto()
        {

            TypeAdapterConfig<ThreadEntity, MyDirectsMessageWhiteCustomerInfoResultDto>
        .NewConfig()
        .Map(x => x.ThreadId, x => x.Id)
        .Map(x => x.LastMessageAt, x => x.LastMessageCreateAt)
        .Map(x => x.IsClosed, x => x.IsClosed)
        .Map(x => x.MessageCount, x => x.MessageCount)
        .Ignore(x => x.LastMessageBody)
        .Ignore(x => x.TargetPerson);
            return this;
        }

        private ApplicationDtosConfigurations ConfigFor_CreateDirectMessageDto_CreateDirectMessageCommand()
        {
            TypeAdapterConfig<CreateDirectMessageDto, CreateDirectMessageCommand>
                .NewConfig()
                .Map(x => x.SecundUserId, x => x.SecoundUserId)
                .Map(x => x.Body, x => x.Body)
                .Map(x => x.isReplay, x => x.IsReplay);
            return this;
        }


        private ApplicationDtosConfigurations ConfigFor_MessageEntity_MessageResultDto()
        {
            TypeAdapterConfig<MessageEntity, MessageResultDto>
                .NewConfig()
                .Map(x => x.Body, x => x.Body)
                .Map(x => x.OwnerId, x => x.OwnerId)
                .Map(x => x.HasReplay, x => x.HasReplay)
                .Map(x => x.Seen, x => x.Seen)
                .Map(x => x.Id, x => x.Id)
                .Map(x => x.CreateAt, x => x.CreatedAt)
                .Ignore(x => x.Replay)
                ;
            return this;
        }


        private ApplicationDtosConfigurations ConfigFor_BlockContactEvent_ThreadEntity()
        {
            TypeAdapterConfig<BlockContactEvent, ThreadEntity>
                .NewConfig()
                .ConstructUsing(x => new ThreadEntity(IDentifiable.New, 0, null, null, null));
            return this;
        }

    }

}

