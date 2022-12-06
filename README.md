# Macronizer API

This API wraps the [Alatius macronizer API service](https://github.com/Myrmex/alatius-macronizer-api) into a more robust, open infrastructure to be consumed by web clients.

## Usage

Apart from endpoints used for diagnostic purposes, the API exposes a single endpoint for the macronization service, `api/macronize`, for a POST request whose body corresponds to a JSON object having this model (\* marks required properties):

- `text` (string)\*: the text to macronize. Max 50,000 characters.
- `maius` (boolean): true to macronize capitalized words.
- `utov` (boolean): true to convert U to V.
- `itoj` (boolean): true to convert I to J.
- `ambiguous` (boolean): true to mark ambiguous results. In this case, the output will be HTML instead of plain text, with `span` elements wrapping each word, eventually with a `class` attribute equal to `ambig` or `unknown`. You can use the options below to convert it before returning the result.
- `normalizeWS` (boolean): true to normalize whitespace in text before macronization. This normalizes space/tab characters by replacing them with a single space, and trimming the text at both edges. It also normalizes CR+LF into LF only.
- `precomposeMN` (boolean): true to to apply Mn-category Unicode characters precomposition before macronization. This precomposes Unicode Mn-category characters with their letters wherever possible. Apply this filter when the input text has Mn-characters to avoid potential issues with macronization.
- `unmarkedEscapeOpen` (string): the optional opening escape to use for an unmarked form instead of the default `<span>`. If not specified, the default is preserved. If empty, the tag is removed.
- `unmarkedEscapeClose` (string): the optional closing escape to use for an unmarked form instead of the default `</span>`. If not specified, the default is preserved. If empty, the tag is removed.
- `ambiguousEscapeOpen` (string): the optional opening escape to use for an ambiguous form instead of the default `<span class="ambig">`. If not specified, the default is preserved. If empty, the tag is removed.
- `ambiguousEscapeOpen` (string): the optional closing escape to use for an ambiguous form instead of the default `</span>`. If not specified, the default is preserved. If empty, the tag is removed.
- `unknownEscapeOpen` (string): the optional opening escape to use for an unknown form instead of the default `<span class="unknown">`. If not specified, the default is preserved. If empty, the tag is removed.
- `unknownEscapeClose` (string): the optional closing escape to use for an unknown form instead of the default `</span>`. If not specified, the default is preserved. If empty, the tag is removed.

The result is a JSON object having this model:

```json
{

}
```

For instance, from this request body:

```json
{
  "ambiguous": true,
  "text": "tota Gallia divisa est"
}
```

you get this response content:

```json
{
  "result": "<span class=\"ambig\">t<span>ō</span>t<span>ā</span></span> <span class=\"ambig\">G<span>a</span>ll<span>i</span><span>ā</span></span> <span class=\"ambig\">d<span>ī</span>v<span>ī</span>s<span>a</span></span> <span class=\"ambig\"><span>e</span>st</span>",
  "error": null,
  "maius": false,
  "utov": false,
  "itoj": false,
  "ambiguous": true
}
```

Using the escape properties allows you to convert HTML output into something else, e.g. a compact plain text with some conventional characters reserved to annotate ambiguous or unknown words. For instance, setting these properties will append `¿` to ambiguous words, and `¡` to unknown words, stripping any HTML markup out:

```json
{
    "ambiguous": true,
    "unmarkedEscapeOpen": "",
    "unmarkedEscapeClose": "",
    "ambiguousEscapeOpen": "",
    "ambiguousEscapeClose": "¿",
    "unknownEscapeOpen": "",
    "unknownEscapeClose": "¡"
}
```

will convert a result like `<span>tota<span>`

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
