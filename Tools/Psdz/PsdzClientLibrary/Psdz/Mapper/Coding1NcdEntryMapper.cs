using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal static class Coding1NcdEntryMapper
    {
        public static IPsdzCoding1NcdEntry Map(Coding1NcdEntryModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzCoding1NcdEntry
            {
                BlockAdress = model.BlockAddress,
                UserData = model.UserData,
                IsWriteable = model.Writeable
            };
        }

        public static Coding1NcdEntryModel Map(IPsdzCoding1NcdEntry ncdEntry)
        {
            if (ncdEntry == null)
            {
                return null;
            }

            return new Coding1NcdEntryModel
            {
                BlockAddress = ncdEntry.BlockAdress,
                UserData = ncdEntry.UserData,
                Writeable = ncdEntry.IsWriteable
            };
        }
    }
}