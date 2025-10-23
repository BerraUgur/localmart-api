namespace WebAPI.ModelViews
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; } // opsiyonel, ileride eklenebilir
        public string NewPassword { get; set; }
    }
}
