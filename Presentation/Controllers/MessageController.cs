using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MediatR;
using Application.Comman;
using Application.Dtos;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MessageController : BaseController
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IMediator _mediator;

        public MessageController(
            ILogger<MessageController> logger,
            //IUnitOfWork<DbContextChatMessage> unitOfWork,
            IMediator _madiator)
        {
            //this.unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = _madiator;
        }



        [HttpPost]
        [ProducesResponseType(typeof(ServiceResult<CreateDirectsMessageResultDto>), 200)]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateDirectMessageDto input)
        {
            var command = input.Adapt<CreateDirectMessageCommand>();
            command.FirstUserId = IUser.Id;
            var result = await _mediator.Send<ServiceResult<CreateDirectsMessageResultDto>>(command);
            return await result?.AsyncResult();
        }



        [HttpDelete("{messageId}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [Authorize]
        public async Task<ActionResult> Delete([FromRoute] string messageId)
        {
            var command = new DeleteDirectMessageCommand(messageId, IUser.Id);
            var result = await _mediator.Send<ServiceResult>(command);
            return await result?.AsyncResult();
        }



        [HttpGet("{threadId}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [Authorize]
        public async Task<ActionResult> Seen([FromRoute] string threadId)
        {
            var command = new SeenMessageOfDirectMessageCommand(threadId, IUser.Id);
            var result = await _mediator.Send<ServiceResult>(command);
            return await result?.AsyncResult();
        }



        [HttpGet]
        [ProducesResponseType(typeof(ServiceResult<ServiceResult<IPagedList<MyDirectsMessageResultDto>>>), 200)]
        [Authorize]
        public async Task<ActionResult> GetMyChats([FromQuery] TermFilter tf)
        {
            var command = new MyDirectsMessageQuery(IUser.Id, tf);
            var result = await _mediator.Send<ServiceResult<IPagedList<MyDirectsMessageResultDto>>>(command);
            return await result?.AsyncResult();
        }



        [HttpPost]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [Authorize]
        public async Task<ActionResult> GetMassages([FromBody] GetThreadMassagesInputDto input, [FromQuery] TermFilter tf)
        {
            var command = new ThreadMessagesQuery(tf, input.threadId, input?.Date, IUser.Id, input.BackWard);
            var result = await _mediator.Send<ServiceResult<IPagedList<MessageResultDto>>>(command);
            return await result?.AsyncResult();
        }


        [HttpGet("{threadId}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [Authorize]
        public async Task<ActionResult> GetForFirstMessages(
            [FromRoute] string threadId, [FromQuery] TermFilter tf)
        {
            var command = new ThreadForFirstMessagesQuery(tf, threadId, IUser.Id);
            var result = await _mediator.Send<ServiceResult<ThreadForFirstMessagesResultDto>>(command);
            return await result?.AsyncResult();
        }



        [HttpGet("{replayId}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [Authorize]
        public async Task<ActionResult> GetReplay(
            [FromRoute] string replayId, [FromQuery] TermFilter tf)
        {
            var command = new GetMessagesByReplayQuery(tf, replayId, IUser.Id);
            var result = await _mediator.Send<ServiceResult<GetReplayMessageDto>>(command);
            return await result?.AsyncResult();
        }




        [HttpDelete("{threadId}")]
        [ProducesResponseType(typeof(ServiceResult<string>), 200)]
        [Authorize]
        public async Task<ActionResult> DeleteThread([FromRoute] string threadId)
        {
            var command = new DeleteThreadCommand(threadId, IUser.Id);
            var result = await _mediator.Send<ServiceResult>(command);
            return await result?.AsyncResult();
        }



        [HttpGet("{threadId}")]
        [ProducesResponseType(typeof(ServiceResult<ServiceResult<GetThreadInfoQueryResultDto>>), 200)]
        [Authorize]
        public async Task<ActionResult> GetThreadInfo([FromRoute] string threadId)
        {
            var command = new GetThreadInfoQuery(threadId);
            var result = await _mediator.Send<ServiceResult<GetThreadInfoQueryResultDto>>(command);
            return await result?.AsyncResult();
        }



        [HttpGet()]
        [ProducesResponseType(typeof(ServiceResult<ServiceResult<MyDirectsMessageResultDto>>), 200)]
        [Authorize]
        public async Task<ActionResult> GeMySaveMessage()
        {
            var command = new GeMySaveMessageQuery(IUser.Id);
            var result = await _mediator.Send<ServiceResult<MyDirectsMessageResultDto>>(command);
            return await result?.AsyncResult();
        }


        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ServiceResult<ServiceResult<string>>), 200)]
        [Authorize]
        public async Task<ActionResult> HasThreadWith([FromRoute] string userId)
        {
            var command = new HasThreadWithQuery(IUser.Id, userId);
            var result = await _mediator.Send<ServiceResult<string>>(command);
            return await result?.AsyncResult();
        }


        [HttpGet("{threadId}")]
        [ProducesResponseType(typeof(ServiceResult<ServiceResult<int>>), 200)]
        [Authorize]
        public async Task<ActionResult> ThreadUnSeenCount([FromRoute] string threadId)
        {
            var command = new ThreadUnSeenCountQuery(threadId);
            var result = await _mediator.Send<ServiceResult<int>>(command);
            return await result?.AsyncResult();
        }


    }
}

