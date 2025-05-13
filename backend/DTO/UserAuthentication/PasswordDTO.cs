    public class ResetPasswordDto
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

        public class ForgotPasswordDto
    {
        public required string Email { get; set; }
    }