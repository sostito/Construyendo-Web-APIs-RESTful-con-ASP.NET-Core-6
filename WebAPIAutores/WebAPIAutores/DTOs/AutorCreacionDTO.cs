using System.ComponentModel.DataAnnotations;
using WebAPIAutores.Validaciones;

namespace WebAPIAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [PrimeraLetraMayuscula]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 15, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
        public string Nombre { get; set; }

    }
}
