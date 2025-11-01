namespace WebAPI.Constants;

/// <summary>
/// Centralized API route definitions
/// </summary>
public static class ApiRoutes
{
    public const string ApiVersion = "v1";
    
    public static class Auth
    {
        private const string Base = "auth";
        
        public const string Login = $"{Base}/login";
        public const string Register = $"{Base}/register";
        public const string RefreshToken = $"{Base}/refresh-token";
        public const string MakeSeller = $"{Base}/{{userId}}/make-seller";
        public const string MakeNormal = $"{Base}/{{userId}}/make-normal";
        public const string UpdateUser = $"{Base}/{{userId}}/update";
        public const string UserList = $"{Base}/userlist";
        public const string GetUser = $"{Base}/{{userId}}";
        public const string DeleteUser = $"{Base}/{{id}}";
        public const string SendMail = $"{Base}/send-mail";
        public const string ResetPassword = $"{Base}/reset-password";
        public const string ForgotPassword = $"{Base}/forgot-password";
    }
    
    public static class Products
    {
        private const string Base = "products";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string GetByUserId = $"{Base}/user/{{userId}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
    }
    
    public static class Orders
    {
        private const string Base = "orders";
        
        public const string GetUserOrders = $"{Base}/user/{{userId}}";
        public const string Create = Base;
        public const string UpdateStatus = $"{Base}/{{id}}/status";
        public const string GetById = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
    }
    
    public static class Comments
    {
        private const string Base = "comments";
        
        public const string GetByProductId = $"{Base}/product/{{productId}}";
        public const string Create = Base;
        public const string Delete = $"{Base}/{{id}}";
    }
    
    public static class Addresses
    {
        private const string Base = "addresses";
        
        public const string GetUserAddresses = $"{Base}/user/{{userId}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
    }
    
    public static class Logs
    {
        private const string Base = "logs";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
    }
}
