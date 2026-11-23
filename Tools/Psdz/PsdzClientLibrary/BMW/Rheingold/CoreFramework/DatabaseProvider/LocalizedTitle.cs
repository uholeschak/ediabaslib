using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.DatabaseProvider.Helpers;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider;

public class LocalizedTitle : ILocalizedTitle
{
    public List<LocalizedTitleItem> Entries { get; set; }

    public string Title => GetLocalizedTitle(ConfigSettings.CurrentUICulture);

    public LocalizedTitle()
    {
        Entries = new List<LocalizedTitleItem>();
    }

    public string GetLocalizedTitle(string language)
    {
        language = NormalizeLanguage(language);
        if (!Entries.Any())
        {
            return "-";
        }
        LocalizedTitleItem localizedTitleItem = Entries.FirstOrDefault((LocalizedTitleItem e) => e.Language.Equals(language, StringComparison.InvariantCultureIgnoreCase));
        if (localizedTitleItem == null)
        {
            Log.Warning("LocalizedTitle.GetLocalizedTitle", "the given Language {0} is not available - language set to enGB", language);
            localizedTitleItem = Entries.First((LocalizedTitleItem e) => e.Language.Equals("en-GB", StringComparison.InvariantCultureIgnoreCase));
        }
        return localizedTitleItem.Value;
    }

    public void AddLocalizedTitle(string title, string language)
    {
        Entries.Add(new LocalizedTitleItem
        {
            Value = title,
            Language = language
        });
    }

    private string NormalizeLanguage(string language)
    {
        if (!language.Contains("-"))
        {
            return LanguageCodeHelper.GetShortLanguageCode(language);
        }
        return language;
    }
}
