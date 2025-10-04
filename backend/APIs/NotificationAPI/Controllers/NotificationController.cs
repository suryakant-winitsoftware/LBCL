using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.Model.Classes;
using Winit.Modules.Notification.Model.Interfaces;

namespace NotificationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationPublisherService _notificationPublisher;
        public NotificationController(INotificationPublisherService notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }
        [HttpPost("PublishMessagesByRoutingKey")]
        public async Task<IActionResult> PublishMessagesByRoutingKey([FromBody] List<NotificationRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest(new List<NotificationPublishResult>
                { 
                    new NotificationPublishResult
                    {
                        LinkedItemType = null,
                        LinkedItemUID = null,
                        Status = "Failed",
                        ErrorMessage = "At least one notification request is required."
                    }
                });
            }

            // Validate requests, exiting early on the first error
            foreach (var request in requests)
            {
                if (string.IsNullOrEmpty(request.TemplateName))
                {
                    return BadRequest(new List<NotificationPublishResult>
                    {
                        new NotificationPublishResult
                        {
                            LinkedItemType = request.LinkedItemType,
                            LinkedItemUID = request.LinkedItemUID,
                            Status = "Failed",
                            ErrorMessage = "TemplateName is required."
                        }
                    });
                }

                if (string.IsNullOrEmpty(request.NotificationRoute))
                {
                    return BadRequest(new List<NotificationPublishResult>
                    {
                        new NotificationPublishResult
                        {
                            LinkedItemType = request.LinkedItemType,
                            LinkedItemUID = request.LinkedItemUID,
                            Status = "Failed",
                            ErrorMessage = "NotificationRoute is required."
                        }
                    });
                }

                if (!string.IsNullOrEmpty(request.LinkedItemType) && string.IsNullOrEmpty(request.LinkedItemUID))
                {
                    return BadRequest(new List<NotificationPublishResult>
                    {
                        new NotificationPublishResult
                        {
                            LinkedItemType = request.LinkedItemType,
                            LinkedItemUID = request.LinkedItemUID,
                            Status = "Failed",
                            ErrorMessage = "LinkedItemUID is required when LinkedItemType is given."
                        }
                    });
                }

                if (string.IsNullOrEmpty(request.LinkedItemType) && (request.Receiver == null || request.Receiver.Count == 0))
                {
                    return BadRequest(new List<NotificationPublishResult>
                    {
                        new NotificationPublishResult
                        {
                            LinkedItemType = request.LinkedItemType,
                            LinkedItemUID = request.LinkedItemUID,
                            Status = "Failed",
                            ErrorMessage = "Receiver is required when LinkedItemType is not given."
                        }
                    });
                }
            }

            // All requests are valid; proceed to publishing
            var publishResults = new List<NotificationPublishResult>();
            bool isAnyFailed = false;
            foreach (var request in requests)
            {
                try
                {
                    await _notificationPublisher.PublishToTopicExchange((INotificationRequest)request, request.NotificationRoute);

                    // Record success
                    publishResults.Add(new NotificationPublishResult
                    {
                        LinkedItemUID = request.LinkedItemUID,
                        Status = "Success",
                        ErrorMessage = null
                    });
                }
                catch (Exception ex)
                {
                    isAnyFailed = true;
                    // Record failure with error details
                    publishResults.Add(new NotificationPublishResult
                    {
                        LinkedItemUID = request.LinkedItemUID,
                        Status = "Failed",
                        ErrorMessage = ex.Message
                    });
                }
            }
            if(isAnyFailed)
            {
                return StatusCode(207, publishResults);
            }
            else
            {
                return Ok(publishResults);
            }
        }


    }
}
