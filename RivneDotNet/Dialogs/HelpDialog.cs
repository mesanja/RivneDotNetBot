using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RivneDotNet.QnA;

namespace RivneDotNet.Dialogs
{
    [Serializable]
    [QnAMakerService("", "")]
    public class HelpDialog : QnAMakerDialog<HelpAnswer>
    {
        private List<string> menuList;

        public HelpDialog()
        {
            menuList = new List<string>
            {
                "hi",
                "where",
                "when",
                "why",
                "speakers",
                "honeycombsoft",
                "help",
                "to other options"
            };
        }

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            if (message.Text.Contains("menu") || message.Text.Contains("help"))
            {
                MakeChoice(context);
            }
            else
            {
                await base.MessageReceived(context, item);
            }
        }

        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            MakeChoice(context);
        }

        private void MakeChoice(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, menuList, "You can ask Rivne .Net community bot about:");
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            await context.PostAsync($"I found an answer that might help...{result.Answer}.");
            context.Wait(MessageReceived);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            HelpAnswer answerResult = new HelpAnswer {Type = HelpDialogResultTypes.QnA};

            var question = await result;

            if (question.Contains("to other options"))
            {
                answerResult.Type = HelpDialogResultTypes.RootDialog;
            }
            else
            {
                answerResult.QnAnswer = QnAService.GetAnswer(question).Answer;
            }

            context.Done(answerResult);
        }
    }
}