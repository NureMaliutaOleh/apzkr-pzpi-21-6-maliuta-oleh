namespace SmartInlet.Server.Services.Email
{
	/// <summary>
	/// Represents a service to send plain text and HTML template emails.
	/// </summary>
	public interface IEmailService
	{
        /// <summary>
		/// Sends an email with the specified raw plain text or HTML code.
		/// </summary>
		/// <param name="email">Receiver.</param>
		/// <param name="subject">Email subject.</param>
		/// <param name="message">Email content.</param>
		/// <param name="isHtml">Type of the content (false - plain text, true - HTML code).</param>
		/// <returns>Task object.</returns>
        public Task SendEmailAsync(
			string email,
			string subject,
			string message,
			bool isHtml = false);

        /// <summary>
        /// Sends an email with the specified HTML template from the folder with the HTML templates.
		/// The special marked places will be replaced with the specified parameters.
        /// </summary>
        /// <param name="email">Receiver.</param>
        /// <param name="templateName">Name of the HTML file from the folder.</param>
        /// <param name="parameters">Parameters to replace the marked places in the HTML code.</param>
        /// <param name="subject">Email subject.</param>
        /// <returns>Task object.</returns>
        public Task SendEmailUseTemplateAsync(
			string email,
			string templateName,
			Dictionary<string, string>? parameters = null,
			string? subject = null);
	}
}
