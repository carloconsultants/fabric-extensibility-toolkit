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
            public static readonly string Table1 = GetEnvVariable("TABLE_1");
            public static readonly string Table2 = GetEnvVariable("TABLE_2");
            public static readonly string Table3 = GetEnvVariable("TABLE_3");
        }

        public static class BlobContainers
        {
            public static readonly string Container1 = GetEnvVariable("CONTAINER_1");
            public static readonly string Container2 = GetEnvVariable("CONTAINER_2");
        }

        public static class StorageAccount
        {
            public static readonly string AccountName = GetEnvVariable("STORAGE_ACCOUNT_NAME");
            // This should be dev only, production should use managed identity
            public static readonly string ConnectionString = GetEnvVariable("STORAGE_CONNECTION_STRING");
        }
    }
}
