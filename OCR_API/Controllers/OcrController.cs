using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IronOcr;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Newtonsoft.Json.Linq;

namespace OCR_API.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class OcrController : ControllerBase {
		private readonly IAmazonSecretsManager _secretsManager;

		public OcrController(IAmazonSecretsManager secretsManager) {
			_secretsManager = secretsManager ?? throw new NullReferenceException("secretsManager");
		}

		[HttpPost("upload")]
		public async Task<ActionResult> UploadAsync(IFormFile file) {
			try {
				if (file == null || file.Length == 0)
					return BadRequest("No file uploaded.");

				//var licenseKey = await GetSecret("YourIronOcrLicenseSecretName");
				//License.LicenseKey = licenseKey;
				
				using (var stream = file.OpenReadStream()) {
					IronTesseract ocr = new IronTesseract();
					ocr.Language = OcrLanguage.English;
					ocr.AddSecondaryLanguage(OcrLanguage.Hebrew);

					using (OcrInput input = new OcrInput(stream)) {

						OcrResult result = await ocr.ReadAsync(input);

						//Console.WriteLine(result.Text);

						return Ok(result.Paragraphs);
					}
				}
			} catch (Exception ex) {
				return StatusCode(StatusCodes.Status500InternalServerError, ex);
			}
		}

		private async Task<string> GetSecret(string secretName) {
			var request = new GetSecretValueRequest {
				SecretId = secretName
			};

			GetSecretValueResponse response = null;

			try {
				response = await _secretsManager.GetSecretValueAsync(request);
			} catch (Exception ex) {
				throw;
			}

			if (response.SecretString != null) {
				var secret = JObject.Parse(response.SecretString);
				return secret["IronOcrLicenseKey"].Value<string>();
			} else {
				throw new InvalidOperationException("Secret binary is not handled in this example.");
			}
		}
	}
}