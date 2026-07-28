using BMW.Rheingold.CoreFramework.Interaction.Models;
using PsdzClient.Core;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Responses
{
    [DataContract]
    public class InteractionQuestionPopupResponse : InteractionResponse
    {
        [DataMember]
        public int Selection { get; private set; }

        [DataMember]
        public QuestionPopupDialogAnswer Answer { get; private set; }

        public InteractionQuestionPopupResponse(QuestionPopupDialogAnswer userSelection)
        {
            Selection = userSelection.ReturnValue;
            Answer = userSelection;
        }
    }
}
