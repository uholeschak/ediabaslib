using PsdzClient.Core;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Interaction.Models;

namespace BMW.Rheingold.CoreFramework.Interaction.Responses
{
    [DataContract]
    public class InteractionQuestionAdvancedPopupResponse : InteractionResponse
    {
        [DataMember]
        public int Selection { get; private set; }

        [DataMember]
        public QuestionPopupAdvancedDialogAnswer Answer { get; private set; }

        public InteractionQuestionAdvancedPopupResponse(QuestionPopupAdvancedDialogAnswer userSelection)
        {
            Selection = userSelection.ReturnValue;
            Answer = userSelection;
        }
    }
}
