namespace TemplateWorkload.Core
{
    public static class StorageConfig
    {
        private static string GetEnvVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) 
                ?? throw new ArgumentNullException($"Environment variable '{variableName}' is not set");
        }

        public static class TableNames
        {
            public static readonly string ProviderProfile = GetEnvVariable("PROVIDER_PROFILE_TABLE");
            public static readonly string Users = GetEnvVariable("USERS_TABLE");
            public static readonly string ProviderUsers = GetEnvVariable("PROVIDER_USERS_TABLE");
            public static readonly string DataShareOffers = GetEnvVariable("DATA_SHARE_OFFERS_TABLE");
            public static readonly string OfferAttachments = GetEnvVariable("OFFER_ATTACHMENTS_TABLE");
            public static readonly string OffersHasReportIndex = GetEnvVariable("OFFER_HAS_REPORT_INDEX_TABLE");
            public static readonly string OffersProviderIndex = GetEnvVariable("OFFER_PROVIDER_INDEX_TABLE");
            public static readonly string PrivateDataSharesIndex = GetEnvVariable("PRIVATE_DATA_SHARES_INDEX_TABLE");
            public static readonly string ProviderToOfferLinkIndex = GetEnvVariable("PROVIDER_TO_OFFER_LINK_INDEX_TABLE");
            public static readonly string AcceptedDataShares = GetEnvVariable("ACCEPTED_DATA_SHARES_TABLE");
            public static readonly string AppRegistrations = GetEnvVariable("APP_REGISTRATIONS_TABLE");
        }

        public static class BlobContainers
        {
            public static readonly string Logos = GetEnvVariable("PROVIDER_LOGO_CONTAINER");
            public static readonly string Attachments = GetEnvVariable("OFFER_ATTACHMENT_CONTAINER");
        }

        public static class StorageAccount
        {
            public static readonly string AccountName = GetEnvVariable("STORAGE_ACCOUNT_NAME");
            // This should be dev only, production should use managed identity
            public static readonly string ConnectionString = GetEnvVariable("STORAGE_CONNECTION_STRING");
        }
    }
}
