# Macronizer API

This API wraps the [Alatius macronizer API service](https://github.com/Myrmex/alatius-macronizer-api) into a more robust, open infrastructure to be consumed by web clients.

## Settings

All these settings can be overridden, usually via environment variables in the Docker compose script.

### Auditing

- `Diagnostics/IsTestEnabled`: enable or disable the API test functions.
- `Mailer`:
  - `IsEnabled`: true to enable mailing.
  - `SenderEmail`: the address to use as the email sender address.
  - `SenderName`: the name to use as the email sender address.
  - `Host`: the URI of the SMTP server.
  - `Port`: the SMTP server's port.
  - `UseSsl`: true to use SSL when connecting to the SMTP server.
  - `UserName`: the SMTP user name.
  - `Password`: the SMTP user password.
  - `Recipients`: the recipient(s) of notification email messages. This is an array of strings.
  - `TestRecipient`: the email address to use as the recipient for a test email message.
- `Messaging`:
  - `AppName`: the name of the app written in a text message.
  - `ApiRootUrl`: the root URL of the app eventually written in a text message.
  - `AppRootUrl`: the root URL of the app frontend eventually written in a text message.
  - `SupportEmail`: the email address for contacting support eventually written in a text message.
- `Serilog/RollingInterval`: the rolling interval for the API log file. Valid values are: `Infinite`, `Year`, `Month`, `Day` (default), `Hour`, `Minute`.

### Network

- `AlatiusMacronizerUri`: the URI of the macronizer service. Default is `http://localhost:51234/` as this is inside the Docker compose stack network.
- `RateLimit`:
  - `IsDisabled`: true to disable rate limiting.
  - `PermitLimit`: the maximum number of requests per time window.
  - `QueueLimit`: the queue limit.
  - `Window`: the time window, usually with format `HH:MM:SS`. Any `TimeSpan`-parsable string can be used.
