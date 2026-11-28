using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities.AdditionalModels
{
    /// <summary>
    /// Server-side guard for user-supplied external links: the value must be an absolute URL
    /// whose scheme is http or https. This blocks unsafe schemes such as javascript:, data: or
    /// file: that the built-in <c>[Url]</c> attribute would otherwise let through. A null/empty
    /// value is treated as valid so it can be paired with <c>[Required]</c> when the field is
    /// mandatory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AbsoluteHttpUrlAttribute : ValidationAttribute
    {
        public AbsoluteHttpUrlAttribute()
            : base("Enter a valid URL that starts with http:// or https://.")
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
