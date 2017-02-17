using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Builder.FormFlow;

namespace RivneDotNet.Users
{
    [Serializable]
    public enum Gender
    {
        Male = 1,
        Female = 2,
        WhoKnows = 3
    }

    [Serializable]
    public class UserForm
    {
        [Prompt("What is your full name")]
        public string FullName { get; set; }

        [Prompt("Your email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Prompt("Your gender {||}")]
        public Gender Gender;

        public static IForm<UserForm> BuildForm()
        {
            // Builds an IForm<T> based on UserForm
            return new FormBuilder<UserForm>().Build();
        }

        public static IFormDialog<UserForm> BuildFormDialog(FormOptions options = FormOptions.PromptInStart)
        {
            // Generated a new FormDialog<T> based on IForm<BasicForm>
            return FormDialog.FromForm(BuildForm, options);
        }
    }
}