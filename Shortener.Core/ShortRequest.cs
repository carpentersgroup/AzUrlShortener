using Shortener.Azure.Pocos;
using System.ComponentModel.DataAnnotations;

namespace Shortener.Core
{
    public class ShortRequest
    {
        /// <summary>
        /// [Optional] the end of the URL. If nothing one will be generated for you.
        /// </summary>
        public string? Vanity { get; set; }

        /// <summary>
        /// [Required] The url you wish to have a short version for
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Uri Url { get; set; } = null!;

        /// <summary>
        ///  [Optional] Title of the page, or text description of your choice.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// [Optional] A schedule for the url to be active. If nothing the url will be active right away.
        /// </summary>
        public Schedule[]? Schedules { get; set; }

        /// <summary>
        /// [Optional] The custom domain that will be able to redirect to this url. If nothing the default custom domain will be used or the azure function url if no custom domain is used.
        /// </summary>
        public Uri? Host { get; set; }
    }
}