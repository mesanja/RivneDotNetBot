using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RivneDotNet.Dialogs;
using RivneDotNet.QnA;

namespace RivneDotNet.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<object> MakeRoot()
        {
            return Chain.From(() => new RivneNetQnADialog());
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                if (message.MembersAdded != null && message.MembersAdded.Any())
                {
                    string membersAdded = string.Join(
                        ", ",
                        message.MembersAdded.Select(
                            newMember =>
                                (newMember.Id != message.Recipient.Id)
                                    ? $"{newMember.Name} (Id: {newMember.Id})"
                                    : $"{message.Recipient.Name} (Id: {message.Recipient.Id})"));

                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    Activity reply = message.CreateReply($"Welcome {membersAdded}");
                    connector.Conversations.ReplyToActivityAsync(reply);
                }

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                if (message.Action == "add")
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    Activity reply = message.CreateReply("I am Rivne .Net community bot. Please visit our official page https://www.facebook.com/net.community.rv/" +
                                                         Environment.NewLine +
                                                         "Type register or help to start.");
                    connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}