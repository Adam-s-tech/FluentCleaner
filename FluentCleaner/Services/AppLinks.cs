namespace FluentCleaner.Services;

// All my external URLs in one place
public static class AppLinks
{
    public const string GitHub        = "https://github.com/builtbybel/FluentCleaner";
    public const string Issues        = "https://github.com/builtbybel/FluentCleaner/issues";
    public const string Releases      = "https://github.com/builtbybel/FluentCleaner/releases";
    public const string Donate        = "https://www.paypal.com/donate/?hosted_button_id=99X8UQJQP96WN";
    public const string KoFi          = "https://ko-fi.com/builtbybel";
    public const string Faq           = "https://github.com/builtbybel/FluentCleaner/blob/main/README.md#faq";
    public const string IconCredit    = "https://github.com/naderi";
    public const string VersionCheck  = "https://raw.githubusercontent.com/builtbybel/FluentCleaner/main/version.txt";

    public static string CreatePayPalDonationUrl(int amount, string currency)
    {
        var item = Uri.EscapeDataString("Support development of FluentCleaner");
        var business = Uri.EscapeDataString("belim@builtbybel.com");
        return $"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business={business}" +
               $"&amount={amount}&currency_code={Uri.EscapeDataString(currency)}&item_name={item}";
    }

    // Custom cleaner community & sharing
    public const string ShareCleaner  = "https://github.com/builtbybel/FluentCleaner/issues";       // share / request inclusion
    public const string Winapp2Repo   = "https://github.com/MoscaDotTo/Winapp2";                    // submit to official winapp2.ini
    public const string Reddit        = "https://www.reddit.com/r/windows/";
    public const string Neowin        = "https://www.neowin.net/forum/";
    public const string Deskmodder    = "https://www.deskmodder.de/";

    public static async Task OpenAsync(string url)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }
}
