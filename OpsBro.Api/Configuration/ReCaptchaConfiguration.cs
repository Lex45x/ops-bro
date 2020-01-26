namespace OpsBro.Api.Configuration
{
    public class ReCaptchaConfiguration : AbstractConfiguration, IReCaptchaConfiguration
    {
        public ReCaptchaConfiguration(IConfigurationRoot root)
            : base(root)
        {
        }

        public string Key { get; set; }
        public string Secret { get; set; }
        public string VerificationUrl { get; set; }
    }
}