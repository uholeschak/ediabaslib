using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz
{
    internal static class LocalizableMessageToMapper
    {
        public static ILocalizableMessageTo Map(LocalizableMessageToModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzLocalizableMessageTo
            {
                Description = model.Description,
                MessageId = model.MessageId
            };
        }

        public static LocalizableMessageToModel Map(ILocalizableMessageTo localizableMessageTo)
        {
            if (localizableMessageTo == null)
            {
                return null;
            }

            return new LocalizableMessageToModel
            {
                Description = localizableMessageTo.Description,
                MessageId = localizableMessageTo.MessageId
            };
        }

        public static LocalizableMessageToModel Map(string description)
        {
            return new LocalizableMessageToModel
            {
                Description = description
            };
        }
    }
}