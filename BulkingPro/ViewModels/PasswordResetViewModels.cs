#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BulkingPro.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";
}

public class VerifyCodeViewModel
{
    [Required(ErrorMessage = "Código é obrigatório")]
    [Display(Name = "Código de verificação")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "O código deve ter 6 dígitos")]
    public string Code { get; set; } = "";
    
    public string Email { get; set; } = "";
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = "";
    
    [Required(ErrorMessage = "Código é obrigatório")]
    public string Code { get; set; } = "";
    
    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NewPassword { get; set; } = "";
    
    [Required(ErrorMessage = "Confirme a nova senha")]
    [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmPassword { get; set; } = "";
}

public class ResendCodeViewModel
{
    public string Email { get; set; } = "";
}