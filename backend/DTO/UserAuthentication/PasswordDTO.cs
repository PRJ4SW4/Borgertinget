    public class ResetPasswordDto
    {
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }

        public class ForgotPasswordDto
    {
        public required string Email { get; set; }
    }