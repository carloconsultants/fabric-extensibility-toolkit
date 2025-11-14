namespace PowerBITips.Api.Models.Constants
{
    public static class RouteConstants
    {

        #region User Routes
        public const string Users = "users";
        public const string UserById = "users/{userId}";
        public const string UserMe = "users/me";
        public const string UserLoginEvent = "users/login/event";
        #endregion

        #region Theme Routes
        // Themes nested under users - user owns their themes
        public const string UserThemes = "users/{userId}/themes";
        public const string UserThemeById = "users/{userId}/themes/{themeId}";
        public const string UserThemeDownload = "users/{userId}/themes/{themeId}/download";
        public const string UserThemeData = "users/{userId}/themes/{themeId}/data";

        // Current user theme operations (uses authenticated user from request)
        public const string Themes = "themes";
        public const string ThemeById = "themes/{themeId}";
        #endregion

        #region Published Item Routes
        public const string Published = "published";
        public const string PublishedById = "published/{itemId}";
        public const string PublishedLayouts = "published/layouts";
        public const string PublishedDownload = "published/{itemId}/download";
        public const string PublishedPreviewImage = "published/{itemId}/preview-image";
        // Legacy published routes (deprecated - retained for temporary compatibility)
        public const string PublishedItems = "published-items"; // DEPRECATED
        public const string PublishedItemsFromTable = "published-items-from-table"; // DEPRECATED
        public const string PublishItem = "publish-item"; // DEPRECATED
        public const string DeletePublishedItem = "published-item"; // DEPRECATED
        #endregion

        #region Subscription Routes (PayPal)
        public const string SubscriptionPlans = "subscriptions/plans";
        public const string Subscriptions = "subscriptions";
        public const string SubscriptionById = "subscriptions/{subscriptionId}";
        #endregion

        #region Analytics Routes
        public const string AnalyticsEvents = "analytics/events";
        public const string AnalyticsEventsGoogle = "analytics/events/google";
        public const string AnalyticsEventsLogin = "analytics/events/login";
        public const string AnalyticsEventsCustom = "analytics/events/custom";
        public const string AnalyticsPageviews = "analytics/pageviews";
        #endregion

        #region Authentication Routes
        public const string AuthTokenPowerBI = "auth/token/powerbi";
        public const string AuthTokenOneLake = "auth/token/onelake";
        #endregion

        #region Embed Routes
        public const string EmbedReports = "embed/reports";
        public const string EmbedReportToken = "embed/reports/{reportId}/token";
        #endregion

        #region Resource Routes
        public const string Resources = "resources";
        public const string ResourcesYoutube = "resources/youtube";
        public const string ResourcesShared = "resources/shared";
        #endregion

        #region Workload Routes (Microsoft Fabric)
        public const string Workload = "workload";
        public const string WorkloadWorkspaces = "workload/workspaces";
        public const string WorkloadWorkspaceById = "workload/workspaces/{workspaceId}";
        public const string WorkloadItems = "workload/workspaces/{workspaceId}/items";
        public const string WorkloadItem = "workload/workspaces/{workspaceId}/items/{itemType}/{itemId}";
        public const string WorkloadItemPayload = "workload/workspaces/{workspaceId}/items/{itemType}/{itemId}/payload";
        public const string WorkloadProxy = "workload/{*route}";
        #endregion

        #region Admin Routes
        public const string AdminUsers = "admin/users";
        public const string AdminUserById = "admin/users/{userId}";
        public const string AdminUserRole = "admin/users/{userId}/role";
        public const string AdminUserTrial = "admin/users/{userId}/trial";
        public const string AdminSubscriptions = "admin/subscriptions";
        public const string AdminAnalytics = "admin/analytics";

        #endregion
    }
}
