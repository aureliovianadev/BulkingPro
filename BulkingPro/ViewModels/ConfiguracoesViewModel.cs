#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BulkingPro.ViewModels
{
    public class ConfiguracoesViewModel
    {
        [Required(ErrorMessage = "Nome obrigatório")]
        public string NomeCompleto { get; set; } = "";

        [Required(ErrorMessage = "E-mail obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = "";

        // Senha
        [DataType(DataType.Password)]
        public string? SenhaAtual { get; set; }

        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem")]
        [DataType(DataType.Password)]
        public string? ConfirmarSenha { get; set; }
    }
}