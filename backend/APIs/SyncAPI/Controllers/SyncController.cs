using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.Model.Classes;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants.RabbitMQ;

namespace SyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly INotificationPublisherService _notificationPublisher;
        public SyncController(INotificationPublisherService notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }
        [HttpPost("PublishSyncMessages")]
        public async Task<ActionResult> PublishSyncMessages([FromBody] List<AppRequest> requests)
        {
            if (requests == null)
            {
                return BadRequest("request should not be null");
            }
            try
            {
                foreach (AppRequest request in requests)
                {
                    await _notificationPublisher.PublishToTopicExchange(request, GetQueueRouteName(request.LinkedItemType));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            return CreateOkApiResponse("Successfully published");
        }
        private string GetQueueRouteName(string moduleName)
        {
            string queueRoute = string.Empty;
            switch (moduleName)
            {
                case "sales":
                    queueRoute = QueueRoute.Sales_General;
                    break;
                case "master":
                    queueRoute = QueueRoute.Master_General;
                    break;
                case "merchandiser":
                    queueRoute = QueueRoute.Merchandiser_General;
                    break;
                case "collection_deposit":
                    queueRoute = QueueRoute.CollectionDeposit_General;
                    break;
                case "return":
                    queueRoute = QueueRoute.Return_General;
                    break;
                case "stock_request":
                    queueRoute = QueueRoute.StockRequest_General;
                    break;
                case "store_check":
                    queueRoute = QueueRoute.StoreCheck_General;
                    break;
                case "collection":
                    queueRoute = QueueRoute.Collection_General;
                    break;
                case "file_sys":
                    queueRoute = QueueRoute.FileSys_General;
                    break;
            }
            return queueRoute;
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
            if (isAnyFailed)
            {
                return StatusCode(207, publishResults);
            }
            else
            {
                return Ok(publishResults);
            }
        }
        protected ActionResult CreateOkApiResponse<T>(T data, DateTime? currentServerTime = null)
        {
            var apiResponse = new ApiResponse<T>(data: data, currentServerTime: currentServerTime);
            return Ok(apiResponse);
        }
    }
}
