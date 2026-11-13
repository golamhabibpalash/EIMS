using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem
{
    /// <summary>
    /// Page title and institute branding for the current request. These used to be plain static
    /// fields shared by the whole process, so concurrent requests overwrote each other's titles and,
    /// with several schools in one process, one school's name/logo could show on another's page.
    /// The ~200 existing call sites keep their syntax; the values now live in HttpContext.Items,
    /// i.e. per request. Outside a request (startup, tests) they fall back to a process-wide slot.
    /// </summary>
    public static class GlobalUI
    {
        private static IHttpContextAccessor _accessor;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> Fallback = new();

        /// <summary>Called once from Program.cs.</summary>
        public static void Configure(IHttpContextAccessor accessor) => _accessor = accessor;

        public static string PageTitle { get => Get(nameof(PageTitle)); set => Set(nameof(PageTitle), value); }
        public static string InstituteName { get => Get(nameof(InstituteName)); set => Set(nameof(InstituteName), value); }
        public static string SiteTitle { get => Get(nameof(SiteTitle)); set => Set(nameof(SiteTitle), value); }
        public static string Favicon { get => Get(nameof(Favicon)); set => Set(nameof(Favicon), value); }
        public static string Logo { get => Get(nameof(Logo)); set => Set(nameof(Logo), value); }
        public static string Language { get => Get(nameof(Language)); set => Set(nameof(Language), value); }

        private static string Get(string name)
        {
            var items = _accessor?.HttpContext?.Items;
            if (items != null)
            {
                return items.TryGetValue("GlobalUI." + name, out var value) ? value as string : null;
            }
            return Fallback.TryGetValue(name, out var fallback) ? fallback : null;
        }

        private static void Set(string name, string value)
        {
            var items = _accessor?.HttpContext?.Items;
            if (items != null)
            {
                items["GlobalUI." + name] = value;
                return;
            }
            Fallback[name] = value;
        }
    }
}
